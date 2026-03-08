using TimesheetApp.API.DTOs.Clients;
using TimesheetApp.API.Models;
using TimesheetApp.API.Services;
using TimesheetApp.Tests.Helpers;

namespace TimesheetApp.Tests.UnitTests.Clients;

public class ClientServiceTests
{
    [Fact]
    public async Task GetAllAsync_Should_ReturnAllClients_OrderedByName()
    {
        var context = TestDbContextFactory.CreateInMemoryContext();
        context.Clients.AddRange(
            new Client { Id = Guid.NewGuid(), Name = "Zebra Corp", ContactEmail = "zebra@test.com", IsActive = true, CreatedAt = DateTime.UtcNow },
            new Client { Id = Guid.NewGuid(), Name = "Alpha Inc", ContactEmail = "alpha@test.com", IsActive = true, CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var service = new ClientService(context);
        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("Alpha Inc", result[0].Name);
        Assert.Equal("Zebra Corp", result[1].Name);
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnClient_WhenClientExists()
    {
        var clientId = Guid.NewGuid();
        var context = TestDbContextFactory.CreateInMemoryContext();
        context.Clients.Add(new Client { Id = clientId, Name = "Test Client", ContactEmail = "test@test.com", IsActive = true, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var service = new ClientService(context);
        var result = await service.GetByIdAsync(clientId);

        Assert.NotNull(result);
        Assert.Equal("Test Client", result.Name);
    }

    [Fact]
    public async Task GetByIdAsync_Should_ReturnNull_WhenClientNotFound()
    {
        var context = TestDbContextFactory.CreateInMemoryContext();
        var service = new ClientService(context);

        var result = await service.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_Should_CreateClient_WhenValid()
    {
        var context = TestDbContextFactory.CreateInMemoryContext();
        var service = new ClientService(context);
        var request = new CreateClientRequest(Name: "New Client", ContactEmail: "new@test.com", IsActive: true);

        var result = await service.CreateAsync(request);

        Assert.NotNull(result);
        Assert.Equal("New Client", result.Name);
        Assert.Equal("new@test.com", result.ContactEmail);
        var clientInDb = await context.Clients.FindAsync(result.Id);
        Assert.NotNull(clientInDb);
    }

    [Fact]
    public async Task UpdateAsync_Should_UpdateClient_WhenClientExists()
    {
        var clientId = Guid.NewGuid();
        var context = TestDbContextFactory.CreateInMemoryContext();
        context.Clients.Add(new Client { Id = clientId, Name = "Old Name", ContactEmail = "old@test.com", IsActive = true, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var service = new ClientService(context);
        var request = new UpdateClientRequest(Name: "Updated Name", ContactEmail: "updated@test.com", IsActive: false);

        var result = await service.UpdateAsync(clientId, request);

        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal("updated@test.com", result.ContactEmail);
        Assert.False(result.IsActive);
    }

    [Fact]
    public async Task UpdateAsync_Should_ReturnNull_WhenClientNotFound()
    {
        var context = TestDbContextFactory.CreateInMemoryContext();
        var service = new ClientService(context);
        var request = new UpdateClientRequest(Name: "Test", ContactEmail: "test@test.com", IsActive: true);

        var result = await service.UpdateAsync(Guid.NewGuid(), request);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_Should_DeleteClient_WhenClientExists()
    {
        var clientId = Guid.NewGuid();
        var context = TestDbContextFactory.CreateInMemoryContext();
        context.Clients.Add(new Client { Id = clientId, Name = "To Delete", ContactEmail = "delete@test.com", IsActive = true, CreatedAt = DateTime.UtcNow });
        await context.SaveChangesAsync();

        var service = new ClientService(context);
        var result = await service.DeleteAsync(clientId);

        Assert.True(result);
        var clientInDb = await context.Clients.FindAsync(clientId);
        Assert.Null(clientInDb);
    }

    [Fact]
    public async Task DeleteAsync_Should_ReturnFalse_WhenClientNotFound()
    {
        var context = TestDbContextFactory.CreateInMemoryContext();
        var service = new ClientService(context);

        var result = await service.DeleteAsync(Guid.NewGuid());

        Assert.False(result);
    }
}
