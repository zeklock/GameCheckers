using GameApi.Data;
using GameApi.Dtos;
using GameApi.Interfaces;
using GameBase;
using GameBase.Dtos;
using GameBase.Events;
using GameBase.Interfaces;
using GameBase.Models;

namespace GameApi.Services;

public class GameService : IGameService
{
    private readonly GameStore _store;
    private readonly object _lock = new();

    public GameService(GameStore store)
    {
        _store = store;
    }

    public Result<GameDto> Start(List<PlayerDto> playerDtos)
    {
        lock (_lock)
        {
            if (playerDtos.Count != 2)
                return Result<GameDto>.Failure("Invalid number of players");

            if (string.IsNullOrEmpty(playerDtos[0].Name))
                return Result<GameDto>.Failure("Invalid name for Player 1");

            if (string.IsNullOrEmpty(playerDtos[1].Name))
                return Result<GameDto>.Failure("Invalid name for Player 2");

            if (playerDtos[0].Name == playerDtos[1].Name)
                return Result<GameDto>.Failure("Players must have different names");

            if (!Enum.TryParse<Color>(playerDtos[0].Color, true, out var color1)
                || !Enum.IsDefined(typeof(Color), color1))
                return Result<GameDto>.Failure("Invalid color for Player 1");

            if (!Enum.TryParse<Color>(playerDtos[1].Color, true, out var color2)
                || !Enum.IsDefined(typeof(Color), color2))
                return Result<GameDto>.Failure("Invalid color for Player 2");

            if (color1 == color2)
                return Result<GameDto>.Failure("Players must have different colors");

            NewGame(playerDtos, color1, color2);
            UpdateGameState();

            return Result<GameDto>.Success(_store.GameDto);
        }
    }

    public Result<List<List<PositionDto>>> GetAvailableMoves(PositionDto positionDto)
    {
        if (_store.Game == null)
            return Result<List<List<PositionDto>>>.Failure("Game not started");

        Position position = new Position(positionDto.X, positionDto.Y);

        if (!GameController.IsInside(position))
            return Result<List<List<PositionDto>>>.Failure("Invalid piece");

        IPiece? piece = _store.Game.GetPiece(position);

        if (piece == null || piece.Color != _store.Game.GetCurrentPlayer().Color)
            return Result<List<List<PositionDto>>>.Failure("Invalid piece");

        List<List<Position>> legalMoves = _store.Game.GetLegalMoves(piece);

        if (legalMoves.Count == 0)
            return Result<List<List<PositionDto>>>.Failure("Invalid piece");

        List<List<PositionDto>> availableMoves = legalMoves
            .Select(path => path
                .Select(p => new PositionDto { X = p.X, Y = p.Y })
                .ToList()
            )
            .ToList();

        return Result<List<List<PositionDto>>>.Success(availableMoves);
    }

    public Result<GameDto> MovePiece(MoveDto moveDto)
    {
        if (_store.Game == null)
            return Result<GameDto>.Failure("Game not started");

        IPiece? piece = _store.Game.GetPiece(new Position(moveDto.Position.X, moveDto.Position.Y));
        List<Position> path = moveDto.Path.Select(p => new Position(p.X, p.Y)).ToList();

        if (piece == null || piece.Color != _store.Game.GetCurrentPlayer().Color || path.Count == 0)
            return Result<GameDto>.Failure("Invalid move");

        if (!_store.Game.IsValidLegalMove(piece, path))
            return Result<GameDto>.Failure("Invalid move");

        lock (_lock)
        {
            _store.GameDto.Notifications.Clear();
            _store.Game.MovePiece(piece, path);
            UpdateGameState();
        }

        return Result<GameDto>.Success(_store.GameDto);
    }

