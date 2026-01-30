namespace GameBase.Models;

public interface IPiece
{
    public PieceType Type { get; set; }
    public Color Color { get; set; }
}
