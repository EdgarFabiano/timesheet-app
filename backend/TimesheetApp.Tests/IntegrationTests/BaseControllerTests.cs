using System.Net;
using System.Net.Http.Headers;

namespace TimesheetApp.Tests.IntegrationTests;

public abstract class BaseControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly CustomWebApplicationFactory Factory;

    protected BaseControllerTests(CustomWebApplicationFactory factory)
    {
        Factory = factory;
    }

    protected HttpClient CreateClientWithToken(string role)
    {
        var client = Factory.CreateClient();
        var token = Helpers.JwtTokenHelper.GenerateToken(
            role == "Admin" 
                ? Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa") 
                : Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
            role,
            role == "Admin" ? "admin@test.com" : "employee@test.com");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    protected HttpClient CreateClientWithInvalidToken()
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "invalid-token");
        return client;
    }

    protected HttpClient CreateClientWithoutToken()
    {
        return Factory.CreateClient();
    }
}
