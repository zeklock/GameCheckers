# GameApp Guidelines

`GameApp` is a .NET 10 console client referencing `GameBase`. Its entry point configures logging and runs the terminal game loop.

## Structure and Behavior
- Use `GameBase.GameController` for game state, legal moves, captures, promotion, turns, and win checks; do not reimplement game rules in the console client.
- Types in `Views/` render the board/player information and guide piece/move selection. Keep terminal presentation and input handling there rather than adding domain rules.
- Preserve the existing one-based coordinates displayed to players while passing the domain's zero-based positions to `GameBase`.
- Validate console input before using it, and keep invalid selections from terminating the game loop.

## Logging and Validation
- Logging is configured with Serilog JSON console and rolling file sinks in `Program.cs`; continue using `ILogger` for domain logging.
- Build with `dotnet build GameApp/GameApp.csproj`.
- Run `dotnet test GameCheckers.sln` when a change affects shared game behavior.
- Keep this file and the repository-level `AGENTS.md` current when fixing code or changing behavior, architecture, dependencies, commands, or constraints.
