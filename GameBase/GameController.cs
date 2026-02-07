using System;
using System.Collections.Generic;
using System.Linq;
using GameBase.Models;
using GameBase.Events;
using GameBase.Interfaces;
using GameBase.Dtos;

namespace GameBase;

public class GameController
{
    public const int BoardSize = 8;
    private IBoard _board;
    private List<IPlayer> _players;
    private IPlayer _currPlayer;
    private IPlayer? _winner;
    private Dictionary<IPlayer, List<IPiece>> _playerPieces;
    public event EventHandler<PieceCapturedEventArgs>? PieceCaptured;
    public event EventHandler<PiecePromotedEventArgs>? PiecePromoted;
    public event EventHandler<TurnChangedEventArgs>? TurnChanged;

    protected virtual void OnPieceCaptured(PieceCapturedEventArgs e)
    {
        PieceCaptured?.Invoke(this, e);
    }

    protected virtual void OnPiecePromoted(PiecePromotedEventArgs e)
    {
        PiecePromoted?.Invoke(this, e);
    }

    protected virtual void OnTurnChanged(TurnChangedEventArgs e)
    {
        TurnChanged?.Invoke(this, e);
    }

    public GameController(IBoard board, List<IPlayer> players)
    {
        _board = board;
        _players = players;
        _currPlayer = players.FirstOrDefault(p => p.Color == Color.Black) ?? players[0];
        _winner = null;
        _playerPieces = new Dictionary<IPlayer, List<IPiece>>();
    }

