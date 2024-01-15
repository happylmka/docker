using k8s.Models;

namespace cz.dvojak.k8s.EdgeOperator.Services.Builders;

/// <summary>
///     Builder for proxy deployment
/// </summary>
public interface IProxyDeploymentBuilder
{
    /// <summary>
    ///     Reset builder to default state
    /// </summary>
    /// <returns></returns>
    IProxyDeploymentBuilder Reset();

    /// <summary>
    ///     Build deployment and return it
    /// </summary>
    /// <returns>Created deployment</returns>
    V1Deployment Build();

    IProxyDeploymentBuilder SetNamespace(string @namespace);

    /// <summary>
    ///     Set name of deployment
    /// </summary>
    /// <param name="name">Name of deployment</param>
    /// <returns></returns>
    IProxyDeploymentBuilder SetName(string name);

    /// <summary>
    ///     Set selector por pod
    /// </summary>
    /// <param name="appName">name of the app</param>
    /// <returns></returns>
    IProxyDeploymentBuilder SetPodSelector(string appName);

    /// <summary>
    ///     Set selector for service
    /// </summary>
    /// <param name="selector">[string, string] pair</param>
    /// <returns></returns>
    IProxyDeploymentBuilder SetPodSelector((string key, string value) selector);

    /// <summary>
    ///     Set labels of deployment (identify operator...)
    /// </summary>
    /// <returns></returns>
    IProxyDeploymentBuilder SetDeploymentLabels();

    /// <summary>
    ///     Set labels of pod (identify operator...)
    /// </summary>
    /// <returns></returns>
    IProxyDeploymentBuilder SetPodLabels();

    /// <summary>
    ///     Set additional network for pod
    /// </summary>
    /// <param name="networkName">Name of network (NetworkAttachmentDefinition)</param>
    /// <returns></returns>
    IProxyDeploymentBuilder SetPodNetwork(string networkName);

    /// <summary>
    ///     Set node selector for pod
    /// </summary>
    /// <returns></returns>
    IProxyDeploymentBuilder SetPodNodeSelector();

    /// <summary>
    ///     Set node for pod scheduling
    /// </summary>
    /// <param name="nodeName">Node to schedule pod</param>
    /// <returns></returns>
    IProxyDeploymentBuilder SetPodNode(string nodeName);

    /// <summary>
    ///     Set toleration for pod so it can be scheduled on edge node
    /// </summary>
    /// <returns></returns>
    IProxyDeploymentBuilder SetPodToleration();

    /// <summary>
    ///     Set containers for pod that works as a proxy
    /// </summary>
    /// <param name="containers">List of containers</param>
    /// <returns></returns>
    IProxyDeploymentBuilder SetContainers(IList<V1Container> containers);
}