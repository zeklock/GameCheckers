using GameBase.Enums;
using GameBase.Interfaces;

namespace GameBase.Models;

public class Piece(PieceType type, Color color) : IPiece
{
    public PieceType Type { get; set; } = type;
    public Color Color { get; set; } = color;
}
