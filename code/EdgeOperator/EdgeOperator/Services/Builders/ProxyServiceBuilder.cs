using cz.dvojak.k8s.EdgeOperator.Configuration.Options;
using cz.dvojak.k8s.EdgeOperator.Models;
using cz.dvojak.k8s.EdgeOperator.Operator.Entities;
using k8s.Models;
using Microsoft.Extensions.Options;

namespace cz.dvojak.k8s.EdgeOperator.Services.Builders;

/// <inheritdoc />
public class ProxyServiceBuilder : IProxyServiceBuilder
{
    private readonly IOptions<DeploymentTemplateOption> _deploymentTemplateOption;
    private V1Service _service = null!;

    public ProxyServiceBuilder(IOptions<DeploymentTemplateOption> deploymentTemplateOption)
    {
        _deploymentTemplateOption = deploymentTemplateOption;
        Reset();
    }

    /// <inheritdoc />
    public void Reset()
    {
        _service = new V1Service
        {
            Metadata = new V1ObjectMeta(),
            Spec = new V1ServiceSpec
            {
                Ports = new List<V1ServicePort>(),
                Selector = new Dictionary<string, string>(),
                Type = "ClusterIP"
            }
        };
    }

    /// <inheritdoc />
    public V1Service Build()
    {
        return _service;
    }

    /// <inheritdoc />
    public IProxyServiceBuilder SetNamespace(string @namespace)
    {
        _service.Metadata.NamespaceProperty = @namespace;
        return this;
    }

    /// <inheritdoc />
    public IProxyServiceBuilder SetName(string name)
    {
        _service.Metadata.Name = name;
        return this;
    }

    /// <inheritdoc />
    public IProxyServiceBuilder SetSelector(string appName)
    {
        _service.Spec.Selector.Add(_deploymentTemplateOption.Value.DefaultPodSelectorKey, appName);
        return this;
    }

    /// <inheritdoc />
    public IProxyServiceBuilder SetSelector((string key, string value) selector)
    {
        _service.Spec.Selector.Add(selector.key, selector.value);
        return this;
    }

    /// <inheritdoc />
    public IProxyServiceBuilder AddHandlers(IDictionary<DeviceEntity.DeviceSpec.Component.Handler, int> handlersMapper)
    {
        foreach (var (handler, port) in handlersMapper)
        {
            var name =
                $"{handler.Name ?? ""}"
                +
                $"{handler.Protocol.ToString().ToLower()}"
                +
                $"{handler.Port}";
            _service.Spec.Ports.Add(new V1ServicePort
            {
                Name = name,
                Port = handler.Port,
                Protocol = (handler.Protocol == Protocol.HTTP ? Protocol.TCP : handler.Protocol).ToString().ToUpper(),
                TargetPort = port
            });
        }

        return this;
    }
}