# GameApi Guidelines

`GameApi` is an ASP.NET Core 10 Web API (`Microsoft.NET.Sdk.Web`). It references `GameBase` and uses OpenAPI, Scalar, and Serilog.

## Structure and Behavior
- Keep HTTP concerns in `Controllers/`, game-session coordination and DTO mapping in `Services/GameService.cs`, and the active in-memory game in `Data/GameStore.cs`.
- Controllers should remain thin. `IGameService` operations return `Result<T>`; controllers return `Ok(result)` for success and `BadRequest(result)` for failure.
- Current routes are under `api/game`: `POST start`, `POST moves`, `POST move`, and `GET state`.
- `GameStore` is registered as a singleton and contains one active game for the running API process. Preserve this lifetime and state model unless the task explicitly changes it.
- The API accepts the frontend's zero-based board coordinates; keep DTO mapping aligned with `GameBase`.

## Hosting and Logging
- CORS policy `GameWeb` currently allows `http://localhost:5173`.
- Serilog is configured in `Program.cs` with JSON console and rolling file sinks. Use the existing `ILogger` integration and structured message templates for relevant events; do not assume that HTTP request logging middleware is enabled.
- OpenAPI and Scalar UI are mapped only in the Development environment.

## Validation
- Run `dotnet build GameApi/GameApi.csproj` for API changes.
- Run `dotnet test GameCheckers.sln` when a change affects shared game behavior.
- Keep this file and the repository-level `AGENTS.md` current when fixing code or changing API behavior, architecture, dependencies, commands, or constraints.
