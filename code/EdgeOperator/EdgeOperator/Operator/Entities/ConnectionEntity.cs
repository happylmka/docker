using cz.dvojak.k8s.EdgeOperator.Configuration.Definitions;
using cz.dvojak.k8s.EdgeOperator.Extends.KubeOps;
using k8s.Models;
using KubeOps.KubernetesClient.Entities;
using KubeOps.Operator.Entities.Annotations;

namespace cz.dvojak.k8s.EdgeOperator.Operator.Entities;

[KubernetesEntity(Kind = "Connection", Group = Definitions.Api.GROUP, ApiVersion = Definitions.Api.API_VERSION)]
[EntityScope]
[KubernetesEntityShortNames("conn")]
[Description($@"Connection represents a connection between a device and kubernetes cluster.
By creating a connection, service will be created for abstracting device connection.
By sending data to that service, data will be sent to device.

For more information, see Device.{Definitions.Api.GROUP}/{Definitions.Api.API_VERSION}.")]
[GenericAdditionalPrinterColumn(".spec.deviceName", "Device", "string")]
[GenericAdditionalPrinterColumn(".spec.componentNames", "Component", "string")]
[GenericAdditionalPrinterColumn(".spec.networkName", "Network", "string", Priority = 5)]
public class ConnectionEntity : CustomKubernetesEntityRequiredSpec<ConnectionEntity.ConnectionSpec,
    ConnectionEntity.ConnectionStatus>
{
    public class ConnectionSpec
    {
        [Required]
        [Description("Name of the Device to connect to")]
        public string DeviceName { get; set; } = null!;

        [Required]
        [Items(MinItems = 1)]
        [Description("Names of the Components (of device) to connect to")]
        public IList<string> ComponentNames { get; set; } = null!;

        [Required]
        [Description("Name of the NetworkAttachmentDefinition to use for the connection")]
        public string NetworkName { get; set; } = null!;
    }

    public class ConnectionStatus
    {
        [IgnoreProperty] public bool Active { get; set; } = false;
        [IgnoreProperty] public bool Running { get; set; } = false;
        [IgnoreProperty] public IList<string> Pods { get; set; } = null!;
    }
}
