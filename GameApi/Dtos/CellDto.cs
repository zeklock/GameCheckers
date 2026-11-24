namespace GameApi.Dtos;

public record CellDto(PositionDto Position, PieceDto? Piece = null);
