using GameApi.Data;
using GameApi.Interfaces;
using GameApi.Services;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;

var builder = WebApplication.CreateBuilder(args);
string logDirectory = FindProjectDirectory("GameApi.csproj") ?? AppContext.BaseDirectory;

// Setup logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .WriteTo.Console(
        new JsonFormatter(),
        restrictedToMinimumLevel: LogEventLevel.Information
    )
    .WriteTo.File(
        new JsonFormatter(),
        Path.Combine(logDirectory, "logs", "gameapi-.log"),
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        shared: true
    )
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// GameApi services
builder.Services.AddSingleton<GameStore>();
builder.Services.AddScoped<IGameService, GameService>();

// CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("GameWeb", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();
app.UseCors("GameWeb");
app.UseAuthorization();
app.MapControllers();

app.Run();

static string? FindProjectDirectory(string projectFileName)
{
    DirectoryInfo? directory = new(AppContext.BaseDirectory);
    while (directory is not null)
    {
        if (File.Exists(Path.Combine(directory.FullName, projectFileName)))
        {
            return directory.FullName;
        }

        directory = directory.Parent;
    }

    return null;
}
