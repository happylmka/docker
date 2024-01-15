using cz.dvojak.k8s.EdgeOperator.Configuration.Definitions;
using cz.dvojak.k8s.EdgeOperator.Extends.KubeOps;
using k8s.Models;
using KubeOps.Operator.Entities.Annotations;

namespace cz.dvojak.k8s.EdgeOperator.Models;

[IgnoreEntity]
[KubernetesEntity(
    Group = Definitions.ExternalApi.NAD_GROUP,
    ApiVersion = Definitions.ExternalApi.NAD_API_VERSION,
    Kind = "NetworkAttachmentDefinition",
    PluralName = "network-attachment-definitions"
)]
public class NADEntity : CustomKubernetesEntityRequiredSpec<NADEntity.NADSpec>
{
    public class NADSpec
    {
        [Required] public string Config { get; set; } = "";
    }
}