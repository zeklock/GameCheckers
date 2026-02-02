using GameBase.Models;
using GameBase.Events;

namespace GameBase;

public class GameController
{
    public const int BoardSize = 8;
    private IBoard _board;
    private List<IPlayer> _players;
    private IPlayer _currPlayer;
    private IPlayer? _winner;
    private Dictionary<IPlayer, List<IPiece>> _playerPieces;
    private List<Position> _lastJumpPath;
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
        _currPlayer = players.First();
        _winner = null;
        _playerPieces = new Dictionary<IPlayer, List<IPiece>>();
        _lastJumpPath = new List<Position>();

        List<int> pieceBlackRow = new List<int> { 0, 1, 2 };
        List<int> pieceWhiteRow = new List<int> { 5, 6, 7 };

        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                Piece? piece;

                if (pieceBlackRow.Contains(y))
                {
                    piece = IsCellForPiece(x, y)
                        ? new Piece(PieceType.Man, Color.Black)
                        : null;
                }
                else if (pieceWhiteRow.Contains(y))
                {
                    piece = IsCellForPiece(x, y)
                        ? new Piece(PieceType.Man, Color.White)
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
        foreach (IPlayer p in _players)
        {
            List<IPiece> pieces = _board.Cells
                .Cast<Cell>()
                .Where(c => c.Piece != null && c.Piece.Color == p.Color)
                .Select(c => (IPiece)c.Piece!)
                .ToList();

            _playerPieces.Add(p, pieces);
        }
    }

    public void Start()
    {
        InitializePieces();
    }

    private static bool IsCellForPiece(int x, int y) => (x + y) % 2 != 0;

    public IBoard GetBoard() => _board;

    public IPlayer GetCurrentPlayer() => _currPlayer;

    public IPlayer? GetWinner() => _winner;

    public Dictionary<IPlayer, List<IPiece>> GetPlayerPieces() => _playerPieces;

    public void CheckWin()
    {
        var movablePieces = GetMovablePieces(_currPlayer);

        if (movablePieces.Count == 0)
        {
            IPlayer opponent = _players.First(p => p != _currPlayer);
            _winner = opponent;
            return;
        }

        foreach (var player in _players)
        {
            var playerPieces = _playerPieces[player];
            if (playerPieces.Count == 0)
            {
                IPlayer opponent = _players.First(p => p != player);
                _winner = opponent;
                return;
            }
        }
    }

