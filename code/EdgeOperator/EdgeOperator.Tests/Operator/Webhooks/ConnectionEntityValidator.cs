using System.Threading.Tasks;
using cz.dvojak.k8s.EdgeOperator.Configuration.Options;
using cz.dvojak.k8s.EdgeOperator.Operator.Entities;
using cz.dvojak.k8s.EdgeOperator.Operator.Webhooks;
using cz.dvojak.k8s.EdgeOperator.Services.Validators;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;
using Xunit;

namespace EdgeOperator.Tests.Operator.Webhooks;

public abstract class AbstractConnectionEntityValidator
{
    protected readonly ILogger<ConnectionEntityValidator> Logger = Mock.Of<ILogger<ConnectionEntityValidator>>();
    protected readonly Mock<IConnectionValidator> ValidatorMock;

    protected AbstractConnectionEntityValidator()
    {
        ValidatorMock = new Mock<IConnectionValidator>();
    }
}

public class ConnectionValidatorNonStrict : AbstractConnectionEntityValidator
{
    private readonly ConnectionEntityValidator _deviceEntityValidator;

    public ConnectionValidatorNonStrict()
    {
        var options = Options.Create(new ValidatorOption());
        _deviceEntityValidator = new ConnectionEntityValidator(Mock.Of<ILogger<ConnectionEntityValidator>>(),
            options, ValidatorMock.Object);
    }

    [Theory]
    [InlineData(true, true, true, true, true, 0)]
    [InlineData(false, true, true, true, true, 1)] // nADExits
    [InlineData(true, false, true, true, true, 1)] // deviceExists
    [InlineData(true, false, false, true, true, 1)] // deviceExists & deviceUp
    [InlineData(true, false, false, false, true, 1)] // deviceExists & deviceUp & deviceContainsComponents
    [InlineData(true, false, false, false, false,
        1)] // deviceExists & deviceUp & deviceContainsComponents & componentsAreUp
    [InlineData(true, true, false, true, true, 1)] // deviceUp
    [InlineData(true, true, false, false, true, 2)] // deviceUp & deviceContainsComponents
    [InlineData(true, true, false, false, false, 3)] // deviceUp & deviceContainsComponents & componentsAreUp
    [InlineData(false, false, false, false, false, 2)] // All
    public async Task CreateConnection_ValidateEntity_NonStrictMode__Result_NonEmptyWarnings(
        bool nAdExits,
        bool deviceExists,
        bool deviceUp,
        bool deviceContainsComponents,
        bool componentsAreUp,
        int expectedNumberOfWarnings)
    {
        // Act
        ValidatorMock.Setup(x => x.NadExits()).Returns(nAdExits);
        ValidatorMock.Setup(x => x.DeviceExists()).Returns(deviceExists);
        ValidatorMock.Setup(x => x.DeviceUp()).Returns(deviceUp);
        ValidatorMock.Setup(x => x.DeviceContainsComponents()).Returns(deviceContainsComponents);
        ValidatorMock.Setup(x => x.ComponentsAreUp()).Returns(componentsAreUp);

        // Act
        var res = await _deviceEntityValidator.CreateAsync(new ConnectionEntity(), false);

        // Assert
        res.Valid.ShouldBeTrue();
        res.Warnings.Count.ShouldBe(expectedNumberOfWarnings);
    }