    private void NewGame(List<PlayerDto> playerDtos, Color color1, Color color2)
    {
        if (_store.Game != null)
        {
            _store.Game.PieceCaptured -= game_PieceCaptured;
            _store.Game.PiecePromoted -= game_PiecePromoted;
            _store.Game.TurnChanged -= game_TurnChanged;
        }

        IBoard board = new Board(GameController.BoardSize);
        List<IPlayer> players = new List<IPlayer>
        {
            new Player(color1, playerDtos[0].Name),
            new Player(color2, playerDtos[1].Name)
        };

        _store.Game = new GameController(board, players);
        _store.Game.Start();

        _store.Game.PieceCaptured += game_PieceCaptured;
        _store.Game.PiecePromoted += game_PiecePromoted;
        _store.Game.TurnChanged += game_TurnChanged;
    }

    private void UpdateGameState()
    {
        if (_store.Game == null)
            return;

        _store.Game.CheckWin();
        IPlayer? gameWinner = _store.Game.GetWinner();

        if (gameWinner != null)
        {
            _store.GameDto.Notifications.Add($"\n{gameWinner.Name} ({gameWinner.Color}) WINS!");
        }

        PlayerDto? winner = gameWinner == null ? null : new PlayerDto
        {
            Name = gameWinner.Name,
            Color = gameWinner.Color.ToString()
        };

        List<PlayerDto> players = _store.Game.GetPlayers()
            .Select(p => new PlayerDto { Name = p.Name, Color = p.Color.ToString() }).ToList();
        PlayerDto currentPlayer = new PlayerDto
        {
            Name = _store.Game.GetCurrentPlayer().Name,
            Color = _store.Game.GetCurrentPlayer().Color.ToString()
        };

        List<MovablePieceDto> movablePieces = _store.Game.GetMovablePieces(_store.Game.GetCurrentPlayer());
        List<AvailablePieceDto> availablePieces = movablePieces
            .Select(mp => new AvailablePieceDto
            {
                Position = new PositionDto { X = mp.Position.X, Y = mp.Position.Y },
                Piece = new PieceDto { Color = mp.Piece.Color, Type = mp.Piece.Type },
            }).ToList();

        _store.GameDto.Board = BoardMapper();
        _store.GameDto.Players = players;
        _store.GameDto.CurrentPlayer = currentPlayer;
        _store.GameDto.Winner = winner;
        _store.GameDto.AvailableMoves = availablePieces;
    }

    private BoardDto BoardMapper()
    {
        if (_store.Game == null)
            return new BoardDto();

        IBoard board = _store.Game.GetBoard();

        BoardDto dto = new BoardDto
        {
            Size = board.Cells.GetLength(0)
        };

        for (int x = 0; x < dto.Size; x++)
        {
            for (int y = 0; y < dto.Size; y++)
            {
                ICell cell = board.Cells[x, y];

                dto.Cells.Add(new CellDto
                {
                    Position = new PositionDto { X = x, Y = y },
                    Piece = cell.Piece == null
                        ? null
                        : new PieceDto
                        {
                            Color = cell.Piece.Color,
                            Type = cell.Piece.Type
                        }
                });
            }
        }

        return dto;
    }

    private void game_PieceCaptured(object? sender, PieceCapturedEventArgs e)
    {
        lock (_lock)
        {
            string notification = $"Piece Captured! {e.CapturedPiece.Color} {e.CapturedPiece.Type} ({e.CapturedPosition.X + 1},{e.CapturedPosition.Y + 1}) was removed from the board.";
            _store.GameDto.Notifications.Add(notification);
        }
    }

    private void game_PiecePromoted(object? sender, PiecePromotedEventArgs e)
    {
        lock (_lock)
        {
            string notification = $"Piece Promoted! {e.PromotedPiece.Color} piece ({e.PromotedPosition.X + 1},{e.PromotedPosition.Y + 1}) has become a King!";
            _store.GameDto.Notifications.Add(notification);
        }
    }

    private void game_TurnChanged(object? sender, TurnChangedEventArgs e)
    {
        lock (_lock)
        {
            string notification = $"Turn switched to {e.CurrentPlayer.Name} ({e.CurrentPlayer.Color})";
            _store.GameDto.Notifications.Add(notification);
        }
    }
}
