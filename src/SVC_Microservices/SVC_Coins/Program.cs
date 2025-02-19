using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Infrastructure;
using SVC_Coins.Repositories;
using SVC_Coins.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

Env.TraversePath().Load();

// Add Aspire service defaults (includes OpenTelemetry)
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<CoinsDbContext>(options =>
    options
        .UseSqlServer(
            $"Server={Environment.GetEnvironmentVariable("DB_SERVER")};"
                + $"Database={Environment.GetEnvironmentVariable("DB_NAME")};"
                + $"User Id={Environment.GetEnvironmentVariable("DB_USER")};"
                + $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD")};"
                + "MultipleActiveResultSets=true;TrustServerCertificate=true;"
        )
        .LogTo(Console.WriteLine, LogLevel.Information)
);

// Register the repository for dependency injection
builder.Services.AddScoped<ICoinsRepository, CoinsRepository>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Map health check endpoints
app.MapDefaultEndpoints();

app.UseExceptionHandler();
app.UseStatusCodePages();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

public partial class Program { }
