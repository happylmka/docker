using cz.dvojak.k8s.EdgeOperator.Configuration.Options;
using cz.dvojak.k8s.EdgeOperator.Models;
using cz.dvojak.k8s.EdgeOperator.Operator.Entities;
using k8s.Models;
using Microsoft.Extensions.Options;

namespace cz.dvojak.k8s.EdgeOperator.Services.Builders;

/// <inheritdoc />
public class ProxyContainerBuilder : IProxyContainerBuilder
{
    private readonly ProxyTemplateOption _proxyTemplateOption;
    private IList<V1Container> _containers = null!;
    private int _port;
    private IDictionary<DeviceEntity.DeviceSpec.Component.Handler, int> _portsMapper;

    public ProxyContainerBuilder(IOptions<ProxyTemplateOption> proxyTemplateOption)
    {
        _proxyTemplateOption = proxyTemplateOption.Value;
        _containers = new List<V1Container>();
        _port = _proxyTemplateOption.BasePort;
        _portsMapper = new Dictionary<DeviceEntity.DeviceSpec.Component.Handler, int>();
    }

    /// <inheritdoc />
    public IList<V1Container> Build()
    {
        return _containers;
    }

    /// <inheritdoc />
    public void SetBasePort(int port)
    {
        _port = port;
    }

    /// <inheritdoc />
    public IProxyContainerBuilder Add(string ip, params DeviceEntity.DeviceSpec.Component.Handler[] service)
    {
        foreach (var variable in service)
        {
            _containers.Add(CreateProxyContainer(ip, (variable, _port)));
            _portsMapper.Add(variable, _port);
            _port++;
        }

        return this;
    }

    public IDictionary<DeviceEntity.DeviceSpec.Component.Handler, int> GetPortsMapper()
    {
        return _portsMapper;
    }

    /// <inheritdoc />
    public IProxyContainerBuilder Reset()
    {
        _containers = new List<V1Container>();
        _port = _proxyTemplateOption.BasePort;
        _portsMapper = new Dictionary<DeviceEntity.DeviceSpec.Component.Handler, int>();
        return this;
    }

    private IEnumerable<string> CreateConnectionArgs(
        string ip,
        (DeviceEntity.DeviceSpec.Component.Handler handler, int outsidePort) service)
    {
        return service.handler.Protocol is Protocol.TCP or Protocol.HTTP
            ? string.Format(_proxyTemplateOption.TcpCommandTemplate,
                    service.outsidePort,
                    ip,
                    service.handler.Port
                )
                .Split(' ')
            : string.Format(_proxyTemplateOption.UdpCommandTemplate,
                    service.outsidePort,
                    ip,
                    service.handler.Port
                )
                .Split(' ');
    }

    private V1Container CreateProxyContainer(string ip,
        (DeviceEntity.DeviceSpec.Component.Handler handler, int outsidePort) handler)
    {
        var name =
            (handler.handler.Name is not null ? $"{handler.handler.Name}-" : "")
            +
            $"{handler.handler.Protocol.ToString().ToLower()}"
            +
            $"{handler.handler.Port}";
        return new V1Container
        {
            Name = name,
            Image = _proxyTemplateOption.ImageName,
            Args = _proxyTemplateOption.Args.Concat(new[] {"-lp", name})
                .Concat(CreateConnectionArgs(ip, handler))
                .ToArray()
        };
    }
}