using KH.Lab.NomadLab.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace KH.Lab.NomadLab.Tests;

public class NomadVariablesControllerTests
{
    private readonly Mock<NomadService> _nomadServiceMock;
    private readonly NomadVariablesController _controller;

    public NomadVariablesControllerTests()
    {
        _nomadServiceMock = new Mock<NomadService>(null); // 此處可傳入 null 因為會使用 Mock
        _controller = new NomadVariablesController(_nomadServiceMock.Object);
    }

    [Fact]
    public async Task GetVariable_ReturnsOkResult_WithVariable()
    {
        // Arrange
        var path = "test-variable";
        var expectedVariable = new NomadVariable { Path = path, Items = new Dictionary<string, string> { { "key", "value" } } };

        _nomadServiceMock.Setup(service => service.GetVariableAsync(path)).ReturnsAsync(expectedVariable);

        // Act
        var result = await _controller.GetVariable(path);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(expectedVariable, okResult.Value);
    }

    [Fact]
    public async Task DeleteVariable_ReturnsNoContentResult()
    {
        // Arrange
        var path = "test-variable";

        _nomadServiceMock.Setup(service => service.DeleteVariableAsync(path)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteVariable(path);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task CreateOrUpdateVariable_ReturnsOkResult()
    {
        // Arrange
        var variable = new NomadVariable { Path = "test-variable", Items = new Dictionary<string, string> { { "key", "value" } } };

        _nomadServiceMock.Setup(service => service.CreateOrUpdateVariableAsync(variable)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CreateOrUpdateVariable(variable);

        // Assert
        Assert.IsType<OkResult>(result);
    }
}
