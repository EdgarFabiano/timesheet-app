using TimesheetApp.API.DTOs.Timesheets;
using TimesheetApp.API.Models;
using TimesheetApp.API.Services;
using TimesheetApp.Tests.Helpers;

namespace TimesheetApp.Tests.UnitTests.Timesheets;

public class TimesheetServiceTests
{
    private Guid _employeeId;
    private Guid _projectId;

    public TimesheetServiceTests()
    {
        var context = TestDbContextFactory.CreateInMemoryContextWithEmployeeAndProject();
        _employeeId = context.Employees.First().Id;
        _projectId = context.Projects.First().Id;
    }

    private AppDbContext CreateContextWithEmployeeAndProject()
    {
        return TestDbContextFactory.CreateInMemoryContextWithEmployeeAndProject(_employeeId, _projectId);
    }

    [Fact]
    public async Task CreateAsync_Should_CreateTimesheet_WhenValid()
    {
        var context = CreateContextWithEmployeeAndProject();
        var service = new TimesheetService(context);
        var request = new CreateTimesheetRequest(
            EmployeeId: _employeeId,
            ProjectId: _projectId,
            Date: DateOnly.FromDateTime(DateTime.UtcNow),
            HoursWorked: 8.0m,
            Notes: "Test work");

        var (result, errorKey) = await service.CreateAsync(request);

        Assert.NotNull(result);
        Assert.Equal(_employeeId, result.EmployeeId);
        Assert.Equal(_projectId, result.ProjectId);
        Assert.Equal(8.0m, result.HoursWorked);
        Assert.Null(errorKey);
    }

    [Fact]
    public async Task CreateAsync_Should_ReturnError_WhenEmployeeNotFound()
    {
        var context = CreateContextWithEmployeeAndProject();
        var service = new TimesheetService(context);
        var request = new CreateTimesheetRequest(
            EmployeeId: Guid.NewGuid(),
            ProjectId: _projectId,
            Date: DateOnly.FromDateTime(DateTime.UtcNow),
            HoursWorked: 8.0m,
            Notes: "Test work");

        var (result, errorKey) = await service.CreateAsync(request);

        Assert.Null(result);
        Assert.Equal("EmployeeNotFound", errorKey);
    }

    [Fact]
    public async Task CreateAsync_Should_ReturnError_WhenProjectNotFound()
    {
        var context = CreateContextWithEmployeeAndProject();
        var service = new TimesheetService(context);
        var request = new CreateTimesheetRequest(
            EmployeeId: _employeeId,
            ProjectId: Guid.NewGuid(),
            Date: DateOnly.FromDateTime(DateTime.UtcNow),
            HoursWorked: 8.0m,
            Notes: "Test work");

        var (result, errorKey) = await service.CreateAsync(request);

        Assert.Null(result);
        Assert.Equal("ProjectNotFound", errorKey);
    }

