using GameBase;
using GameBase.Models;

namespace Checkers.Views;

public static class CellRenderer
{
    public const int Width = 5;
    public const int Height = 3;

    public static void Render(Cell cell, int heightIndex)
    {
        int centerRow = CenterIndex(Height);

        if (cell.Position.X == 0)
        {
            if (centerRow == heightIndex)
            {
                Row((cell.Position.Y + 1).ToString());
            }
            else
            {
                Row();
            }
        }

        Console.Write("|");

        bool isCellForPiece = GameController.IsCellForPiece(cell.Position.X, cell.Position.Y);
        Console.BackgroundColor = isCellForPiece
            ? ConsoleColor.DarkGray : ConsoleColor.Gray;

        if (cell.Piece != null && centerRow == heightIndex)
        {
            Console.ForegroundColor = cell.Piece.Color == Color.Black
                ? ConsoleColor.Black : ConsoleColor.White;
            Row(cell.Piece.Type == PieceType.Man ? "M" : "K");
        }
        else
        {
            Row();
        }

        Console.ResetColor();
    }

    public static int CenterIndex(int n)
    {
        return n % 2 == 0 ? (n / 2) - 1 : (n - 1) / 2;
    }

    public static void Row(string output = " ")
    {
        for (int i = 0; i < Width; i++)
        {
            if (i == CenterIndex(Width))
            {
                Console.Write(output);
            }
            else
            {
                Console.Write(" ");
            }
        }
    }
}
