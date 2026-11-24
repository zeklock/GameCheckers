namespace GameApi.Dtos;

public record MoveDto(PieceDto? Piece = null, PositionDto? Position = null, List<PositionDto>? Path = null);
