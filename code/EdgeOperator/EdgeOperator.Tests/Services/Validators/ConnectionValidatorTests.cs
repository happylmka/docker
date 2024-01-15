using System.Collections.Generic;
using System.Threading.Tasks;
using cz.dvojak.k8s.EdgeOperator.Operator.Entities;
using cz.dvojak.k8s.EdgeOperator.Services.Validators;
using Moq;
using Shouldly;
using Xunit;

namespace EdgeOperator.Tests.Services.Validators;

public class ConnectionValidatorTests
{
    private readonly ConnectionEntity _sampleConnection = new()
    {
        Spec = new ConnectionEntity.ConnectionSpec
        {
            ComponentNames = new List<string> {"component1", "component2"}, NetworkName = "nad-name",
            DeviceName = "device-name"
        },
        Status = new ConnectionEntity.ConnectionStatus()
    };

    private readonly Mock<IConnectionValidator> _validatorMock = new();

    [Fact]
    public async Task NADExits_Should_Return_False_If_Required_NAD_Does_Not_Exists()
    {
        // Arrange
        var validator = _validatorMock.Object;

        await validator.SetConnection(_sampleConnection);

        // Act
        var validity = validator.NadExits();

        // Assert
        validity.ShouldBeFalse();
    }


    [Fact]
    public void ComponentsAreUp_Should_Return_False_If_Not_All_Required_Components_Are_Up()
    {
        // Arrange
        var validator = _validatorMock.Object;

        // Act
        var validity = validator.ComponentsAreUp();

        // Assert
        validity.ShouldBeFalse();
    }


    [Fact]
    public void DeviceContainsComponents_Should_Return_False_if_Device_Not_Contains_All_Required_Components()
    {
        // Arrange
        var validator = _validatorMock.Object;

        // Act
        var validity = validator.DeviceContainsComponents();

        // Assert
        validity.ShouldBeFalse();
    }


    [Fact]
    public void DeviceExists_Should_Return_False_If_Device_Does_Not_Exists()
    {
        // Arrange
        var validator = _validatorMock.Object;

        // Act
        var validity = validator.DeviceExists();

        // Assert
        validity.ShouldBeFalse();
    }


    [Fact]
    public void DeviceUp_Should_Return_False_If_Device_Is_Down()
    {
        // Arrange
        var validator = _validatorMock.Object;

        // Act
        var validity = validator.DeviceUp();

        // Assert
        validity.ShouldBeFalse();
    }
}