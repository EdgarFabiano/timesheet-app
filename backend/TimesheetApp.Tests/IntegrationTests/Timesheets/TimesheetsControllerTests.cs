using System.Net;
using System.Net.Http.Json;
using TimesheetApp.API.DTOs.Timesheets;
using TimesheetApp.API.Models;

namespace TimesheetApp.Tests.IntegrationTests.Timesheets;

public class TimesheetsControllerTests : BaseControllerTests
{
    public TimesheetsControllerTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetByEmployeeAndDateRange_Should_Return200_WhenAuthorized()
    {
        var client = CreateClientWithToken("Admin");
        var employeeId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var response = await client.GetAsync($"/api/timesheets?employeeId={employeeId}&startDate=2024-01-01&endDate=2024-12-31");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetByProject_Should_Return200_WhenAuthorized()
    {
        var client = CreateClientWithToken("Admin");
        var projectId = Guid.Parse("44444444-4444-4444-4444-444444444444");

        var response = await client.GetAsync($"/api/timesheets?projectId={projectId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_Should_Return400_WhenNoFilters()
    {
        var client = CreateClientWithToken("Admin");

        var response = await client.GetAsync("/api/timesheets");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetByEmployee_Should_Return400_WhenMissingDateRange()
    {
        var client = CreateClientWithToken("Admin");
        var employeeId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var response = await client.GetAsync($"/api/timesheets?employeeId={employeeId}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetByEmployee_Should_Return400_WhenStartDateAfterEndDate()
    {
        var client = CreateClientWithToken("Admin");
        var employeeId = Guid.Parse("22222222-2222-2222-2222-222222222222");

        var response = await client.GetAsync($"/api/timesheets?employeeId={employeeId}&startDate=2024-12-31&endDate=2024-01-01");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_Should_Return200_WhenExists()
    {
        var createClient = CreateClientWithToken("Admin");
        var employeeId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var projectId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var request = new CreateTimesheetRequest(
            EmployeeId: employeeId,
            ProjectId: projectId,
            Date: DateOnly.FromDateTime(DateTime.UtcNow),
            HoursWorked: 8.0m,
            Notes: "Test work");
        var createResponse = await createClient.PostAsJsonAsync("/api/timesheets", request);
        var created = await createResponse.Content.ReadFromJsonAsync<TimesheetResponse>();

        var client = CreateClientWithToken("Admin");
        var response = await client.GetAsync($"/api/timesheets/{created!.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_Should_Return404_WhenNotFound()
    {
        var client = CreateClientWithToken("Admin");

        var response = await client.GetAsync($"/api/timesheets/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_Should_Return201_WhenValid()
    {
        var client = CreateClientWithToken("Admin");
        var employeeId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var projectId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var request = new CreateTimesheetRequest(
            EmployeeId: employeeId,
            ProjectId: projectId,
            Date: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-5)),
            HoursWorked: 8.0m,
            Notes: "Test work");

        var response = await client.PostAsJsonAsync("/api/timesheets", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_Should_Return404_WhenEmployeeNotFound()
    {
        var client = CreateClientWithToken("Admin");
        var projectId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var request = new CreateTimesheetRequest(
            EmployeeId: Guid.NewGuid(),
            ProjectId: projectId,
            Date: DateOnly.FromDateTime(DateTime.UtcNow),
            HoursWorked: 8.0m,
            Notes: "Test work");

        var response = await client.PostAsJsonAsync("/api/timesheets", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_Should_Return404_WhenProjectNotFound()
    {
        var client = CreateClientWithToken("Admin");
        var employeeId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var request = new CreateTimesheetRequest(
            EmployeeId: employeeId,
            ProjectId: Guid.NewGuid(),
            Date: DateOnly.FromDateTime(DateTime.UtcNow),
            HoursWorked: 8.0m,
            Notes: "Test work");

        var response = await client.PostAsJsonAsync("/api/timesheets", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_Should_Return409_WhenDuplicateEntry()
    {
        var employeeId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var projectId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-10));

        var createClient = CreateClientWithToken("Admin");
        var createRequest = new CreateTimesheetRequest(
            EmployeeId: employeeId,
            ProjectId: projectId,
            Date: date,
            HoursWorked: 8.0m,
            Notes: "Test work");
        await createClient.PostAsJsonAsync("/api/timesheets", createRequest);

        var client = CreateClientWithToken("Admin");
        var request = new CreateTimesheetRequest(
            EmployeeId: employeeId,
            ProjectId: projectId,
            Date: date,
            HoursWorked: 4.0m,
            Notes: "More work");

        var response = await client.PostAsJsonAsync("/api/timesheets", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Update_Should_Return200_WhenValid()
    {
        var createClient = CreateClientWithToken("Admin");
        var employeeId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var projectId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var createRequest = new CreateTimesheetRequest(
            EmployeeId: employeeId,
            ProjectId: projectId,
            Date: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-15)),
            HoursWorked: 8.0m,
            Notes: "Original");
        var createResponse = await createClient.PostAsJsonAsync("/api/timesheets", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<TimesheetResponse>();

        var client = CreateClientWithToken("Admin");
        var request = new UpdateTimesheetRequest(HoursWorked: 10.0m, Notes: "Updated");
        var response = await client.PutAsJsonAsync($"/api/timesheets/{created!.Id}", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<TimesheetResponse>();
        Assert.Equal(10.0m, result!.HoursWorked);
    }

    [Fact]
    public async Task Update_Should_Return404_WhenNotFound()
    {
        var client = CreateClientWithToken("Admin");
        var request = new UpdateTimesheetRequest(HoursWorked: 10.0m, Notes: "Updated");

        var response = await client.PutAsJsonAsync($"/api/timesheets/{Guid.NewGuid()}", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Create_Should_Return400_WhenHoursWorkedBelowMinimum()
    {
        var client = CreateClientWithToken("Admin");
        var employeeId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var projectId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var request = new CreateTimesheetRequest(
            EmployeeId: employeeId,
            ProjectId: projectId,
            Date: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-20)),
            HoursWorked: 0.25m,
            Notes: "Too few hours");

        var response = await client.PostAsJsonAsync("/api/timesheets", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_Should_Return400_WhenHoursWorkedAboveMaximum()
    {
        var client = CreateClientWithToken("Admin");
        var employeeId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var projectId = Guid.Parse("44444444-4444-4444-4444-444444444444");
        var request = new CreateTimesheetRequest(
            EmployeeId: employeeId,
            ProjectId: projectId,
            Date: DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-25)),
            HoursWorked: 25.0m,
            Notes: "Too many hours");

        var response = await client.PostAsJsonAsync("/api/timesheets", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetById_Should_Return401_WhenNotAuthenticated()
    {
        var client = CreateClientWithoutToken();

        var response = await client.GetAsync($"/api/timesheets/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
