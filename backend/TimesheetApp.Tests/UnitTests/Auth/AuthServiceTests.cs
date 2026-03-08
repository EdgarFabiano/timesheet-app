using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TimesheetApp.API.DTOs;
using TimesheetApp.API.Models;
using TimesheetApp.API.Services;
using TimesheetApp.Tests.Helpers;

namespace TimesheetApp.Tests.UnitTests.Auth;

public class AuthServiceTests
{
    private readonly IConfiguration _configuration;

    public AuthServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string?> {
            {"Jwt:Key", "SuperSecretTestKey123456789!"},
            {"Jwt:Issuer", "TestIssuer"},
            {"Jwt:Audience", "TestAudience"}
        };
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    [Fact]
    public async Task RegisterAsync_Should_CreateUser_WhenEmailIsUnique()
    {
        var context = TestDbContextFactory.CreateInMemoryContext();
        var service = new AuthService(context, _configuration);

        var request = new RegisterRequest(
            Email: "newuser@test.com",
            Password: "password123",
            FullName: "New User",
            Department: "Engineering",
            Role: UserRole.Employee);

        var result = await service.RegisterAsync(request);

        Assert.NotNull(result);
        Assert.Equal("newuser@test.com", result.Email);
        Assert.Equal("Employee", result.Role);
        Assert.NotNull(result.Token);
    }

    [Fact]
    public async Task RegisterAsync_Should_ReturnNull_WhenEmailAlreadyExists()
    {
        var context = TestDbContextFactory.CreateInMemoryContext();
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "existing@test.com",
            PasswordHash = "hashedpassword",
            Role = UserRole.Employee
        };
        context.Users.Add(existingUser);
        await context.SaveChangesAsync();

        var service = new AuthService(context, _configuration);
        var request = new RegisterRequest(
            Email: "existing@test.com",
            Password: "password123",
            FullName: "Existing User",
            Department: "Engineering",
            Role: UserRole.Employee);

        var result = await service.RegisterAsync(request);

        Assert.Null(result);
    }

    [Fact]
    public async Task RegisterAsync_Should_CreateAdminUser_WhenRoleIsAdmin()
    {
        var context = TestDbContextFactory.CreateInMemoryContext();
        var service = new AuthService(context, _configuration);

        var request = new RegisterRequest(
            Email: "admin@test.com",
            Password: "admin123",
            FullName: "Admin User",
            Department: "Management",
            Role: UserRole.Admin);

        var result = await service.RegisterAsync(request);

        Assert.NotNull(result);
        Assert.Equal("Admin", result.Role);
    }

    [Fact]
    public async Task LoginAsync_Should_ReturnToken_WhenCredentialsAreValid()
    {
        var context = TestDbContextFactory.CreateInMemoryContext();
        var password = "password123";
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "valid@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            Role = UserRole.Employee
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new AuthService(context, _configuration);
        var request = new LoginRequest(Email: "valid@test.com", Password: password);

        var result = await service.LoginAsync(request);

        Assert.NotNull(result);
        Assert.Equal("valid@test.com", result.Email);
        Assert.NotNull(result.Token);
    }

    [Fact]
    public async Task LoginAsync_Should_ReturnNull_WhenUserNotFound()
    {
        var context = TestDbContextFactory.CreateInMemoryContext();
        var service = new AuthService(context, _configuration);
        var request = new LoginRequest(Email: "nonexistent@test.com", Password: "password");

        var result = await service.LoginAsync(request);

        Assert.Null(result);
    }

    [Fact]
    public async Task LoginAsync_Should_ReturnNull_WhenPasswordIsInvalid()
    {
        var context = TestDbContextFactory.CreateInMemoryContext();
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword"),
            Role = UserRole.Employee
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var service = new AuthService(context, _configuration);
        var request = new LoginRequest(Email: "user@test.com", Password: "wrongpassword");

        var result = await service.LoginAsync(request);

        Assert.Null(result);
    }

    [Fact]
    public async Task RegisterAsync_Should_CreateEmployee_WhenRegistering()
    {
        var context = TestDbContextFactory.CreateInMemoryContext();
        var service = new AuthService(context, _configuration);

        var request = new RegisterRequest(
            Email: "employee@test.com",
            Password: "password123",
            FullName: "Test Employee",
            Department: "Engineering",
            Role: UserRole.Employee);

        var result = await service.RegisterAsync(request);

        Assert.NotNull(result);
        var employee = await context.Employees.FirstOrDefaultAsync(e => e.Email == "employee@test.com");
        Assert.NotNull(employee);
        Assert.Equal("Test Employee", employee.FullName);
    }
}
