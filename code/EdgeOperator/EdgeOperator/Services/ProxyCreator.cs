using cz.dvojak.k8s.EdgeOperator.Configuration.Options;
using cz.dvojak.k8s.EdgeOperator.Operator.Entities;
using cz.dvojak.k8s.EdgeOperator.Services.Builders;
using cz.dvojak.k8s.EdgeOperator.Services.Validators;
using k8s.Models;
using KubeOps.KubernetesClient;
using Microsoft.Extensions.Options;

namespace cz.dvojak.k8s.EdgeOperator.Services;

/// <inheritdoc />
public class ProxyCreator : IProxyCreator
{
    /// <summary>
    ///     Kubernetes client
    /// </summary>
    private readonly IKubernetesClient _client;

    /// <summary>
    ///     Validator for ConnectionEntity
    /// </summary>
    private readonly IConnectionValidator _connectionValidator;

    /// <summary>
    ///     Options for proxy deployment definition
    /// </summary>
    private readonly DeploymentTemplateOption _deploymentTemplateOption;

    /// <summary>
    ///     Logger
    /// </summary>
    private readonly ILogger<ProxyCreator> _logger;

    /// <summary>
    ///     Builder for creating containers
    /// </summary>
    private readonly IProxyContainerBuilder _proxyContainerBuilder;

    /// <summary>
    ///     Builder for deployments forming proxy
    /// </summary>
    private readonly IProxyDeploymentBuilder _proxyDeploymentBuilder;

    /// <summary>
    ///     Builder for building service proxy
    /// </summary>
    private readonly IProxyServiceBuilder _proxyServiceBuilder;

    public ProxyCreator(
        IKubernetesClient client,
        IProxyDeploymentBuilder proxyDeploymentBuilder,
        IProxyServiceBuilder proxyServiceBuilder,
        IProxyContainerBuilder proxyContainerBuilder,
        IOptions<DeploymentTemplateOption> deploymentTemplateOption,
        IConnectionValidator connectionValidator,
        ILogger<ProxyCreator> logger)
    {
        _client = client;
        _proxyDeploymentBuilder = proxyDeploymentBuilder;
        _proxyServiceBuilder = proxyServiceBuilder;
        _proxyContainerBuilder = proxyContainerBuilder;
        _connectionValidator = connectionValidator;
        _logger = logger;
        _deploymentTemplateOption = deploymentTemplateOption.Value;
    }

    /// <inheritdoc />
    public string GetProxyName(ConnectionEntity entity)
    {
        return _deploymentTemplateOption.PrefixName + entity.Metadata.Name;
    }

    /// <inheritdoc />
    public async Task<V1Deployment> CreateProxyDeployment(ConnectionEntity entity, string networkName,
        string @namespace)
    {
        var device = await Validate(entity);
        var nodeName = device.Spec.NodeName;

        var handlers = device.Spec.Components
            .Where(c => entity.Spec.ComponentNames.Contains(c.Name))
            .SelectMany(c => c.Handlers)
            .ToArray();
        _proxyContainerBuilder.Add(device.Spec.IpAddress, handlers);
        var containers = _proxyContainerBuilder.Build();

        return _proxyDeploymentBuilder.SetName(GetProxyName(entity))
            .SetNamespace(@namespace)
            .SetDeploymentLabels()
            .SetPodLabels()
            .SetPodNetwork(networkName)
            .SetPodNodeSelector()
            .SetPodNode(nodeName)
            .SetPodToleration()
            .SetPodSelector(GetProxyName(entity))
            .SetContainers(containers)
            .Build();
    }

    /// <inheritdoc />
    public async Task<V1Service> CreateProxyService(ConnectionEntity entity, string @namespace)
    {
        var device = await Validate(entity);
        var handlers
            = device.Spec.Components
                .Where(c => entity.Spec.ComponentNames.Contains(c.Name))
                .SelectMany(c => c.Handlers)
                .ToList();

        var mapper = _proxyContainerBuilder.GetPortsMapper()!;
        var mappersEqual = !mapper.Keys.Except(handlers).Any() && !handlers.Except(mapper.Keys).Any();
        if (!mappersEqual)
        {
            _logger.LogWarning("Unknown handlers mapper for services");
            _proxyContainerBuilder.Reset();
            _proxyContainerBuilder.Add("", handlers.ToArray());
            mapper = _proxyContainerBuilder.GetPortsMapper();
            _proxyContainerBuilder.Reset();
        }

        return _proxyServiceBuilder
            .SetName(GetProxyName(entity))
            .SetNamespace(@namespace)
            .SetSelector(GetProxyName(entity))
            .AddHandlers(mapper)
            .Build();
    }

    private static ICollection<DeviceEntity.DeviceSpec.Component.Handler> GetNeededHandlers(ConnectionEntity entity,
        DeviceEntity device)
    {
        return device.Spec.Components
            .Where(c => entity.Spec.ComponentNames.Contains(c.Name))
            .SelectMany(c => c.Handlers)
            .ToList();
    }

    private async Task<ICollection<DeviceEntity.DeviceSpec.Component.Handler>> GetNeededHandlers(
        ConnectionEntity entity)
    {
        var device = await _client.Get<DeviceEntity>(entity.Spec.DeviceName);
        if (device is null)
            throw new NullReferenceException(_connectionValidator.DeviceExistsMessage);
        return GetNeededHandlers(entity, device);
    }

    private async Task<DeviceEntity> Validate(ConnectionEntity entity)
    {
        await _connectionValidator.SetConnection(entity);

        // validate if device exists
        if (!_connectionValidator.DeviceExists())
        {
            _logger.LogError(_connectionValidator.DeviceExistsMessage);
            throw new NullReferenceException(_connectionValidator.DeviceExistsMessage);
        }

        if (!_connectionValidator.DeviceContainsComponents())
        {
            _logger.LogError(_connectionValidator.DeviceExistsMessage);
            throw new NullReferenceException(_connectionValidator.DeviceContainsComponentsMessage);
        }

        var device = await _client.Get<DeviceEntity>(entity.Spec.DeviceName);
        return device!;
    }
}