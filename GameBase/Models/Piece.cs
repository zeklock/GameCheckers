namespace GameBase.Models;

public class Piece : IPiece
{
    public PieceType Type { get; set; }
    public Color Color { get; set; }

    public Piece(PieceType type, Color color)
    {
        Type = type;
        Color = color;
    }
}
