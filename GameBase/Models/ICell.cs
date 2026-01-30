namespace GameBase.Models;

public interface ICell
{
    public Position Position { get; set; }
    public Piece? Piece { get; set; }
}
