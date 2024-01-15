using cz.dvojak.k8s.EdgeOperator.Operator.Entities;
using k8s.Models;

namespace cz.dvojak.k8s.EdgeOperator.Services;

/// <summary>
///     Service for creating objects for proxy
/// </summary>
public interface IProxyCreator
{
    /// <summary>
    ///     Returns name of the proxy deployment
    /// </summary>
    /// <param name="entity"></param>
    /// <returns>Name of the proxy deployment</returns>
    string GetProxyName(ConnectionEntity entity);

    /// <summary>
    ///     Creates a proxy deployment to be deployed. This deployment will be serve as a proxy to the devices.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="networkName">Name of <i>network attachment definitions</i> to use as a secondary CNI</param>
    /// <param name="namespace">k8s namespace, where deployment will be deployed</param>
    /// <returns>Deployment</returns>
    Task<V1Deployment> CreateProxyDeployment(ConnectionEntity entity, string networkName, string @namespace);

    /// <summary>
    ///     Creates a proxy service to be deployed.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="namespace"></param>
    /// <returns>service</returns>
    Task<V1Service> CreateProxyService(ConnectionEntity entity, string @namespace);
}