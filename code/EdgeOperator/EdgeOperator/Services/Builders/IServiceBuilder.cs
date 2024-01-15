using cz.dvojak.k8s.EdgeOperator.Operator.Entities;
using k8s.Models;

namespace cz.dvojak.k8s.EdgeOperator.Services.Builders;

/// <summary>
///     Builder for proxy service
/// </summary>
public interface IProxyServiceBuilder
{
    /// <summary>
    ///     Reset builder to default state
    /// </summary>
    void Reset();

    /// <summary>
    ///     Build service and return it
    /// </summary>
    /// <returns></returns>
    V1Service Build();

    /// <summary>
    ///     Set namespace for service
    /// </summary>
    /// <param name="namespace"></param>
    /// <returns></returns>
    IProxyServiceBuilder SetNamespace(string @namespace);

    /// <summary>
    ///     Set name for service
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    IProxyServiceBuilder SetName(string name);

    /// <summary>
    ///     Set selector for service
    /// </summary>
    /// <param name="appName">name of the app</param>
    /// <returns></returns>
    IProxyServiceBuilder SetSelector(string appName);

    /// <summary>
    ///     Set selector for service
    /// </summary>
    /// <param name="selector">[string, string] pair</param>
    /// <returns></returns>
    IProxyServiceBuilder SetSelector((string key, string value) selector);

    /// <summary>
    ///     Add port to service
    /// </summary>
    /// <param name="handlers"></param>
    /// <returns></returns>
    IProxyServiceBuilder AddHandlers(IDictionary<DeviceEntity.DeviceSpec.Component.Handler, int> handlersMapper);
}