using System.Net;
using System.Net.Http.Json;
using TimesheetApp.API.DTOs.Projects;
using TimesheetApp.API.Models;

namespace TimesheetApp.Tests.IntegrationTests.Projects;

public class ProjectsControllerTests : BaseControllerTests
{
    public ProjectsControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetAll_Should_Return200_WhenAuthorized()
    {
        var client = CreateClientWithToken("Admin");

        var response = await client.GetAsync("/api/projects");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_Should_Return200_WhenExists()
    {
        var client = CreateClientWithToken("Admin");
        var projectId = Guid.Parse("44444444-4444-4444-4444-444444444444");

        var response = await client.GetAsync($"/api/projects/{projectId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_Should_Return404_WhenNotFound()
    {
        var client = CreateClientWithToken("Admin");

        var response = await client.GetAsync($"/api/projects/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_Should_Return201_WhenValid()
    {
        var client = CreateClientWithToken("Admin");
        var request = new CreateProjectRequest(
            Name: "New Project",
            Description: "Description",
            StartDate: DateTime.UtcNow,
            EndDate: DateTime.UtcNow.AddMonths(3),
            IsActive: true,
            ClientId: Guid.Parse("11111111-1111-1111-1111-111111111111"));

        var response = await client.PostAsJsonAsync("/api/projects", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_Should_Return404_WhenClientNotFound()
    {
        var client = CreateClientWithToken("Admin");
        var request = new CreateProjectRequest(
            Name: "New Project",
            Description: "Description",
            StartDate: DateTime.UtcNow,
            EndDate: DateTime.UtcNow.AddMonths(3),
            IsActive: true,
            ClientId: Guid.NewGuid());

        var response = await client.PostAsJsonAsync("/api/projects", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_Should_Return200_WhenValid()
    {
        var client = CreateClientWithToken("Admin");
        var projectId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var request = new UpdateProjectRequest(
            Name: "Updated Project",
            Description: "Updated Description",
            StartDate: DateTime.UtcNow,
            EndDate: DateTime.UtcNow.AddMonths(6),
            IsActive: false,
            ClientId: Guid.Parse("11111111-1111-1111-1111-111111111111"));

        var response = await client.PutAsJsonAsync($"/api/projects/{projectId}", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Update_Should_Return404_WhenProjectNotFound()
    {
        var client = CreateClientWithToken("Admin");
        var request = new UpdateProjectRequest(
            Name: "Test",
            Description: "Test",
            StartDate: DateTime.UtcNow,
            EndDate: DateTime.UtcNow,
            IsActive: true,
            ClientId: Guid.Parse("11111111-1111-1111-1111-111111111111"));

        var response = await client.PutAsJsonAsync($"/api/projects/{Guid.NewGuid()}", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_Should_Return404_WhenClientNotFound()
    {
        var client = CreateClientWithToken("Admin");
        var projectId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var request = new UpdateProjectRequest(
            Name: "Updated",
            Description: "Test",
            StartDate: DateTime.UtcNow,
            EndDate: DateTime.UtcNow,
            IsActive: true,
            ClientId: Guid.NewGuid());

        var response = await client.PutAsJsonAsync($"/api/projects/{projectId}", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Should_Return204_WhenDeleted()
    {
        var createClient = CreateClientWithToken("Admin");
        var clientId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var createRequest = new CreateProjectRequest(
            Name: "ToDelete",
            Description: "Test",
            StartDate: DateTime.UtcNow,
            EndDate: DateTime.UtcNow,
            IsActive: true,
            ClientId: clientId);
        var createResponse = await createClient.PostAsJsonAsync("/api/projects", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ProjectResponse>();

        var client = CreateClientWithToken("Admin");
        var response = await client.DeleteAsync($"/api/projects/{created!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Should_Return404_WhenNotFound()
    {
        var client = CreateClientWithToken("Admin");

        var response = await client.DeleteAsync($"/api/projects/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_Should_Return401_WhenNotAuthenticated()
    {
        var client = CreateClientWithoutToken();

        var response = await client.GetAsync("/api/projects");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
