---
name: dotnet-test
description: >-
  Official Microsoft .NET testing skill from dotnet/skills (plugins/dotnet-test).
  Covers unit testing, integration testing, test execution with dotnet test,
  AAA pattern, test isolation, mocking dependencies, and test filtering.
metadata:
  author: dotnet
  source: https://github.com/dotnet/skills/tree/main/plugins/dotnet-test
---

# .NET Testing Guidelines (Microsoft Official dotnet/skills)

This skill provides testing engineering guidelines from the official Microsoft `dotnet/skills` repository (`plugins/dotnet-test`).

## 1. Test Organization: Arrange, Act, Assert (AAA)

Structure every unit test into three distinct phases:
1. **Arrange**: Initialize objects, set up dependencies, configure test doubles/mocks.
2. **Act**: Execute the specific method or behavior under test.
3. **Assert**: Verify expected state changes, return values, or fired events.

```csharp
[Test]
public void Move_ValidMove_UpdatesBoardAndSwitchesTurn()
{
    // Arrange
    _gameController.Start();
    var from = new Position(1, 2);
    var to = new Position(2, 3);

    // Act
    bool moved = _gameController.Move(from, to);

    // Assert
    Assert.That(moved, Is.True);
    Assert.That(_board.GetPiece(to), Is.Not.Null);
    Assert.That(_board.GetPiece(from), Is.Null);
}
```

## 2. Test Naming Convention

Follow Microsoft's standard naming pattern:
`[UnitOfWork]_[StateUnderTest]_[ExpectedBehavior]`

Examples:
- `Start_WhenCalled_InitializesPiecesCorrectly()`
- `GetLegalMoves_NoAvailableMoves_ReturnsEmptyList()`
- `Move_JumpOverOpponentPiece_RemovesCapturedPiece()`

## 3. Mocking Best Practices

- **Mock Boundaries and Interfaces**: Mock external dependencies like `IPlayer` or `ILogger<T>`.
- **Do Not Mock Domain Models**: Never mock domain entities, DTOs, or value objects (`Position`, `Piece`, `Board`). Use real objects for domain testing.
- **Verify Interactions**: Use Moq's `.Verify()` to check essential callbacks or event logging.

## 4. Data-Driven Testing

Use parameterized tests to validate multiple boundary inputs and edge cases efficiently:
```csharp
[TestCase(0, 1, true)]
[TestCase(0, 0, false)]
[TestCase(7, 6, true)]
public void IsValidPlaySquare_Coordinates_ReturnsExpected(int x, int y, bool expected)
{
    bool result = Board.IsDarkSquare(new Position(x, y));
    Assert.That(result, Is.EqualTo(expected));
}
```

## 5. Test Execution Commands

- **Run all tests**:
  ```bash
  dotnet test
  ```
- **Run project-specific tests**:
  ```bash
  dotnet test GameBase.Tests/GameBase.Tests.csproj
  ```
- **Filter tests by name**:
  ```bash
  dotnet test --filter "FullyQualifiedName~Move"
  ```
- **Collect coverage**:
  ```bash
  dotnet test --collect:"XPlat Code Coverage"
  ```
