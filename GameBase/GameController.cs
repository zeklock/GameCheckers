using GameBase.Models;

namespace GameBase;

public class GameController
{
    public const int BoardSize = 8;
    private IBoard _board;
    private List<IPlayer> _players;
    private IPlayer _currPlayer;
    private Dictionary<IPlayer, List<IPiece>> _playerPieces;
    private IPlayer? _winner;
    private List<Position> _lastJumpPath;
    public Action<Piece>? OnPieceCaptured;
    public Action<Piece>? OnPiecePromoted;
    public Action<IPlayer>? OnTurnChanged;

    public GameController(IBoard board, List<IPlayer> players)
    {
        _board = board;
        _players = players;
        _currPlayer = players.First();
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

    /// <summary>
    /// Moves a piece from its current position to a new position.
    /// Handles both normal moves and jump captures.
    /// </summary>
    public void MovePiece(Piece piece, Position to)
    {
        Position? from = GetPiecePosition(piece);

        if (from == null || !IsInside(to) || _board.Cells[to.X, to.Y].Piece != null)
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
                OnPieceCaptured?.Invoke(capturedPiece);

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
    /// Gets all legal moves for a piece (both normal moves and jumps).
    /// Returns list of possible destination positions.
    /// </summary>
    public List<Position> GetLegalMoves(Piece piece)
    {
        List<Position> legalMoves = new List<Position>();
        Position? piecePos = GetPiecePosition(piece);

        if (piecePos == null)
            return legalMoves;

        // Check for jump moves first (forced)
        List<Position> jumps = GetJumps(piece, piecePos.Value);
        if (jumps.Count > 0)
            return jumps;

        // If no jumps available, return normal moves
        return GetNormalMoves(piece, piecePos.Value);
    }

    /// <summary>
    /// Gets all normal (non-jump) moves for a piece.
    /// </summary>
    private List<Position> GetNormalMoves(Piece piece, Position position)
    {
        List<Position> moves = new List<Position>();
        int[][] directions = GetMovementDirections(piece);

        foreach (int[] dir in directions)
        {
            Position newPos = new Position(position.X + dir[0], position.Y + dir[1]);
            if (IsInside(newPos) && _board.Cells[newPos.X, newPos.Y].Piece == null)
            {
                moves.Add(newPos);
            }
        }

        return moves;
    }

    /// <summary>
    /// Gets all jump moves available for a piece.
    /// </summary>
    private List<Position> GetJumps(Piece piece, Position position)
    {
        List<Position> jumps = new List<Position>();
        ExploreJumps(piece, position, jumps, new HashSet<string>());
        return jumps;
    }

    /// <summary>
    /// Recursively explores all possible jump sequences from a position.
    /// Used to find all valid multi-jump paths.
    /// </summary>
    public void ExploreJumps(Piece piece, Position currentPos, List<Position> result, HashSet<string> visited)
    {
        int[][] jumpDirections = new int[][] { new int[] { 2, 2 }, new int[] { 2, -2 }, new int[] { -2, 2 }, new int[] { -2, -2 } };

        foreach (int[] dir in jumpDirections)
        {
            Position capturePos = new Position(currentPos.X + dir[0] / 2, currentPos.Y + dir[1] / 2);
            Position landPos = new Position(currentPos.X + dir[0], currentPos.Y + dir[1]);

            if (!IsInside(landPos))
                continue;

            Piece? capturedPiece = _board.Cells[capturePos.X, capturePos.Y].Piece;
            Piece? landPiece = _board.Cells[landPos.X, landPos.Y].Piece;

            // Check if we can capture
            if (capturedPiece != null && capturedPiece.Color != piece.Color && landPiece == null)
            {
                string visitKey = $"{landPos.X},{landPos.Y}";
                if (!visited.Contains(visitKey))
                {
                    visited.Add(visitKey);
                    result.Add(landPos);

                    // Simulate the move to check for further jumps
                    Piece? temp = _board.Cells[capturePos.X, capturePos.Y].Piece;
                    _board.Cells[capturePos.X, capturePos.Y].Piece = null;
                    _board.Cells[currentPos.X, currentPos.Y].Piece = null;
                    _board.Cells[landPos.X, landPos.Y].Piece = piece;

                    ExploreJumps(piece, landPos, result, visited);

                    // Undo simulation
                    _board.Cells[currentPos.X, currentPos.Y].Piece = piece;
                    _board.Cells[landPos.X, landPos.Y].Piece = null;
                    _board.Cells[capturePos.X, capturePos.Y].Piece = temp;

                    visited.Remove(visitKey);
                }
            }
        }
    }

    /// <summary>
    /// Checks if a piece has additional jump moves available from its current position.
    /// </summary>
    private bool HasAdditionalJumps(Piece piece, Position position)
    {
        int[][] jumpDirections = new int[][] { new int[] { 2, 2 }, new int[] { 2, -2 }, new int[] { -2, 2 }, new int[] { -2, -2 } };

        foreach (int[] dir in jumpDirections)
        {
            Position capturePos = new Position(position.X + dir[0] / 2, position.Y + dir[1] / 2);
            Position landPos = new Position(position.X + dir[0], position.Y + dir[1]);

            if (!IsInside(landPos))
                continue;

            Piece? capturedPiece = _board.Cells[capturePos.X, capturePos.Y].Piece;
            Piece? landPiece = _board.Cells[landPos.X, landPos.Y].Piece;

            if (capturedPiece != null && capturedPiece.Color != piece.Color && landPiece == null)
            {
                return true;
            }
        }

        return false;
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
        if (GetJumps(piece, from).Count > 0)
            return false;

        return true;
    }

    /// <summary>
    /// Gets the position of a captured piece between two diagonal positions.
    /// </summary>
    private Position GetCapturedPiecePosition(Position from, Position to)
    {
        return new Position((from.X + to.X) / 2, (from.Y + to.Y) / 2);
    }

    /// <summary>
    /// Gets movement directions based on piece type.
    /// Kings can move in all diagonal directions, Men only forward.
    /// </summary>
    private int[][] GetMovementDirections(Piece piece)
    {
        if (piece.Type == PieceType.King)
        {
            return new int[][]
            {
                new int[] { 1, 1 },
                new int[] { 1, -1 },
                new int[] { -1, 1 },
                new int[] { -1, -1 }
            };
        }
        else if (piece.Color == Color.Black)
        {
            // Black moves down (positive Y)
            return new int[][]
            {
                new int[] { 1, 1 },
                new int[] { -1, 1 }
            };
        }
        else
        {
            // White moves up (negative Y)
            return new int[][]
            {
                new int[] { 1, -1 },
                new int[] { -1, -1 }
            };
        }
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
            OnPiecePromoted?.Invoke(piece);
        }
        else if (piece.Color == Color.White && position.Y == 0)
        {
            piece.Type = PieceType.King;
            OnPiecePromoted?.Invoke(piece);
        }
    }

    /// <summary>
    /// Gets the position of a piece on the board.
    /// </summary>
    private Position? GetPiecePosition(Piece piece)
    {
        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                if (_board.Cells[x, y].Piece == piece)
                {
                    return new Position(x, y);
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Checks if a position is within board boundaries.
    /// </summary>
    public bool IsInside(Position position)
    {
        return position.X >= 0 && position.X < BoardSize && position.Y >= 0 && position.Y < BoardSize;
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
        OnTurnChanged?.Invoke(_currPlayer);
    }

    /// <summary>
    /// Checks if the game has a winner.
    /// A player wins when the opponent has no pieces or no legal moves.
    /// </summary>
    public void CheckWin()
    {
        _winner = null;

        // Check if current player has any legal moves
        var movablePieces = GetMovablePieces(_currPlayer);
        if (movablePieces.Count == 0)
        {
            // Current player has no moves, opponent wins
            IPlayer opponent = _players.First(p => p != _currPlayer);
            _winner = opponent;
            return;
        }

        // Check if any player has no pieces left
        foreach (var player in _players)
        {
            var playerPieces = _playerPieces[player];
            if (playerPieces.Count == 0)
            {
                // This player has no pieces, opponent wins
                IPlayer opponent = _players.First(p => p != player);
                _winner = opponent;
                return;
            }
        }
    }

    /// <summary>
    /// Gets all pieces the player can move (has legal moves available).
    /// </summary>
    public List<(Piece piece, Position position)> GetMovablePieces(IPlayer player)
    {
        var result = new List<(Piece, Position)>();

        for (int y = 0; y < BoardSize; y++)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                Piece? piece = _board.Cells[x, y].Piece;

                if (piece != null && piece.Color == player.Color)
                {
                    List<Position> legalMoves = GetLegalMoves(piece);
                    if (legalMoves.Count > 0)
                    {
                        result.Add((piece, new Position(x, y)));
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Gets the current game winner (if any).
    /// </summary>
    public IPlayer? GetWinner() => _winner;

    /// <summary>
    /// Gets the current player.
    /// </summary>
    public IPlayer GetCurrentPlayer() => _currPlayer;

    private static bool IsCellForPiece(int x, int y) => (x + y) % 2 != 0;

    public Dictionary<IPlayer, List<IPiece>> GetPlayerPieces() => _playerPieces;

    public IBoard GetBoard() => _board;
}
