using Microsoft.EntityFrameworkCore;
using TimesheetApp.API.Data;
using TimesheetApp.API.DTOs.Clients;
using TimesheetApp.API.Models;

namespace TimesheetApp.API.Services;

public class ClientService
{
    private readonly AppDbContext _db;

    public ClientService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ClientResponse>> GetAllAsync()
    {
        return await _db.Clients
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new ClientResponse(
                c.Id,
                c.Name,
                c.ContactEmail,
                c.IsActive,
                c.CreatedAt))
            .ToListAsync();
    }

    public async Task<ClientResponse?> GetByIdAsync(Guid id)
    {
        var client = await _db.Clients
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (client == null)
            return null;

        return new ClientResponse(
            client.Id,
            client.Name,
            client.ContactEmail,
            client.IsActive,
            client.CreatedAt);
    }

    public async Task<ClientResponse> CreateAsync(CreateClientRequest request)
    {
        var client = new Client
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            ContactEmail = request.ContactEmail,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.Clients.Add(client);
        await _db.SaveChangesAsync();

        return new ClientResponse(
            client.Id,
            client.Name,
            client.ContactEmail,
            client.IsActive,
            client.CreatedAt);
    }

    public async Task<ClientResponse?> UpdateAsync(Guid id, UpdateClientRequest request)
    {
        var client = await _db.Clients.FirstOrDefaultAsync(c => c.Id == id);

        if (client == null)
            return null;

        client.Name = request.Name;
        client.ContactEmail = request.ContactEmail;
        client.IsActive = request.IsActive;

        await _db.SaveChangesAsync();

        return new ClientResponse(
            client.Id,
            client.Name,
            client.ContactEmail,
            client.IsActive,
            client.CreatedAt);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var client = await _db.Clients.FirstOrDefaultAsync(c => c.Id == id);

        if (client == null)
            return false;

        _db.Clients.Remove(client);
        await _db.SaveChangesAsync();

        return true;
    }
}
