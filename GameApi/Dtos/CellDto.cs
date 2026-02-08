namespace GameApi.Dtos
{
    public class CellDto
    {
        public required PositionDto Position { get; set; }
        public PieceDto? Piece { get; set; }
    }
}
