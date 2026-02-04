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

        List<int> pieceTopRow = new List<int> { 0, 1, 2 };
        List<int> pieceBottomRow = new List<int> { 5, 6, 7 };

        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                Piece? piece;

                if (pieceBottomRow.Contains(y))
                {
                    piece = IsCellForPiece(x, y)
                        ? new Piece(PieceType.Man, _players[0].Color)
                        : null;
                }
                else if (pieceTopRow.Contains(y))
                {
                    piece = IsCellForPiece(x, y)
                        ? new Piece(PieceType.Man, _players[1].Color)
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

    public static bool IsCellForPiece(int x, int y) => (x + y) % 2 != 0;

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
        var jumpCandidates = new List<(Piece piece, Position pos, int jumpLen)>();
        var normals = new List<(Piece, Position)>();

        for (int x = 0; x < BoardSize; x++)
            for (int y = 0; y < BoardSize; y++)
            {
                var piece = _board.Cells[x, y].Piece;
                if (piece == null || piece.Color != player.Color)
                    continue;

                var pos = _board.Cells[x, y].Position;
                var jumps = GetJumpPaths(piece, pos);

                if (jumps.Count > 0)
                    jumpCandidates.Add((piece, pos, jumps.Max(p => p.Count)));
                else if (GetNormalMoves(piece, pos).Count > 0)
                    normals.Add((piece, pos));
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
    public List<List<Position>> GetLegalMoves(Piece piece)
    {
        Position? start = GetPiecePosition(piece);
        if (start == null)
            return new List<List<Position>>();

        var jumpPaths = GetJumpPaths(piece, start.Value);

        // FORCED JUMP
        if (jumpPaths.Count > 0)
        {
            int max = jumpPaths.Max(p => p.Count);
            return jumpPaths.Where(p => p.Count == max).ToList();
        }

        return GetNormalMoves(piece, start.Value)
            .Select(p => new List<Position> { p })
            .ToList();
    }

    private List<List<Position>> GetJumpPaths(Piece piece, Position start)
    {
        var result = new List<List<Position>>();
        ExploreJumpPaths(piece, start, new List<Position>(), result, new HashSet<Position>());
        return result;
    }

    private void ExploreJumpPaths(Piece piece, Position currentPos, List<Position> path, List<List<Position>> result, HashSet<Position> visited)
    {
        bool jumped = false;

        foreach (var dir in GetMovementDirections(piece))
        {
            Position capture = new Position(currentPos.X + dir.Move.X, currentPos.Y + dir.Move.Y);
            Position land = new Position(currentPos.X + dir.Jump.X, currentPos.Y + dir.Jump.Y);

            if (!IsInsideBoard(capture) || !IsInsideBoard(land))
                continue;

            Piece? enemy = _board.Cells[capture.X, capture.Y].Piece;

            if (enemy == null || enemy.Color == piece.Color)
                continue;

            if (_board.Cells[land.X, land.Y].Piece != null || visited.Contains(land))
                continue;

            // simulate path locally
            path.Add(land);
            visited.Add(land);
            jumped = true;

            ExploreJumpPaths(piece, land, path, result, visited);

            path.RemoveAt(path.Count - 1);
            visited.Remove(land);
        }

        if (!jumped && path.Count > 0)
            result.Add(new List<Position>(path));
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
    /// Player1 (at bottom, _players[0]) moves up. Player2 (at top, _players[1]) moves down.
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
        else if (piece.Color == _players[0].Color)
        {
            // Player1 at bottom moves up (toward Y=0)
            return new List<Direction>
            {
                Direction.TopLeft,
                Direction.TopRight
            };
        }
        else if (piece.Color == _players[1].Color)
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
    public static bool IsInsideBoard(Position position)
    {
        return position.X >= 0 && position.X < BoardSize && position.Y >= 0 && position.Y < BoardSize;
    }

    /// <summary>
    /// Moves a piece from its current position to a new position.
    /// Handles both normal moves and jump captures.
    /// </summary>
    public void MovePiece(Piece piece, Position to)
    {
        MovePiece(piece, new List<Position> { to });
    }

    /// <summary>
    /// Moves a piece following a sequence of positions (multi-jump moves).
    /// Validates piece ownership, existence, and path legality before executing moves.
    /// </summary>
    public void MovePiece(Piece piece, List<Position> path)
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
        var legalPaths = GetLegalMoves(piece);
        if (!legalPaths.Any(p => p.SequenceEqual(path)))
            return;

        for (int i = 0; i < path.Count; i++)
        {
            Position to = path[i];
            Position? from = GetPiecePosition(piece);

            if (from == null || !IsInsideBoard(to) || _board.Cells[to.X, to.Y].Piece != null)
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

                if (!IsInsideBoard(capturedPos))
                    return;

                Piece? capturedPiece = _board.Cells[capturedPos.X, capturedPos.Y].Piece;
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
            // Player1 (bottom, _players[0]) must move up (Y decreases, deltaY < 0)
            if (piece.Color == _players[0].Color && deltaY >= 0)
                return false;

            // Player2 (top, _players[1]) must move down (Y increases, deltaY > 0)
            if (piece.Color == _players[1].Color && deltaY <= 0)
                return false;
        }

        // Check if any jumps are available (jumps are forced)
        if (PlayerHasAnyJump(_currPlayer))
            return false;

        return true;
    }

    private bool PlayerHasAnyJump(IPlayer player)
    {
        for (int x = 0; x < BoardSize; x++)
        {
            for (int y = 0; y < BoardSize; y++)
            {
                var piece = _board.Cells[x, y].Piece;

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
    private Position GetCapturedPiecePosition(Position from, Position to)
    {
        return new Position((from.X + to.X) / 2, (from.Y + to.Y) / 2);
    }

    /// <summary>
    /// Checks if a piece should be promoted to a king.
    /// Player1 (at bottom) promotes when reaching row 0 (top).
    /// Player2 (at top) promotes when reaching row 7 (bottom).
    /// </summary>
    private void CheckPromotion(Piece piece, Position position)
    {
        if (piece.Type == PieceType.King)
            return;

        // Player1 promotes at top row
        if (piece.Color == _players[0].Color && position.Y == 0)
        {
            piece.Type = PieceType.King;
            OnPiecePromoted(new PiecePromotedEventArgs(piece, position));
        }
        // Player2 promotes at bottom row
        else if (piece.Color == _players[1].Color && position.Y == BoardSize - 1)
        {
            piece.Type = PieceType.King;
            OnPiecePromoted(new PiecePromotedEventArgs(piece, position));
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
        foreach (var playerPiece in _playerPieces)
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
