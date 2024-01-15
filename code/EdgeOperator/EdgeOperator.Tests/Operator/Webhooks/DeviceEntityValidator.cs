using System.Threading.Tasks;
using cz.dvojak.k8s.EdgeOperator.Configuration.Options;
using cz.dvojak.k8s.EdgeOperator.Models;
using cz.dvojak.k8s.EdgeOperator.Operator.Entities;
using cz.dvojak.k8s.EdgeOperator.Operator.Webhooks;
using cz.dvojak.k8s.EdgeOperator.Services.Validators;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;
using Xunit;

namespace EdgeOperator.Tests.Operator.Webhooks;

public abstract class AbstractDeviceEntityValidator
{
    protected readonly ILogger<DeviceEntityValidator> Logger;
    protected readonly Mock<IDeviceValidator> ValidatorMock;

    protected AbstractDeviceEntityValidator()
    {
        Logger = Mock.Of<ILogger<DeviceEntityValidator>>();
        ValidatorMock = new Mock<IDeviceValidator>();
    }
}

public class DeviceEntityValidatorNonStrict : AbstractDeviceEntityValidator
{
    private readonly DeviceEntityValidator _deviceEntityValidator;

    public DeviceEntityValidatorNonStrict()
    {
        var options = Options.Create(new ValidatorOption());
        _deviceEntityValidator = new DeviceEntityValidator(Logger, options, ValidatorMock.Object);
    }

    [Theory]
    [InlineData(true, true, true, true, true, 0)]
    [InlineData(false, true, true, true, true, 1)] // IpAddress
    [InlineData(true, false, true, true, true, 1)] // ValidaComponentsUniqueName
    [InlineData(true, true, false, true, true, 2)] // ValidateProtocolCollision
    [InlineData(true, true, true, false, true, 1)] // ValidateNodeExists
    [InlineData(true, true, true, true, false, 1)] // NodeHasLabels
    [InlineData(false, false, false, false, false, 5)] // All
    [InlineData(false, false, false, false, true, 5)] // All
    [InlineData(false, false, false, true, false, 5)] // All
    public async Task CreateDevice_ValidateDevice_NonStrictMode__Result_NonEmptyWarnings(
        bool ipAddress,
        bool validaComponentsUniqueName,
        bool validateProtocolCollision,
        bool validateNodeExists,
        bool nodeHasLabels,
        int expectedNumberOfWarnings)
    {
        // Act
        ValidatorMock.Setup(x => x.IpAddress()).Returns(ipAddress);
        ValidatorMock.Setup(x => x.ValidaComponentsUniqueName()).Returns(validaComponentsUniqueName);
        ValidatorMock.Setup(x => x.ValidateProtocolCollision(It.IsAny<Protocol>())).Returns(validateProtocolCollision);
        ValidatorMock.Setup(x => x.ValidateNodeExists()).Returns(validateNodeExists);
        ValidatorMock.Setup(x => x.NodeHasLabels()).Returns(nodeHasLabels);
        // Act
        var res = await _deviceEntityValidator.CreateAsync(new DeviceEntity(), false);

        // Assert
        res.Valid.ShouldBeTrue();
        res.Warnings.Count.ShouldBe(expectedNumberOfWarnings);
    }

