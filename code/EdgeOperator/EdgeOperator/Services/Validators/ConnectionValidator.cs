using cz.dvojak.k8s.EdgeOperator.Configuration.Definitions;
using cz.dvojak.k8s.EdgeOperator.Models;
using cz.dvojak.k8s.EdgeOperator.Operator.Entities;
using k8s;
using k8s.Models;
using KubeOps.KubernetesClient;
using Newtonsoft.Json;

namespace cz.dvojak.k8s.EdgeOperator.Services.Validators;

/// <inheritdoc />
public partial class ConnectionValidator : IConnectionValidator
{
    /// <inheritdoc />
    public string DeviceExistsMessage => $"Device {_device.Name()} does not exist";

    /// <inheritdoc />
    public bool DeviceExists()
    {
        return _device is not null;
    }

    /// <inheritdoc />
    public string DeviceUpMessage => $"Device {_device.Name()} is not up";

    /// <inheritdoc />
    public bool DeviceUp()
    {
        return _device!.Spec.Up;
    }

    /// <inheritdoc />
    public string DeviceContainsComponentMessage =>
        $"Device {_device.Name()} does not contain component {Connection.Spec.ComponentNames}";

    /// <inheritdoc />
    public bool DeviceContainsComponent(string name)
    {
        return _device!.Spec.Components.Select(c => c.Name).Contains(name);
    }

    /// <inheritdoc />
    public string DeviceContainsComponentsMessage =>
        $"Device {_device.Name()} does not contain components {Connection.Spec.ComponentNames}";

    /// <inheritdoc />
    public bool DeviceContainsComponents()
    {
        return Connection.Spec.ComponentNames.All(n => _device!.Spec.Components.Select(c => c.Name).Contains(n));
    }

    /// <inheritdoc />
    public string ComponentsAreUpMessage =>
        $"Components {Connection.Spec.ComponentNames} in device {_device.Name()} are not up";

    /// <inheritdoc />
    public bool ComponentsAreUp()
    {
        return _device!.Spec.Components.Where(c => Connection.Spec.ComponentNames.Contains<string>(c.Name))
            .All(c => c.Up);
    }

    /// <inheritdoc />
    public string NadExitsMessage => $"NAD {_nad.Name()} does not exist";

    /// <inheritdoc />
    public bool NadExits()
    {
        return _nad is not null;
    }
}

/// <inheritdoc />
public partial class ConnectionValidator
{
    private readonly IKubernetesClient _kubernetesClient;
    private ConnectionEntity? _connection;
    private DeviceEntity? _device;
    private NADEntity? _nad;

    public ConnectionValidator(IKubernetesClient kubernetesClient)
    {
        _kubernetesClient = kubernetesClient;
    }

    public ConnectionEntity Connection => _connection ?? throw new InvalidOperationException("Connection is not set");

    public async Task SetConnection(ConnectionEntity connection)
    {
        _connection = connection;
        _device = await _kubernetesClient.Get<DeviceEntity>(Connection.Spec.DeviceName);

        var a =
            await _kubernetesClient
                .ApiClient
                .CustomObjects
                .ListClusterCustomObjectAsync(
                    Definitions.ExternalApi.NAD_GROUP,
                    Definitions.ExternalApi.NAD_API_VERSION,
                    Definitions.ExternalApi.NAD_PLURAL_NAME
                );
        if (a is null)
        {
            _nad = null;
        }
        else
        {
            var b = JsonConvert
                .DeserializeAnonymousType(
                    a.ToString()!,
                    new {items = new List<NADEntity>()}
                );
            _nad = b
                .items
                .FirstOrDefault(e => e.Metadata.Name == Connection.Spec.NetworkName);
        }
    }
}