    [Fact]
    public async Task CreateAsync_Should_ReturnError_WhenDuplicateEntry()
    {
        var context = CreateContextWithEmployeeAndProject();
        var date = DateOnly.FromDateTime(DateTime.UtcNow);
        context.Timesheets.Add(new Timesheet
        {
            Id = Guid.NewGuid(),
            EmployeeId = _employeeId,
            ProjectId = _projectId,
            Date = date,
            HoursWorked = 8.0m,
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = new TimesheetService(context);
        var request = new CreateTimesheetRequest(
            EmployeeId: _employeeId,
            ProjectId: _projectId,
            Date: date,
            HoursWorked: 4.0m,
            Notes: "More work");

        var (result, errorKey) = await service.CreateAsync(request);

        Assert.Null(result);
        Assert.Equal("DuplicateEntry", errorKey);
    }

    [Fact]
    public async Task UpdateAsync_Should_UpdateTimesheet_WhenExists()
    {
        var context = CreateContextWithEmployeeAndProject();
        var timesheetId = Guid.NewGuid();
        context.Timesheets.Add(new Timesheet
        {
            Id = timesheetId,
            EmployeeId = _employeeId,
            ProjectId = _projectId,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            HoursWorked = 8.0m,
            Notes = "Original",
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = new TimesheetService(context);
        var request = new UpdateTimesheetRequest(HoursWorked: 10.0m, Notes: "Updated");

        var (result, notFound) = await service.UpdateAsync(timesheetId, request);

        Assert.NotNull(result);
        Assert.Equal(10.0m, result.HoursWorked);
        Assert.Equal("Updated", result.Notes);
        Assert.False(notFound);
    }

    [Fact]
    public async Task UpdateAsync_Should_ReturnNotFound_WhenTimesheetNotExists()
    {
        var context = CreateContextWithEmployeeAndProject();
        var service = new TimesheetService(context);
        var request = new UpdateTimesheetRequest(HoursWorked: 10.0m, Notes: "Updated");

        var (result, notFound) = await service.UpdateAsync(Guid.NewGuid(), request);

        Assert.Null(result);
        Assert.True(notFound);
    }

    [Fact]
    public async Task GetByEmployeeAndDateRangeAsync_Should_ReturnTimesheets_ForEmployeeInRange()
    {
        var context = CreateContextWithEmployeeAndProject();
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);

        context.Timesheets.AddRange(
            new Timesheet { Id = Guid.NewGuid(), EmployeeId = _employeeId, ProjectId = _projectId, Date = startDate.AddDays(1), HoursWorked = 8.0m, CreatedAt = DateTime.UtcNow },
            new Timesheet { Id = Guid.NewGuid(), EmployeeId = _employeeId, ProjectId = _projectId, Date = endDate.AddDays(-1), HoursWorked = 8.0m, CreatedAt = DateTime.UtcNow },
            new Timesheet { Id = Guid.NewGuid(), EmployeeId = _employeeId, ProjectId = _projectId, Date = endDate.AddDays(10), HoursWorked = 8.0m, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = new TimesheetService(context);
        var result = await service.GetByEmployeeAndDateRangeAsync(_employeeId, startDate, endDate);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByProjectIdAsync_Should_ReturnTimesheets_ForProject()
    {
        var context = CreateContextWithEmployeeAndProject();
        var employeeId2 = Guid.NewGuid();
        context.Employees.Add(new Employee { Id = employeeId2, FullName = "Employee 2", Email = "emp2@test.com", AzureAdObjectId = "azure-2", Department = "IT", IsActive = true, CreatedAt = DateTime.UtcNow });

        context.Timesheets.AddRange(
            new Timesheet { Id = Guid.NewGuid(), EmployeeId = _employeeId, ProjectId = _projectId, Date = DateOnly.FromDateTime(DateTime.UtcNow), HoursWorked = 8.0m, CreatedAt = DateTime.UtcNow },
            new Timesheet { Id = Guid.NewGuid(), EmployeeId = employeeId2, ProjectId = _projectId, Date = DateOnly.FromDateTime(DateTime.UtcNow), HoursWorked = 8.0m, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = new TimesheetService(context);
        var result = await service.GetByProjectIdAsync(_projectId);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByProjectIdAsync_Should_FilterByDateRange()
    {
        var context = CreateContextWithEmployeeAndProject();
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7));
        var endDate = DateOnly.FromDateTime(DateTime.UtcNow);

        context.Timesheets.AddRange(
            new Timesheet { Id = Guid.NewGuid(), EmployeeId = _employeeId, ProjectId = _projectId, Date = startDate.AddDays(1), HoursWorked = 8.0m, CreatedAt = DateTime.UtcNow },
            new Timesheet { Id = Guid.NewGuid(), EmployeeId = _employeeId, ProjectId = _projectId, Date = endDate.AddDays(5), HoursWorked = 8.0m, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = new TimesheetService(context);
        var result = await service.GetByProjectIdAsync(_projectId, startDate, endDate);

        Assert.Single(result);
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnTimesheet_WhenExists()
    {
        var context = CreateContextWithEmployeeAndProject();
        var timesheetId = Guid.NewGuid();
        context.Timesheets.Add(new Timesheet
        {
            Id = timesheetId,
            EmployeeId = _employeeId,
            ProjectId = _projectId,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            HoursWorked = 8.0m,
            Notes = "Test",
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = new TimesheetService(context);
        var result = await service.GetByIdAsync(timesheetId);

        Assert.NotNull(result);
        Assert.Equal(timesheetId, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnNull_WhenNotFound()
    {
        var context = CreateContextWithEmployeeAndProject();
        var service = new TimesheetService(context);

        var result = await service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }
}
