using cz.dvojak.k8s.EdgeOperator.Configuration.Options;
using cz.dvojak.k8s.EdgeOperator.Models;
using cz.dvojak.k8s.EdgeOperator.Operator.Entities;
using k8s.Models;
using KubeOps.KubernetesClient;
using Microsoft.Extensions.Options;

namespace cz.dvojak.k8s.EdgeOperator.Services.Validators;

/// <inheritdoc />
public partial class DeviceValidator : IDeviceValidator
{
    /// <inheritdoc />
    public string ValidIpAddressMessage => $"Invalid IP address {DeviceEntity.Spec.IpAddress}";

    /// <inheritdoc />
    public bool IpAddress()
    {
        var bytes = DeviceEntity.Spec.IpAddress.Split('.');
        return bytes.Length == 4 && bytes.All(r => byte.TryParse(r, out _));
    }

    /// <inheritdoc />
    public string NodeHasLabelMessage => $"Node {_node.Name()} does not have required labels for node selector";

    /// <inheritdoc />
    public bool NodeHasLabels()
    {
        return _deploymentTemplateOption.NodeSelectorLabels.All(
            pair => _node.Labels().ContainsKey(pair.Key) && _node.Labels()[pair.Key] == pair.Value
        );
    }

    /// <inheritdoc />
    public string ValidaComponentsUniqueNameMessage =>
        $"Components names in {DeviceEntity.Name()} device are not unique";

    /// <inheritdoc />
    public bool ValidaComponentsUniqueName()
    {
        return DeviceEntity.Spec.Components.Select(x => x.Name).Distinct().Count() ==
               DeviceEntity.Spec.Components.Count;
    }

    /// <inheritdoc />
    public string ValidateProtocolCollisionMessage(Protocol p)
    {
        return $"Components in {DeviceEntity.Name()} device have same {p} ports";
    }

    /// <inheritdoc />
    public bool ValidateProtocolCollision(Protocol p)
    {
        var key = p == Protocol.UDP
            ? Protocol.UDP
            : Protocol.TCP;
        if (!_protocolPorts!.TryGetValue(key, out var ports))
            return true;
        return ports.Count == ports.Distinct().Count();
    }

    /// <inheritdoc />
    public string ValidateNodeExistsMessage => $"Node {DeviceEntity.Spec.NodeName} does not exist";

    /// <inheritdoc />
    public bool ValidateNodeExists()
    {
        return _node is not null;
    }
}

/// <inheritdoc />
public partial class DeviceValidator
{
    /// <summary>
    /// </summary>
    private readonly DeploymentTemplateOption _deploymentTemplateOption;

    /// <summary>
    /// </summary>
    private readonly IKubernetesClient _kubernetesClient;

    /// <summary>
    ///     Holds device entity that is being validated
    /// </summary>
    private DeviceEntity? _deviceEntity;

    /// <summary>
    ///     Holds node that is referred by device entity
    /// </summary>
    private V1Node? _node;

    /// <summary>
    ///     Grouped ports by protocol
    ///     Protocol.UDP => List of UDP ports
    ///     Protocol.TCP => List of TCP and HTTP ports
    /// </summary>
    private IDictionary<Protocol, List<int>>? _protocolPorts;

    public DeviceValidator(IKubernetesClient kubernetesClient,
        IOptions<DeploymentTemplateOption> deploymentTemplateOption)
    {
        _kubernetesClient = kubernetesClient;
        _deploymentTemplateOption = deploymentTemplateOption.Value;
    }

    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">If device is not set</exception>
    public DeviceEntity DeviceEntity => _deviceEntity ?? throw new InvalidOperationException("DeviceEntity is null");

    /// <inheritdoc />
    public async Task SetDeviceEntity(DeviceEntity deviceEntity)
    {
        _deviceEntity = deviceEntity;
        _node = await _kubernetesClient.Get<V1Node>(DeviceEntity.Spec.NodeName);
        _protocolPorts = DeviceEntity.Spec.Components
            .SelectMany(c => c.Handlers)
            .GroupBy(h => h.Protocol == Protocol.UDP
                ? Protocol.UDP
                : Protocol.TCP)
            .ToDictionary(
                h => h.Key,
                h => h.Select(hh => hh.Port).ToList());
    }
}