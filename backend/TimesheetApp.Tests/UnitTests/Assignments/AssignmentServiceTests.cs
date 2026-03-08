using TimesheetApp.API.DTOs.Assignments;
using TimesheetApp.API.Models;
using TimesheetApp.API.Services;
using TimesheetApp.Tests.Helpers;

namespace TimesheetApp.Tests.UnitTests.Assignments;

public class AssignmentServiceTests
{
    private Guid _employeeId;
    private Guid _projectId;

    public AssignmentServiceTests()
    {
        _employeeId = Guid.NewGuid();
        _projectId = Guid.NewGuid();
    }

    private AppDbContext CreateContextWithEmployeeAndProject()
    {
        var context = TestDbContextFactory.CreateInMemoryContext();
        var clientId = Guid.NewGuid();
        context.Clients.Add(new Client { Id = clientId, Name = "Test Client", ContactEmail = "client@test.com", IsActive = true, CreatedAt = DateTime.UtcNow });
        context.Employees.Add(new Employee { Id = _employeeId, FullName = "Test Employee", Email = "employee@test.com", AzureAdObjectId = "azure-123", Department = "Engineering", IsActive = true, CreatedAt = DateTime.UtcNow });
        context.Projects.Add(new Project { Id = _projectId, Name = "Test Project", ClientId = clientId, IsActive = true, CreatedAt = DateTime.UtcNow });
        return context;
    }

    [Fact]
    public async Task CreateAsync_Should_CreateAssignment_WhenValid()
    {
        var context = CreateContextWithEmployeeAndProject();
        var service = new AssignmentService(context);
        var request = new CreateAssignmentRequest(EmployeeId: _employeeId, ProjectId: _projectId, IsActive: true);

        var (result, errorKey) = await service.CreateAsync(request);

        Assert.NotNull(result);
        Assert.Equal(_employeeId, result.EmployeeId);
        Assert.Equal(_projectId, result.ProjectId);
        Assert.Null(errorKey);
    }

    [Fact]
    public async Task CreateAsync_Should_ReturnError_WhenEmployeeNotFound()
    {
        var context = CreateContextWithEmployeeAndProject();
        var service = new AssignmentService(context);
        var request = new CreateAssignmentRequest(EmployeeId: Guid.NewGuid(), ProjectId: _projectId, IsActive: true);

        var (result, errorKey) = await service.CreateAsync(request);

        Assert.Null(result);
        Assert.Equal("EmployeeNotFound", errorKey);
    }

    [Fact]
    public async Task CreateAsync_Should_ReturnError_WhenProjectNotFound()
    {
        var context = CreateContextWithEmployeeAndProject();
        var service = new AssignmentService(context);
        var request = new CreateAssignmentRequest(EmployeeId: _employeeId, ProjectId: Guid.NewGuid(), IsActive: true);

        var (result, errorKey) = await service.CreateAsync(request);

        Assert.Null(result);
        Assert.Equal("ProjectNotFound", errorKey);
    }

    [Fact]
    public async Task CreateAsync_Should_ReturnError_WhenAlreadyAssigned()
    {
        var context = CreateContextWithEmployeeAndProject();
        context.Assignments.Add(new Assignment { Id = Guid.NewGuid(), EmployeeId = _employeeId, ProjectId = _projectId, IsActive = true, AssignedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var service = new AssignmentService(context);
        var request = new CreateAssignmentRequest(EmployeeId: _employeeId, ProjectId: _projectId, IsActive: true);

        var (result, errorKey) = await service.CreateAsync(request);

        Assert.Null(result);
        Assert.Equal("AlreadyAssigned", errorKey);
    }

    [Fact]
    public async Task DeleteAsync_Should_DeleteAssignment_WhenAssignmentExists()
    {
        var context = CreateContextWithEmployeeAndProject();
        var assignmentId = Guid.NewGuid();
        context.Assignments.Add(new Assignment { Id = assignmentId, EmployeeId = _employeeId, ProjectId = _projectId, IsActive = true, AssignedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var service = new AssignmentService(context);
        var result = await service.DeleteAsync(assignmentId);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_Should_ReturnFalse_WhenAssignmentNotFound()
    {
        var context = CreateContextWithEmployeeAndProject();
        var service = new AssignmentService(context);

        var result = await service.DeleteAsync(Guid.NewGuid());

        Assert.False(result);
    }

    [Fact]
    public async Task GetByProjectIdAsync_Should_ReturnAssignments_ForProject()
    {
        var context = CreateContextWithEmployeeAndProject();
        var employeeId2 = Guid.NewGuid();
        context.Employees.Add(new Employee { Id = employeeId2, FullName = "Employee 2", Email = "emp2@test.com", AzureAdObjectId = "azure-2", Department = "IT", IsActive = true, CreatedAt = DateTime.UtcNow });
        context.Assignments.AddRange(
            new Assignment { Id = Guid.NewGuid(), EmployeeId = _employeeId, ProjectId = _projectId, IsActive = true, AssignedAt = DateTime.UtcNow },
            new Assignment { Id = Guid.NewGuid(), EmployeeId = employeeId2, ProjectId = _projectId, IsActive = true, AssignedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = new AssignmentService(context);
        var result = await service.GetByProjectIdAsync(_projectId);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByEmployeeIdAsync_Should_ReturnAssignments_ForEmployee()
    {
        var context = CreateContextWithEmployeeAndProject();
        var projectId2 = Guid.NewGuid();
        context.Projects.Add(new Project { Id = projectId2, Name = "Project 2", ClientId = Guid.NewGuid(), IsActive = true, CreatedAt = DateTime.UtcNow });
        context.Assignments.AddRange(
            new Assignment { Id = Guid.NewGuid(), EmployeeId = _employeeId, ProjectId = _projectId, IsActive = true, AssignedAt = DateTime.UtcNow },
            new Assignment { Id = Guid.NewGuid(), EmployeeId = _employeeId, ProjectId = projectId2, IsActive = true, AssignedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = new AssignmentService(context);
        var result = await service.GetByEmployeeIdAsync(_employeeId);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByProjectAndEmployeeAsync_Should_ReturnAssignments_ForBoth()
    {
        var context = CreateContextWithEmployeeAndProject();
        var projectId2 = Guid.NewGuid();
        context.Projects.Add(new Project { Id = projectId2, Name = "Project 2", ClientId = Guid.NewGuid(), IsActive = true, CreatedAt = DateTime.UtcNow });
        context.Assignments.Add(new Assignment { Id = Guid.NewGuid(), EmployeeId = _employeeId, ProjectId = _projectId, IsActive = true, AssignedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var service = new AssignmentService(context);
        var result = await service.GetByProjectAndEmployeeAsync(_projectId, _employeeId);

        Assert.Single(result);
        Assert.Equal(_projectId, result[0].ProjectId);
        Assert.Equal(_employeeId, result[0].EmployeeId);
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnAssignment_WhenExists()
    {
        var context = CreateContextWithEmployeeAndProject();
        var assignmentId = Guid.NewGuid();
        context.Assignments.Add(new Assignment { Id = assignmentId, EmployeeId = _employeeId, ProjectId = _projectId, IsActive = true, AssignedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var service = new AssignmentService(context);
        var result = await service.GetByIdAsync(assignmentId);

        Assert.NotNull(result);
        Assert.Equal(assignmentId, result.Id);
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnNull_WhenNotFound()
    {
        var context = CreateContextWithEmployeeAndProject();
        var service = new AssignmentService(context);

        var result = await service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }
}
