using System.Net;
using System.Net.Http.Json;
using Moq;
using Moq.Protected;

namespace KH.Lab.NomadLab.Tests;

public class NomadServiceTests
{
    private readonly NomadService _nomadService;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;

    public NomadServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("http://localhost:4646")
        };
        _nomadService = new NomadService(httpClient);
    }

    [Fact]
    public async Task GetVariableAsync_ReturnsVariable()
    {
        // Arrange
        var path = "test-variable";
        var expectedVariable = new NomadVariable
        {
            Namespace = "default",
            Path = path,
            Items = new Dictionary<string, string> { { "key", "value" } }
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString().Contains(path)),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = JsonContent.Create(expectedVariable)
            });

        // Act
        var result = await _nomadService.GetVariableAsync(path);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(expectedVariable.Path, result.Path);
        Assert.Equal(expectedVariable.Items, result.Items);
    }

    [Fact]
    public async Task DeleteVariableAsync_CallsCorrectEndpoint()
    {
        // Arrange
        var path = "test-variable";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Delete && req.RequestUri.ToString().Contains(path)),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.NoContent });

        // Act
        await _nomadService.DeleteVariableAsync(path);

        // Assert
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Delete && req.RequestUri.ToString().Contains(path)),
            ItExpr.IsAny<CancellationToken>()
        );
    }

    [Fact]
    public async Task CreateVariableAsync_SendsCorrectData()
    {
        // Arrange
        var variable = new NomadVariable
        {
            Namespace = "default",
            Path = "test-variable",
            Items = new Dictionary<string, string> { { "key", "value" } }
        };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put && req.RequestUri.ToString().Contains(variable.Path)),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage { StatusCode = HttpStatusCode.OK });

        // Act
        await _nomadService.CreateVariableAsync(variable);

        // Assert
        _httpMessageHandlerMock.Protected().Verify(
            "SendAsync",
            Times.Once(),
            ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put && req.RequestUri.ToString().Contains(variable.Path)),
            ItExpr.IsAny<CancellationToken>()
        );
    }
}
