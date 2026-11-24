using GameBase.Interfaces;

namespace GameBase.Models;

public class Board(int size) : IBoard
{
    public ICell[,] Cells { get; } = new Cell[size, size];
}
