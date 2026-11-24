# GameBase.Tests (NUnit Unit Testing) Guidelines

This directory contains the unit tests for the Checkers game engine.

## Tech Stack
- .NET 10, NUnit 4, Moq 4, Coverlet collector.

## Official Skills (from dotnet/skills)
- **`dotnet-test`**: Refer to this skill for Microsoft's unit testing standards, AAA pattern, test naming, and mocking guidelines.
- **`dotnet`**: Refer to this skill for C# coding standards and assertion idioms.

## Coding Standards & Rules
1. **Test Structure**:
   - Follow Arrange-Act-Assert (AAA) pattern.
   - Name test methods descriptively: `MethodName_Condition_ExpectedResult`.
2. **Mocking**:
   - Use Moq to mock `IPlayer` and `ILogger<GameController>` where appropriate.
   - Do not mock domain entities (`Position`, `Piece`, `Board`).
3. **Coverage Targets**:
   - Ensure comprehensive coverage of initial setup, regular moves, jump captures, multi-jump chains, King promotions, and win/loss states.
