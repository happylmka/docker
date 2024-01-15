using cz.dvojak.k8s.EdgeOperator.Configuration.Options;
using k8s.Models;
using Microsoft.Extensions.Options;

namespace cz.dvojak.k8s.EdgeOperator.Services.Builders;

/// <inheritdoc />
public class ProxyDeploymentBuilder : IProxyDeploymentBuilder
{
    private readonly IOptions<DeploymentTemplateOption> _deploymentTemplateOption;
    private V1Deployment _deployment = null!;

    public ProxyDeploymentBuilder(IOptions<DeploymentTemplateOption> deploymentTemplateOption)
    {
        _deploymentTemplateOption = deploymentTemplateOption;
        Reset();
    }

    /// <inheritdoc />
    public V1Deployment Build()
    {
        return _deployment;
    }

    /// <inheritdoc />
    public IProxyDeploymentBuilder SetNamespace(string @namespace)
    {
        _deployment.Metadata.NamespaceProperty = @namespace;
        _deployment.Spec.Template.Metadata.NamespaceProperty = @namespace;
        return this;
    }

    /// <inheritdoc />
    public IProxyDeploymentBuilder Reset()
    {
        _deployment = new V1Deployment
        {
            Metadata = new V1ObjectMeta
            {
                Labels = new Dictionary<string, string>(),
                Annotations = new Dictionary<string, string>()
            },
            Spec = new V1DeploymentSpec
            {
                Template = new V1PodTemplateSpec
                {
                    Metadata = new V1ObjectMeta
                    {
                        Labels = new Dictionary<string, string>(),
                        Annotations = new Dictionary<string, string>()
                    },
                    Spec = new V1PodSpec
                    {
                        Containers = new List<V1Container>(),
                        RestartPolicy = "Always"
                    }
                }
            }
        };
        return this;
    }

    /// <inheritdoc />
    public IProxyDeploymentBuilder SetPodSelector(string appName)
    {
        return SetPodSelector((_deploymentTemplateOption.Value.DefaultPodSelectorKey, appName));
    }

    /// <inheritdoc />
    public IProxyDeploymentBuilder SetPodSelector((string key, string value) selector)
    {
        _deployment.Spec.Selector = new V1LabelSelector
        {
            MatchLabels = new Dictionary<string, string>
            {
                {selector.key, selector.value}
            }
        };
        _deployment.Spec.Template.Metadata.Labels.Add(selector.key, selector.value);
        return this;
    }

    /// <inheritdoc />
    public IProxyDeploymentBuilder SetName(string name)
    {
        _deployment.Metadata.Name = name;
        return this;
    }

    /// <inheritdoc />
    public IProxyDeploymentBuilder SetDeploymentLabels()
    {
        _deployment.Metadata.Labels = _deployment.Metadata.Labels
            .Concat(_deploymentTemplateOption.Value.Labels)
            .ToDictionary(x => x.Key, x => x.Value);
        return this;
    }

    /// <inheritdoc />
    public IProxyDeploymentBuilder SetPodLabels()
    {
        _deployment.Spec.Template.Metadata.Labels = _deployment.Spec.Template.Metadata.Labels
            .Concat(_deploymentTemplateOption.Value.Labels)
            .ToDictionary(x => x.Key, x => x.Value);
        return this;
    }

    /// <inheritdoc />
    public IProxyDeploymentBuilder SetPodNetwork(string networkName)
    {
        _deployment.Spec.Template.Metadata.Annotations.Add(
            _deploymentTemplateOption.Value.AdditionalNetworkAnnotationKey, networkName);
        return this;
    }

    /// <inheritdoc />
    public IProxyDeploymentBuilder SetPodNodeSelector()
    {
        _deployment.Spec.Template.Spec.NodeSelector = _deploymentTemplateOption.Value.NodeSelectorLabels;
        return this;
    }

    /// <inheritdoc />
    public IProxyDeploymentBuilder SetPodNode(string nodeName)
    {
        _deployment.Spec.Template.Spec.NodeName = nodeName;
        return this;
    }

    /// <inheritdoc />
    public IProxyDeploymentBuilder SetPodToleration()
    {
        _deployment.Spec.Template.Spec.Tolerations = _deploymentTemplateOption.Value.Toleration;
        return this;
    }

    /// <inheritdoc />
    public IProxyDeploymentBuilder SetContainers(IList<V1Container> containers)
    {
        _deployment.Spec.Template.Spec.Containers = containers;
        return this;
    }
}