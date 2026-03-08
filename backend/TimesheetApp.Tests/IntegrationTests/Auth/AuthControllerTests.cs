using System.Net;
using System.Net.Http.Json;
using TimesheetApp.API.DTOs;
using TimesheetApp.API.Models;

namespace TimesheetApp.Tests.IntegrationTests.Auth;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthControllerTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_Should_Return200_WhenValid()
    {
        var client = _factory.CreateClient();
        var request = new RegisterRequest(
            Email: "newuser@test.com",
            Password: "password123",
            FullName: "New User",
            Department: "Engineering",
            Role: UserRole.Employee);

        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(result);
        Assert.Equal("newuser@test.com", result.Email);
    }

    [Fact]
    public async Task Register_Should_Return400_WhenEmailAlreadyExists()
    {
        var client = _factory.CreateClient();
        var request = new RegisterRequest(
            Email: "existing@test.com",
            Password: "password123",
            FullName: "Existing User",
            Department: "Engineering",
            Role: UserRole.Employee);

        await client.PostAsJsonAsync("/api/auth/register", request);
        var response = await client.PostAsJsonAsync("/api/auth/register", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_Should_Return200_WhenCredentialsValid()
    {
        var client = _factory.CreateClient();
        var registerRequest = new RegisterRequest(
            Email: "logintest@test.com",
            Password: "password123",
            FullName: "Login Test",
            Department: "Engineering",
            Role: UserRole.Employee);
        await client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest(Email: "logintest@test.com", Password: "password123");
        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(result);
        Assert.NotNull(result.Token);
    }

    [Fact]
    public async Task Login_Should_Return401_WhenUserNotFound()
    {
        var client = _factory.CreateClient();
        var request = new LoginRequest(Email: "nonexistent@test.com", Password: "password");

        var response = await client.PostAsJsonAsync("/api/auth/login", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_Should_Return401_WhenPasswordInvalid()
    {
        var client = _factory.CreateClient();
        var registerRequest = new RegisterRequest(
            Email: "wrongpw@test.com",
            Password: "correctpassword",
            FullName: "Test User",
            Department: "Engineering",
            Role: UserRole.Employee);
        await client.PostAsJsonAsync("/api/auth/register", registerRequest);

        var loginRequest = new LoginRequest(Email: "wrongpw@test.com", Password: "wrongpassword");
        var response = await client.PostAsJsonAsync("/api/auth/login", loginRequest);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
