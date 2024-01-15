using cz.dvojak.k8s.EdgeOperator.Configuration.Options;
using cz.dvojak.k8s.EdgeOperator.Models;
using cz.dvojak.k8s.EdgeOperator.Operator.Entities;
using cz.dvojak.k8s.EdgeOperator.Services.Validators;
using KubeOps.Operator.Webhooks;
using Microsoft.Extensions.Options;

namespace cz.dvojak.k8s.EdgeOperator.Operator.Webhooks;

public class DeviceEntityValidator : IValidationWebhook<DeviceEntity>
{
    private readonly IDeviceValidator _deviceValidator;

    private readonly ILogger<DeviceEntityValidator> _logger;

    private readonly ValidatorOption _validatorOption;

    public DeviceEntityValidator(
        ILogger<DeviceEntityValidator> logger,
        IOptions<ValidatorOption> validatorOption,
        IDeviceValidator deviceValidator)
    {
        _logger = logger;
        _deviceValidator = deviceValidator;
        _validatorOption = validatorOption.Value;
    }

    public AdmissionOperations Operations => AdmissionOperations.Create | AdmissionOperations.Update;

    public Task<ValidationResult> CreateAsync(DeviceEntity newEntity, bool dryRun)
    {
        return Validate(newEntity);
    }

    public Task<ValidationResult> UpdateAsync(DeviceEntity oldEntity, DeviceEntity newEntity, bool dryRun)
    {
        return Validate(newEntity);
    }

    private async Task<ValidationResult> Validate(DeviceEntity entity)
    {
        IList<string> warnings = new List<string>();
        _logger.LogDebug($"Validating device {entity.Metadata.Name} entity");


        await _deviceValidator.SetDeviceEntity(entity);
        if (!_deviceValidator.IpAddress())
        {
            var w = _deviceValidator.ValidIpAddressMessage;
            _logger.LogInformation(w);
            warnings.Add(w);
            if (_validatorOption.DeviceStrict)
                return ValidationResult.Fail(StatusCodes.Status400BadRequest, w);
        }

        if (!_deviceValidator.ValidateNodeExists())
        {
            var w = _deviceValidator.ValidateNodeExistsMessage;
            _logger.LogInformation(w);
            warnings.Add(w);
            if (_validatorOption.DeviceStrict)
                return ValidationResult.Fail(StatusCodes.Status400BadRequest, w);
        }
        else
        {
            if (!_deviceValidator.NodeHasLabels())
            {
                var w = _deviceValidator.NodeHasLabelMessage;
                _logger.LogInformation(w);
                warnings.Add(w);
                if (_validatorOption.DeviceStrict)
                    return ValidationResult.Fail(StatusCodes.Status400BadRequest, w);
            }
        }

        if (!_deviceValidator.ValidaComponentsUniqueName())
        {
            var w = _deviceValidator.ValidaComponentsUniqueNameMessage;
            _logger.LogInformation(w);
            warnings.Add(w);
            if (_validatorOption.DeviceStrict)
                return ValidationResult.Fail(StatusCodes.Status400BadRequest, w);
        }

        if (!_deviceValidator.ValidateProtocolCollision(Protocol.TCP))
        {
            var w = _deviceValidator.ValidateProtocolCollisionMessage(Protocol.TCP);
            _logger.LogInformation(w);
            warnings.Add(w);
            if (_validatorOption.DeviceStrict)
                return ValidationResult.Fail(StatusCodes.Status400BadRequest, w);
        }

        if (!_deviceValidator.ValidateProtocolCollision(Protocol.UDP))
        {
            var w = _deviceValidator.ValidateProtocolCollisionMessage(Protocol.UDP);
            _logger.LogInformation(w);
            warnings.Add(w);
            if (_validatorOption.DeviceStrict)
                return ValidationResult.Fail(StatusCodes.Status400BadRequest, w);
        }


        return ValidationResult.Success(warnings.ToArray());
    }
}