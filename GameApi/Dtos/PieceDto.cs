using GameBase.Enums;

namespace GameApi.Dtos;

public record PieceDto(Color Color = Color.Black, PieceType Type = PieceType.Man);
