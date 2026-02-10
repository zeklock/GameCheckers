using GameBase.Dtos;
using GameBase.Enums;
using GameBase.Interfaces;
using GameBase.Models;
using Moq;

namespace GameBase.Tests;

[TestFixture]
public class GameControllerTest
{
    private GameController _gameController;
    private IBoard _board;
    private List<IPlayer> _players;

    [SetUp]
    public void Setup()
    {
        _board = new Board(GameController.BoardSize);
        _players = new List<IPlayer>();

        Mock<IPlayer> player1 = new Mock<IPlayer>();
        player1.SetupGet(p => p.Name).Returns("Player 1");
        player1.SetupGet(p => p.Color).Returns(Color.Black);
        _players.Add(player1.Object);

        Mock<IPlayer> player2 = new Mock<IPlayer>();
        player2.SetupGet(p => p.Name).Returns("Player 2");
        player2.SetupGet(p => p.Color).Returns(Color.White);
        _players.Add(player2.Object);

        _gameController = new GameController(_board, _players);
    }

    #region Start
    [Test]
    public void Start_PiecesOnBoard_ShouldBe12()
    {
        // Arrange
        List<Color> colors = new List<Color> { Color.Black, Color.White };
        int expectedPieces = 12;

        // Act
        _gameController.Start();
        Dictionary<Color, int> actualPieces = _board.Cells
            .Cast<ICell>()
            .Where(c => c.Piece != null)
            .GroupBy(c => c.Piece!.Color)
            .ToDictionary(g => g.Key, g => g.Count());

        // Assert
        Assert.That(actualPieces[Color.Black], Is.EqualTo(expectedPieces));
        Assert.That(actualPieces[Color.White], Is.EqualTo(expectedPieces));
    }

    [Test]
    public void Start_PiecesOnBoard_InvalidPieceCount()
    {
        // Arrange
        List<Color> colors = new List<Color> { Color.Black, Color.White };
        int invalidCount;
        do
        {
            invalidCount = Random.Shared.Next(0, 100);
        } while (invalidCount == 12);

        // Act
        _gameController.Start();
        Dictionary<Color, int> actualPieces = _board.Cells
            .Cast<ICell>()
            .Where(c => c.Piece != null)
            .GroupBy(c => c.Piece!.Color)
            .ToDictionary(g => g.Key, g => g.Count());

        // Assert
        Assert.That(actualPieces[Color.Black], Is.Not.EqualTo(invalidCount));
        Assert.That(actualPieces[Color.White], Is.Not.EqualTo(invalidCount));
    }

    [Test]
    public void Start_PlayerPieces_ShouldBe12()
    {
        // Arrange
        Dictionary<IPlayer, List<IPiece>> playerPieces = _gameController.GetPlayerPieces();
        int expectedPieces = 12;

        // Act
        _gameController.Start();
        Dictionary<IPlayer, int> actualPieces = playerPieces
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
    public void Start_PlayerPieces_ShouldNotLessThan12(int invalidCount)
    {
        // Arrange
        Dictionary<IPlayer, List<IPiece>> playerPieces = _gameController.GetPlayerPieces();

        // Act
        _gameController.Start();
        Dictionary<IPlayer, int> actualPieces = playerPieces
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
    public void Start_PlayerPieces_ShouldNotGreaterThan12(int invalidCount)
    {
        // Arrange
        Dictionary<IPlayer, List<IPiece>> playerPieces = _gameController.GetPlayerPieces();

        // Act
        _gameController.Start();
        Dictionary<IPlayer, int> actualPieces = playerPieces
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
    public void CheckWin_StartGame_WinnerIsNull()
    {
        // Arrange
        _gameController.Start();

        // Act
        _gameController.CheckWin();

        // Assert
        Assert.That(_gameController.GetWinner(), Is.Null);
    }
    [Test]
    public void CheckWin_NoMovablePieces_WinnerIsOpponent()
    {
        // Arrange
        var gameMock = new Mock<GameController>(_board, _players) { CallBase = true };
        IPlayer currentPlayer = gameMock.Object.GetCurrentPlayer();
        IPlayer opponentPlayer = gameMock.Object.GetPlayers().First(p => p != currentPlayer);
        gameMock
            .Setup(g => g.GetMovablePieces(currentPlayer))
            .Returns(new List<MovablePieceDto>());

        // Act
        gameMock.Object.CheckWin();

        // Assert
        Assert.That(gameMock.Object.GetWinner(), Is.EqualTo(opponentPlayer));
    }

    [Test]
    public void CheckWin_NoMovablePieces_WinnerIsNotCurrentPlayer()
    {
        // Arrange
        var gameMock = new Mock<GameController>(_board, _players) { CallBase = true };
        IPlayer currentPlayer = gameMock.Object.GetCurrentPlayer();
        gameMock
            .Setup(g => g.GetMovablePieces(currentPlayer))
            .Returns(new List<MovablePieceDto>());

        // Act
        gameMock.Object.CheckWin();

        // Assert
        Assert.That(gameMock.Object.GetWinner(), Is.Not.EqualTo(currentPlayer));
    }

    [Test]
    public void CheckWin_NoPiecesLeft_WinnerIsOpponent()
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
    public void CheckWin_NoPiecesLeft_WinnerIsNotCurrentPlayer()
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
}
