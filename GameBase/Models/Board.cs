namespace GameBase.Models;

public class Board : IBoard
{
    public Cell[,] Cells { get; }

    public Board(int size)
    {
        Cells = new Cell[size, size];
    }
}
