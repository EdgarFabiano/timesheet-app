using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using TimesheetApp.API.DTOs.Assignments;
using TimesheetApp.API.Models;

namespace TimesheetApp.Tests.IntegrationTests.Assignments;

public class AssignmentsControllerTests : BaseControllerTests
{
    public AssignmentsControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetByProjectId_Should_Return200_WhenAuthorized()
    {
        var client = CreateClientWithToken("Admin");
        var projectId = Guid.Parse("44444444-4444-4444-4444-444444444444");

        var response = await client.GetAsync($"/api/assignments?projectId={projectId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetByEmployeeId_Should_Return200_WhenAuthorized()
    {
        var client = CreateClientWithToken("Admin");
        var employeeId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var response = await client.GetAsync($"/api/assignments?employeeId={employeeId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetByBoth_Should_Return200_WhenAuthorized()
    {
        var client = CreateClientWithToken("Admin");
        var projectId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var employeeId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var response = await client.GetAsync($"/api/assignments?projectId={projectId}&employeeId={employeeId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_Should_Return400_WhenNoFilters()
    {
        var client = CreateClientWithToken("Admin");

        var response = await client.GetAsync("/api/assignments");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_Should_Return200_WhenExists()
    {
        var createClient = CreateClientWithToken("Admin");
        var createRequest = new CreateAssignmentRequest(
            EmployeeId: Guid.Parse("66666666-6666-6666-6666-666666666666"),
            ProjectId: Guid.Parse("77777777-7777-7777-7777-777777777777"),
            IsActive: true);
        var createResponse = await createClient.PostAsJsonAsync("/api/assignments", createRequest);
        
        if (!createResponse.IsSuccessStatusCode)
        {
            var errorContent = await createResponse.Content.ReadAsStringAsync();
            Assert.Fail($"POST failed with {createResponse.StatusCode}: {errorContent}");
        }
        
        var created = await createResponse.Content.ReadFromJsonAsync<AssignmentResponse>();

        var client = CreateClientWithToken("Admin");
        var response = await client.GetAsync($"/api/assignments/{created!.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Create_Should_Return201_WhenValid()
    {
        var employeeId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        
        var setupClient = CreateClientWithToken("Admin");
        var clientId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        await setupClient.PostAsJsonAsync("/api/clients", new CreateClientRequest("SetupClient", "setup@test.com", true));
        
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<API.Data.AppDbContext>();
        context.Employees.Add(new Employee { Id = employeeId, FullName = "New Emp", Email = "newemp@test.com", AzureAdObjectId = "azure-new", Department = "IT", IsActive = true, CreatedAt = DateTime.UtcNow });
        context.Projects.Add(new Project { Id = projectId, Name = "New Project", ClientId = clientId, IsActive = true, CreatedAt = DateTime.UtcNow });
        context.SaveChanges();

        var client = CreateClientWithToken("Admin");
        var request = new CreateAssignmentRequest(EmployeeId: employeeId, ProjectId: projectId, IsActive: true);

        var response = await client.PostAsJsonAsync("/api/assignments", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_Should_Return404_WhenEmployeeNotFound()
    {
        var client = CreateClientWithToken("Admin");
        var projectId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var request = new CreateAssignmentRequest(EmployeeId: Guid.NewGuid(), ProjectId: projectId, IsActive: true);

        var response = await client.PostAsJsonAsync("/api/assignments", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_Should_Return404_WhenProjectNotFound()
    {
        var client = CreateClientWithToken("Admin");
        var employeeId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var request = new CreateAssignmentRequest(EmployeeId: employeeId, ProjectId: Guid.NewGuid(), IsActive: true);

        var response = await client.PostAsJsonAsync("/api/assignments", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_Should_Return409_WhenAlreadyAssigned()
    {
        var employeeId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var projectId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        
        var setupClient = CreateClientWithToken("Admin");
        var createRequest = new CreateAssignmentRequest(EmployeeId: employeeId, ProjectId: projectId, IsActive: true);
        await setupClient.PostAsJsonAsync("/api/assignments", createRequest);

        var client = CreateClientWithToken("Admin");
        var request = new CreateAssignmentRequest(EmployeeId: employeeId, ProjectId: projectId, IsActive: true);

        var response = await client.PostAsJsonAsync("/api/assignments", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Should_Return204_WhenDeleted()
    {
        var employeeId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        
        using var scope = Factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<API.Data.AppDbContext>();
        var clientId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        context.Employees.Add(new Employee { Id = employeeId, FullName = "Del Emp", Email = "delemp@test.com", AzureAdObjectId = "azure-del", Department = "IT", IsActive = true, CreatedAt = DateTime.UtcNow });
        context.Projects.Add(new Project { Id = projectId, Name = "Del Project", ClientId = clientId, IsActive = true, CreatedAt = DateTime.UtcNow });
        context.SaveChanges();

        var createClient = CreateClientWithToken("Admin");
        var createRequest = new CreateAssignmentRequest(EmployeeId: employeeId, ProjectId: projectId, IsActive: true);
        var createResponse = await createClient.PostAsJsonAsync("/api/assignments", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<AssignmentResponse>();

        var client = CreateClientWithToken("Admin");
        var response = await client.DeleteAsync($"/api/assignments/{created!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task GetById_Should_Return401_WhenNotAuthenticated()
    {
        var client = CreateClientWithoutToken();

        var response = await client.GetAsync($"/api/assignments/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
