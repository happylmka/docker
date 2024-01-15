using cz.dvojak.k8s.EdgeOperator.Configuration.Options;
using cz.dvojak.k8s.EdgeOperator.Operator.Entities;
using cz.dvojak.k8s.EdgeOperator.Services.Validators;
using KubeOps.Operator.Webhooks;
using Microsoft.Extensions.Options;

namespace cz.dvojak.k8s.EdgeOperator.Operator.Webhooks;

public class ConnectionEntityValidator : IValidationWebhook<ConnectionEntity>
{
    private readonly IConnectionValidator _connectionValidator;
    private readonly ILogger<ConnectionEntityValidator> _logger;
    private readonly IOptions<ValidatorOption> _validatorOption;

    public ConnectionEntityValidator(
        ILogger<ConnectionEntityValidator> logger,
        IOptions<ValidatorOption> validatorOption,
        IConnectionValidator connectionValidator)
    {
        _logger = logger;
        _connectionValidator = connectionValidator;
        _validatorOption = validatorOption;
    }

    public AdmissionOperations Operations => AdmissionOperations.Create | AdmissionOperations.Update;

    public Task<ValidationResult> CreateAsync(ConnectionEntity newEntity, bool dryRun)
    {
        return ValidateConnectionEntity(newEntity);
    }

    public Task<ValidationResult> UpdateAsync(ConnectionEntity oldEntity, ConnectionEntity newEntity, bool dryRun)
    {
        return ValidateConnectionEntity(newEntity);
    }

    private async Task<ValidationResult> ValidateConnectionEntity(ConnectionEntity entity)
    {
        _logger.LogDebug($"Validating connection {entity.Metadata.Name} entity");
        IList<string> warnings = new List<string>();
        await _connectionValidator.SetConnection(entity);

        // Check if device exists
        if (!_connectionValidator.DeviceExists())
        {
            var w = _connectionValidator.DeviceExistsMessage;
            _logger.LogInformation(w);
            warnings.Add(w);
            if (_validatorOption.Value.ConnectionStrict)
                return ValidationResult.Fail(StatusCodes.Status400BadRequest, w);
        }
        else
        {
            // Check if device is up
            if (!_connectionValidator.DeviceUp())
            {
                var w = _connectionValidator.DeviceUpMessage;
                _logger.LogInformation(w);
                warnings.Add(w);
                if (_validatorOption.Value.ConnectionStrict)
                    return ValidationResult.Fail(StatusCodes.Status400BadRequest, w);
            }

            // Check if device contains components
            if (!_connectionValidator.DeviceContainsComponents())
            {
                var w = _connectionValidator.DeviceContainsComponentsMessage;
                _logger.LogInformation(w);
                warnings.Add(w);
                if (_validatorOption.Value.ConnectionStrict)
                    return ValidationResult.Fail(StatusCodes.Status400BadRequest, w);
            }

            // Check if component are up  
            if (!_connectionValidator.ComponentsAreUp())
            {
                var w = _connectionValidator.ComponentsAreUpMessage;
                _logger.LogInformation(w);
                warnings.Add(w);
                if (_validatorOption.Value.ConnectionStrict)
                    return ValidationResult.Fail(StatusCodes.Status400BadRequest, w);
            }
        }

        // Check if NAD exists
        if (!_connectionValidator.NadExits())
        {
            var w = _connectionValidator.NadExitsMessage;
            _logger.LogInformation(w);
            warnings.Add(w);
            if (_validatorOption.Value.ConnectionStrict)
                return ValidationResult.Fail(StatusCodes.Status400BadRequest, w);
        }

        return ValidationResult.Success(warnings.ToArray());
    }
}