namespace GameApi.Dtos;

public class BoardDto
{
    public int Size { get; set; }
    public List<CellDto> Cells { get; set; } = new List<CellDto>();
}
