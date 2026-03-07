using Microsoft.EntityFrameworkCore;
using TimesheetApp.API.Models;

namespace TimesheetApp.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<Timesheet> Timesheets => Set<Timesheet>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.AzureAdObjectId)
            .IsUnique();

        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.Email)
            .IsUnique();

        modelBuilder.Entity<Assignment>()
            .HasIndex(a => new { a.EmployeeId, a.ProjectId })
            .IsUnique();

        modelBuilder.Entity<Timesheet>()
            .HasIndex(t => new { t.EmployeeId, t.ProjectId, t.Date })
            .IsUnique();

        // User
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasOne(u => u.Employee)
            .WithOne()
            .HasForeignKey<User>(u => u.EmployeeId)
            .IsRequired(false);
    }
}