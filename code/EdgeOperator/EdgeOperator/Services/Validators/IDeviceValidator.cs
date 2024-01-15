using cz.dvojak.k8s.EdgeOperator.Models;
using cz.dvojak.k8s.EdgeOperator.Operator.Entities;

namespace cz.dvojak.k8s.EdgeOperator.Services.Validators;

/// <summary>
///     Validate device entity
/// </summary>
public interface IDeviceValidator
{
    /// <summary>
    ///     Error message
    /// </summary>
    string ValidIpAddressMessage { get; }

    /// <summary>
    ///     Error message
    /// </summary>
    string NodeHasLabelMessage { get; }

    /// <summary>
    ///     Error message
    /// </summary>
    string ValidaComponentsUniqueNameMessage { get; }

    /// <summary>
    ///     Error message
    /// </summary>
    string ValidateNodeExistsMessage { get; }

    /// <summary>
    ///     Holds device entity that is being validated
    /// </summary>
    /// <returns></returns>
    DeviceEntity DeviceEntity { get; }

    /// <summary>
    ///     Error message for protocol collision
    /// </summary>
    /// <param name="p"></param>
    /// <returns>Error message for protocol collision</returns>
    string ValidateProtocolCollisionMessage(Protocol p);

    /// <summary>
    ///     Validates IP address of device
    ///     only IPv4 is supported
    /// </summary>
    bool IpAddress();

    /// <summary>
    ///     Validates if node has required labels
    /// </summary>
    /// <returns></returns>
    bool NodeHasLabels();

    /// <summary>
    ///     Validates if components names are unique
    /// </summary>
    /// <returns></returns>
    bool ValidaComponentsUniqueName();

    /// <summary>
    ///     Validates if components have unique ports for protocol
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    bool ValidateProtocolCollision(Protocol p);

    /// <summary>
    ///     Validates if node exists
    /// </summary>
    /// <returns></returns>
    bool ValidateNodeExists();

    /// <summary>
    ///     Sets device entity that is being validated
    /// </summary>
    /// <param name="deviceEntity"></param>
    /// <returns></returns>
    Task SetDeviceEntity(DeviceEntity deviceEntity);
}