using System.ComponentModel.DataAnnotations;

namespace TimesheetApp.API.DTOs.Timesheets;

public record CreateTimesheetRequest(
    [Required] Guid EmployeeId,
    [Required] Guid ProjectId,
    [Required] DateOnly Date,
    [Required, Range(0.5, 24, ErrorMessage = "Hours must be between 0.5 and 24.")] decimal HoursWorked,
    [MaxLength(2000)] string? Notes = null
);

public record UpdateTimesheetRequest(
    [Required, Range(0.5, 24, ErrorMessage = "Hours must be between 0.5 and 24.")] decimal HoursWorked,
    [MaxLength(2000)] string? Notes = null
);

public record TimesheetResponse(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    Guid ProjectId,
    string ProjectName,
    DateOnly Date,
    decimal HoursWorked,
    string? Notes,
    DateTime CreatedAt
);

public record BulkTimesheetEntry(
    [Required] Guid ProjectId,
    [Required] DateOnly Date,
    [Required, Range(0, 24, ErrorMessage = "Hours must be between 0 and 24.")] decimal HoursWorked,
    [MaxLength(2000)] string? Notes = null
);

public record BulkSaveTimesheetRequest(
    [Required] Guid EmployeeId,
    [Required] List<BulkTimesheetEntry> Entries
);

public record BulkSaveTimesheetResponse(
    IReadOnlyList<TimesheetResponse> Saved,
    IReadOnlyList<string> Errors
);
