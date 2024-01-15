using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cz.dvojak.k8s.EdgeOperator.Configuration.Options;
using cz.dvojak.k8s.EdgeOperator.Models;
using cz.dvojak.k8s.EdgeOperator.Operator.Entities;
using cz.dvojak.k8s.EdgeOperator.Services.Validators;
using k8s.Models;
using KubeOps.KubernetesClient;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;
using Xunit;

namespace EdgeOperator.Tests.Services.Validators;

public class DeviceValidatorTests
{
    private readonly Mock<IOptions<DeploymentTemplateOption>> _deploymentTemplateOptionMock = new();
    private readonly Mock<IKubernetesClient> _kubernetesClientMock = new();

    [Theory]
    [InlineData("192.168.0.1", true)]
    [InlineData("10.0.0.1", true)]
    [InlineData("255.255.255.255", true)]
    [InlineData("256.256.256.256", false)]
    [InlineData("192.168.0", false)]
    [InlineData("192.168.0.1.2", false)]
    public async Task IpAddress_Should_Return_Correct_Result(string ipAddress, bool expected)
    {
        // Arrange
        var deviceEntity = new DeviceEntity
        {
            Metadata = new V1ObjectMeta(),
            Spec = new DeviceEntity.DeviceSpec
                {IpAddress = ipAddress, Components = new List<DeviceEntity.DeviceSpec.Component>(), NodeName = ""}
        };
        var validator = new DeviceValidator(_kubernetesClientMock.Object, _deploymentTemplateOptionMock.Object);
        await validator.SetDeviceEntity(deviceEntity);

        // Act
        var result = validator.IpAddress();

        // Assert
        result.ShouldBe(expected);
    }

    [Fact]
    public async Task NodeHasLabels_Should_Return_True_If_All_Required_Labels_Are_Present()
    {
        // Arrange
        var nodeSelector = new Dictionary<string, string> {{"label1", "value1"}, {"label2", "value2"}};
        var deviceEntity = new DeviceEntity
        {
            Spec = new DeviceEntity.DeviceSpec
                {NodeName = "test-node", Components = new List<DeviceEntity.DeviceSpec.Component>()}
        };
        var node = new V1Node
        {
            Metadata = new V1ObjectMeta {Name = "test-node", Labels = nodeSelector}
        };
        _kubernetesClientMock.Setup(x => x.Get<V1Node>("test-node", null)).ReturnsAsync(node);
        var option = Options.Create(new DeploymentTemplateOption {NodeSelectorLabels = nodeSelector});

        var validator = new DeviceValidator(_kubernetesClientMock.Object, option);
        await validator.SetDeviceEntity(deviceEntity);

        // Act
        var result = validator.NodeHasLabels();

        // Assert
        result.ShouldBeTrue();
    }

    [Fact]
    public async Task NodeHasLabels_Should_Return_False_If_Not_All_Required_Labels_Are_Present()
    {
        var nodeSelector = new Dictionary<string, string> {{"label1", "value1"}};
        var nodeLabels = new Dictionary<string, string> {{"label2", "value2"}};

        // Arrange
        var deviceEntity = new DeviceEntity
        {
            Spec = new DeviceEntity.DeviceSpec
                {NodeName = "test-node", Components = new List<DeviceEntity.DeviceSpec.Component>()}
        };
        var node = new V1Node
        {
            Metadata = new V1ObjectMeta {Name = "test-node", Labels = nodeLabels}
        };
        _kubernetesClientMock.Setup(x => x.Get<V1Node>("test-node", null)).ReturnsAsync(node);
        var option = Options.Create(new DeploymentTemplateOption {NodeSelectorLabels = nodeSelector});

        var validator = new DeviceValidator(_kubernetesClientMock.Object, option);
        await validator.SetDeviceEntity(deviceEntity);

        // Act
        var result = validator.NodeHasLabels();

        // Assert
        result.ShouldBeFalse();
    }

    [Theory]
    [InlineData(new[] {"ComponentA", "ComponentB", "ComponentC"}, true)]
    [InlineData(new[] {"ComponentA", "ComponentA", "ComponentC"}, false)]
    public async Task ValidateComponentsUniqueName_ShouldReturnExpectedResult(string[] componentNames,
        bool expectedResult)
    {
        // Arrange
        var deviceEntity = new DeviceEntity
        {
            Spec = new DeviceEntity.DeviceSpec
            {
                NodeName = "", IpAddress = "",
                Components = componentNames
                    .Select(n => new DeviceEntity.DeviceSpec.Component
                    {
                        Name = n,
                        Handlers = new List<DeviceEntity.DeviceSpec.Component.Handler>()
                    })
                    .ToList()
            }
        };
        var validator = new DeviceValidator(_kubernetesClientMock.Object, _deploymentTemplateOptionMock.Object);
        await validator.SetDeviceEntity(deviceEntity);

        // Act
        var result = validator.ValidaComponentsUniqueName();

        // Assert
        result.ShouldBe(expectedResult);
    }

    [Fact]
    public async Task ValidateNodeExists_ShouldReturnTrue_WhenNodeExists()
    {
        // Arrange
        var deviceEntity = new DeviceEntity
        {
            Spec = new DeviceEntity.DeviceSpec
            {
                NodeName = "test-node",
                Components = new List<DeviceEntity.DeviceSpec.Component>(),
                IpAddress = ""
            }
        };
        _kubernetesClientMock.Setup(c => c.Get<V1Node>("test-node", null)).ReturnsAsync(new V1Node());
        var validator = new DeviceValidator(_kubernetesClientMock.Object, _deploymentTemplateOptionMock.Object);
        await validator.SetDeviceEntity(deviceEntity);
        validator.SetDeviceEntity(deviceEntity).Wait();

        // Act
        var result = validator.ValidateNodeExists();

        // Assert
        result.ShouldBeTrue();
    }

