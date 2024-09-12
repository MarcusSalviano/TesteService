using ReceiverService.Models.Dtos;
using ReceiverService.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Text.Json;
using System.Net;
using System.Text;

namespace ReceiverService.Test;
public class ReceiverControllerTest : IClassFixture<WebApplicationFactory<ReceiverController>>
{
    private readonly HttpClient _client;

    public ReceiverControllerTest(WebApplicationFactory<ReceiverController> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task PostLogin_ReturnsOkResponse_WhenValidLogin()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Usuario = "root"
        };
        var jsonContent = new StringContent(JsonSerializer.Serialize(loginDto), Encoding.UTF8, "application/json");
        var requestUrl = "/receiver/login";

        // Act
        var response = await _client.PostAsync(requestUrl, jsonContent);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("root1")]
    [InlineData("1root")]
    public async Task PostLogin_ReturnsUnauthorizedResponse_WhenInvalidLogin(string usuario)
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Usuario = usuario
        };
        var jsonContent = new StringContent(JsonSerializer.Serialize(loginDto), Encoding.UTF8, "application/json");
        var requestUrl = "/receiver/login";

        // Act
        var response = await _client.PostAsync(requestUrl, jsonContent);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ReturnsOKResponse_WhenValidTokenIsPassed()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Usuario = "root"
        };
        var jsonContent = new StringContent(JsonSerializer.Serialize(loginDto), Encoding.UTF8, "application/json");

        // Act
        var responseLogin = await _client.PostAsync("/receiver/login", jsonContent);
        var responseLoginString = await responseLogin.Content.ReadAsStringAsync();

        var jsonResponse = JsonSerializer.Deserialize<LoginResponse>(responseLoginString, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var token = jsonResponse.Token;

        _client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var response = await _client.GetAsync("/receiver");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("root", "/receiver/")]
    [InlineData("root", "/receiver/1")]
    public async Task Get_ReturnsUnauthorizedResponse_WhenValidTokenIsNotPassed(string usuario, string requestUrl)
    {
        // Arrange
        _client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await _client.GetAsync(requestUrl);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private class LoginResponse
    {
        public string Token { get; set; }
    }
}