using GameBase.Interfaces;

namespace GameBase.Models;

public class Cell : ICell
{
    public Position Position { get; set; }
    public IPiece? Piece { get; set; }

    public Cell(Position position, IPiece? piece)
    {
        Position = position;
        Piece = piece;
    }
}