    [Theory]
    [InlineData(Protocol.TCP)]
    [InlineData(Protocol.HTTP)]
    [InlineData(Protocol.UDP)]
    public void ValidateProtocolCollision_WhenPortsAreDistinct_ReturnsTrue(Protocol protocol)
    {
        var deviceValidator = new DeviceValidator(_kubernetesClientMock.Object, _deploymentTemplateOptionMock.Object);
        deviceValidator.SetDeviceEntity(new DeviceEntity
            {
                Spec = new DeviceEntity.DeviceSpec
                {
                    Components = new List<DeviceEntity.DeviceSpec.Component>
                    {
                        new()
                        {
                            Name = "ComponentA",
                            Handlers = new List<DeviceEntity.DeviceSpec.Component.Handler>
                            {
                                new() {Protocol = protocol, Port = 80}
                            }
                        },
                        new()
                        {
                            Name = "ComponentB",
                            Handlers = new List<DeviceEntity.DeviceSpec.Component.Handler>
                            {
                                new() {Protocol = protocol, Port = 443}
                            }
                        }
                    },
                    IpAddress = "",
                    NodeName = ""
                }
            })
            .Wait();

        var result = deviceValidator.ValidateProtocolCollision(protocol);

        result.ShouldBeTrue();
    }

    [Theory]
    [InlineData(Protocol.TCP)]
    [InlineData(Protocol.HTTP)]
    [InlineData(Protocol.UDP)]
    public void ValidateProtocolCollision_WhenPortsAreNotDistinct_ReturnsFalse(Protocol protocol)
    {
        var deviceValidator = new DeviceValidator(_kubernetesClientMock.Object, _deploymentTemplateOptionMock.Object);
        deviceValidator.SetDeviceEntity(new DeviceEntity
            {
                Spec = new DeviceEntity.DeviceSpec
                {
                    Components = new List<DeviceEntity.DeviceSpec.Component>
                    {
                        new()
                        {
                            Name = "ComponentA",
                            Handlers = new List<DeviceEntity.DeviceSpec.Component.Handler>
                            {
                                new() {Protocol = protocol, Port = 80}
                            }
                        },
                        new()
                        {
                            Name = "ComponentB",
                            Handlers = new List<DeviceEntity.DeviceSpec.Component.Handler>
                            {
                                new() {Protocol = protocol, Port = 80}
                            }
                        }
                    },
                    IpAddress = "",
                    NodeName = ""
                }
            })
            .Wait();

        var result = deviceValidator.ValidateProtocolCollision(protocol);

        result.ShouldBeFalse();
    }

    [Theory]
    [InlineData(Protocol.TCP, false)]
    [InlineData(Protocol.HTTP, false)]
    [InlineData(Protocol.UDP, true)]
    public void ValidateProtocolCollision_WhenPortsAreNotDistinct_HTTPMix_ReturnsFalse(Protocol protocol,
        bool expectedResult)
    {
        var deviceValidator = new DeviceValidator(_kubernetesClientMock.Object, _deploymentTemplateOptionMock.Object);
        deviceValidator.SetDeviceEntity(new DeviceEntity
            {
                Spec = new DeviceEntity.DeviceSpec
                {
                    Components = new List<DeviceEntity.DeviceSpec.Component>
                    {
                        new()
                        {
                            Name = "ComponentA",
                            Handlers = new List<DeviceEntity.DeviceSpec.Component.Handler>
                            {
                                new() {Protocol = Protocol.HTTP, Port = 80}
                            }
                        },
                        new()
                        {
                            Name = "ComponentB",
                            Handlers = new List<DeviceEntity.DeviceSpec.Component.Handler>
                            {
                                new() {Protocol = protocol, Port = 80}
                            }
                        }
                    },
                    IpAddress = "",
                    NodeName = ""
                }
            })
            .Wait();

        var result = deviceValidator.ValidateProtocolCollision(protocol);

        result.ShouldBe(expectedResult);
    }

    [Theory]
    [InlineData(Protocol.TCP)]
    [InlineData(Protocol.HTTP)]
    [InlineData(Protocol.UDP)]
    public async Task ValidateProtocolCollision_WhenPortsAreDistinct_HTTPMix_ReturnsTrue(Protocol protocol)
    {
        var deviceValidator = new DeviceValidator(_kubernetesClientMock.Object, _deploymentTemplateOptionMock.Object);
        var device = new DeviceEntity
        {
            Spec = new DeviceEntity.DeviceSpec
            {
                Components = new List<DeviceEntity.DeviceSpec.Component>
                {
                    new()
                    {
                        Name = "ComponentA",
                        Handlers = new List<DeviceEntity.DeviceSpec.Component.Handler>
                        {
                            new() {Protocol = Protocol.HTTP, Port = 80}
                        }
                    },
                    new()
                    {
                        Name = "ComponentB",
                        Handlers = new List<DeviceEntity.DeviceSpec.Component.Handler>
                        {
                            new() {Protocol = protocol, Port = 443}
                        }
                    }
                },
                IpAddress = "",
                NodeName = ""
            }
        };
        await deviceValidator.SetDeviceEntity(device);

        var result = deviceValidator.ValidateProtocolCollision(protocol);

        result.ShouldBeTrue();
    }
}