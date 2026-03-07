namespace TimesheetApp.API.Models;

public enum UserRole
{
    Admin,
    Employee
}

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.Employee;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid? EmployeeId { get; set; }
    public Employee? Employee { get; set; }
}