    [Theory]
    [InlineData(true, true, true, true, true, 0)]
    [InlineData(false, true, true, true, true, 1)] // nADExits
    [InlineData(true, false, true, true, true, 1)] // deviceExists
    [InlineData(true, false, false, true, true, 1)] // deviceExists & deviceUp
    [InlineData(true, false, false, false, true, 1)] // deviceExists & deviceUp & deviceContainsComponents
    [InlineData(true, false, false, false, false,
        1)] // deviceExists & deviceUp & deviceContainsComponents & componentsAreUp
    [InlineData(true, true, false, true, true, 1)] // deviceUp
    [InlineData(true, true, false, false, true, 2)] // deviceUp & deviceContainsComponents
    [InlineData(true, true, false, false, false, 3)] // deviceUp & deviceContainsComponents & componentsAreUp
    [InlineData(false, false, false, false, false, 2)] // All
    public async Task UpdateConnection_ValidateEntity_NonStrictMode__Result_NonEmptyWarnings(
        bool nAdExits,
        bool deviceExists,
        bool deviceUp,
        bool deviceContainsComponents,
        bool componentsAreUp,
        int expectedNumberOfWarnings)
    {
        // Act
        ValidatorMock.Setup(x => x.NadExits()).Returns(nAdExits);
        ValidatorMock.Setup(x => x.DeviceExists()).Returns(deviceExists);
        ValidatorMock.Setup(x => x.DeviceUp()).Returns(deviceUp);
        ValidatorMock.Setup(x => x.DeviceContainsComponents()).Returns(deviceContainsComponents);
        ValidatorMock.Setup(x => x.ComponentsAreUp()).Returns(componentsAreUp);

        // Act
        var res = await _deviceEntityValidator.UpdateAsync(new ConnectionEntity(), new ConnectionEntity(), false);

        // Assert
        res.Valid.ShouldBeTrue();
        res.Warnings.Count.ShouldBe(expectedNumberOfWarnings);
    }
}

public class ConnectionValidatorStrict : AbstractConnectionEntityValidator
{
    private readonly ConnectionEntityValidator _deviceEntityValidator;

    public ConnectionValidatorStrict()
    {
        var options = Options.Create(new ValidatorOption {ConnectionStrict = true, DeviceStrict = true});
        _deviceEntityValidator =
            new ConnectionEntityValidator(Logger, options, ValidatorMock.Object);
    }


    [Theory]
    [InlineData(true, true, true, true, true, true)]
    [InlineData(false, true, true, true, true, false)]
    [InlineData(true, false, true, true, true, false)]
    [InlineData(true, true, false, true, true, false)]
    [InlineData(true, true, true, false, true, false)]
    [InlineData(true, true, true, true, false, false)]
    public async Task CreateConnection_ValidateDevice_NonStrictMode__Result(
        bool nAdExits,
        bool deviceExists,
        bool deviceUp,
        bool deviceContainsComponents,
        bool componentsAreUp,
        bool expected)
    {
        // Arrange
        ValidatorMock.Setup(x => x.NadExits()).Returns(nAdExits);
        ValidatorMock.Setup(x => x.DeviceExists()).Returns(deviceExists);
        ValidatorMock.Setup(x => x.DeviceUp()).Returns(deviceUp);
        ValidatorMock.Setup(x => x.DeviceContainsComponents()).Returns(deviceContainsComponents);
        ValidatorMock.Setup(x => x.ComponentsAreUp()).Returns(componentsAreUp);

        // Act
        var res = await _deviceEntityValidator.CreateAsync(new ConnectionEntity(), false);

        // Assert
        res.Valid.ShouldBe(expected);
    }

    [Theory]
    [InlineData(true, true, true, true, true, true)]
    [InlineData(false, true, true, true, true, false)]
    [InlineData(true, false, true, true, true, false)]
    [InlineData(true, true, false, true, true, false)]
    [InlineData(true, true, true, false, true, false)]
    [InlineData(true, true, true, true, false, false)]
    public async Task UpdateConnection_ValidateDevice_NonStrictMode__Result(
        bool nAdExits,
        bool deviceExists,
        bool deviceUp,
        bool deviceContainsComponents,
        bool componentsAreUp,
        bool expected)
    {
        // Arrange
        ValidatorMock.Setup(x => x.NadExits()).Returns(nAdExits);
        ValidatorMock.Setup(x => x.DeviceExists()).Returns(deviceExists);
        ValidatorMock.Setup(x => x.DeviceUp()).Returns(deviceUp);
        ValidatorMock.Setup(x => x.DeviceContainsComponents()).Returns(deviceContainsComponents);
        ValidatorMock.Setup(x => x.ComponentsAreUp()).Returns(componentsAreUp);

        // Act
        var res = await _deviceEntityValidator.UpdateAsync(new ConnectionEntity(), new ConnectionEntity(), false);

        // Assert
        res.Valid.ShouldBe(expected);
    }
}