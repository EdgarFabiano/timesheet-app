using System.ComponentModel.DataAnnotations;

namespace TimesheetApp.API.DTOs.Assignments;

public record CreateAssignmentRequest(
    [Required] Guid EmployeeId,
    [Required] Guid ProjectId,
    bool IsActive = true
);

public record AssignmentResponse(
    Guid Id,
    Guid EmployeeId,
    string EmployeeName,
    Guid ProjectId,
    string ProjectName,
    DateTime AssignedAt,
    bool IsActive
);
