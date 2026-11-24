using GameBase.Dtos;
using GameBase.Enums;
using GameBase.Interfaces;
using GameBase.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace GameBase.Tests;

[TestFixture]
public class GameControllerTest
{
    private GameController _gameController = null!;
    private IBoard _board = null!;
    private List<IPlayer> _players = null!;

    [SetUp]
    public void Setup()
    {
        _board = new Board(GameController.BoardSize);
        _players =
        [
            new Player(Color.Black, "Player 1"),
            new Player(Color.White, "Player 2")
        ];

        Mock<ILogger<GameController>> loggerMock = new();
        _gameController = new GameController(_board, _players, loggerMock.Object);
    }

    #region Start
    [Test]
    public void Start_WhenGameStarts_PiecesOnBoardShouldBe12()
    {
        // Arrange
        const int expectedPieces = 12;

        // Act
        _gameController.Start();
        Dictionary<Color, int> actualPieces = _board.Cells
            .Cast<ICell>()
            .Where(c => c.Piece is not null)
            .GroupBy(c => c.Piece!.Color)
            .ToDictionary(g => g.Key, g => g.Count());

        // Assert
        Assert.That(actualPieces[Color.Black], Is.EqualTo(expectedPieces));
        Assert.That(actualPieces[Color.White], Is.EqualTo(expectedPieces));
    }

