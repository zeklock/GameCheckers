namespace GameApi.Dtos;

public class AvailablePieceDto
{
    public required PieceDto Piece { get; set; }
    public required PositionDto Position { get; set; }
}
