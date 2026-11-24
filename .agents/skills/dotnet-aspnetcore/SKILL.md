---
name: dotnet-aspnetcore
description: >-
  Official Microsoft ASP.NET Core skill from dotnet/skills (plugins/dotnet-aspnetcore).
  Covers Web API architecture, minimal APIs vs controllers, middleware pipeline,
  routing, dependency injection, OpenAPI/Scalar, and async request handling.
metadata:
  author: dotnet
  source: https://github.com/dotnet/skills/tree/main/plugins/dotnet-aspnetcore
---

# ASP.NET Core Web API Guidelines (Microsoft Official dotnet/skills)

This skill provides official Microsoft standards and patterns for developing Web APIs, minimal APIs, and backend services with ASP.NET Core, sourced from the official Microsoft `dotnet/skills` repository.

## 1. Middleware Pipeline & Application Bootstrap

The ASP.NET Core middleware pipeline processes HTTP requests sequentially. Maintain Microsoft's standard pipeline ordering in `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// 1. Service Registrations (DI)
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("GameWeb", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// 2. Request Processing Pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("GameWeb");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

## 2. Controllers & Routing Standards

- **Routing Attributes**: Use `[Route("api/[controller]")]` or explicit kebab-case routes.
- **RESTful Verbs & HTTP Status Codes**:
  - `GET /api/game/state`: Return `200 OK` with state payload or `404 Not Found`.
  - `POST /api/game/start`: Return `200 OK` or `201 Created` on success; `400 BadRequest` on invalid inputs.
  - `POST /api/game/move`: Validate moves and return domain result with appropriate status.
- **Result Pattern**: Encapsulate business logic outcomes in `Result<T>`:
  - If `!result.IsSuccess`, return `BadRequest(result)`.
  - If `result.IsSuccess`, return `Ok(result)`.

## 3. Dependency Injection (DI) Lifetimes

Follow Microsoft's DI lifetime rules:
- **`Singleton`** (`AddSingleton`): For thread-safe in-memory stores (e.g. `GameStore`).
- **`Scoped`** (`AddScoped`): For per-request services (e.g. `IGameService, GameService`).
- **`Transient`** (`AddTransient`): For lightweight stateless utility operations.

> **Caution**: Avoid captive dependencies—never inject a `Scoped` service into a `Singleton`.

## 4. Asynchronous Request Pipeline

- Always use `async`/`await` for I/O operations (database, files, external services).
- Propagate `CancellationToken` from controller parameters to downstream services.
- Never use blocking calls like `.Result` or `.Wait()`.

## 5. OpenAPI & Scalar Documentation

- Register OpenAPI via `builder.Services.AddOpenApi()`.
- Use Scalar (`Scalar.AspNetCore`) for clean, interactive API testing in development mode.
