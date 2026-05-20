using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using VPS.API.Middleware;
using VPS.Application.Interfaces;
using VPS.Application.Validators;
using VPS.Infrastructure.Data;
using VPS.Infrastructure.Email;
using VPS.Infrastructure.Jobs;
using VPS.Infrastructure.ML;
using VPS.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ----------------------
// Serilog
// ----------------------
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/vps-.log", rollingInterval: RollingInterval.Day)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// ----------------------
// Connection String
// ----------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new Exception("❌ Connection string 'DefaultConnection' is missing!");
}

// ----------------------
// EF Core
// ----------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// ----------------------
// JWT Authentication
// ----------------------
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new Exception("❌ JWT Key is missing!");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// ----------------------
// Authorization Policies
// ----------------------
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
    options.AddPolicy("StaffOrAdmin", p => p.RequireRole("Staff", "Admin"));
    options.AddPolicy("CustomerOnly", p => p.RequireRole("Customer"));
});

// ----------------------
// CORS
// ----------------------
var allowedOrigins = builder.Configuration
    .GetSection("Cors:Origins")
    .Get<string[]>() ?? new[] { "http://localhost:5173" };

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// ----------------------
// Services
// ----------------------
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IStaffService, StaffService>();
builder.Services.AddScoped<IVendorService, VendorService>();
builder.Services.AddScoped<IPartService, PartService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IPurchaseService, PurchaseService>();
builder.Services.AddScoped<ISalesService, SalesService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IPartRequestService, PartRequestService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddSingleton<IFailurePredictionService, FailurePredictionService>();

builder.Services.AddScoped<StockAlertJob>();
builder.Services.AddScoped<CreditReminderJob>();

// ----------------------
// Hangfire
// ----------------------
builder.Services.AddHangfire(config =>
    config.UsePostgreSqlStorage(c =>
        c.UseNpgsqlConnection(connectionString)));

builder.Services.AddHangfireServer();

// ----------------------
// FluentValidation
// ----------------------
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<LoginDtoValidator>();

// ----------------------
// Controllers & Swagger (🔥 FIXED)
// ----------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "VPS API",
        Version = "v1"
    });

    // 🔐 JWT Auth in Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {your JWT token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

var app = builder.Build();

// ----------------------
// DB Seed
// ----------------------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(db);
}

// ----------------------
// Middleware
// ----------------------
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseSerilogRequestLogging();

// ----------------------
// Swagger
// ----------------------
app.UseSwagger();
app.UseSwaggerUI();

// ----------------------
// Pipeline
// ----------------------
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.UseHangfireDashboard("/hangfire");

// ----------------------
// Background Jobs
// ----------------------
RecurringJob.AddOrUpdate<StockAlertJob>(
    "stock-alert", j => j.RunAsync(), Cron.Hourly);

RecurringJob.AddOrUpdate<CreditReminderJob>(
    "credit-reminder", j => j.RunAsync(), Cron.Daily);

app.Run();