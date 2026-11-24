# GameApp (Console Terminal Application) Guidelines

This directory contains the .NET 10 console client for Checkers.

## Tech Stack
- .NET 10 Console (`OutputType: Exe`), Serilog, DI (`Microsoft.Extensions.DependencyInjection`).

## Official Skill (from dotnet/skills)
- **`dotnet`**: Refer to this skill for core C# coding conventions, primary constructors, and dependency injection patterns.

## Coding Standards & Rules
1. **Separation of Concerns**:
   - Renderers in `Views/` must only handle presentation and formatting.
   - Do not duplicate game rules; rely completely on `GameBase.GameController`.
2. **Terminal UX**:
   - Render the board clearly with coordinates and piece indicators.
   - Handle invalid inputs gracefully without crashing the loop.
