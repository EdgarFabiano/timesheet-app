using System.ComponentModel.DataAnnotations;

namespace TimesheetApp.API.DTOs.Projects;

public record CreateProjectRequest(
    [Required, MaxLength(200)] string Name,
    [MaxLength(1000)] string? Description,
    [Required] DateTime StartDate,
    DateTime? EndDate,
    bool IsActive,
    [Required] Guid ClientId
);

public record UpdateProjectRequest(
    [Required, MaxLength(200)] string Name,
    [MaxLength(1000)] string? Description,
    [Required] DateTime StartDate,
    DateTime? EndDate,
    bool IsActive,
    [Required] Guid ClientId
);

public record ProjectResponse(
    Guid Id,
    string Name,
    string? Description,
    DateTime StartDate,
    DateTime? EndDate,
    bool IsActive,
    DateTime CreatedAt,
    Guid ClientId,
    string ClientName
);

