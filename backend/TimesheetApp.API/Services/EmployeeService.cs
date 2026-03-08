using Microsoft.EntityFrameworkCore;
using TimesheetApp.API.Data;
using TimesheetApp.API.DTOs.Employees;
using TimesheetApp.API.Models;

namespace TimesheetApp.API.Services;

public class EmployeeService
{
    private readonly AppDbContext _db;

    public EmployeeService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<EmployeeResponse>> GetAllAsync()
    {
        return await _db.Employees
            .AsNoTracking()
            .OrderBy(e => e.FullName)
            .Select(e => new EmployeeResponse(
                e.Id,
                e.AzureAdObjectId,
                e.FullName,
                e.Email,
                e.Department,
                e.IsActive,
                e.CreatedAt))
            .ToListAsync();
    }

    public async Task<EmployeeResponse?> GetByIdAsync(Guid id)
    {
        var employee = await _db.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null)
            return null;

        return MapToResponse(employee);
    }

    /// <returns>(created employee, or null if duplicate). conflictKey is "Email" or "AzureAdObjectId" when duplicate.</returns>
    public async Task<(EmployeeResponse? created, string? conflictKey)> CreateAsync(CreateEmployeeRequest request)
    {
        if (await _db.Employees.AnyAsync(e => e.Email == request.Email))
            return (null, "Email");
        if (await _db.Employees.AnyAsync(e => e.AzureAdObjectId == request.AzureAdObjectId))
            return (null, "AzureAdObjectId");

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            AzureAdObjectId = request.AzureAdObjectId,
            FullName = request.FullName,
            Email = request.Email,
            Department = request.Department,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _db.Employees.Add(employee);
        await _db.SaveChangesAsync();

        return (MapToResponse(employee), null);
    }

    /// <returns>(updated employee, or null if not found). conflictKey is "Email" or "AzureAdObjectId" when duplicate by another employee.</returns>
    public async Task<(EmployeeResponse? updated, string? conflictKey)> UpdateAsync(
        Guid id,
        UpdateEmployeeRequest request)
    {
        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == id);
        if (employee == null)
            return (null, null);

        if (await _db.Employees.AnyAsync(e => e.Id != id && e.Email == request.Email))
            return (null, "Email");
        if (await _db.Employees.AnyAsync(e => e.Id != id && e.AzureAdObjectId == request.AzureAdObjectId))
            return (null, "AzureAdObjectId");

        employee.AzureAdObjectId = request.AzureAdObjectId;
        employee.FullName = request.FullName;
        employee.Email = request.Email;
        employee.Department = request.Department;
        employee.IsActive = request.IsActive;

        await _db.SaveChangesAsync();

        return (MapToResponse(employee), null);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == id);
        if (employee == null)
            return false;

        _db.Employees.Remove(employee);
        await _db.SaveChangesAsync();
        return true;
    }

    private static EmployeeResponse MapToResponse(Employee e)
    {
        return new EmployeeResponse(
            e.Id,
            e.AzureAdObjectId,
            e.FullName,
            e.Email,
            e.Department,
            e.IsActive,
            e.CreatedAt);
    }
}
