# GameBase (Core Checkers Engine) Guidelines

This directory contains the pure C# domain logic and rules engine for the Checkers game.

## Tech Stack
- .NET 10 Class Library (`Microsoft.NET.Sdk`), `Microsoft.Extensions.Logging.Abstractions`.

## Official Skill (from dotnet/skills)
- **`dotnet`**: Refer to this skill for core C# language idioms, clean object-oriented architecture, and performance best practices.

## Coding Standards & Rules
1. **Zero UI / Network Dependencies**:
   - `GameBase` must remain a pure domain library with no references to ASP.NET Core, Console, or Web libraries.
2. **Encapsulation & Immutability**:
   - Keep board state access controlled through `GameController` and `Board` methods.
   - Enforce checkers rules strictly: 8x8 grid, diagonal movement, forward-only for Man, multi-direction for King, mandatory jump chains.
3. **Events**:
   - Fire appropriate domain events: `PieceCaptured`, `PiecePromoted`, and `TurnChanged`.
4. **Testing Requirement**:
   - Any logic changes here must be accompanied or verified by tests in `GameBase.Tests`.
