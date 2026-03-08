using Microsoft.EntityFrameworkCore;
using TimesheetApp.API.DTOs.Projects;
using TimesheetApp.API.Models;
using TimesheetApp.API.Services;
using TimesheetApp.Tests.Helpers;

namespace TimesheetApp.Tests.UnitTests.Projects;

public class ProjectServiceTests
{
    private Guid _clientId;

    public ProjectServiceTests()
    {
        _clientId = Guid.NewGuid();
    }

    private AppDbContext CreateContextWithClient()
    {
        var context = TestDbContextFactory.CreateInMemoryContext();
        context.Clients.Add(new Client { Id = _clientId, Name = "Test Client", ContactEmail = "client@test.com", IsActive = true, CreatedAt = DateTime.UtcNow });
        return context;
    }

    [Fact]
    public async Task GetAllAsync_Should_ReturnAllProjects_OrderedByName()
    {
        var context = CreateContextWithClient();
        context.Projects.AddRange(
            new Project { Id = Guid.NewGuid(), Name = "Zebra Project", ClientId = _clientId, IsActive = true, CreatedAt = DateTime.UtcNow },
            new Project { Id = Guid.NewGuid(), Name = "Alpha Project", ClientId = _clientId, IsActive = true, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = new ProjectService(context);
        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("Alpha Project", result[0].Name);
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnProject_WhenProjectExists()
    {
        var context = CreateContextWithClient();
        var projectId = Guid.NewGuid();
        context.Projects.Add(new Project { Id = projectId, Name = "Test Project", ClientId = _clientId, IsActive = true, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var service = new ProjectService(context);
        var result = await service.GetByIdAsync(projectId);

        Assert.NotNull(result);
        Assert.Equal("Test Project", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnNull_WhenProjectNotFound()
    {
        var context = CreateContextWithClient();
        var service = new ProjectService(context);

        var result = await service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_Should_CreateProject_WhenClientExists()
    {
        var context = CreateContextWithClient();
        var service = new ProjectService(context);
        var request = new CreateProjectRequest(
            Name: "New Project",
            Description: "Description",
            StartDate: DateTime.UtcNow,
            EndDate: DateTime.UtcNow.AddMonths(3),
            IsActive: true,
            ClientId: _clientId);

        var result = await service.CreateAsync(request);

        Assert.NotNull(result);
        Assert.Equal("New Project", result.Name);
        Assert.Equal("Test Client", result.ClientName);
    }

    [Fact]
    public async Task CreateAsync_Should_ReturnNull_WhenClientNotFound()
    {
        var context = CreateContextWithClient();
        var service = new ProjectService(context);
        var request = new CreateProjectRequest(
            Name: "New Project",
            Description: "Description",
            StartDate: DateTime.UtcNow,
            EndDate: DateTime.UtcNow.AddMonths(3),
            IsActive: true,
            ClientId: Guid.NewGuid());

        var result = await service.CreateAsync(request);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_Should_UpdateProject_WhenProjectExists()
    {
        var context = CreateContextWithClient();
        var projectId = Guid.NewGuid();
        context.Projects.Add(new Project { Id = projectId, Name = "Old Name", ClientId = _clientId, IsActive = true, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var service = new ProjectService(context);
        var request = new UpdateProjectRequest(
            Name: "Updated Name",
            Description: "Updated Description",
            StartDate: DateTime.UtcNow,
            EndDate: DateTime.UtcNow.AddMonths(6),
            IsActive: false,
            ClientId: _clientId);

        var (result, clientNotFound) = await service.UpdateAsync(projectId, request);

        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        Assert.False(result.IsActive);
        Assert.False(clientNotFound);
    }

    [Fact]
    public async Task UpdateAsync_Should_ReturnNull_WhenProjectNotFound()
    {
        var context = CreateContextWithClient();
        var service = new ProjectService(context);
        var request = new UpdateProjectRequest(
            Name: "Test",
            Description: "Test",
            StartDate: DateTime.UtcNow,
            EndDate: DateTime.UtcNow,
            IsActive: true,
            ClientId: _clientId);

        var (result, clientNotFound) = await service.UpdateAsync(Guid.NewGuid(), request);

        Assert.Null(result);
        Assert.False(clientNotFound);
    }

    [Fact]
    public async Task UpdateAsync_Should_ReturnClientNotFound_WhenNewClientNotFound()
    {
        var context = CreateContextWithClient();
        var projectId = Guid.NewGuid();
        context.Projects.Add(new Project { Id = projectId, Name = "Test Project", ClientId = _clientId, IsActive = true, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var service = new ProjectService(context);
        var request = new UpdateProjectRequest(
            Name: "Updated",
            Description: "Test",
            StartDate: DateTime.UtcNow,
            EndDate: DateTime.UtcNow,
            IsActive: true,
            ClientId: Guid.NewGuid());

        var (result, clientNotFound) = await service.UpdateAsync(projectId, request);

        Assert.Null(result);
        Assert.True(clientNotFound);
    }

    [Fact]
    public async Task DeleteAsync_Should_DeleteProject_WhenProjectExists()
    {
        var context = CreateContextWithClient();
        var projectId = Guid.NewGuid();
        context.Projects.Add(new Project { Id = projectId, Name = "To Delete", ClientId = _clientId, IsActive = true, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var service = new ProjectService(context);
        var result = await service.DeleteAsync(projectId);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_Should_ReturnFalse_WhenProjectNotFound()
    {
        var context = CreateContextWithClient();
        var service = new ProjectService(context);

        var result = await service.DeleteAsync(Guid.NewGuid());

        Assert.False(result);
    }
}
