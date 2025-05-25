using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Api;
using Application.DTOs;

public class AuthIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ShouldReturnToken_WhenDataIsValid()
    {
        var dto = new RegisterDto
        {
            UserName = $"testuser_{Guid.NewGuid().ToString("N").Substring(0, 6)}",
            Email = $"test_{Guid.NewGuid().ToString("N").Substring(0, 4)}@example.com",
            Password = "StrongP@ss1"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", dto);
        var content = await response.Content.ReadAsStringAsync();

        Assert.True(response.IsSuccessStatusCode, $"Registration failed: {content}");

        var result = await response.Content.ReadFromJsonAsync<AuthDto>();
        Assert.True(result.IsAuthenticated);
        Assert.False(string.IsNullOrWhiteSpace(result.Token));
    }

   [Fact]
public async Task Login_ShouldReturnToken_WhenCredentialsAreCorrect()
{
    var registerDto = new RegisterDto
    {
        UserName = $"user_{Guid.NewGuid().ToString("N").Substring(0, 6)}",
        Email = $"login_{Guid.NewGuid().ToString("N").Substring(0, 4)}@example.com",
        Password = "TestP@ssw0rd!"
    };

    var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
    registerResponse.EnsureSuccessStatusCode();

    var GetTokenDto = new TokenRequestModel
    {
        Username = registerDto.UserName,
        Password = registerDto.Password
    };

    var GetTokenResponse = await _client.PostAsJsonAsync("/api/auth/token", GetTokenDto);
    var content = await GetTokenResponse.Content.ReadAsStringAsync();

    Assert.True(GetTokenResponse.IsSuccessStatusCode, $"Login failed: {content}");

    var result = await GetTokenResponse.Content.ReadFromJsonAsync<AuthDto>();
    Assert.NotNull(result);
    Assert.True(result.IsAuthenticated);
    Assert.False(string.IsNullOrWhiteSpace(result.Token));
}

}
