using TimesheetApp.API.DTOs.Employees;
using TimesheetApp.API.Models;
using TimesheetApp.API.Services;
using TimesheetApp.Tests.Helpers;

namespace TimesheetApp.Tests.UnitTests.Employees;

public class EmployeeServiceTests
{
    [Fact]
    public async Task GetAllAsync_Should_ReturnAllEmployees_OrderedByFullName()
    {
        var context = TestDbContextFactory.CreateInMemoryContext();
        context.Employees.AddRange(
            new Employee { Id = Guid.NewGuid(), FullName = "Zebra Employee", Email = "zebra@test.com", AzureAdObjectId = "azure-2", Department = "IT", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Employee { Id = Guid.NewGuid(), FullName = "Alpha Employee", Email = "alpha@test.com", AzureAdObjectId = "azure-1", Department = "IT", IsActive = true, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = new EmployeeService(context);
        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("Alpha Employee", result[0].FullName);
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnEmployee_WhenEmployeeExists()
    {
        var employeeId = Guid.NewGuid();
        var context = TestDbContextFactory.CreateInMemoryContext();
        context.Employees.Add(new Employee { Id = employeeId, FullName = "Test Employee", Email = "test@test.com", AzureAdObjectId = "azure-123", Department = "Engineering", IsActive = true, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var service = new EmployeeService(context);
        var result = await service.GetByIdAsync(employeeId);

        Assert.NotNull(result);
        Assert.Equal("Test Employee", result.FullName);
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnNull_WhenEmployeeNotFound()
    {
        var context = TestDbContextFactory.CreateInMemoryContext();
        var service = new EmployeeService(context);

        var result = await service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_Should_CreateEmployee_WhenValid()
    {
        var context = TestDbContextFactory.CreateInMemoryContext();
        var service = new EmployeeService(context);
        var request = new CreateEmployeeRequest(
            AzureAdObjectId: "azure-new",
            FullName: "New Employee",
            Email: "new@test.com",
            Department: "Engineering",
            IsActive: true);

        var (result, conflictKey) = await service.CreateAsync(request);

        Assert.NotNull(result);
        Assert.Equal("New Employee", result.FullName);
        Assert.Null(conflictKey);
    }

    [Fact]
    public async Task CreateAsync_Should_ReturnConflict_WhenEmailExists()
    {
        var context = TestDbContextFactory.CreateInMemoryContext();
        context.Employees.Add(new Employee { Id = Guid.NewGuid(), FullName = "Existing", Email = "existing@test.com", AzureAdObjectId = "azure-1", Department = "IT", IsActive = true, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var service = new EmployeeService(context);
        var request = new CreateEmployeeRequest(
            AzureAdObjectId: "azure-new",
            FullName: "New Employee",
            Email: "existing@test.com",
            Department: "Engineering",
            IsActive: true);

        var (result, conflictKey) = await service.CreateAsync(request);

        Assert.Null(result);
        Assert.Equal("Email", conflictKey);
    }

    [Fact]
    public async Task CreateAsync_Should_ReturnConflict_WhenAzureAdObjectIdExists()
    {
        var context = TestDbContextFactory.CreateInMemoryContext();
        context.Employees.Add(new Employee { Id = Guid.NewGuid(), FullName = "Existing", Email = "existing@test.com", AzureAdObjectId = "azure-existing", Department = "IT", IsActive = true, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var service = new EmployeeService(context);
        var request = new CreateEmployeeRequest(
            AzureAdObjectId: "azure-existing",
            FullName: "New Employee",
            Email: "new@test.com",
            Department: "Engineering",
            IsActive: true);

        var (result, conflictKey) = await service.CreateAsync(request);

        Assert.Null(result);
        Assert.Equal("AzureAdObjectId", conflictKey);
    }

    [Fact]
    public async Task UpdateAsync_Should_UpdateEmployee_WhenEmployeeExists()
    {
        var employeeId = Guid.NewGuid();
        var context = TestDbContextFactory.CreateInMemoryContext();
        context.Employees.Add(new Employee { Id = employeeId, FullName = "Old Name", Email = "old@test.com", AzureAdObjectId = "azure-old", Department = "IT", IsActive = true, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var service = new EmployeeService(context);
        var request = new UpdateEmployeeRequest(
            AzureAdObjectId: "azure-updated",
            FullName: "Updated Name",
            Email: "updated@test.com",
            Department: "Engineering",
            IsActive: false);

        var (result, conflictKey) = await service.UpdateAsync(employeeId, request);

        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.FullName);
        Assert.Null(conflictKey);
    }

    [Fact]
    public async Task UpdateAsync_Should_ReturnNull_WhenEmployeeNotFound()
    {
        var context = TestDbContextFactory.CreateInMemoryContext();
        var service = new EmployeeService(context);
        var request = new UpdateEmployeeRequest(
            AzureAdObjectId: "azure-new",
            FullName: "Test",
            Email: "test@test.com",
            Department: "IT",
            IsActive: true);

        var (result, conflictKey) = await service.UpdateAsync(Guid.NewGuid(), request);

        Assert.Null(result);
        Assert.Null(conflictKey);
    }

    [Fact]
    public async Task UpdateAsync_Should_ReturnConflict_WhenEmailExistsOnAnotherEmployee()
    {
        var employeeId1 = Guid.NewGuid();
        var employeeId2 = Guid.NewGuid();
        var context = TestDbContextFactory.CreateInMemoryContext();
        context.Employees.AddRange(
            new Employee { Id = employeeId1, FullName = "Employee 1", Email = "employee1@test.com", AzureAdObjectId = "azure-1", Department = "IT", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Employee { Id = employeeId2, FullName = "Employee 2", Email = "employee2@test.com", AzureAdObjectId = "azure-2", Department = "IT", IsActive = true, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = new EmployeeService(context);
        var request = new UpdateEmployeeRequest(
            AzureAdObjectId: "azure-2-updated",
            FullName: "Updated Employee 2",
            Email: "employee1@test.com",
            Department: "Engineering",
            IsActive: true);

        var (result, conflictKey) = await service.UpdateAsync(employeeId2, request);

        Assert.Null(result);
        Assert.Equal("Email", conflictKey);
    }

    [Fact]
    public async Task DeleteAsync_Should_DeleteEmployee_WhenEmployeeExists()
    {
        var employeeId = Guid.NewGuid();
        var context = TestDbContextFactory.CreateInMemoryContext();
        context.Employees.Add(new Employee { Id = employeeId, FullName = "To Delete", Email = "delete@test.com", AzureAdObjectId = "azure-delete", Department = "IT", IsActive = true, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var service = new EmployeeService(context);
        var result = await service.DeleteAsync(employeeId);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_Should_ReturnFalse_WhenEmployeeNotFound()
    {
        var context = TestDbContextFactory.CreateInMemoryContext();
        var service = new EmployeeService(context);

        var result = await service.DeleteAsync(Guid.NewGuid());

        Assert.False(result);
    }
}
