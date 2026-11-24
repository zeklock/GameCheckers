# GameBase.Tests Guidelines

`GameBase.Tests` is a .NET 10 NUnit 4 test project for `GameBase`, using Moq and the .NET test SDK.

## Test Practices
- Follow Arrange-Act-Assert and use descriptive names such as `MethodName_Condition_ExpectedResult`.
- Prefer real domain objects (`Board`, `Player`, `Piece`, and `Position`) for rule tests. Use Moq for collaborators such as `ILogger<GameController>` or to isolate virtual controller behavior where useful.
- Existing tests cover controller startup and piece placement, win checks, movable/legal moves, piece lookup, board bounds, and move validation. Do not treat other behavior as covered without adding tests.
- When changing captures, multi-jump selection, promotion, turns, or win conditions, add focused tests for the changed behavior and important edge cases.

## Validation
- Run `dotnet test GameBase.Tests/GameBase.Tests.csproj` for this project.
- Run `dotnet test GameCheckers.sln` when validating a shared domain change across the solution.
- Keep this file and the repository-level `AGENTS.md` current whenever fixing code or changing test tooling, commands, dependencies, or constraints.
