namespace GameApi.Dtos;

public class MoveDto
{
    public required PieceDto Piece { get; set; }
    public required PositionDto Position { get; set; }
    public required List<PositionDto> Path { get; set; }
}
