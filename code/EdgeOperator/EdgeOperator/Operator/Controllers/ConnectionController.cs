using cz.dvojak.k8s.EdgeOperator.Extends.KubeOps.Client;
using cz.dvojak.k8s.EdgeOperator.Models;
using cz.dvojak.k8s.EdgeOperator.Operator.Entities;
using cz.dvojak.k8s.EdgeOperator.Services;
using k8s.Models;
using KubeOps.KubernetesClient;
using KubeOps.Operator.Controller;
using KubeOps.Operator.Controller.Results;
using KubeOps.Operator.Rbac;

namespace cz.dvojak.k8s.EdgeOperator.Operator.Controllers;

[EntityRbac(typeof(ConnectionEntity), Verbs = RbacVerb.All)]
[EntityRbac(typeof(DeviceEntity), Verbs = RbacVerb.All)]
[EntityRbac(typeof(V1Deployment), Verbs = RbacVerb.All)]
[EntityRbac(typeof(V1Service), Verbs = RbacVerb.All)]
[EntityRbac(typeof(V1Node), Verbs = RbacVerb.Get | RbacVerb.List)]
[EntityRbac(typeof(NADEntity), Verbs = RbacVerb.Get | RbacVerb.List)]
public class ConnectionController : IResourceController<ConnectionEntity>
{
    private readonly IKubernetesClient _client;
    private readonly ILogger<ConnectionController> _logger;
    private readonly IProxyCreator _proxyCreator;

    public ConnectionController(ILogger<ConnectionController> logger, IKubernetesClient client,
        IProxyCreator proxyCreator)
    {
        _logger = logger;
        _client = client;
        _proxyCreator = proxyCreator;
    }

    public async Task<ResourceControllerResult?> ReconcileAsync(ConnectionEntity entity)
    {
        var proxyName = _proxyCreator.GetProxyName(entity);
        var @namespace = entity.Namespace();

        try
        {
            if (!await _client.Exists<V1Deployment>(proxyName, @namespace))
            {
                _logger.LogInformation($"Constructing proxy deployment {proxyName}");
                var deployment =
                    await _proxyCreator.CreateProxyDeployment(entity, entity.Spec.NetworkName, entity.Namespace());
                _logger.LogInformation($"Scheduling proxy deployment {proxyName}");
                await _client.Save(deployment);
                _logger.LogInformation($"Proxy deployment {proxyName} scheduled");
            }
            else
            {
                _logger.LogInformation($"Proxy deployment {proxyName} already exists");
                _logger.LogInformation($"Constructing proxy deployment {proxyName}");
                var deployment =
                    await _proxyCreator.CreateProxyDeployment(entity, entity.Spec.NetworkName, entity.Namespace());
                _logger.LogInformation($"Updating proxy deployment {proxyName}");
                await _client.Update(deployment);
                _logger.LogInformation($"Proxy deployment {proxyName} updated");}
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }

        try
        {
            if (!await _client.Exists<V1Service>(proxyName, @namespace))
            {
                _logger.LogInformation($"Constructing proxy service {proxyName}");
                var service = await _proxyCreator.CreateProxyService(entity, entity.Namespace());
                _logger.LogInformation($"Scheduling proxy service {proxyName}");
                await _client.Save(service);
                _logger.LogInformation($"Proxy service {proxyName} scheduled");
            }
            else
            {
                _logger.LogInformation($"Proxy service {proxyName} already exists");
                _logger.LogInformation($"Constructing proxy service {proxyName}");
                var service = await _proxyCreator.CreateProxyService(entity, entity.Namespace());
                _logger.LogInformation($"Updating proxy service {proxyName}");
                await _client.Update(service);
                _logger.LogInformation($"Proxy service {proxyName} updated");            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
        }

        return null;
    }

    public Task StatusModifiedAsync(ConnectionEntity entity)
    {
        return Task.CompletedTask;
    }

    public async Task DeletedAsync(ConnectionEntity entity)
    {
        var proxyName = _proxyCreator.GetProxyName(entity);
        try
        {
            _logger.LogInformation("Delete proxy service {connectionName}", proxyName);
            // await _client.Delete<V1Service>(proxyName,entity.Namespace());
            // await _client.ApiClient.AppsV1.DeleteNamespacedDeploymentWithHttpMessagesAsync(proxyName,entity.Namespace());
            await _client.ApiClient.CoreV1.DeleteNamespacedServiceWithHttpMessagesAsync(proxyName, entity.Namespace());
            _logger.LogInformation("Service {connectionName} deleted", proxyName);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while deleting deployment {connectionName}", proxyName);
        }

        try
        {
            _logger.LogInformation("Delete proxy deployment {connectionName}", proxyName);
            await _client.ApiClient.AppsV1.DeleteNamespacedDeploymentWithHttpMessagesAsync(proxyName,
                entity.Namespace());
            _logger.LogInformation("Deployment {connectionName} deleted", proxyName);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while deleting deployment {connectionName}", proxyName);
        }
    }
}
