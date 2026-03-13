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

    [Fact]
    public async Task DeleteAsync_Should_DeleteTimesheet_WhenExists()
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
            CreatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var service = new TimesheetService(context);

        var result = await service.DeleteAsync(timesheetId);

        Assert.True(result);
        Assert.Null(await context.Timesheets.FindAsync(timesheetId));
    }

    [Fact]
    public async Task DeleteAsync_Should_ReturnFalse_WhenNotFound()
    {
        var context = CreateContextWithEmployeeAndProject();
        var service = new TimesheetService(context);

        var result = await service.DeleteAsync(Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task BulkSaveAsync_Should_CreateNewEntries_WhenValid()
    {
        var context = CreateContextWithEmployeeAndProject();
        var projectId2 = Guid.NewGuid();
        context.Projects.Add(new Project { Id = projectId2, Name = "Project 2", IsActive = true, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var service = new TimesheetService(context);
        var request = new BulkSaveTimesheetRequest(
            EmployeeId: _employeeId,
            Entries: new List<BulkTimesheetEntry>
            {
                new(_projectId, DateOnly.FromDateTime(DateTime.UtcNow), 8.0m, "Day 1 work"),
                new(_projectId, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), 7.5m, "Day 2 work"),
                new(projectId2, DateOnly.FromDateTime(DateTime.UtcNow), 6.0m, "Other project")
            });

        var result = await service.BulkSaveAsync(request);

        Assert.Empty(result.Errors);
        Assert.Equal(3, result.Saved.Count);
    }

    [Fact]
    public async Task BulkSaveAsync_Should_UpdateExistingEntries_WhenTimesheetExists()
    {
        var context = CreateContextWithEmployeeAndProject();
        var date1 = DateOnly.FromDateTime(DateTime.UtcNow);
        var date2 = date1.AddDays(1);
        
        context.Timesheets.AddRange(
            new Timesheet { Id = Guid.NewGuid(), EmployeeId = _employeeId, ProjectId = _projectId, Date = date1, HoursWorked = 4.0m, CreatedAt = DateTime.UtcNow },
            new Timesheet { Id = Guid.NewGuid(), EmployeeId = _employeeId, ProjectId = _projectId, Date = date2, HoursWorked = 3.0m, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = new TimesheetService(context);
        var request = new BulkSaveTimesheetRequest(
            EmployeeId: _employeeId,
            Entries: new List<BulkTimesheetEntry>
            {
                new(_projectId, date1, 8.0m, "Updated"),
                new(_projectId, date2, 7.5m, "Updated 2")
            });

        var result = await service.BulkSaveAsync(request);

        Assert.Empty(result.Errors);
        Assert.Equal(2, result.Saved.Count);
        var entry1 = result.Saved.First(s => s.Date == date1);
        var entry2 = result.Saved.First(s => s.Date == date2);
        Assert.Equal(8.0m, entry1.HoursWorked);
        Assert.Equal("Updated", entry1.Notes);
        Assert.Equal(7.5m, entry2.HoursWorked);
    }

    [Fact]
    public async Task BulkSaveAsync_Should_SkipEntriesWithZeroHours()
    {
        var context = CreateContextWithEmployeeAndProject();
        var date = DateOnly.FromDateTime(DateTime.UtcNow);

        var service = new TimesheetService(context);
        var request = new BulkSaveTimesheetRequest(
            EmployeeId: _employeeId,
            Entries: new List<BulkTimesheetEntry>
            {
                new(_projectId, date, 0m, "Zero hours"),
                new(_projectId, date.AddDays(1), 8.0m, "Valid hours")
            });

        var result = await service.BulkSaveAsync(request);

        Assert.Empty(result.Errors);
        Assert.Single(result.Saved);
        Assert.Equal(8.0m, result.Saved[0].HoursWorked);
    }

    [Fact]
    public async Task BulkSaveAsync_Should_ReturnError_WhenEmployeeNotFound()
    {
        var context = CreateContextWithEmployeeAndProject();
        
        var service = new TimesheetService(context);
        var request = new BulkSaveTimesheetRequest(
            EmployeeId: Guid.NewGuid(),
            Entries: new List<BulkTimesheetEntry>
            {
                new(_projectId, DateOnly.FromDateTime(DateTime.UtcNow), 8.0m, null)
            });

        var result = await service.BulkSaveAsync(request);

        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, e => e.Contains("Employee"));
        Assert.Empty(result.Saved);
    }

    [Fact]
    public async Task BulkSaveAsync_Should_ReturnErrorForInvalidProject()
    {
        var context = CreateContextWithEmployeeAndProject();
        var invalidProjectId = Guid.NewGuid();
        
        var service = new TimesheetService(context);
        var request = new BulkSaveTimesheetRequest(
            EmployeeId: _employeeId,
            Entries: new List<BulkTimesheetEntry>
            {
                new(invalidProjectId, DateOnly.FromDateTime(DateTime.UtcNow), 8.0m, null),
                new(_projectId, DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)), 8.0m, null)
            });

        var result = await service.BulkSaveAsync(request);

        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, e => e.Contains("Project"));
        Assert.Single(result.Saved);
    }

    [Fact]
    public async Task BulkSaveAsync_Should_MixNewAndExistingEntries()
    {
        var context = CreateContextWithEmployeeAndProject();
        var projectId2 = Guid.NewGuid();
        context.Projects.Add(new Project { Id = projectId2, Name = "Project 2", IsActive = true, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var existingDate = DateOnly.FromDateTime(DateTime.UtcNow);
        context.Timesheets.Add(new Timesheet 
        { 
            Id = Guid.NewGuid(), 
            EmployeeId = _employeeId, 
            ProjectId = _projectId, 
            Date = existingDate, 
            HoursWorked = 2.0m, 
            CreatedAt = DateTime.UtcNow 
        });
        await context.SaveChangesAsync();

        var service = new TimesheetService(context);
        var request = new BulkSaveTimesheetRequest(
            EmployeeId: _employeeId,
            Entries: new List<BulkTimesheetEntry>
            {
                new(_projectId, existingDate, 8.0m, "Updated existing"),
                new(_projectId, existingDate.AddDays(1), 7.0m, "New entry"),
                new(projectId2, existingDate, 6.0m, "New project entry")
            });

        var result = await service.BulkSaveAsync(request);

        Assert.Empty(result.Errors);
        Assert.Equal(3, result.Saved.Count);
    }

    [Fact]
    public async Task BulkSaveAsync_Should_PreserveEntriesForOtherWeeks()
    {
        var context = CreateContextWithEmployeeAndProject();
        var currentWeekDate = DateOnly.FromDateTime(DateTime.UtcNow);
        var lastWeekDate = currentWeekDate.AddDays(-7);

        context.Timesheets.Add(new Timesheet 
        { 
            Id = Guid.NewGuid(), 
            EmployeeId = _employeeId, 
            ProjectId = _projectId, 
            Date = lastWeekDate, 
            HoursWorked = 8.0m, 
            CreatedAt = DateTime.UtcNow 
        });
        await context.SaveChangesAsync();

        var service = new TimesheetService(context);
        var request = new BulkSaveTimesheetRequest(
            EmployeeId: _employeeId,
            Entries: new List<BulkTimesheetEntry>
            {
                new(_projectId, currentWeekDate, 8.0m, null)
            });

        var result = await service.BulkSaveAsync(request);

        Assert.Empty(result.Errors);
        Assert.Single(result.Saved);
        var allTimesheets = context.Timesheets.ToList();
        Assert.Equal(2, allTimesheets.Count);
    }
}