    public void PlacePiece()
    {
        List<int> pieceTopRow = new List<int> { 0, 1, 2 };
        List<int> pieceBottomRow = new List<int> { 5, 6, 7 };

        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                IPiece? piece;

                if (pieceBottomRow.Contains(y))
                {
                    piece = IsCellForPiece(x, y)
                        ? new Piece(PieceType.Man, _players.First().Color)
                        : null;
                }
                else if (pieceTopRow.Contains(y))
                {
                    piece = IsCellForPiece(x, y)
                        ? new Piece(PieceType.Man, _players.Last().Color)
                        : null;
                }
                else
                {
                    piece = null;
                }

                _board.Cells[x, y] = new Cell(
                    new Position(x, y),
                    piece
                );
            }
        }
    }

    public void InitializePieces()
    {
        PlacePiece();

        foreach (IPlayer p in _players)
        {
            List<IPiece> pieces = _board.Cells
                .Cast<ICell>()
                .Where(c => c.Piece != null && c.Piece.Color == p.Color)
                .Select(c => c.Piece!)
                .ToList();

            _playerPieces.Add(p, pieces);
        }
    }

    public void Start()
    {
        InitializePieces();
    }

    public static bool IsCellForPiece(int x, int y) => (x + y) % 2 != 0;

    public IBoard GetBoard() => _board;

    public IPlayer GetCurrentPlayer() => _currPlayer;

    public IPlayer? GetWinner() => _winner;

    public Dictionary<IPlayer, List<IPiece>> GetPlayerPieces() => _playerPieces;

    public void CheckWin()
    {
        List<MovablePieceDto> movablePieces = GetMovablePieces(_currPlayer);

        if (movablePieces.Count == 0)
        {
            IPlayer opponent = _players.First(p => p != _currPlayer);
            _winner = opponent;
            return;
        }

        foreach (IPlayer player in _players)
        {
            List<IPiece> playerPieces = _playerPieces[player];

            if (playerPieces.Count == 0)
            {
                IPlayer opponent = _players.First(p => p != player);
                _winner = opponent;
                return;
            }
        }
    }

    public List<MovablePieceDto> GetMovablePieces(IPlayer player)
    {
        List<MovablePieceDto> jumpPieceMoves = new List<MovablePieceDto>();
        List<MovablePieceDto> normalPieceMoves = new List<MovablePieceDto>();

        for (int x = 0; x < BoardSize; x++)
        {
            for (int y = 0; y < BoardSize; y++)
            {
                Position position = new Position(x, y);
                IPiece? piece = GetPiece(position);

                if (piece == null || piece.Color != player.Color)
                    continue;

                List<List<Position>> jumpPaths = GetJumpPaths(piece, position);
                List<Position> normalMoves = GetNormalMoves(piece, position);

                if (jumpPaths.Count > 0)
                    jumpPieceMoves.Add(new MovablePieceDto { Piece = piece, Position = position, JumpLen = jumpPaths.Max(p => p.Count) });
                else if (normalMoves.Count > 0)
                    normalPieceMoves.Add(new MovablePieceDto { Piece = piece, Position = position });
            }
        }

        if (jumpPieceMoves.Count > 0)
        {
            int maxJumpPiece = jumpPieceMoves.Max(j => j.JumpLen);
            List<MovablePieceDto> maxJumpPieceMoves = jumpPieceMoves
                .Where(j => j.JumpLen == maxJumpPiece)
                .ToList();

            return maxJumpPieceMoves;
        }

        return normalPieceMoves;
    }

    /// <summary>
    /// Gets all legal moves for a piece (normal moves and all jump sequences).
    /// Returns List of Paths (multi-jump possible)
    /// </summary>
    public List<List<Position>> GetLegalMoves(IPiece piece)
    {
        List<List<Position>> legalMoves = new List<List<Position>>();
        Position? startPosition = GetPiecePosition(piece);

        if (startPosition == null)
            return legalMoves;

        List<List<Position>> jumpPaths = GetJumpPaths(piece, startPosition.Value);

        // FORCED JUMP
        if (jumpPaths.Count > 0)
        {
            int maxJumpPath = jumpPaths.Max(p => p.Count);
            List<List<Position>> jumpMoves = jumpPaths.Where(p => p.Count == maxJumpPath).ToList();
            return jumpMoves;
        }

        List<List<Position>> normalMoves = GetNormalMoves(piece, startPosition.Value)
            .Select(p => new List<Position> { p })
            .ToList();

        return normalMoves;
    }

    public List<List<Position>> GetJumpPaths(IPiece piece, Position start)
    {
        List<List<Position>> result = new List<List<Position>>();
        ExploreJumps(piece, start, new List<Position>(), result, new HashSet<Position>(), new HashSet<Position>());
        return result;
    }

    public void ExploreJumps(IPiece piece, Position currentPos, List<Position> path, List<List<Position>> result, HashSet<Position> visited, HashSet<Position> captured)
    {
        bool jumped = false;
        IEnumerable<Direction> directions = GetMovementDirections(piece);

        foreach (Direction dir in directions)
        {
            Position capturePos = new Position(currentPos.X + dir.Move.X, currentPos.Y + dir.Move.Y);
            Position landPos = new Position(currentPos.X + dir.Jump.X, currentPos.Y + dir.Jump.Y);

            if (!IsInside(capturePos) || !IsInside(landPos))
                continue;

            IPiece? capturePiece = GetPiece(capturePos);
            IPiece? landPiece = GetPiece(landPos);

            // ignore already-captured pieces
            if (capturePiece == null || capturePiece.Color == piece.Color)
                continue;

            if (captured.Contains(capturePos))
                continue;

            // Treat the moving piece as not blocking landing squares (it will be moved during simulation)
            if (landPiece != null && landPiece == piece)
                landPiece = null;

            if (landPiece != null || visited.Contains(landPos))
                continue;

            // simulate path locally: record landing and captured positions
            path.Add(landPos);
            visited.Add(landPos);
            captured.Add(capturePos);
            jumped = true;

            ExploreJumps(piece, landPos, path, result, visited, captured);

            path.RemoveAt(path.Count - 1);
            visited.Remove(landPos);
            captured.Remove(capturePos);
        }

        if (!jumped && path.Count > 0)
            result.Add(new List<Position>(path));
    }

    /// <summary>
    /// Gets normal (non-jump) moves from a position.
    /// </summary>
    public List<Position> GetNormalMoves(IPiece piece, Position position)
    {
        List<Position> moves = new List<Position>();
        IEnumerable<Direction> directions = GetMovementDirections(piece);

        foreach (Direction direction in directions)
        {
            Position newPosition = new Position(position.X + direction.Move.X, position.Y + direction.Move.Y);

            if (IsInside(newPosition) && _board.Cells[newPosition.X, newPosition.Y].Piece == null)
            {
                moves.Add(newPosition);
            }
        }

        return moves;
    }

    /// <summary>
    /// Gets the board position of a piece.
    /// </summary>
    public Position? GetPiecePosition(IPiece piece)
    {
        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                ICell cell = _board.Cells[x, y];

                if (cell.Piece == piece)
                {
                    Position piecePosition = cell.Position;
                    return piecePosition;
                }
            }
        }

        return null;
    }

    public IPiece? GetPiece(Position position)
    {
        return _board.Cells[position.X, position.Y].Piece;
    }

    /// <summary>
    /// Gets the valid movement directions for a piece based on its type and color.
    /// Player1 (at bottom, _players[0]) moves up. Player2 (at top, _players[1]) moves down.
    /// Kings can move in all diagonal directions, Men only forward.
    /// </summary>
    public List<Direction> GetMovementDirections(IPiece piece)
    {
        List<Direction> directions = new List<Direction>();

        if (piece.Type == PieceType.King)
        {
            directions.AddRange(new List<Direction>
            {
                Direction.TopLeft,
                Direction.TopRight,
                Direction.BottomLeft,
                Direction.BottomRight
            });
        }
        else if (piece.Color == _players.First().Color)
        {
            // Player1 at bottom moves up (toward Y=0)
            directions.AddRange(new List<Direction>
            {
                Direction.TopLeft,
                Direction.TopRight
            });
        }
        else if (piece.Color == _players.Last().Color)
        {
            // Player2 at top moves down (toward Y=7)
            directions.AddRange(new List<Direction>
            {
                Direction.BottomLeft,
                Direction.BottomRight
            });
        }

        return directions;
    }

    /// <summary>
    /// Checks if a position is within board boundaries.
    /// </summary>
    public static bool IsInside(Position position)
    {
        return position.X >= 0 && position.X < BoardSize && position.Y >= 0 && position.Y < BoardSize;
    }

    /// <summary>
    /// Moves a piece from its current position to a new position.
    /// Handles both normal moves and jump captures.
    /// </summary>
    public void MovePiece(IPiece piece, Position to)
    {
        MovePiece(piece, new List<Position> { to });
    }

    /// <summary>
    /// Moves a piece following a sequence of positions (multi-jump moves).
    /// Validates piece ownership, existence, and path legality before executing moves.
    /// </summary>
    public void MovePiece(IPiece piece, List<Position> path)
    {
        if (path == null || path.Count == 0)
            return;

        // Validate piece exists on board
        Position? piecePosition = GetPiecePosition(piece);

        if (piecePosition == null)
            return;

        // Validate piece belongs to current player
        if (piece.Color != _currPlayer.Color)
            return;

        // Validate path is legal for this piece
        List<List<Position>> legalMoves = GetLegalMoves(piece);

        if (!legalMoves.Any(p => p.SequenceEqual(path)))
            return;

        int deltaMove = 1;
        int deltaJump = 2;

        for (int i = 0; i < path.Count; i++)
        {
            Position pathTo = path[i];
            Position? pathFrom = GetPiecePosition(piece);
            IPiece? pieceTo = GetPiece(pathTo);

            if (pathFrom == null || !IsInside(pathTo) || pieceTo != null)
                return;

            int deltaX = Math.Abs(pathTo.X - pathFrom.Value.X);
            int deltaY = Math.Abs(pathTo.Y - pathFrom.Value.Y);

            // Normal move (one diagonal square) - only allowed if it's the only step
            if (deltaX == deltaMove && deltaY == deltaMove)
            {
                if (!IsValidNormalMove(piece, pathFrom.Value, pathTo))
                    return;

                if (path.Count > 1)
                    return; // invalid: normal move cannot be part of multi-step path

                _board.Cells[pathFrom.Value.X, pathFrom.Value.Y].Piece = null;
                _board.Cells[pathTo.X, pathTo.Y].Piece = piece;
                CheckPromotion(piece, pathTo);
                SwitchPlayer();
                return;
            }
            // Jump move (two diagonal squares)
            else if (deltaX == deltaJump && deltaY == deltaJump)
            {
                Position capturedPosition = GetCapturedPiecePosition(pathFrom.Value, pathTo);

                if (!IsInside(capturedPosition))
                    return;

                IPiece? capturedPiece = GetPiece(capturedPosition);

                if (capturedPiece == null || capturedPiece.Color == piece.Color)
                    return;

                _board.Cells[pathFrom.Value.X, pathFrom.Value.Y].Piece = null;
                _board.Cells[pathTo.X, pathTo.Y].Piece = piece;
                RemovePiece(capturedPiece);
                OnPieceCaptured(new PieceCapturedEventArgs(capturedPiece, capturedPosition));

                // If this is the last step in the provided path, finish turn
                if (i == path.Count - 1)
                {
                    CheckPromotion(piece, pathTo);
                    SwitchPlayer();
                    return;
                }

                // otherwise continue to next step in the path
                continue;
            }
            else
            {
                return; // invalid step
            }
        }
    }

    /// <summary>
    /// Validates if a normal (non-jump) move is legal for a piece.
    /// Player1 at bottom moves up (Y decreases). Player2 at top moves down (Y increases).
    /// </summary>
    public bool IsValidNormalMove(IPiece piece, Position from, Position to)
    {
        int deltaX = to.X - from.X;
        int deltaY = to.Y - from.Y;

        // Check if move is diagonal
        if (Math.Abs(deltaX) != 1 || Math.Abs(deltaY) != 1)
            return false;

        // Man pieces can only move forward
        if (piece.Type == PieceType.Man)
        {
            // Player1 (bottom, _players[0]) must move up (Y decreases, deltaY < 0)
            if (piece.Color == _players.First().Color && deltaY >= 0)
                return false;

            // Player2 (top, _players[1]) must move down (Y increases, deltaY > 0)
            if (piece.Color == _players.Last().Color && deltaY <= 0)
                return false;
        }

        // Check if any jumps are available (jumps are forced)
        if (PlayerHasAnyJump(_currPlayer))
            return false;

        return true;
    }

    public bool PlayerHasAnyJump(IPlayer player)
    {
        for (int x = 0; x < BoardSize; x++)
        {
            for (int y = 0; y < BoardSize; y++)
            {
                Position position = new Position(x, y);
                IPiece? piece = GetPiece(position);

                if (piece != null && piece.Color == player.Color)
                {
                    List<List<Position>> jumpPaths = GetJumpPaths(piece, position);

                    if (jumpPaths.Count > 0)
                        return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Gets the position of a captured piece between two diagonal positions.
    /// </summary>
    public Position GetCapturedPiecePosition(Position from, Position to)
    {
        return new Position((from.X + to.X) / 2, (from.Y + to.Y) / 2);
    }

    /// <summary>
    /// Checks if a piece should be promoted to a king.
    /// Player1 (at bottom) promotes when reaching row 0 (top).
    /// Player2 (at top) promotes when reaching row 7 (bottom).
    /// </summary>
    public void CheckPromotion(IPiece piece, Position position)
    {
        if (piece.Type == PieceType.King)
            return;

        // Player1 promotes at top row
        if (piece.Color == _players.First().Color && position.Y == 0)
        {
            Promote(piece, position);
        }
        // Player2 promotes at bottom row
        else if (piece.Color == _players.Last().Color && position.Y == BoardSize - 1)
        {
            Promote(piece, position);
        }
    }

    public void Promote(IPiece piece, Position position)
    {
        piece.Type = PieceType.King;
        OnPiecePromoted(new PiecePromotedEventArgs(piece, position));
    }

    /// <summary>
    /// Removes a piece from the board and from the player's piece list.
    /// </summary>
    public void RemovePiece(IPiece piece)
    {
        Position? position = GetPiecePosition(piece);

        if (position != null)
        {
            _board.Cells[position.Value.X, position.Value.Y].Piece = null;
        }

        // Remove from player pieces
        foreach (KeyValuePair<IPlayer, List<IPiece>> playerPiece in _playerPieces)
        {
            playerPiece.Value.Remove(piece);
        }
    }

    /// <summary>
    /// Switches to the next player's turn.
    /// </summary>
    public void SwitchPlayer()
    {
        int currentIndex = _players.IndexOf(_currPlayer);
        _currPlayer = _players[(currentIndex + 1) % _players.Count];
        OnTurnChanged(new TurnChangedEventArgs(_currPlayer));
    }
}
