using GameBase.Models;

namespace Checkers.Views;

public static class CellRenderer
{
    public static string RenderCell(Cell cell)
    {
        if (cell.Piece == null)
            return "   "; // kosong
        else
            return cell.Piece.Color == Color.Black ? " B " : " W ";
    }
}
