using Microsoft.EntityFrameworkCore;
using TimesheetApp.API.Data;
using TimesheetApp.API.DTOs.Timesheets;
using TimesheetApp.API.Models;

namespace TimesheetApp.API.Services;

public class TimesheetService
{
    private readonly AppDbContext _db;

    public TimesheetService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>Submit hours. Returns (timesheet, null) on success, or (null, errorKey) for EmployeeNotFound, ProjectNotFound, or DuplicateEntry.</summary>
    public async Task<(TimesheetResponse? timesheet, string? errorKey)> CreateAsync(CreateTimesheetRequest request)
    {
        var employeeExists = await _db.Employees.AnyAsync(e => e.Id == request.EmployeeId);
        if (!employeeExists)
            return (null, "EmployeeNotFound");

        var projectExists = await _db.Projects.AnyAsync(p => p.Id == request.ProjectId);
        if (!projectExists)
            return (null, "ProjectNotFound");

        var duplicate = await _db.Timesheets.AnyAsync(t =>
            t.EmployeeId == request.EmployeeId
            && t.ProjectId == request.ProjectId
            && t.Date == request.Date);
        if (duplicate)
            return (null, "DuplicateEntry");

        var timesheet = new Timesheet
        {
            Id = Guid.NewGuid(),
            EmployeeId = request.EmployeeId,
            ProjectId = request.ProjectId,
            Date = request.Date,
            HoursWorked = request.HoursWorked,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _db.Timesheets.Add(timesheet);
        await _db.SaveChangesAsync();

        await _db.Entry(timesheet)
            .Reference(t => t.Employee)
            .LoadAsync();
        await _db.Entry(timesheet)
            .Reference(t => t.Project)
            .LoadAsync();

        return (MapToResponse(timesheet), null);
    }

    /// <summary>Update hours and/or notes. Returns (timesheet, null) on success, or (null, true) when not found.</summary>
    public async Task<(TimesheetResponse? timesheet, bool notFound)> UpdateAsync(Guid id, UpdateTimesheetRequest request)
    {
        var timesheet = await _db.Timesheets
            .Include(t => t.Employee)
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (timesheet == null)
            return (null, true);

        timesheet.HoursWorked = request.HoursWorked;
        timesheet.Notes = request.Notes;

        await _db.SaveChangesAsync();

        return (MapToResponse(timesheet), false);
    }

    public async Task<IReadOnlyList<TimesheetResponse>> GetByEmployeeAndDateRangeAsync(
        Guid employeeId,
        DateOnly startDate,
        DateOnly endDate)
    {
        return await _db.Timesheets
            .AsNoTracking()
            .Where(t => t.EmployeeId == employeeId && t.Date >= startDate && t.Date <= endDate)
            .OrderBy(t => t.Date)
            .ThenBy(t => t.Project.Name)
            .Select(t => new TimesheetResponse(
                t.Id,
                t.EmployeeId,
                t.Employee.FullName,
                t.ProjectId,
                t.Project.Name,
                t.Date,
                t.HoursWorked,
                t.Notes,
                t.CreatedAt))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<TimesheetResponse>> GetByProjectIdAsync(
        Guid projectId,
        DateOnly? startDate = null,
        DateOnly? endDate = null)
    {
        var query = _db.Timesheets
            .AsNoTracking()
            .Where(t => t.ProjectId == projectId);

        if (startDate.HasValue)
            query = query.Where(t => t.Date >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(t => t.Date <= endDate.Value);

        return await query
            .OrderBy(t => t.Date)
            .ThenBy(t => t.Employee.FullName)
            .Select(t => new TimesheetResponse(
                t.Id,
                t.EmployeeId,
                t.Employee.FullName,
                t.ProjectId,
                t.Project.Name,
                t.Date,
                t.HoursWorked,
                t.Notes,
                t.CreatedAt))
            .ToListAsync();
    }

    public async Task<TimesheetResponse?> GetByIdAsync(Guid id)
    {
        var timesheet = await _db.Timesheets
            .AsNoTracking()
            .Include(t => t.Employee)
            .Include(t => t.Project)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (timesheet == null)
            return null;

        return MapToResponse(timesheet);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var timesheet = await _db.Timesheets.FirstOrDefaultAsync(t => t.Id == id);

        if (timesheet == null)
            return false;

        _db.Timesheets.Remove(timesheet);
        await _db.SaveChangesAsync();

        return true;
    }

    private static TimesheetResponse MapToResponse(Timesheet t)
    {
        return new TimesheetResponse(
            t.Id,
            t.EmployeeId,
            t.Employee.FullName,
            t.ProjectId,
            t.Project.Name,
            t.Date,
            t.HoursWorked,
            t.Notes,
            t.CreatedAt);
    }
}
