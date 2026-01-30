using Checkers.Views;
using GameBase;
using GameBase.Models;

Console.Clear();
Console.WriteLine("======================================");
Console.WriteLine("===                                ===");
Console.WriteLine("=== Welcome To Checkers Board Game ===");
Console.WriteLine("===                                ===");
Console.WriteLine("======================================");

Console.WriteLine("\nPress any key to start...");
Console.ReadKey();

IBoard board = new Board(GameController.BoardSize);
List<IPlayer> players = new List<IPlayer>
        {
            new Player(Color.White, "Player 1"),
            new Player(Color.Black, "Player 2")
        };

GameController gameController = new GameController(board, players);
gameController.Start();

GameRenderer.RenderGame(gameController);
