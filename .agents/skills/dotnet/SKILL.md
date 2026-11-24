---
name: dotnet
description: >-
  Official Microsoft .NET and C# core skill from dotnet/skills (plugins/dotnet).
  Covers modern C# language features (.NET 8/9/10), object-oriented domain design,
  clean coding standards, memory efficiency, and runtime best practices.
metadata:
  author: dotnet
  source: https://github.com/dotnet/skills/tree/main/plugins/dotnet
---

# .NET and C# Core Development Guidelines (Microsoft Official dotnet/skills)

This skill provides core .NET and C# engineering standards from the official Microsoft `dotnet/skills` repository (`plugins/dotnet`).

## 1. Modern C# Language Features (.NET 8 / 9 / 10)

- **Nullable Reference Types**: Keep `#nullable enable` active. Use `?` explicitly for nullable fields (e.g. `IPlayer? _winner`, `IPiece? piece`). Check with pattern matching `if (value is not null)`.
- **Pattern Matching & Switch Expressions**:
  ```csharp
  string description = status switch
  {
      Status.Active => "Game in progress",
      Status.Finished => "Game over",
      _ => "Unknown"
  };
  ```
- **Primary Constructors & Records**:
  ```csharp
  // Immutable Record DTO:
  public record PositionDto(int X, int Y);

  // Class with primary constructor:
  public class GameService(GameStore store, ILogger<GameService> logger) : IGameService
  ```
- **Collection Expressions (`[...]`)**:
  ```csharp
  List<Position> emptyPositions = [];
  int[] validRows = [0, 1, 2];
  ```

## 2. Naming Conventions

- **PascalCase**: Classes, structs, records, interfaces (prefixed with `I`), methods, properties, public events.
- **camelCase**: Method parameters, local variables.
- **_camelCase**: Private instance fields (`private readonly IBoard _board;`).

## 3. Clean Domain Architecture

- **Zero Presentation Dependencies**: Domain engines (e.g. `GameBase`) must have no dependencies on Web, HTTP, Console, or UI frameworks.
- **Encapsulation**: Expose domain actions through explicit methods (`Start()`, `Move()`, `GetLegalMoves()`) rather than public state mutation.
- **Domain Events**: Disseminate domain occurrences via typed event arguments (`PieceCapturedEventArgs`, `PiecePromotedEventArgs`, `TurnChangedEventArgs`).

## 4. Resource & Memory Management

- Use `using` declarations for any class implementing `IDisposable`.
- Clean up event subscriptions when the subscriber has a shorter lifecycle than the publisher to avoid event handler memory leaks.
- Avoid multiple enumerations of `IEnumerable<T>`. Materialize with `.ToList()` or `.ToArray()` when iterating multiple times.
