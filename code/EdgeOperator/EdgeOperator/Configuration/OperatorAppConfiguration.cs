using cz.dvojak.k8s.EdgeOperator.Configuration.Options;
using cz.dvojak.k8s.EdgeOperator.Extends.IHost;
using KubeOps.Operator;
using Microsoft.Extensions.Options;

namespace cz.dvojak.k8s.EdgeOperator.Configuration;

public static class OperatorAppConfiguration
{
    public static void ConfigureEdgeOperatorApp(this WebApplication app)
    {
        app.UseKubernetesOperator();
    }

    public static void PreRunLogging(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment()) return;
        app.LogObject<IOptions<ProxyTemplateOption>>(writeIndented: true);
        app.LogObject<IOptions<DeploymentTemplateOption>>(writeIndented: true);
    }
}