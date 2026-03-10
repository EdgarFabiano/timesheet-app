using System.Net;
using System.Net.Http.Json;
using TimesheetApp.API.DTOs.Clients;
using TimesheetApp.API.Models;

namespace TimesheetApp.Tests.IntegrationTests.Clients;

public class ClientsControllerTests : BaseControllerTests
{
    public ClientsControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetAll_Should_Return200_WhenAuthorized()
    {
        var client = CreateClientWithToken("Admin");

        var response = await client.GetAsync("/api/clients");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<List<ClientResponse>>();
        Assert.NotNull(result);
    }

    [Fact]
    public async Task GetAll_Should_Return401_WhenNotAuthenticated()
    {
        var client = CreateClientWithoutToken();

        var response = await client.GetAsync("/api/clients");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetById_Should_Return200_WhenExists()
    {
        var client = CreateClientWithToken("Admin");
        var clientId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        var response = await client.GetAsync($"/api/clients/{clientId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_Should_Return404_WhenNotFound()
    {
        var client = CreateClientWithToken("Admin");

        var response = await client.GetAsync($"/api/clients/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_Should_Return201_WhenValid()
    {
        var client = CreateClientWithToken("Admin");
        var request = new CreateClientRequest(Name: "New Client", ContactEmail: "new@test.com", IsActive: true);

        var response = await client.PostAsJsonAsync("/api/clients", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ClientResponse>();
        Assert.NotNull(result);
        Assert.Equal("New Client", result.Name);
    }

    [Fact]
    public async Task Update_Should_Return200_WhenValid()
    {
        var client = CreateClientWithToken("Admin");
        var clientId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var request = new UpdateClientRequest(Name: "Updated Client", ContactEmail: "updated@test.com", IsActive: false);

        var response = await client.PutAsJsonAsync($"/api/clients/{clientId}", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ClientResponse>();
        Assert.Equal("Updated Client", result!.Name);
    }

    [Fact]
    public async Task Update_Should_Return404_WhenNotFound()
    {
        var client = CreateClientWithToken("Admin");
        var request = new UpdateClientRequest(Name: "Test", ContactEmail: "test@test.com", IsActive: true);

        var response = await client.PutAsJsonAsync($"/api/clients/{Guid.NewGuid()}", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Should_Return204_WhenDeleted()
    {
        var createClient = CreateClientWithToken("Admin");
        var createRequest = new CreateClientRequest(Name: "ToDelete", ContactEmail: "delete@test.com", IsActive: true);
        var createResponse = await createClient.PostAsJsonAsync("/api/clients", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ClientResponse>();

        var client = CreateClientWithToken("Admin");
        var response = await client.DeleteAsync($"/api/clients/{created!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Should_Return404_WhenNotFound()
    {
        var client = CreateClientWithToken("Admin");

        var response = await client.DeleteAsync($"/api/clients/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
