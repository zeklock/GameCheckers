using Checkers.Views;
using GameBase;
using GameBase.Dtos;
using GameBase.Enums;
using GameBase.Events;
using GameBase.Interfaces;
using GameBase.Models;

namespace GameApp;

internal class Program
{
    static int _combinationChoice;

    static void Main(string[] args)
    {
        bool isSelectPlayerCombination = true;

        while (isSelectPlayerCombination)
        {
            Console.Clear();
            Console.WriteLine("======================================");
            Console.WriteLine("===                                ===");
            Console.WriteLine("=== Welcome To Checkers Board Game ===");
            Console.WriteLine("===                                ===");
            Console.WriteLine("======================================");

            Console.WriteLine("\nPlayer Combination");
            Console.WriteLine("1. Player 1 Black, Player 2 White");
            Console.WriteLine("2. Player 1 White, Player 2 Black");

            Console.Write("\nSelect Player Combination (1 or 2): ");

            if (!int.TryParse(Console.ReadLine(), out _combinationChoice)
                || _combinationChoice < 0
                || _combinationChoice > 2)
            {
                Console.WriteLine("\nInvalid selection. Please try again.");
                Console.ReadKey();
                continue;
            }

            isSelectPlayerCombination = false;
        }

        IBoard board = new Board(GameController.BoardSize);
        List<IPlayer> players = new List<IPlayer>
        {
            new Player(_combinationChoice == 1 ? Color.Black : Color.White, "Player 1"),
            new Player(_combinationChoice == 1 ? Color.White : Color.Black, "Player 2")
        };

        GameController gameController = new GameController(board, players);
        gameController.Start();

        // Subscribe to events
        gameController.PieceCaptured += game_PieceCaptured;
        gameController.PiecePromoted += game_PiecePromoted;
        gameController.TurnChanged += game_TurnChanged;

        bool isGameRunning = true;
        while (isGameRunning)
        {
            // Render current game state
            GameRenderer.Render(gameController);

            // Get current player info
            IPlayer currentPlayer = gameController.GetCurrentPlayer();
            Console.WriteLine($"\n{currentPlayer.Name}'s Turn ({currentPlayer.Color})");

            // Check for winner
            gameController.CheckWin();
            IPlayer? winner = gameController.GetWinner();

            if (winner != null)
            {
                Console.WriteLine($"\n{winner.Name} ({winner.Color}) WINS!");
                isGameRunning = false;
                break;
            }

            // Get all pieces the current player can move
            List<MovablePieceDto> movablePieces = gameController.GetMovablePieces(currentPlayer);

            if (movablePieces.Count == 0)
            {
                Console.WriteLine("No legal moves available! You lose.");
                gameController.CheckWin();
                continue;
            }

            GameRenderer.SelectPieceAndMove(gameController, movablePieces);
        }

        Console.WriteLine("\nThanks for playing Checkers!");
        Console.ReadKey();
    }

    static void game_PieceCaptured(object? sender, PieceCapturedEventArgs e)
    {
        Console.WriteLine($"Piece Captured! {e.CapturedPiece.Color} {e.CapturedPiece.Type} ({e.CapturedPosition.X + 1},{e.CapturedPosition.Y + 1}) was removed from the board.");
        Console.ReadKey();
    }

    static void game_PiecePromoted(object? sender, PiecePromotedEventArgs e)
    {
        Console.WriteLine($"Piece Promoted! {e.PromotedPiece.Color} piece ({e.PromotedPosition.X + 1},{e.PromotedPosition.Y + 1}) has become a King!");
        Console.ReadKey();
    }

    static void game_TurnChanged(object? sender, TurnChangedEventArgs e)
    {
        Console.WriteLine($"Turn switched to {e.CurrentPlayer.Name} ({e.CurrentPlayer.Color})");
        Console.ReadKey();
    }
}
