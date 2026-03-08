using System.Net;
using System.Net.Http.Json;
using TimesheetApp.API.DTOs.Employees;
using TimesheetApp.API.Models;

namespace TimesheetApp.Tests.IntegrationTests.Employees;

public class EmployeesControllerTests : BaseControllerTests
{
    public EmployeesControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetAll_Should_Return200_WhenAuthorized()
    {
        var client = CreateClientWithToken("Admin");

        var response = await client.GetAsync("/api/employees");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_Should_Return200_WhenExists()
    {
        var client = CreateClientWithToken("Admin");
        var employeeId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var response = await client.GetAsync($"/api/employees/{employeeId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_Should_Return404_WhenNotFound()
    {
        var client = CreateClientWithToken("Admin");

        var response = await client.GetAsync($"/api/employees/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_Should_Return201_WhenAdmin()
    {
        var client = CreateClientWithToken("Admin");
        var request = new CreateEmployeeRequest(
            AzureAdObjectId: "azure-new",
            FullName: "New Employee",
            Email: "newemp@test.com",
            Department: "Engineering",
            IsActive: true);

        var response = await client.PostAsJsonAsync("/api/employees", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_Should_Return403_WhenEmployee()
    {
        var client = CreateClientWithToken("Employee");
        var request = new CreateEmployeeRequest(
            AzureAdObjectId: "azure-new",
            FullName: "New Employee",
            Email: "newemp@test.com",
            Department: "Engineering",
            IsActive: true);

        var response = await client.PostAsJsonAsync("/api/employees", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_Should_Return401_WhenNotAuthenticated()
    {
        var client = CreateClientWithoutToken();
        var request = new CreateEmployeeRequest(
            AzureAdObjectId: "azure-new",
            FullName: "New Employee",
            Email: "newemp@test.com",
            Department: "Engineering",
            IsActive: true);

        var response = await client.PostAsJsonAsync("/api/employees", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Update_Should_Return200_WhenAdmin()
    {
        var client = CreateClientWithToken("Admin");
        var employeeId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var request = new UpdateEmployeeRequest(
            AzureAdObjectId: "azure-updated",
            FullName: "Updated Employee",
            Email: "updated@test.com",
            Department: "Sales",
            IsActive: false);

        var response = await client.PutAsJsonAsync($"/api/employees/{employeeId}", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Update_Should_Return403_WhenEmployee()
    {
        var client = CreateClientWithToken("Employee");
        var employeeId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var request = new UpdateEmployeeRequest(
            AzureAdObjectId: "azure-updated",
            FullName: "Updated Employee",
            Email: "updated@test.com",
            Department: "Sales",
            IsActive: false);

        var response = await client.PutAsJsonAsync($"/api/employees/{employeeId}", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Should_Return204_WhenAdmin()
    {
        var createClient = CreateClientWithToken("Admin");
        var createRequest = new CreateEmployeeRequest(
            AzureAdObjectId: "azure-todelete",
            FullName: "ToDelete",
            Email: "todelete@test.com",
            Department: "IT",
            IsActive: true);
        var createResponse = await createClient.PostAsJsonAsync("/api/employees", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<EmployeeResponse>();

        var client = CreateClientWithToken("Admin");
        var response = await client.DeleteAsync($"/api/employees/{created!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Should_Return403_WhenEmployee()
    {
        var client = CreateClientWithToken("Employee");
        var employeeId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var response = await client.DeleteAsync($"/api/employees/{employeeId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_Should_Return401_WhenNotAuthenticated()
    {
        var client = CreateClientWithoutToken();

        var response = await client.GetAsync("/api/employees");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
