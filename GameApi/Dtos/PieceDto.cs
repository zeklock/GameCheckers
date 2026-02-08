using GameBase.Models;

namespace GameApi.Dtos;

public class PieceDto
{
    public Color Color { get; set; }
    public PieceType Type { get; set; }
}
