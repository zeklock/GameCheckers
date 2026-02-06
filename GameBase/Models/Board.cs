using GameBase.Interfaces;

namespace GameBase.Models;

public class Board : IBoard
{
    public ICell[,] Cells { get; }

    public Board(int size)
    {
        Cells = new Cell[size, size];
    }
}
