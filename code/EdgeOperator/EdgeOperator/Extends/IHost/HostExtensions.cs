using System.Text.Json;

namespace cz.dvojak.k8s.EdgeOperator.Extends.IHost;

public static class HostExtensions
{
    /// <summary>
    ///     Write content of object to log
    ///     Its useful for debugging options pattern
    /// </summary>
    /// <param name="host"></param>
    /// <param name="logLevel"></param>
    /// <param name="writeIndented"></param>
    /// <typeparam name="TObj"></typeparam>
    /// <returns></returns>
    public static Microsoft.Extensions.Hosting.IHost LogObject<TObj>(
        this Microsoft.Extensions.Hosting.IHost host,
        LogLevel logLevel = LogLevel.Information,
        bool writeIndented = false)
    {
        var scope = host.Services.CreateScope().ServiceProvider;
        var logger = scope.GetService<ILoggerFactory>()?.CreateLogger(nameof(HostExtensions));
        var option = scope.GetService<TObj>();
        if (logger is null || option is null)
            return host;
        logger.Log(logLevel,
            $"Object: {JsonSerializer.Serialize(option, new JsonSerializerOptions {WriteIndented = writeIndented})}");
        return host;
    }
}