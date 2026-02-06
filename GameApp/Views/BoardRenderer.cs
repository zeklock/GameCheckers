using GameBase.Interfaces;

namespace Checkers.Views
{
    public static class BoardRenderer
    {
        public static void Render(IBoard board)
        {
            Console.Clear();
            Console.WriteLine("\nCheckers Board\n");

            int size = board.Cells.GetLength(0);
            int centerHeight = CellRenderer.CenterIndex(CellRenderer.Height);

            for (int h = 0; h < CellRenderer.Height; h++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (x == 0)
                    {
                        CellRenderer.Row();
                        Console.Write(" ");
                    }

                    if (h == centerHeight)
                    {
                        CellRenderer.Row((x + 1).ToString());
                    }
                    else
                    {
                        CellRenderer.Row();
                    }

                    Console.Write(" ");
                }

                Console.WriteLine();
            }

            for (int y = 0; y < size; y++)
            {
                for (int h = 0; h < CellRenderer.Height; h++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        CellRenderer.Render(board.Cells[x, y], h);
                    }
                    Console.WriteLine("|");
                }
            }

            Console.WriteLine();
        }
    }
}
