using Microsoft.EntityFrameworkCore;
using TimesheetApp.API.Data;

namespace TimesheetApp.Tests.Helpers;

public static class TestDbContextFactory
{
    public static AppDbContext CreateInMemoryContext(string? databaseName = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName ?? Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    public static AppDbContext CreateInMemoryContextWithData(string databaseName)
    {
        var context = CreateInMemoryContext(databaseName);
        SeedData(context);
        return context;
    }

    private static void SeedData(AppDbContext context)
    {
        var client = new TimesheetApp.API.Models.Client
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Test Client",
            ContactEmail = "client@test.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Clients.Add(client);

        var employee = new TimesheetApp.API.Models.Employee
        {
            Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            FullName = "Test Employee",
            Email = "employee@test.com",
            Department = "Engineering",
            AzureAdObjectId = "azure-123",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Employees.Add(employee);

        var project = new TimesheetApp.API.Models.Project
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(3),
            IsActive = true,
            ClientId = client.Id,
            CreatedAt = DateTime.UtcNow
        };
        context.Projects.Add(project);

        context.SaveChanges();
    }
}
