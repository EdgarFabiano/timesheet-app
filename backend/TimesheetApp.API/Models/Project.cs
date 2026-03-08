namespace TimesheetApp.API.Models;

public class Project
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid ClientId { get; set; }
    public Client Client { get; set; } = null!;

    public ICollection<Assignment> Assignments { get; set; } = [];
    public ICollection<Timesheet> Timesheets { get; set; } = [];
}