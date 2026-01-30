using GameBase.Models;

namespace GameBase;

public class GameController
{
    public const int BoardSize = 8;
    private IBoard _board;
    private List<IPlayer> _players;
    private IPlayer _currPlayer;
    private Dictionary<IPlayer, List<IPiece>> _playerPieces;
    public Action<Piece>? OnPieceCaptured;

    public GameController(IBoard board, List<IPlayer> players)
    {
        _board = board;
        _players = players;
        _currPlayer = players.First();
        _playerPieces = new Dictionary<IPlayer, List<IPiece>>();

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

    public void CheckWin() { }

    public void SwitchPlayer() { }

    public void MovePiece(Piece piece, Position to) { }

    // public List<Cell> GetLegalMoves(Piece piece) { }

    public void ExploreJumps(Piece piece, Position currentPos, List<Position> result, HashSet<string> visited) { }

    // public List<Cell> GetJumpPath(Position start, Position end) { }

    public void Promote() { }

    public void RemovePiece(Piece piece) { }

    // public Piece GetPiece(Position position) { }

    // public bool IsInside(Position position) { }

    public void PlacePiece(Piece piece) { }

    private static bool IsCellForPiece(int x, int y) => (x + y) % 2 != 0;

    public Dictionary<IPlayer, List<IPiece>> GetPlayerPieces() => _playerPieces;

    public IBoard GetBoard() => _board;
}
