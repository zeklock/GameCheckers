using GameApi.Data;
using GameApi.Dtos;
using GameApi.Interfaces;
using GameBase;
using GameBase.Dtos;
using GameBase.Enums;
using GameBase.Events;
using GameBase.Interfaces;
using GameBase.Models;

namespace GameApi.Services;

public class GameService(
    GameStore store,
    ILoggerFactory loggerFactory
) : IGameService
{
    private readonly GameStore _store = store;
    private readonly ILoggerFactory _loggerFactory = loggerFactory;

    public Result<GameDto> Start(List<PlayerDto> playerDtos)
    {
        if (playerDtos.Count != 2)
            return Result<GameDto>.Failure("Invalid number of players");

        if (string.IsNullOrWhiteSpace(playerDtos[0].Name))
            return Result<GameDto>.Failure("Invalid name for Player 1");

        if (string.IsNullOrWhiteSpace(playerDtos[1].Name))
            return Result<GameDto>.Failure("Invalid name for Player 2");

        if (playerDtos[0].Name == playerDtos[1].Name)
            return Result<GameDto>.Failure("Players must have different names");

        if (!Enum.IsDefined(typeof(Color), playerDtos[0].Color))
            return Result<GameDto>.Failure("Invalid color for Player 1");

        if (!Enum.IsDefined(typeof(Color), playerDtos[1].Color))
            return Result<GameDto>.Failure("Invalid color for Player 2");

        if (playerDtos[0].Color == playerDtos[1].Color)
            return Result<GameDto>.Failure("Players must have different colors");

        NewGame(playerDtos);
        UpdateGameState();

        return Result<GameDto>.Success(_store.GameDto);
    }

    public Result<List<List<PositionDto>>> GetAvailableMoves(PositionDto positionDto)
    {
        if (_store.Game is null)
            return Result<List<List<PositionDto>>>.Failure("Game not started");

        Position position = new(positionDto.X, positionDto.Y);

        if (!GameController.IsInside(position))
            return Result<List<List<PositionDto>>>.Failure("Invalid piece");

        IPiece? piece = _store.Game.GetPiece(position);

        if (piece is null || piece.Color != _store.Game.GetCurrentPlayer().Color)
            return Result<List<List<PositionDto>>>.Failure("Invalid piece");

        List<List<Position>> legalMoves = _store.Game.GetLegalMoves(piece);

        if (legalMoves.Count == 0)
            return Result<List<List<PositionDto>>>.Failure("Invalid piece");

        List<List<PositionDto>> availableMoves = legalMoves
            .Select(path => path
                .Select(p => new PositionDto(p.X, p.Y))
                .ToList()
            )
            .ToList();

        return Result<List<List<PositionDto>>>.Success(availableMoves);
    }

    public Result<GameDto> MovePiece(MoveDto moveDto)
    {
        if (_store.Game is null)
            return Result<GameDto>.Failure("Game not started");

        if (moveDto.Position is null || moveDto.Path is null)
            return Result<GameDto>.Failure("Invalid move");

        IPiece? piece = _store.Game.GetPiece(new Position(moveDto.Position.X, moveDto.Position.Y));
        List<Position> path = moveDto.Path.Select(p => new Position(p.X, p.Y)).ToList();

        if (piece is null || piece.Color != _store.Game.GetCurrentPlayer().Color || path.Count == 0)
            return Result<GameDto>.Failure("Invalid move");

        if (!_store.Game.IsValidLegalMove(piece, path))
            return Result<GameDto>.Failure("Invalid move");

        _store.GameDto.Notifications.Clear();
        _store.GameDto.Status.Clear();
        _store.Game.MovePiece(piece, path);
        _store.GameDto.Status.Add(Status.Move);
        UpdateGameState();

        return Result<GameDto>.Success(_store.GameDto);
    }

    public Result<GameDto> GetState()
    {
        return Result<GameDto>.Success(_store.GameDto);
    }

    private void NewGame(List<PlayerDto> playerDtos)
    {
        if (_store.Game is not null)
        {
            _store.Game.PieceCaptured -= Game_PieceCaptured;
            _store.Game.PiecePromoted -= Game_PiecePromoted;
            _store.Game.TurnChanged -= Game_TurnChanged;
            _store.GameDto.Notifications.Clear();
            _store.GameDto.Status.Clear();
        }

        IBoard board = new Board(GameController.BoardSize);
        List<IPlayer> players =
        [
            new Player(playerDtos[0].Color, playerDtos[0].Name),
            new Player(playerDtos[1].Color, playerDtos[1].Name)
        ];

        ILogger<GameController> gameLogger = _loggerFactory.CreateLogger<GameController>();
        _store.Game = new GameController(board, players, gameLogger);
        _store.Game.Start();

        _store.Game.PieceCaptured += Game_PieceCaptured;
        _store.Game.PiecePromoted += Game_PiecePromoted;
        _store.Game.TurnChanged += Game_TurnChanged;
    }

    private void UpdateGameState()
    {
        if (_store.Game is null)
            return;

        _store.Game.CheckWin();
        IPlayer? gameWinner = _store.Game.GetWinner();

        if (gameWinner is not null)
        {
            _store.GameDto.Notifications.Add($"\n{gameWinner.Name} ({gameWinner.Color}) WINS!");
            _store.GameDto.Status.Add(Status.GameOver);
        }

        PlayerDto? winner = gameWinner is null ? null : new PlayerDto(gameWinner.Name, gameWinner.Color);

        List<PlayerDto> players = _store.Game.GetPlayers()
            .Select(p => new PlayerDto(p.Name, p.Color))
            .ToList();

        IPlayer currentDomainPlayer = _store.Game.GetCurrentPlayer();
        PlayerDto currentPlayer = new(currentDomainPlayer.Name, currentDomainPlayer.Color);

        List<MovablePieceDto> movablePieces = _store.Game.GetMovablePieces(currentDomainPlayer);
        List<AvailablePieceDto> availablePieces = movablePieces
            .Select(mp => new AvailablePieceDto(
                new PositionDto(mp.Position.X, mp.Position.Y),
                new PieceDto(mp.Piece.Color, mp.Piece.Type)
            )).ToList();

        _store.GameDto.Board = BoardMapper();
        _store.GameDto.Players = players;
        _store.GameDto.CurrentPlayer = currentPlayer;
        _store.GameDto.Winner = winner;
        _store.GameDto.AvailablePieces = availablePieces;
    }

    private BoardDto BoardMapper()
    {
        if (_store.Game is null)
            return new BoardDto();

        IBoard board = _store.Game.GetBoard();
        int size = board.Cells.GetLength(0);

        BoardDto dto = new()
        {
            Size = size
        };

        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                ICell cell = board.Cells[x, y];
                dto.Cells.Add(new CellDto(
                    new PositionDto(x, y),
                    cell.Piece is null ? null : new PieceDto(cell.Piece.Color, cell.Piece.Type)
                ));
            }
        }

        return dto;
    }

    private void Game_PieceCaptured(object? sender, PieceCapturedEventArgs e)
    {
        string notification = $"Piece Captured! {e.CapturedPiece.Color} {e.CapturedPiece.Type} ({e.CapturedPosition.X + 1},{e.CapturedPosition.Y + 1}) was removed from the board.";
        _store.GameDto.Notifications.Add(notification);
        _store.GameDto.Status.Add(Status.Capture);
    }

    private void Game_PiecePromoted(object? sender, PiecePromotedEventArgs e)
    {
        string notification = $"Piece Promoted! {e.PromotedPiece.Color} piece ({e.PromotedPosition.X + 1},{e.PromotedPosition.Y + 1}) has become a King!";
        _store.GameDto.Notifications.Add(notification);
        _store.GameDto.Status.Add(Status.Promote);
    }

    private void Game_TurnChanged(object? sender, TurnChangedEventArgs e)
    {
        string notification = $"Turn switched to {e.CurrentPlayer.Name} ({e.CurrentPlayer.Color})";
        _store.GameDto.Notifications.Add(notification);
    }
}
