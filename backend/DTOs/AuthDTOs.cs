using TimesheetApp.API.Models;

namespace TimesheetApp.API.DTOs;

public record RegisterRequest(
    string Email,
    string Password,
    string FullName,
    string Department,
    UserRole Role = UserRole.Employee
);

public record LoginRequest(
    string Email,
    string Password
);

public record AuthResponse(
    string Token,
    string Email,
    string Role,
    Guid UserId
);