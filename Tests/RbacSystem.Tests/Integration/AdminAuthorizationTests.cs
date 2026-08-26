using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RbacSystem.Domain.Common;

namespace RbacSystem.Tests.Integration;

public class AdminAuthorizationTests : IClassFixture<AdminAuthorizationTests.TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private const string TestJwtKey = "super_secret_test_jwt_key_that_is_at_least_32_bytes_long_123!";
    private const string Issuer = "RbacSystem";
    private const string Audience = "RbacSystemUsers";

    public AdminAuthorizationTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAdminEndpoint_WithoutAuthentication_ShouldReturn401Unauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/admin");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAdminEndpoint_WithInvalidToken_ShouldReturn401Unauthorized()
    {
        // Arrange
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", "invalid.jwt.token");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetAdminEndpoint_WithNonAdminRole_ShouldReturn403Forbidden()
    {
        // Arrange
        var token = GenerateToken(role: "user");
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetAdminEndpoint_WithoutAnyRole_ShouldReturn403Forbidden()
    {
        // Arrange
        var token = GenerateToken(role: null);
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetAdminEndpoint_WithAdminRole_ShouldReturn200Ok()
    {
        // Arrange
        var token = GenerateToken(role: AppRoles.Admin);
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/admin");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private static string GenerateToken(string? role)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new(ClaimTypes.Email, "user@example.com")
        };

        if (!string.IsNullOrEmpty(role))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }

    public class TestWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Jwt:Issuer"] = Issuer,
                    ["Jwt:Audience"] = Audience,
                    ["Jwt:Key"] = TestJwtKey
                });
            });
        }
    }
}
