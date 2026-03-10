namespace TimesheetApp.API.Models;

public class Employee
{
    public Guid Id { get; set; }
    public string AzureAdObjectId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid? UserId { get; set; }
    public User? User { get; set; }

    public ICollection<Assignment> Assignments { get; set; } = [];
    public ICollection<Timesheet> Timesheets { get; set; } = [];
}