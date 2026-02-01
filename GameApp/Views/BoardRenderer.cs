using GameBase.Models;

namespace Checkers.Views
{
    public static class BoardRenderer
    {
        public static void RenderBoard(IBoard board)
        {
            Console.Clear();
            Console.WriteLine("\nCheckers Board");
            Console.WriteLine();

            int size = board.Cells.GetLength(0);

            // Index X
            for (int x = 0; x < size; x++)
            {
                string indexSpace = x == 0 ? "  " : "";
                Console.Write($"{indexSpace}  {x + 1} ");
            }
            Console.WriteLine();

            // Border atas
            for (int x = 0; x < size; x++)
            {
                string indexSpace = x == 0 ? "  " : "";
                Console.Write($"{indexSpace} ___");
            }
            Console.WriteLine();

            for (int y = 0; y < size; y++)
            {
                // Baris isi
                string rowContent = "";
                for (int x = 0; x < size; x++)
                {
                    rowContent += x == 0 ? y + 1 + " |" : "|";
                    rowContent += CellRenderer.RenderCell(board.Cells[x, y]);
                }
                rowContent += "|"; // tutup border kanan
                Console.WriteLine(rowContent);

                // Baris bawah border
                string rowBorder = "";
                for (int x = 0; x < size; x++)
                {
                    string indexSpace = x == 0 ? "  " : "";
                    rowBorder += indexSpace + "|___";
                }
                rowBorder += "|";
                Console.WriteLine(rowBorder);
            }

            Console.WriteLine();
        }
    }
}
