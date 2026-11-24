using GameBase.Interfaces;

namespace GameBase.Models;

public class Cell(Position position, IPiece? piece) : ICell
{
    public Position Position { get; set; } = position;
    public IPiece? Piece { get; set; } = piece;
}
