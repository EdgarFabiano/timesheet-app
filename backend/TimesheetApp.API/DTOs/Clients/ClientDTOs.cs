using System.ComponentModel.DataAnnotations;

namespace TimesheetApp.API.DTOs.Clients;

public record CreateClientRequest(
    [Required, MaxLength(200)] string Name,
    [Required, EmailAddress, MaxLength(320)] string ContactEmail,
    bool IsActive = true
);

public record UpdateClientRequest(
    [Required, MaxLength(200)] string Name,
    [Required, EmailAddress, MaxLength(320)] string ContactEmail,
    bool IsActive
);

public record ClientResponse(
    Guid Id,
    string Name,
    string ContactEmail,
    bool IsActive,
    DateTime CreatedAt
);
