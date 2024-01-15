using cz.dvojak.k8s.EdgeOperator.Configuration.Options;
using cz.dvojak.k8s.EdgeOperator.Services;
using cz.dvojak.k8s.EdgeOperator.Services.Builders;
using cz.dvojak.k8s.EdgeOperator.Services.Validators;
using KubeOps.Operator;
using Serilog;

namespace cz.dvojak.k8s.EdgeOperator.Configuration;

public static class OperatorBuilderConfiguration
{
    public static void ConfigureEdgeOperatorProject(this WebApplicationBuilder builder)
    {
        ConfigureLogging(builder.Configuration, builder.Logging);
        // ConfigureDatabase(builder.Configuration, builder.Services);
        // ConfigureAuth(builder.Configuration, builder.Services);
        // ConfigureAuth0ManagementApi(builder.Configuration, builder.Services);
        // ConfigureCors(builder.Services);
        // ConfigureWebServerService(builder.Configuration, builder.Services);
        ConfigureRuntimeDiService(builder.Services, builder.Configuration, builder.Environment);
        // ConfigureSwagger(builder.Services);
    }

    private static void ConfigureLogging(IConfiguration configuration, ILoggingBuilder logging)
    {
        // === Logging Setup ===
        logging.ClearProviders();
        logging.AddSerilog(new LoggerConfiguration().ReadFrom.Configuration(configuration).CreateLogger());
    }

    private static void ConfigureRuntimeDiService(
        IServiceCollection builderServices,
        IConfiguration configuration,
        IHostEnvironment webHostEnvironment)
    {
        // === DI Setup ===
        builderServices.AddKubernetesOperator();

        builderServices.Configure<ProxyTemplateOption>(configuration.GetSection(ProxyTemplateOption.PROXY_TEMPLATE));
        builderServices.Configure<DeploymentTemplateOption>(
            configuration.GetSection(DeploymentTemplateOption.DEPLOYMENT_TEMPLATE));
        builderServices.Configure<ValidatorOption>(configuration.GetSection(ValidatorOption.VALIDATOR));

        builderServices.AddScoped<IProxyContainerBuilder, ProxyContainerBuilder>();
        builderServices.AddScoped<IProxyDeploymentBuilder, ProxyDeploymentBuilder>();
        builderServices.AddScoped<IProxyCreator, ProxyCreator>();
        builderServices.AddScoped<IConnectionValidator, ConnectionValidator>();
        builderServices.AddScoped<IDeviceValidator, DeviceValidator>();
        builderServices.AddScoped<IProxyServiceBuilder, ProxyServiceBuilder>();

        var _ = webHostEnvironment.IsDevelopment()
            ? builderServices.AddKubernetesOperator().AddWebhookLocaltunnel()
            : builderServices.AddKubernetesOperator();
    }
}