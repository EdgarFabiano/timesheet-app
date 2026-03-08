using Microsoft.EntityFrameworkCore;
using TimesheetApp.API.Data;
using TimesheetApp.API.DTOs.Projects;
using TimesheetApp.API.Models;

namespace TimesheetApp.API.Services;

public class ProjectService
{
    private readonly AppDbContext _db;

    public ProjectService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ProjectResponse>> GetAllAsync()
    {
        return await _db.Projects
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => new ProjectResponse(
                p.Id,
                p.Name,
                p.Description == null || p.Description == "" ? null : p.Description,
                p.StartDate,
                p.EndDate,
                p.IsActive,
                p.CreatedAt,
                p.ClientId,
                p.Client.Name))
            .ToListAsync();
    }

    public async Task<ProjectResponse?> GetByIdAsync(Guid id)
    {
        var project = await _db.Projects
            .AsNoTracking()
            .Include(p => p.Client)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
            return null;

        return MapToResponse(project);
    }

    public async Task<ProjectResponse?> CreateAsync(CreateProjectRequest request)
    {
        var clientExists = await _db.Clients.AnyAsync(c => c.Id == request.ClientId);
        if (!clientExists)
            return null;

        var project = new Project
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description ?? string.Empty,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            IsActive = request.IsActive,
            ClientId = request.ClientId,
            CreatedAt = DateTime.UtcNow
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync();

        await _db.Entry(project)
            .Reference(p => p.Client)
            .LoadAsync();

        return MapToResponse(project);
    }

    /// <returns>(ProjectResponse, clientNotFound). project is null if project not found or client not found.</returns>
    public async Task<(ProjectResponse? project, bool clientNotFound)> UpdateAsync(
        Guid id,
        UpdateProjectRequest request)
    {
        var project = await _db.Projects
            .Include(p => p.Client)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
            return (null, false);

        var clientExists = await _db.Clients.AnyAsync(c => c.Id == request.ClientId);
        if (!clientExists)
            return (null, true);

        project.Name = request.Name;
        project.Description = request.Description ?? string.Empty;
        project.StartDate = request.StartDate;
        project.EndDate = request.EndDate;
        project.IsActive = request.IsActive;
        project.ClientId = request.ClientId;

        await _db.SaveChangesAsync();

        await _db.Entry(project)
            .Reference(p => p.Client)
            .LoadAsync();

        return (MapToResponse(project), false);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == id);

        if (project == null)
            return false;

        _db.Projects.Remove(project);
        await _db.SaveChangesAsync();

        return true;
    }

    private static ProjectResponse MapToResponse(Project p)
    {
        return new ProjectResponse(
            p.Id,
            p.Name,
            string.IsNullOrEmpty(p.Description) ? null : p.Description,
            p.StartDate,
            p.EndDate,
            p.IsActive,
            p.CreatedAt,
            p.ClientId,
            p.Client.Name);
    }
}