    [Test]
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(3)]
    [TestCase(5)]
    [TestCase(7)]
    [TestCase(11)]
    public void Start_WhenGameStarts_PiecesOnBoardNotLessThan12(int invalidCount)
    {
        // Arrange & Act
        _gameController.Start();
        Dictionary<Color, int> actualPieces = _board.Cells
            .Cast<ICell>()
            .Where(c => c.Piece is not null)
            .GroupBy(c => c.Piece!.Color)
            .ToDictionary(g => g.Key, g => g.Count());

        // Assert
        Assert.That(actualPieces[Color.Black], Is.Not.LessThan(invalidCount));
        Assert.That(actualPieces[Color.White], Is.Not.LessThan(invalidCount));
    }

    [Test]
    [TestCase(13)]
    [TestCase(17)]
    [TestCase(19)]
    public void Start_WhenGameStarts_PiecesOnBoardNotGreaterThan12(int invalidCount)
    {
        // Arrange & Act
        _gameController.Start();
        Dictionary<Color, int> actualPieces = _board.Cells
            .Cast<ICell>()
            .Where(c => c.Piece is not null)
            .GroupBy(c => c.Piece!.Color)
            .ToDictionary(g => g.Key, g => g.Count());

        // Assert
        Assert.That(actualPieces[Color.Black], Is.Not.GreaterThan(invalidCount));
        Assert.That(actualPieces[Color.White], Is.Not.GreaterThan(invalidCount));
    }

    [Test]
    public void Start_WhenGameStarts_PlayerPiecesShouldBe12()
    {
        // Arrange
        const int expectedPieces = 12;

        // Act
        _gameController.Start();
        Dictionary<IPlayer, int> actualPieces = _gameController.GetPlayerPieces()
            .ToDictionary(g => g.Key, g => g.Value.Count);

        // Assert
        foreach (IPlayer player in _players)
        {
            Assert.That(actualPieces[player], Is.EqualTo(expectedPieces));
        }
    }

    [Test]
    [TestCase(0)]
    [TestCase(1)]
    [TestCase(3)]
    [TestCase(5)]
    [TestCase(7)]
    [TestCase(11)]
    public void Start_WhenGameStarts_PlayerPiecesNotLessThan12(int invalidCount)
    {
        // Arrange & Act
        _gameController.Start();
        Dictionary<IPlayer, int> actualPieces = _gameController.GetPlayerPieces()
            .ToDictionary(g => g.Key, g => g.Value.Count);

        // Assert
        foreach (IPlayer player in _players)
        {
            Assert.That(actualPieces[player], Is.Not.LessThan(invalidCount));
        }
    }

    [Test]
    [TestCase(13)]
    [TestCase(17)]
    [TestCase(19)]
    public void Start_WhenGameStarts_PlayerPiecesNotGreaterThan12(int invalidCount)
    {
        // Arrange & Act
        _gameController.Start();
        Dictionary<IPlayer, int> actualPieces = _gameController.GetPlayerPieces()
            .ToDictionary(g => g.Key, g => g.Value.Count);

        // Assert
        foreach (IPlayer player in _players)
        {
            Assert.That(actualPieces[player], Is.Not.GreaterThan(invalidCount));
        }
    }
    #endregion

    #region CheckWin
    [Test]
    public void CheckWin_AtGameStart_WinnerIsNull()
    {
        // Arrange
        _gameController.Start();

        // Act
        _gameController.CheckWin();

        // Assert
        Assert.That(_gameController.GetWinner(), Is.Null);
    }

    [Test]
    public void CheckWin_CurrentPlayerHasNoMovablePieces_OpponentWins()
    {
        // Arrange
        Mock<ILogger<GameController>> loggerMock = new();
        Mock<GameController> gameMock = new(_board, _players, loggerMock.Object) { CallBase = true };
        IPlayer currentPlayer = gameMock.Object.GetCurrentPlayer();
        IPlayer opponentPlayer = gameMock.Object.GetPlayers().First(p => p != currentPlayer);
        gameMock
            .Setup(g => g.GetMovablePieces(currentPlayer))
            .Returns([]);

        // Act
        gameMock.Object.CheckWin();

        // Assert
        Assert.That(gameMock.Object.GetWinner(), Is.EqualTo(opponentPlayer));
    }

    [Test]
    public void CheckWin_CurrentPlayerHasNoMovablePieces_WinnerIsNotCurrentPlayer()
    {
        // Arrange
        Mock<ILogger<GameController>> loggerMock = new();
        Mock<GameController> gameMock = new(_board, _players, loggerMock.Object) { CallBase = true };
        IPlayer currentPlayer = gameMock.Object.GetCurrentPlayer();
        gameMock
            .Setup(g => g.GetMovablePieces(currentPlayer))
            .Returns([]);

        // Act
        gameMock.Object.CheckWin();

        // Assert
        Assert.That(gameMock.Object.GetWinner(), Is.Not.EqualTo(currentPlayer));
    }

    [Test]
    public void CheckWin_CurrentPlayerHasNoPiecesLeft_OpponentWins()
    {
        // Arrange
        _gameController.Start();
        IPlayer currentPlayer = _gameController.GetCurrentPlayer();
        IPlayer opponentPlayer = _players.First(p => p != currentPlayer);
        Dictionary<IPlayer, List<IPiece>> playerPieces = _gameController.GetPlayerPieces();
        playerPieces[currentPlayer].Clear();

        // Act
        _gameController.CheckWin();

        // Assert
        Assert.That(_gameController.GetWinner(), Is.EqualTo(opponentPlayer));
    }

    [Test]
    public void CheckWin_CurrentPlayerHasNoPiecesLeft_WinnerIsNotCurrentPlayer()
    {
        // Arrange
        _gameController.Start();
        IPlayer currentPlayer = _gameController.GetCurrentPlayer();
        Dictionary<IPlayer, List<IPiece>> playerPieces = _gameController.GetPlayerPieces();
        playerPieces[currentPlayer].Clear();

        // Act
        _gameController.CheckWin();

        // Assert
        Assert.That(_gameController.GetWinner(), Is.Not.EqualTo(currentPlayer));
    }
    #endregion

    #region GetMovablePieces
    [Test]
    [TestCase(Color.Black, 0, 5)]
    [TestCase(Color.Black, 2, 5)]
    [TestCase(Color.Black, 4, 5)]
    [TestCase(Color.Black, 6, 5)]
    [TestCase(Color.White, 1, 2)]
    [TestCase(Color.White, 3, 2)]
    [TestCase(Color.White, 5, 2)]
    [TestCase(Color.White, 7, 2)]
    public void GetMovablePieces_FrontPieceAtGameStart_IsInMovableList(
        Color color, int x, int y
    )
    {
        // Arrange
        IPlayer player = _players.First(p => p.Color == color);
        Position expectedMovablePiece = new(x, y);

        // Act
        _gameController.Start();
        List<Position> actualMovablePieces = _gameController.GetMovablePieces(player)
            .Select(mp => mp.Position)
            .ToList();

        // Assert
        Assert.That(actualMovablePieces, Contains.Item(expectedMovablePiece));
    }

    [Test]
    [TestCase(Color.Black, 1, 6)]
    [TestCase(Color.Black, 3, 6)]
    [TestCase(Color.Black, 5, 6)]
    [TestCase(Color.Black, 7, 6)]
    [TestCase(Color.White, 0, 3)]
    [TestCase(Color.White, 2, 3)]
    [TestCase(Color.White, 4, 3)]
    [TestCase(Color.White, 6, 3)]
    public void GetMovablePieces_BackPieceAtGameStart_IsNotInMovableList(
        Color color, int x, int y
    )
    {
        // Arrange
        IPlayer player = _players.First(p => p.Color == color);
        Position expectedMovablePiece = new(x, y);

        // Act
        _gameController.Start();
        List<Position> actualMovablePieces = _gameController.GetMovablePieces(player)
            .Select(mp => mp.Position)
            .ToList();

        // Assert
        Assert.That(actualMovablePieces, Does.Not.Contain(expectedMovablePiece));
    }
    #endregion

    #region GetLegalMoves
    [Test]
    [TestCase(0, 5)]
    [TestCase(2, 5)]
    [TestCase(4, 5)]
    [TestCase(6, 5)]
    [TestCase(1, 2)]
    [TestCase(3, 2)]
    [TestCase(5, 2)]
    [TestCase(7, 2)]
    public void GetLegalMoves_FrontPieceAtGameStart_ReturnsAvailableMoves(
        int x, int y
    )
    {
        // Arrange
        _gameController.Start();
        Position position = new(x, y);
        IPiece piece = _gameController.GetPiece(position)!;

        // Act
        List<List<Position>> actualLegalMoves = _gameController.GetLegalMoves(piece);

        // Assert
        Assert.That(actualLegalMoves.Count, Is.GreaterThan(0));
    }

    [Test]
    [TestCase(1, 6)]
    [TestCase(3, 6)]
    [TestCase(5, 6)]
    [TestCase(7, 6)]
    [TestCase(0, 3)]
    [TestCase(2, 3)]
    [TestCase(4, 3)]
    [TestCase(6, 3)]
    public void GetLegalMoves_BackPieceAtGameStart_ReturnsNoMoves(
        int x, int y
    )
    {
        // Arrange
        _gameController.Start();
        Position position = new(x, y);
        IPiece piece = _gameController.GetPiece(position)!;

        // Act
        List<List<Position>> actualLegalMoves = _gameController.GetLegalMoves(piece);

        // Assert
        Assert.That(actualLegalMoves.Count, Is.EqualTo(0));
    }
    #endregion

    #region GetPiece
    [Test]
    [TestCase(0, 1)]
    [TestCase(0, 5)]
    public void GetPiece_ExistingPiecePosition_ReturnsPiece(int x, int y)
    {
        // Arrange
        Position position = new(x, y);

        // Act
        _gameController.Start();
        IPiece? piece = _gameController.GetPiece(position);

        // Assert
        Assert.That(piece, Is.Not.Null);
    }

    [Test]
    [TestCase(0, 3)]
    [TestCase(0, 4)]
    public void GetPiece_EmptyCellPosition_ReturnsNull(int x, int y)
    {
        // Arrange
        Position position = new(x, y);

        // Act
        _gameController.Start();
        IPiece? piece = _gameController.GetPiece(position);

        // Assert
        Assert.That(piece, Is.Null);
    }
    #endregion

    #region IsInside
    [Test]
    [TestCase(0, 1)]
    [TestCase(0, 5)]
    public void IsInside_CoordinatesWithinBoard_ReturnsTrue(int x, int y)
    {
        // Arrange
        Position position = new(x, y);

        // Act
        bool isInside = GameController.IsInside(position);

        // Assert
        Assert.That(isInside, Is.True);
    }

    [Test]
    [TestCase(0, -1)]
    [TestCase(0, 8)]
    public void IsInside_CoordinatesOutsideBoard_ReturnsFalse(int x, int y)
    {
        // Arrange
        Position position = new(x, y);

        // Act
        bool isInside = GameController.IsInside(position);

        // Assert
        Assert.That(isInside, Is.False);
    }
    #endregion

    #region MovePiece
    [Test]
    [TestCase(0, 5, 1, 4)]
    [TestCase(2, 5, 3, 4)]
    public void MovePiece_ValidDestination_UpdatesBoardPositions(
        int xFrom, int yFrom, int xTo, int yTo
    )
    {
        // Arrange
        _gameController.Start();
        Position from = new(xFrom, yFrom);
        Position to = new(xTo, yTo);
        IPiece piece = _gameController.GetPiece(from)!;

        // Act
        _gameController.MovePiece(piece, to);

        // Assert
        Assert.That(_gameController.GetPiece(from), Is.Null);
        Assert.That(_gameController.GetPiece(to), Is.EqualTo(piece));
    }

    [Test]
    [TestCase(0, 5, 0, 4)]
    [TestCase(2, 5, 2, 4)]
    public void MovePiece_InvalidDestination_DoesNotUpdatePiecePosition(
        int xFrom, int yFrom, int xTo, int yTo
    )
    {
        // Arrange
        _gameController.Start();
        Position from = new(xFrom, yFrom);
        Position to = new(xTo, yTo);
        IPiece piece = _gameController.GetPiece(from)!;

        // Act
        _gameController.MovePiece(piece, to);

        // Assert
        Assert.That(_gameController.GetPiece(from), Is.EqualTo(piece));
        Assert.That(_gameController.GetPiece(to), Is.Null);
    }
    #endregion

    #region IsValidLegalMove
    [Test]
    [TestCase(0, 5, 1, 4)]
    [TestCase(2, 5, 3, 4)]
    public void IsValidLegalMove_ValidDestination_ReturnsTrue(
        int xFrom, int yFrom, int xTo, int yTo
    )
    {
        // Arrange
        Position from = new(xFrom, yFrom);
        Position to = new(xTo, yTo);
        List<Position> path = [to];

        _gameController.Start();
        IPiece piece = _gameController.GetPiece(from)!;

        // Act
        bool isValid = _gameController.IsValidLegalMove(piece, path);

        // Assert
        Assert.That(isValid, Is.True);
    }

    [Test]
    [TestCase(0, 5, 0, 4)]
    [TestCase(2, 5, 2, 4)]
    public void IsValidLegalMove_InvalidDestination_ReturnsFalse(
        int xFrom, int yFrom, int xTo, int yTo
    )
    {
        // Arrange
        Position from = new(xFrom, yFrom);
        Position to = new(xTo, yTo);
        List<Position> path = [to];

        _gameController.Start();
        IPiece piece = _gameController.GetPiece(from)!;

        // Act
        bool isValid = _gameController.IsValidLegalMove(piece, path);

        // Assert
        Assert.That(isValid, Is.False);
    }
    #endregion
}
