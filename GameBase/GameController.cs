using GameBase.Models;
using GameBase.Events;
using GameBase.Interfaces;

namespace GameBase;

public class GameController
{
    public const int BoardSize = 8;
    private IBoard _board;
    private IList<IPlayer> _players;
    private IPlayer _currPlayer;
    private IPlayer? _winner;
    private IDictionary<IPlayer, IList<IPiece>> _playerPieces;
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

    public GameController(IBoard board, IList<IPlayer> players)
    {
        _board = board;
        _players = players;
        _currPlayer = players.FirstOrDefault(p => p.Color == Color.Black) ?? players[0];
        _winner = null;
        _playerPieces = new Dictionary<IPlayer, IList<IPiece>>();
    }

    public void PlacePiece()
    {
        IList<int> pieceTopRow = new List<int> { 0, 1, 2 };
        IList<int> pieceBottomRow = new List<int> { 5, 6, 7 };

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
            IList<IPiece> pieces = _board.Cells
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

    public IDictionary<IPlayer, IList<IPiece>> GetPlayerPieces() => _playerPieces;

    public void CheckWin()
    {
        IList<(IPiece piece, Position position)> movablePieces = GetMovablePieces(_currPlayer);

        if (movablePieces.Count == 0)
        {
            IPlayer opponent = _players.First(p => p != _currPlayer);
            _winner = opponent;
            return;
        }

        foreach (IPlayer player in _players)
        {
            IList<IPiece> playerPieces = _playerPieces[player];

            if (playerPieces.Count == 0)
            {
                IPlayer opponent = _players.First(p => p != player);
                _winner = opponent;
                return;
            }
        }
    }

    public IList<(IPiece piece, Position position)> GetMovablePieces(IPlayer player)
    {
        IList<(IPiece piece, Position pos, int jumpLen)> jumpCandidates = new List<(IPiece piece, Position pos, int jumpLen)>();
        IList<(IPiece, Position)> normals = new List<(IPiece, Position)>();

        for (int x = 0; x < BoardSize; x++)
        {
            for (int y = 0; y < BoardSize; y++)
            {
                IPiece? piece = _board.Cells[x, y].Piece;

                if (piece == null || piece.Color != player.Color)
                    continue;

                Position position = _board.Cells[x, y].Position;
                IList<IList<Position>> jumps = GetJumpPaths(piece, position);

                if (jumps.Count > 0)
                    jumpCandidates.Add((piece, position, jumps.Max(p => p.Count)));
                else if (GetNormalMoves(piece, position).Count() > 0)
                    normals.Add((piece, position));
            }
        }

        if (jumpCandidates.Count > 0)
        {
            int max = jumpCandidates.Max(j => j.jumpLen);
            return jumpCandidates
                .Where(j => j.jumpLen == max)
                .Select(j => (j.piece, j.pos))
                .ToList();
        }

        return normals;
    }

    /// <summary>
    /// Gets all legal moves for a piece (normal moves and all jump sequences).
    /// Returns List of Paths (multi-jump possible)
    /// </summary>
    public IList<IList<Position>> GetLegalMoves(IPiece piece)
    {
        Position? start = GetPiecePosition(piece);
        if (start == null)
            return new List<IList<Position>>();

        IList<IList<Position>> jumpPaths = GetJumpPaths(piece, start.Value);

        // FORCED JUMP
        if (jumpPaths.Count > 0)
        {
            int max = jumpPaths.Max(p => p.Count);
            return jumpPaths.Where(p => p.Count == max).ToList();
        }

        IList<IList<Position>> legalMoves = GetNormalMoves(piece, start.Value)
            .Select(p => (IList<Position>)new List<Position> { p })
            .ToList();

        return legalMoves;
    }

    public IList<IList<Position>> GetJumpPaths(IPiece piece, Position start)
    {
        IList<IList<Position>> result = new List<IList<Position>>();
        ExploreJumps(piece, start, new List<Position>(), result, new HashSet<Position>());
        return result;
    }

    public void ExploreJumps(IPiece piece, Position currentPos, IList<Position> path, IList<IList<Position>> result, HashSet<Position> visited)
    {
        bool jumped = false;
        IEnumerable<Direction> directions = GetMovementDirections(piece);

        foreach (Direction dir in directions)
        {
            Position capturePos = new Position(currentPos.X + dir.Move.X, currentPos.Y + dir.Move.Y);
            Position landPos = new Position(currentPos.X + dir.Jump.X, currentPos.Y + dir.Jump.Y);

            if (!IsInside(capturePos) || !IsInside(landPos))
                continue;

            IPiece? enemyPiece = GetPiece(capturePos);

            if (enemyPiece == null || enemyPiece.Color == piece.Color)
                continue;

            if (_board.Cells[landPos.X, landPos.Y].Piece != null || visited.Contains(landPos))
                continue;

            // simulate path locally
            path.Add(landPos);
            visited.Add(landPos);
            jumped = true;

            ExploreJumps(piece, landPos, path, result, visited);

            path.RemoveAt(path.Count - 1);
            visited.Remove(landPos);
        }

        if (!jumped && path.Count > 0)
            result.Add(new List<Position>(path));
    }

    /// <summary>
    /// Gets normal (non-jump) moves from a position.
    /// </summary>
    public IList<Position> GetNormalMoves(IPiece piece, Position position)
    {
        IList<Position> moves = new List<Position>();
        IEnumerable<Direction> directions = GetMovementDirections(piece);

        foreach (Direction direction in directions)
        {
            Position newPos = new Position(position.X + direction.Move.X, position.Y + direction.Move.Y);

            if (IsInside(newPos) && _board.Cells[newPos.X, newPos.Y].Piece == null)
            {
                moves.Add(newPos);
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
                if (_board.Cells[x, y].Piece == piece)
                {
                    return _board.Cells[x, y].Position;
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
    public IEnumerable<Direction> GetMovementDirections(IPiece piece)
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
        else if (piece.Color == _players.First().Color)
        {
            // Player1 at bottom moves up (toward Y=0)
            return new List<Direction>
            {
                Direction.TopLeft,
                Direction.TopRight
            };
        }
        else if (piece.Color == _players.Last().Color)
        {
            // Player2 at top moves down (toward Y=7)
            return new List<Direction>
            {
                Direction.BottomLeft,
                Direction.BottomRight,
            };
        }
        else
        {
            return new List<Direction>();
        }
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
    public void MovePiece(IPiece piece, IList<Position> path)
    {
        if (path == null || path.Count == 0)
            return;

        // Validate piece exists on board
        if (GetPiecePosition(piece) == null)
            return;

        // Validate piece belongs to current player
        if (piece.Color != _currPlayer.Color)
            return;

        // Validate path is legal for this piece
        IList<IList<Position>> legalPaths = GetLegalMoves(piece);
        if (!legalPaths.Any(p => p.SequenceEqual(path)))
            return;

        for (int i = 0; i < path.Count; i++)
        {
            Position to = path[i];
            Position? from = GetPiecePosition(piece);

            if (from == null || !IsInside(to) || _board.Cells[to.X, to.Y].Piece != null)
                return;

            int deltaX = Math.Abs(to.X - from.Value.X);
            int deltaY = Math.Abs(to.Y - from.Value.Y);

            // Normal move (one diagonal square) - only allowed if it's the only step
            if (deltaX == 1 && deltaY == 1)
            {
                if (!IsValidNormalMove(piece, from.Value, to))
                    return;

                if (path.Count > 1)
                    return; // invalid: normal move cannot be part of multi-step path

                _board.Cells[from.Value.X, from.Value.Y].Piece = null;
                _board.Cells[to.X, to.Y].Piece = piece;
                CheckPromotion(piece, to);
                SwitchPlayer();
                return;
            }
            // Jump move (two diagonal squares)
            else if (deltaX == 2 && deltaY == 2)
            {
                Position capturedPos = GetCapturedPiecePosition(from.Value, to);

                if (!IsInside(capturedPos))
                    return;

                IPiece? capturedPiece = _board.Cells[capturedPos.X, capturedPos.Y].Piece;
                Position capturedPosition = _board.Cells[capturedPos.X, capturedPos.Y].Position;

                if (capturedPiece == null || capturedPiece.Color == piece.Color)
                    return;

                _board.Cells[from.Value.X, from.Value.Y].Piece = null;
                _board.Cells[to.X, to.Y].Piece = piece;
                RemovePiece(capturedPiece);
                OnPieceCaptured(new PieceCapturedEventArgs(capturedPiece, capturedPosition));

                // If this is the last step in the provided path, finish turn
                if (i == path.Count - 1)
                {
                    CheckPromotion(piece, to);
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
                IPiece? piece = _board.Cells[x, y].Piece;

                if (piece != null && piece.Color == player.Color)
                {
                    if (GetJumpPaths(piece, _board.Cells[x, y].Position).Count > 0)
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
        foreach (KeyValuePair<IPlayer, IList<IPiece>> playerPiece in _playerPieces)
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