    [Theory]
    [InlineData(true, true, true, true, true, 0)]
    [InlineData(false, true, true, true, true, 1)] // IpAddress
    [InlineData(true, false, true, true, true, 1)] // ValidaComponentsUniqueName
    [InlineData(true, true, false, true, true, 2)] // ValidateProtocolCollision
    [InlineData(true, true, true, false, true, 1)] // ValidateNodeExists
    [InlineData(true, true, true, true, false, 1)] // NodeHasLabels
    [InlineData(false, false, false, false, false, 5)] // All
    [InlineData(false, false, false, false, true, 5)] // All
    [InlineData(false, false, false, true, false, 5)] // All
    public async Task UpdateDevice_ValidateDevice_NonStrictMode__Result_NonEmptyWarnings(
        bool ipAddress,
        bool validaComponentsUniqueName,
        bool validateProtocolCollision,
        bool validateNodeExists,
        bool nodeHasLabels,
        int expectedNumberOfWarnings)
    {
        // Act
        ValidatorMock.Setup(x => x.IpAddress()).Returns(ipAddress);
        ValidatorMock.Setup(x => x.ValidaComponentsUniqueName()).Returns(validaComponentsUniqueName);
        ValidatorMock.Setup(x => x.ValidateProtocolCollision(It.IsAny<Protocol>())).Returns(validateProtocolCollision);
        ValidatorMock.Setup(x => x.ValidateNodeExists()).Returns(validateNodeExists);
        ValidatorMock.Setup(x => x.NodeHasLabels()).Returns(nodeHasLabels);
        // Act
        var res = await _deviceEntityValidator.UpdateAsync(new DeviceEntity(), new DeviceEntity(), false);

        // Assert
        res.Valid.ShouldBeTrue();
        res.Warnings.Count.ShouldBe(expectedNumberOfWarnings);
    }
}

public class DeviceEntityValidatorStrict : AbstractDeviceEntityValidator
{
    private readonly DeviceEntityValidator _deviceEntityValidator;

    public DeviceEntityValidatorStrict()
    {
        var options = Options.Create(new ValidatorOption {ConnectionStrict = true, DeviceStrict = true});
        _deviceEntityValidator = new DeviceEntityValidator(Logger, options, ValidatorMock.Object);
    }


    [Theory]
    [InlineData(true, true, true, true, true, true)]
    [InlineData(false, true, true, true, true, false)]
    [InlineData(true, false, true, true, true, false)]
    [InlineData(true, true, false, true, true, false)]
    [InlineData(true, true, true, false, true, false)]
    [InlineData(true, true, true, true, false, false)]
    public async Task CreateDevice_ValidateDevice_NonStrictMode__Result(
        bool ipAddress,
        bool validaComponentsUniqueName,
        bool validateProtocolCollision,
        bool validateNodeExists,
        bool nodeHasLabels,
        bool expected)
    {
        // Arrange
        ValidatorMock.Setup(x => x.IpAddress()).Returns(ipAddress);
        ValidatorMock.Setup(x => x.NodeHasLabels()).Returns(nodeHasLabels);
        ValidatorMock.Setup(x => x.ValidaComponentsUniqueName()).Returns(validaComponentsUniqueName);
        ValidatorMock.Setup(x => x.ValidateProtocolCollision(It.IsAny<Protocol>())).Returns(validateProtocolCollision);
        ValidatorMock.Setup(x => x.ValidateNodeExists()).Returns(validateNodeExists);

        // Act
        var res = await _deviceEntityValidator.CreateAsync(new DeviceEntity(), false);

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
    public async Task UpdateDevice_ValidateDevice_NonStrictMode__Result(
        bool ipAddress,
        bool validaComponentsUniqueName,
        bool validateProtocolCollision,
        bool validateNodeExists,
        bool nodeHasLabels,
        bool expected)
    {
        // Arrange
        ValidatorMock.Setup(x => x.IpAddress()).Returns(ipAddress);
        ValidatorMock.Setup(x => x.NodeHasLabels()).Returns(nodeHasLabels);
        ValidatorMock.Setup(x => x.ValidaComponentsUniqueName()).Returns(validaComponentsUniqueName);
        ValidatorMock.Setup(x => x.ValidateProtocolCollision(It.IsAny<Protocol>())).Returns(validateProtocolCollision);
        ValidatorMock.Setup(x => x.ValidateNodeExists()).Returns(validateNodeExists);

        // Act
        var res = await _deviceEntityValidator.UpdateAsync(new DeviceEntity(), new DeviceEntity(), false);

        // Assert
        res.Valid.ShouldBe(expected);
    }
}