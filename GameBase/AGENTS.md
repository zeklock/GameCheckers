# GameBase Guidelines

`GameBase` is the .NET 10 Checkers domain library. Its only current package dependency is `Microsoft.Extensions.Logging.Abstractions`.

## Boundaries and Rules
- Keep this library free of UI, console, HTTP, and ASP.NET Core dependencies. Do not move API or terminal concerns into the domain.
- `GameController` operates on `IBoard` and players; keep the rules centralized there and use the existing interfaces, models, DTOs, enums, and domain events.
- The game uses an 8x8 board with 12 starting pieces per player. Coordinates are zero-based (`x`, `y`).
- Preserve the implemented move rules: diagonal moves, mandatory captures, multi-jump paths, and selection of the longest available capture sequence. Kings may move in both directions; men move forward and are promoted on reaching the opposite end.
- Preserve the existing `PieceCaptured`, `PiecePromoted`, and `TurnChanged` events and structured `ILogger` messages when changing related behavior.

## Testing and Validation
- Add or update focused NUnit tests in `GameBase.Tests` for changes to game rules or controller behavior.
- Build with `dotnet build GameBase/GameBase.csproj` and run relevant tests with `dotnet test GameCheckers.sln`.
- Keep this file and the repository-level `AGENTS.md` current whenever fixing code or changing rules, APIs, dependencies, commands, or constraints.
