using TimesheetApp.API.Data;
using TimesheetApp.API.Models;

namespace TimesheetApp.API;

public static class DbSeeder
{
    public static void Seed(AppDbContext db)
    {
        if (db.Users.Any()) return;

        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@timesheet.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            Role = UserRole.Admin,
            IsActive = true
        };

        var employeeUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "employee@timesheet.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
            Role = UserRole.Employee,
            IsActive = true
        };

        db.Users.AddRange(adminUser, employeeUser);

        var employees = new List<Employee>
        {
            new Employee
            {
                Id = Guid.NewGuid(),
                AzureAdObjectId = Guid.NewGuid().ToString(),
                FullName = "Admin User",
                Email = "admin@timesheet.com",
                Department = "Management",
                UserId = adminUser.Id,
                User = adminUser
            },
            new Employee
            {
                Id = Guid.NewGuid(),
                AzureAdObjectId = Guid.NewGuid().ToString(),
                FullName = "John Doe",
                Email = "employee@timesheet.com",
                Department = "Engineering",
                UserId = employeeUser.Id,
                User = employeeUser
            },
            new Employee
            {
                Id = Guid.NewGuid(),
                AzureAdObjectId = Guid.NewGuid().ToString(),
                FullName = "Jane Smith",
                Email = "jane@timesheet.com",
                Department = "Design"
            }
        };

        db.Employees.AddRange(employees);

        var clients = new List<Client>
        {
            new Client
            {
                Id = Guid.NewGuid(),
                Name = "Acme Corp",
                ContactEmail = "contact@acme.com"
            },
            new Client
            {
                Id = Guid.NewGuid(),
                Name = "TechStart Inc",
                ContactEmail = "hello@techstart.io"
            }
        };

        db.Clients.AddRange(clients);

        var projects = new List<Project>
        {
            new Project
            {
                Id = Guid.NewGuid(),
                Name = "Website Redesign",
                Description = "Redesign company website with new branding",
                StartDate = DateTime.UtcNow.AddMonths(-2),
                ClientId = clients[0].Id,
                Client = clients[0]
            },
            new Project
            {
                Id = Guid.NewGuid(),
                Name = "Mobile App Development",
                Description = "Build native mobile app for iOS and Android",
                StartDate = DateTime.UtcNow.AddMonths(-1),
                ClientId = clients[1].Id,
                Client = clients[1]
            }
        };

        db.Projects.AddRange(projects);

        var assignments = new List<Assignment>
        {
            new Assignment
            {
                Id = Guid.NewGuid(),
                EmployeeId = employees[1].Id,
                Employee = employees[1],
                ProjectId = projects[0].Id,
                Project = projects[0]
            },
            new Assignment
            {
                Id = Guid.NewGuid(),
                EmployeeId = employees[2].Id,
                Employee = employees[2],
                ProjectId = projects[0].Id,
                Project = projects[0]
            },
            new Assignment
            {
                Id = Guid.NewGuid(),
                EmployeeId = employees[1].Id,
                Employee = employees[1],
                ProjectId = projects[1].Id,
                Project = projects[1]
            }
        };

        db.Assignments.AddRange(assignments);

        var timesheets = new List<Timesheet>
        {
            new Timesheet
            {
                Id = Guid.NewGuid(),
                EmployeeId = employees[1].Id,
                Employee = employees[1],
                ProjectId = projects[0].Id,
                Project = projects[0],
                Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-3)),
                HoursWorked = 4,
                Notes = "Initial design mockups"
            },
            new Timesheet
            {
                Id = Guid.NewGuid(),
                EmployeeId = employees[1].Id,
                Employee = employees[1],
                ProjectId = projects[0].Id,
                Project = projects[0],
                Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2)),
                HoursWorked = 6,
                Notes = "Client meeting and feedback"
            },
            new Timesheet
            {
                Id = Guid.NewGuid(),
                EmployeeId = employees[2].Id,
                Employee = employees[2],
                ProjectId = projects[0].Id,
                Project = projects[0],
                Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)),
                HoursWorked = 5,
                Notes = "UI component design"
            }
        };

        db.Timesheets.AddRange(timesheets);

        db.SaveChanges();
    }
}
