using Microsoft.EntityFrameworkCore;
using TimesheetApp.API.Data;
using TimesheetApp.API.DTOs.Assignments;
using TimesheetApp.API.Models;

namespace TimesheetApp.API.Services;

public class AssignmentService
{
    private readonly AppDbContext _db;

    public AssignmentService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Assigns an employee to a project. Returns (assignment, null) on success, or (null, errorKey) when employee not found, project not found, or already assigned.</summary>
    public async Task<(AssignmentResponse? assignment, string? errorKey)> CreateAsync(CreateAssignmentRequest request)
    {
        var employeeExists = await _db.Employees.AnyAsync(e => e.Id == request.EmployeeId);
        if (!employeeExists)
            return (null, "EmployeeNotFound");

        var projectExists = await _db.Projects.AnyAsync(p => p.Id == request.ProjectId);
        if (!projectExists)
            return (null, "ProjectNotFound");

        var alreadyAssigned = await _db.Assignments.AnyAsync(a =>
            a.EmployeeId == request.EmployeeId && a.ProjectId == request.ProjectId);
        if (alreadyAssigned)
            return (null, "AlreadyAssigned");

        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            EmployeeId = request.EmployeeId,
            ProjectId = request.ProjectId,
            AssignedAt = DateTime.UtcNow,
            IsActive = request.IsActive
        };

        _db.Assignments.Add(assignment);
        await _db.SaveChangesAsync();

        await _db.Entry(assignment)
            .Reference(a => a.Employee)
            .LoadAsync();
        await _db.Entry(assignment)
            .Reference(a => a.Project)
            .LoadAsync();

        return (MapToResponse(assignment), null);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var assignment = await _db.Assignments.FirstOrDefaultAsync(a => a.Id == id);
        if (assignment == null)
            return false;

        _db.Assignments.Remove(assignment);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<IReadOnlyList<AssignmentResponse>> GetByProjectIdAsync(Guid projectId)
    {
        return await _db.Assignments
            .AsNoTracking()
            .Where(a => a.ProjectId == projectId)
            .Select(a => new AssignmentResponse(
                a.Id,
                a.EmployeeId,
                a.Employee.FullName,
                a.ProjectId,
                a.Project.Name,
                a.AssignedAt,
                a.IsActive))
            .OrderBy(a => a.EmployeeName)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<AssignmentResponse>> GetByEmployeeIdAsync(Guid employeeId)
    {
        return await _db.Assignments
            .AsNoTracking()
            .Where(a => a.EmployeeId == employeeId)
            .Select(a => new AssignmentResponse(
                a.Id,
                a.EmployeeId,
                a.Employee.FullName,
                a.ProjectId,
                a.Project.Name,
                a.AssignedAt,
                a.IsActive))
            .OrderBy(a => a.ProjectName)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<AssignmentResponse>> GetByProjectAndEmployeeAsync(
        Guid projectId,
        Guid employeeId)
    {
        return await _db.Assignments
            .AsNoTracking()
            .Where(a => a.ProjectId == projectId && a.EmployeeId == employeeId)
            .Select(a => new AssignmentResponse(
                a.Id,
                a.EmployeeId,
                a.Employee.FullName,
                a.ProjectId,
                a.Project.Name,
                a.AssignedAt,
                a.IsActive))
            .ToListAsync();
    }

    public async Task<AssignmentResponse?> GetByIdAsync(Guid id)
    {
        var assignment = await _db.Assignments
            .AsNoTracking()
            .Include(a => a.Employee)
            .Include(a => a.Project)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (assignment == null)
            return null;

        return MapToResponse(assignment);
    }

    private static AssignmentResponse MapToResponse(Assignment a)
    {
        return new AssignmentResponse(
            a.Id,
            a.EmployeeId,
            a.Employee.FullName,
            a.ProjectId,
            a.Project.Name,
            a.AssignedAt,
            a.IsActive);
    }
}
