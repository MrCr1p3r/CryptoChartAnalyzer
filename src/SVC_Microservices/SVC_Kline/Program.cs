using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Infrastructure;
using SVC_Kline.Repositories;
using SVC_Kline.Repositories.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables from the .env file
Env.TraversePath().Load();

// Add Aspire service defaults
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<KlineDataDbContext>(options =>
    options.UseSqlServer(
        $"Server={Environment.GetEnvironmentVariable("DB_SERVER")};"
            + $"Database={Environment.GetEnvironmentVariable("DB_NAME")};"
            + $"User Id={Environment.GetEnvironmentVariable("DB_USER")};"
            + $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD")};"
            + "MultipleActiveResultSets=true;TrustServerCertificate=true;"
    )
);

// Register the repository for dependency injection
builder.Services.AddScoped<IKlineDataRepository, KlineDataRepository>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

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
