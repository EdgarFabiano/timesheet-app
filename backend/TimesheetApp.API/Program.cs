using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using TimesheetApp.API;
using TimesheetApp.API.Data;
using TimesheetApp.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Database
if (builder.Environment.IsEnvironment("Test"))
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseInMemoryDatabase("TestDatabase"));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// Auth Service
builder.Services.AddScoped<AuthService>();

// Client Service
builder.Services.AddScoped<ClientService>();

// Project Service
builder.Services.AddScoped<ProjectService>();

// Employee Service
builder.Services.AddScoped<EmployeeService>();

// Assignment Service
builder.Services.AddScoped<AssignmentService>();

// Timesheet Service
builder.Services.AddScoped<TimesheetService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtKey = builder.Environment.IsEnvironment("Test")
            ? "SuperSecretTestKey12345678901234567890"
            : builder.Configuration["Jwt:Key"]!;
        var jwtIssuer = builder.Environment.IsEnvironment("Test")
            ? "TestIssuer"
            : builder.Configuration["Jwt:Issuer"]!;
        var jwtAudience = builder.Environment.IsEnvironment("Test")
            ? "TestAudience"
            : builder.Configuration["Jwt:Audience"]!;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    var maxRetries = 10;
    for (var i = 0; i < maxRetries; i++)
    {
        try
        {
            db.Database.Migrate();
            break;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Database not ready, retrying ({RetryCount}/{MaxRetries})...", i + 1, maxRetries);
            Thread.Sleep(2000);
        }
    }

    if (app.Environment.IsDevelopment())
    {
        DbSeeder.Seed(db);
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();