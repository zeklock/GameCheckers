# GameApi (.NET 10 Web API) Guidelines

This directory contains the ASP.NET Core 10 Web API backend.

## Tech Stack
- ASP.NET Core 10 (`Microsoft.NET.Sdk.Web`), OpenAPI / Scalar, Serilog.

## Official Skills (from dotnet/skills)
- **`dotnet-aspnetcore`**: Refer to this skill for Web API architecture, middleware pipeline ordering, DI lifetimes, and OpenAPI configuration.
- **`dotnet`**: Refer to this skill for modern C# conventions, pattern matching, records, and async/await guidelines.

## Coding Standards & Rules
1. **Controller & Service Layer**:
   - Keep controllers thin in `Controllers/GameController.cs`.
   - Business logic and session coordination belong in `Services/GameService.cs`.
2. **Result Pattern**:
   - Always return `Result<T>` from `GameService`.
   - Return `BadRequest(result)` on failure, and `Ok(result)` on success.
3. **CORS & Integration**:
   - Ensure CORS policy allows requests from the React frontend (`http://localhost:5173`).
4. **Logging**:
   - Use Serilog structured logging with JSON formatting for all requests and state changes.
