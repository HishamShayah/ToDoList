using Api.Middlewares;
using Application.Interfaces;
using Application.Profiles;
using Application.Services;
using Application.Validators;
using Core.Helpers;
using Core.Interfaces;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.SqlServer;
using Infrastructure.Data;
using Infrastructure.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System;
using System.Text;
using ToDoList.Application.BackgroundJobs;
using ToDoList.Infrastructure.BackgroundJobs;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .Enrich.FromLogContext()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();
builder.WebHost.UseUrls("http://*:5000");

// ✅ Hangfire configuration 
builder.Services.AddHangfire(config =>
{
    config.UseSqlServerStorage(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new SqlServerStorageOptions
        {
            SchemaName = "Hangfire" 
        });
});

// ✅ Hangfire Worker
builder.Services.AddHangfireServer();

// 1. Add Controllers and Validators
builder.Services.AddControllers();
builder.Services
    .AddFluentValidationAutoValidation()
    .AddFluentValidationClientsideAdapters();

builder.Services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<TokenRequestModelValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<InviteUserDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateTaskDtoValidator>();

// 2. Configure JWT settings from appsettings
var jwtSection = builder.Configuration.GetSection("Jwt");
builder.Services.Configure<JWT>(jwtSection);
var jwtSettings = jwtSection.Get<JWT>()
    ?? throw new InvalidOperationException("Missing JWT configuration section: Jwt.");

// 3. Add JWT Authentication only
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
    };
});

// 4. Authorization
builder.Services.AddAuthorization();

// 5. Swagger with JWT support
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "ToDoList API", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Please enter JWT with Bearer into field"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// 6. DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure()));

// 7. Identity - Core only (No Cookie UI)
builder.Services.AddIdentityCore<IdentityUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// 8. AutoMapper
builder.Services.AddAutoMapper(typeof(AutoMapperProfile).Assembly);

// 9. Application Services & Repositories
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IInvitationRepository, InvitationRepository>();
builder.Services.AddScoped<IInvitationService, InvitationService>();
builder.Services.AddScoped<ITodoJobs, TodoJobs>();

var app = builder.Build();
await ApplyMigrationsAndSeedAsync(app);

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseHangfireDashboard("/hangfire");
RecurringJob.AddOrUpdate<ITodoJobs>(
    "daily-todo-summary",
    x => x.SendDailySummaryAsync(),
    Cron.Daily(20)
);

app.Run();
static async Task ApplyMigrationsAndSeedAsync(WebApplication app)
{
    const int maxAttempts = 8;

    for (var attempt = 1; attempt <= maxAttempts; attempt++)
    {
        try
        {
            using var scope = app.Services.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var pendingMigrations = await db.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                await db.Database.MigrateAsync();
            }

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await ApplicationDbContextSeeder.SeedAsync(db, userManager, roleManager);
            return;
        }
        catch (SqlException ex) when (attempt < maxAttempts && ShouldRetryMigration(ex))
        {
            var delay = TimeSpan.FromSeconds(Math.Min(20, attempt * 2));
            Log.Warning(
                ex,
                "Database migration attempt {Attempt}/{MaxAttempts} failed with SQL error {ErrorNumber}. Retrying in {DelaySeconds}s...",
                attempt,
                maxAttempts,
                ex.Number,
                (int)delay.TotalSeconds);
            await Task.Delay(delay);
        }
    }

    throw new InvalidOperationException("Database migration and seeding failed after multiple retries.");
}


static bool ShouldRetryMigration(SqlException ex)
{
    // 1801: Database already exists (usually a startup race condition).
    // 4060/927/945: database not available yet (startup/recovery window).
    // -2/233/1205/0: transient connection/timeout/deadlock errors seen during startup.
    return ex.Number is 1801 or 4060 or 927 or 945 or -2 or 233 or 1205 or 0;
}
public partial class Program { }
