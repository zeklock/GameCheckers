using GameBase.Models;

namespace GameBase.Interfaces;

public interface ICell
{
    public Position Position { get; set; }
    public IPiece? Piece { get; set; }
}
