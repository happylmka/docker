using cz.dvojak.k8s.EdgeOperator.Configuration.Definitions;
using cz.dvojak.k8s.EdgeOperator.Extends.KubeOps;
using cz.dvojak.k8s.EdgeOperator.Models;
using k8s.Models;
using KubeOps.KubernetesClient.Entities;
using KubeOps.Operator.Entities.Annotations;

namespace cz.dvojak.k8s.EdgeOperator.Operator.Entities;

[KubernetesEntity(Kind = "Device", Group = Definitions.Api.GROUP, ApiVersion = Definitions.Api.API_VERSION)]
[EntityScope(EntityScope.Cluster)]
[KubernetesEntityShortNames("dev")]
[Description($@"Device represents a device connected to the cluster.
This device should be available over one of the nodes in the cluster.
Usually those devices are directly connected to one of nodes.
Devices are often placed in private network that is not accessible from anywhere,
but node(s) in the cluster, that serve as a gateway to the device. 

Device represents a entity that can communicate over TCP/UDP.
This entity can be a physical device or any other service.

This information holds information about the device that are referred 
in Connection.{Definitions.Api.GROUP}/{Definitions.Api.API_VERSION}.

For more information, see Connection.{Definitions.Api.GROUP}/{Definitions.Api.API_VERSION}.
")]
[GenericAdditionalPrinterColumn(".spec.nodeName", "NodeName", "string")]
[GenericAdditionalPrinterColumn(".spec.up", "Up/Active", "integer", Priority = 5)]
[GenericAdditionalPrinterColumn(".spec.ipAddress", "IpAddress", "string")]
//using advanced jq for nicer formats is not possible see https://github.com/kubernetes/kubectl/issues/517 and https://github.com/kubernetes/kubernetes/pull/101205
[GenericAdditionalPrinterColumn(".spec.components", "Components", "string", Priority = 50)]
public class DeviceEntity : CustomKubernetesEntityRequiredSpec<DeviceEntity.DeviceSpec>
{
    public class DeviceSpec
    {
        [Required]
        [Description("Name of the node, the device is connected to")]
        public string NodeName { get; set; } = string.Empty;

        [Description("Determines if the device is up or down (ready and running or not)")]
        public bool Up { get; set; } = true;

        [Required]
        [Description("IP address of the device")]
        public string IpAddress { get; set; } = null!;

        [Description("List of components on the device")]
        [Required]
        [Items(MinItems = 1)]
        public IList<Component> Components { get; set; } = null!;

        [Description("Components represents a service running on the device")]
        public class Component
        {
            [Description("Name of the component")]
            [Required]
            public string Name { get; set; } = null!;

            [Description("Determines if the component is up or down (ready and running or not)")]
            public bool Up { get; set; } = true;

            [Required]
            [Description("List of handlers of the component (ports)")]
            [Items(MinItems = 1)]
            public IList<Handler> Handlers { get; set; } = null!;

            [Description("Handler represents a port of the component")]
            public class Handler
            {
                [Description("Name of the handler for the component")]
                public string? Name { get; set; } = null!;

                [Description("Type of the handler protocol (TCP, UDP, HTTP)")]
                public Protocol Protocol { get; set; } = Protocol.TCP;

                [Description("Port of the handler")]
                [Required]
                [RangeMinimum(Minimum = 0)]
                [RangeMaximum(Maximum = 65535)]
                public int Port { get; set; }

                [Description("List of endpoints of the handler")]
                public IList<string>? EndPoints { get; set; }

                public override bool Equals(object? obj)
                {
                    return obj is Handler handler &&
                           Name == handler.Name &&
                           Protocol == handler.Protocol &&
                           Port == handler.Port;
                }

                public override int GetHashCode()
                {
                    return HashCode.Combine(Name, Protocol, Port);
                }
            }
        }
    }
}
