using GameBase.Enums;

namespace GameBase.Interfaces;

public interface IPiece
{
    public PieceType Type { get; set; }
    public Color Color { get; set; }
}