    public List<(Piece piece, Position position)> GetMovablePieces(IPlayer player)
    {
        var result = new List<(Piece, Position)>();

        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                Piece? piece = _board.Cells[x, y].Piece;

                if (piece != null && piece.Color == player.Color && CanPieceMove(piece))
                {
                    result.Add((piece, _board.Cells[x, y].Position));
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Checks if a piece has any legal moves available.
    /// </summary>
    private bool CanPieceMove(Piece piece)
    {
        return GetLegalMoves(piece).Count > 0;
    }

    /// <summary>
    /// Gets all legal moves for a piece (normal moves and all jump sequences).
    /// </summary>
    public List<Position> GetLegalMoves(Piece piece)
    {
        var legalMoves = new List<Position>();

        Position? piecePos = GetPiecePosition(piece);
        if (piecePos == null)
            return new List<Position>();

        // Get normal moves (non-jump only)
        var normalMoves = GetNormalMoves(piece, piecePos.Value);
        legalMoves.AddRange(normalMoves);

        // Get all jump sequences (single and multi-jumps)
        var jumpMoves = GetJumpMoves(piece, piecePos.Value);
        legalMoves.AddRange(jumpMoves);

        return legalMoves;
    }

    /// <summary>
    /// Gets normal (non-jump) moves from a position.
    /// </summary>
    private List<Position> GetNormalMoves(Piece piece, Position position)
    {
        var moves = new List<Position>();
        var directions = GetMovementDirections(piece);

        foreach (var direction in directions)
        {
            var newPos = new Position(position.X + direction.Move.X, position.Y + direction.Move.Y);

            if (IsInsideBoard(newPos) && _board.Cells[newPos.X, newPos.Y].Piece == null)
            {
                moves.Add(newPos);
            }
        }

        return moves;
    }

    /// <summary>
    /// Gets the board position of a piece.
    /// </summary>
    private Position? GetPiecePosition(Piece piece)
    {
        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                if (_board.Cells[x, y].Piece == piece)
                {
                    return _board.Cells[x, y].Position;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the valid movement directions for a piece based on its type and color.
    /// Kings can move in all diagonal directions, Men only forward.
    /// </summary>
    private List<Direction> GetMovementDirections(Piece piece)
    {
        if (piece.Type == PieceType.King)
        {
            return new List<Direction>
            {
                Direction.TopLeft,
                Direction.TopRight,
                Direction.BottomLeft,
                Direction.BottomRight,
            };
        }
        else if (piece.Color == Color.Black)
        {
            return new List<Direction>
            {
                Direction.BottomLeft,
                Direction.BottomRight,
            };
        }
        else
        {
            return new List<Direction>
            {
                Direction.TopLeft,
                Direction.TopRight
            };
        }
    }

    /// <summary>
    /// Checks if a position is within board boundaries.
    /// </summary>
    public static bool IsInsideBoard(Position position)
    {
        return position.X >= 0 && position.X < BoardSize && position.Y >= 0 && position.Y < BoardSize;
    }

    /// <summary>
    /// Gets all possible multi-jump sequences from a position.
    /// </summary>
    private List<Position> GetJumpMoves(Piece piece, Position position)
    {
        var jumps = new List<Position>();
        ExploreJumps(piece, position, jumps, new HashSet<string>());
        return jumps;
    }

    /// <summary>
    /// Recursively explores all possible jump sequences from a position.
    /// Simulates moves on the board to find valid multi-jump paths.
    /// </summary>
    public void ExploreJumps(Piece piece, Position currentPos, List<Position> result, HashSet<string> visited)
    {
        List<Direction> directions = new List<Direction>
        {
            Direction.TopLeft,
            Direction.TopRight,
            Direction.BottomLeft,
            Direction.BottomRight
        };

        foreach (Direction direction in directions)
        {
            Position capturePos = new Position(currentPos.X + direction.Move.X, currentPos.Y + direction.Move.Y);
            Position landPos = new Position(currentPos.X + direction.Jump.X, currentPos.Y + direction.Jump.Y);

            if (!IsInsideBoard(landPos))
                continue;

            Piece? capturedPiece = _board.Cells[capturePos.X, capturePos.Y].Piece;
            Piece? landPiece = _board.Cells[landPos.X, landPos.Y].Piece;

            if (capturedPiece != null && capturedPiece.Color != piece.Color && landPiece == null)
            {
                string visitKey = $"{landPos.X},{landPos.Y}";

                if (!visited.Contains(visitKey))
                {
                    visited.Add(visitKey);
                    result.Add(landPos);

                    Piece? temp = _board.Cells[capturePos.X, capturePos.Y].Piece;
                    _board.Cells[capturePos.X, capturePos.Y].Piece = null;
                    _board.Cells[currentPos.X, currentPos.Y].Piece = null;
                    _board.Cells[landPos.X, landPos.Y].Piece = piece;

                    ExploreJumps(piece, landPos, result, visited);

                    _board.Cells[currentPos.X, currentPos.Y].Piece = piece;
                    _board.Cells[landPos.X, landPos.Y].Piece = null;
                    _board.Cells[capturePos.X, capturePos.Y].Piece = temp;

                    visited.Remove(visitKey);
                }
            }
        }
    }

    /// <summary>
    /// Moves a piece from its current position to a new position.
    /// Handles both normal moves and jump captures.
    /// </summary>
    public void MovePiece(Piece piece, Position to)
    {
        Position? from = GetPiecePosition(piece);

        if (from == null || !IsInsideBoard(to) || _board.Cells[to.X, to.Y].Piece != null)
        {
            return;
        }

        int deltaX = Math.Abs(to.X - from.Value.X);
        int deltaY = Math.Abs(to.Y - from.Value.Y);

        // Normal move (one diagonal square)
        if (deltaX == 1 && deltaY == 1)
        {
            if (IsValidNormalMove(piece, from.Value, to))
            {
                _board.Cells[from.Value.X, from.Value.Y].Piece = null;
                _board.Cells[to.X, to.Y].Piece = piece;
                _lastJumpPath.Clear();
                CheckPromotion(piece, to);
                SwitchPlayer();
            }
        }
        // Jump move (two diagonal squares)
        else if (deltaX == 2 && deltaY == 2)
        {
            Position capturedPos = GetCapturedPiecePosition(from.Value, to);
            Piece? capturedPiece = _board.Cells[capturedPos.X, capturedPos.Y].Piece;

            if (capturedPiece != null && capturedPiece.Color != piece.Color)
            {
                _board.Cells[from.Value.X, from.Value.Y].Piece = null;
                _board.Cells[to.X, to.Y].Piece = piece;
                RemovePiece(capturedPiece);
                OnPieceCaptured(new PieceCapturedEventArgs(capturedPiece));

                _lastJumpPath.Add(to);

                CheckPromotion(piece, to);

                // Check for additional jumps
                if (!HasAdditionalJumps(piece, to))
                {
                    _lastJumpPath.Clear();
                    SwitchPlayer();
                }
            }
        }
    }

    /// <summary>
    /// Checks if a piece has additional multi-jump moves available from current position.
    /// </summary>
    private bool HasAdditionalJumps(Piece piece, Position position)
    {
        return GetJumpMoves(piece, position).Count > 0;
    }

    /// <summary>
    /// Validates if a normal (non-jump) move is legal for a piece.
    /// </summary>
    private bool IsValidNormalMove(Piece piece, Position from, Position to)
    {
        int deltaX = to.X - from.X;
        int deltaY = to.Y - from.Y;

        // Check if move is diagonal
        if (Math.Abs(deltaX) != 1 || Math.Abs(deltaY) != 1)
            return false;

        // Man pieces can only move forward
        if (piece.Type == PieceType.Man)
        {
            if (piece.Color == Color.Black && deltaY <= 0)
                return false;

            if (piece.Color == Color.White && deltaY >= 0)
                return false;
        }

        // Check if any jumps are available (jumps are forced)
        if (HasAvailableJumps(piece, from))
            return false;

        return true;
    }

    /// <summary>
    /// Checks if a piece has any immediate single-jump moves available.
    /// </summary>
    private bool HasAvailableJumps(Piece piece, Position position)
    {
        return GetJumpMoves(piece, position).Count > 0;
    }

    /// <summary>
    /// Gets the position of a captured piece between two diagonal positions.
    /// </summary>
    private Position GetCapturedPiecePosition(Position from, Position to)
    {
        return new Position((from.X + to.X) / 2, (from.Y + to.Y) / 2);
    }

    /// <summary>
    /// Checks if a piece should be promoted to a king.
    /// </summary>
    private void CheckPromotion(Piece piece, Position position)
    {
        if (piece.Type == PieceType.King)
            return;

        if (piece.Color == Color.Black && position.Y == BoardSize - 1)
        {
            piece.Type = PieceType.King;
            OnPiecePromoted(new PiecePromotedEventArgs(piece));
        }
        else if (piece.Color == Color.White && position.Y == 0)
        {
            piece.Type = PieceType.King;
            OnPiecePromoted(new PiecePromotedEventArgs(piece));
        }
    }

    /// <summary>
    /// Removes a piece from the board and from the player's piece list.
    /// </summary>
    public void RemovePiece(Piece piece)
    {
        Position? pos = GetPiecePosition(piece);
        if (pos != null)
        {
            _board.Cells[pos.Value.X, pos.Value.Y].Piece = null;
        }

        // Remove from player pieces
        foreach (var player in _playerPieces)
        {
            player.Value.Remove(piece);
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
