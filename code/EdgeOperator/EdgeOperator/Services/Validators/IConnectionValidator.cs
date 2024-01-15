using cz.dvojak.k8s.EdgeOperator.Operator.Entities;

namespace cz.dvojak.k8s.EdgeOperator.Services.Validators;

/// <summary>
///     Validate connection entity
/// </summary>
public interface IConnectionValidator
{
    /// <summary>
    ///     Error message
    /// </summary>
    string DeviceExistsMessage { get; }

    /// <summary>
    ///     Error message
    /// </summary>
    string DeviceUpMessage { get; }

    /// <summary>
    ///     Error message
    /// </summary>
    string DeviceContainsComponentMessage { get; }

    /// <summary>
    ///     Error message
    /// </summary>
    string DeviceContainsComponentsMessage { get; }

    /// <summary>
    ///     Error message
    /// </summary>
    string ComponentsAreUpMessage { get; }

    /// <summary>
    ///     Error message
    /// </summary>
    string NadExitsMessage { get; }

    /// <summary>
    ///     Holds Connection entity that is being validated
    /// </summary>
    /// <returns></returns>
    ConnectionEntity Connection { get; }

    /// <summary>
    ///     Validates if device exists
    /// </summary>
    /// <returns></returns>
    bool DeviceExists();

    /// <summary>
    ///     Validates if device is up
    /// </summary>
    /// <returns></returns>
    bool DeviceUp();

    /// <summary>
    ///     Validates if device contains component
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    bool DeviceContainsComponent(string name);

    /// <summary>
    ///     Validates if device contains all required components
    /// </summary>
    /// <returns></returns>
    bool DeviceContainsComponents();

    /// <summary>
    ///     Validates if all required components are up
    /// </summary>
    /// <returns></returns>
    bool ComponentsAreUp();

    /// <summary>
    ///     Validates if NAD (Network Attachment Definition) exists
    /// </summary>
    /// <returns></returns>
    bool NadExits();

    /// <summary>
    ///     Sets connection to be validated
    /// </summary>
    /// <param name="connection"></param>
    /// <returns></returns>
    Task SetConnection(ConnectionEntity connection);
}