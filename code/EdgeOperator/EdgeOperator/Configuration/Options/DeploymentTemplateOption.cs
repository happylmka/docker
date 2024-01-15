using k8s.Models;

namespace cz.dvojak.k8s.EdgeOperator.Configuration.Options;

public class DeploymentTemplateOption
{
    public const string DEPLOYMENT_TEMPLATE = "DeploymentTemplate";
    public string PrefixName { get; set; } = null!;
    public string DefaultPodSelectorKey { get; set; } = null!;
    public string AdditionalNetworkAnnotationKey { get; set; } = null!;
    public IDictionary<string, string> Labels { get; set; } = null!;

    public IList<V1Toleration> Toleration { get; set; } = null!;

    public IDictionary<string, string> NodeSelectorLabels { get; set; } = null!;
}