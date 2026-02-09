using GameBase;
using GameBase.Dtos;
using GameBase.Enums;
using GameBase.Interfaces;
using GameBase.Models;

namespace Checkers.Views;

public static class GameRenderer
{
    public static void Render(GameController gameController)
    {
        BoardRenderer.Render(gameController.GetBoard());
        PlayerInfoRenderer.RenderPlayerPieces(gameController.GetPlayerPieces());
    }

    public static void SelectPieceAndMove(GameController gameController, List<MovablePieceDto> movablePieces)
    {
        // Display all movable pieces
        Console.WriteLine("\nAvailable Pieces to Move:");
        for (int i = 0; i < movablePieces.Count; i++)
        {
            IPiece piece = movablePieces[i].Piece;
            Position position = movablePieces[i].Position;
            string pieceType = piece.Type == PieceType.Man ? "Man" : "King";
            Console.WriteLine($"  ({i + 1}) Position ({position.X + 1},{position.Y + 1}) - {pieceType}");
        }

        // Get piece selection
        Console.Write("\nSelect piece number: ");
        if (!int.TryParse(Console.ReadLine(), out int pieceChoice) ||
            pieceChoice < 1 || pieceChoice > movablePieces.Count)
        {
            Console.WriteLine("Invalid selection. Please try again.");
            Console.ReadKey();
            return;
        }

        IPiece selectedPiece = movablePieces[pieceChoice - 1].Piece;
        Position selectedPiecePosition = movablePieces[pieceChoice - 1].Position;

        // Get legal moves for selected piece
        List<List<Position>> legalPaths = gameController.GetLegalMoves(selectedPiece);

        if (legalPaths.Count == 0)
        {
            Console.WriteLine("No legal moves available for this piece. Please try another piece.");
            Console.ReadKey();
            return;
        }

        // Display available moves with multi-jump paths
        DisplayMovesAndExecute(gameController, selectedPiece, selectedPiecePosition, legalPaths);
    }

    public static void DisplayMovesAndExecute(GameController gameController, IPiece piece, Position piecePos, List<List<Position>> legalPaths)
    {
        Console.WriteLine($"\nAvailable Moves for Piece at ({piecePos.X + 1},{piecePos.Y + 1}):");
        Console.WriteLine($"  (0) Back");

        for (int i = 0; i < legalPaths.Count; i++)
        {
            List<Position> path = legalPaths[i];
            string moveDescription = FormatMovePath(path);
            Console.WriteLine($"  ({i + 1}) {moveDescription}");
        }

        // Get move selection
        Console.Write("\nSelect move number: ");
        if (!int.TryParse(Console.ReadLine(), out int moveChoice) ||
            moveChoice < 0 || moveChoice > legalPaths.Count)
        {
            Console.WriteLine("Invalid selection. Please try again.");
            Console.ReadKey();
            return;
        }

        // Check if user wants to go back
        if (moveChoice == 0)
            return;

        List<Position> selectedPath = legalPaths[moveChoice - 1];

        // Execute move
        gameController.MovePiece(piece, selectedPath);
    }

    public static string FormatMovePath(List<Position> path)
    {
        if (path.Count == 0)
            return "Move (invalid path)";

        if (path.Count == 1)
            return $"Move to ({path[0].X + 1},{path[0].Y + 1})";

        // Multi-jump path: "Move to (x,y) -> (x,y) -> ..."
        string result = $"Move to ({path[0].X + 1},{path[0].Y + 1})";
        for (int i = 1; i < path.Count; i++)
        {
            result += $" -> ({path[i].X + 1}, {path[i].Y + 1})";
        }
        return result;
    }
}
