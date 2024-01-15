using k8s;
using k8s.Models;
using KubeOps.KubernetesClient;

namespace cz.dvojak.k8s.EdgeOperator.Extends.KubeOps.Client;

public static class ResourceExists
{
    /// <summary>
    ///     Checks if resource exists
    /// </summary>
    /// <param name="client"></param>
    /// <param name="name"></param>
    /// <param name="namespace"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static async Task<bool> Exists<T>(this IKubernetesClient client, string name, string? @namespace = null)
        where T : class, IKubernetesObject<V1ObjectMeta>
    {
        return await client.Get<T>(name, @namespace) != null;
    }
}