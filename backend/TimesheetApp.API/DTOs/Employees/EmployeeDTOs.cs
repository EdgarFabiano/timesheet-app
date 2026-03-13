using System.ComponentModel.DataAnnotations;

namespace TimesheetApp.API.DTOs.Employees;

public record CreateEmployeeRequest(
    [Required, MaxLength(256)] string AzureAdObjectId,
    [Required, MaxLength(200)] string FullName,
    [Required, EmailAddress, MaxLength(320)] string Email,
    [Required, MaxLength(100)] string Department,
    bool IsActive = true
);

public record UpdateEmployeeRequest(
    [Required, MaxLength(256)] string AzureAdObjectId,
    [Required, MaxLength(200)] string FullName,
    [Required, EmailAddress, MaxLength(320)] string Email,
    [Required, MaxLength(100)] string Department,
    bool IsActive
);

public record EmployeeResponse(
    Guid Id,
    string AzureAdObjectId,
    string FullName,
    string Email,
    string Department,
    bool IsActive,
    DateTime CreatedAt,
    int AssignmentsCount
);
