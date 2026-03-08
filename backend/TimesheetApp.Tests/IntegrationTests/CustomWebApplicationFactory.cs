using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TimesheetApp.API.Data;
using TimesheetApp.API.Models;

namespace TimesheetApp.Tests.IntegrationTests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("TestDatabase_" + Guid.NewGuid()));

            using var scope = services.BuildServiceProvider().CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            context.Database.EnsureCreated();

            SeedData(context);
        });
    }

    private static void SeedData(AppDbContext context)
    {
        var adminUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var employeeUserId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

        var client = new Client
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Name = "Test Client",
            ContactEmail = "client@test.com",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Clients.Add(client);

        var employee = new Employee
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

        var adminEmployee = new Employee
        {
            Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            FullName = "Admin Employee",
            Email = "admin@test.com",
            Department = "Management",
            AzureAdObjectId = "azure-admin",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
        context.Employees.Add(adminEmployee);

        var project = new Project
        {
            Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
            Name = "Test Project",
            Description = "Test Description",
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(3),
            IsActive = true,
            ClientId = client.Id,
            CreatedAt = DateTime.UtcNow
        };
        context.Projects.Add(project);

        var adminUser = new User
        {
            Id = adminUserId,
            Email = "admin@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
            Role = UserRole.Admin,
            EmployeeId = adminEmployee.Id,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(adminUser);

        var employeeUser = new User
        {
            Id = employeeUserId,
            Email = "employee@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("employee123"),
            Role = UserRole.Employee,
            EmployeeId = employee.Id,
            CreatedAt = DateTime.UtcNow
        };
        context.Users.Add(employeeUser);

        context.SaveChanges();
    }
}
