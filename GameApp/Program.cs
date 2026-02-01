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

// Subscribe to events
gameController.OnPieceCaptured += (piece) =>
{
    Console.WriteLine($"🔴 Piece Captured! {piece.Color} {piece.Type} was removed from the board.");
    Console.ReadKey();
};

gameController.OnPiecePromoted += (piece) =>
{
    Console.WriteLine($"👑 Piece Promoted! {piece.Color} piece has become a King!");
    Console.ReadKey();
};

gameController.OnTurnChanged += (player) =>
{
    Console.WriteLine($"⏩ Turn switched to {player.Name} ({player.Color})");
    Console.ReadKey();
};

// Game Loop
bool isGameRunning = true;
while (isGameRunning)
{
    Console.Clear();

    // Render current game state
    BoardRenderer.RenderBoard(gameController.GetBoard());
    PlayerInfoRenderer.RenderPlayerPieces(gameController.GetPlayerPieces());

    // Get current player info
    IPlayer currentPlayer = gameController.GetCurrentPlayer();
    Console.WriteLine($"\n{currentPlayer.Name}'s Turn ({currentPlayer.Color})");

    // Check for winner
    gameController.CheckWin();
    IPlayer? winner = gameController.GetWinner();
    if (winner != null)
    {
        Console.WriteLine($"\n🎉 {winner.Name} ({winner.Color}) WINS! 🎉");
        isGameRunning = false;
        break;
    }

    // Get all pieces the current player can move
    List<(Piece piece, Position position)> movablePieces = gameController.GetMovablePieces(currentPlayer);

    if (movablePieces.Count == 0)
    {
        Console.WriteLine("No legal moves available! You lose.");
        gameController.CheckWin();
        continue;
    }

    // Display all movable pieces
    Console.WriteLine("\n📍 Available Pieces to Move:");
    for (int i = 0; i < movablePieces.Count; i++)
    {
        var (piece, pos) = movablePieces[i];
        string pieceType = piece.Type == PieceType.Man ? "Man" : "King";
        Console.WriteLine($"  ({i + 1}) Position ({pos.X + 1},{pos.Y + 1}) - {pieceType}");
    }

    // Get piece selection
    Console.WriteLine("\nSelect piece number:");
    if (!int.TryParse(Console.ReadLine(), out int pieceChoice) ||
        pieceChoice < 1 || pieceChoice > movablePieces.Count)
    {
        Console.WriteLine("Invalid selection. Please try again.");
        Console.ReadKey();
        continue;
    }

    var (selectedPiece, selectedPiecePos) = movablePieces[pieceChoice - 1];

    // Get legal moves for selected piece
    List<Position> legalMoves = gameController.GetLegalMoves(selectedPiece);

    if (legalMoves.Count == 0)
    {
        Console.WriteLine("No legal moves available for this piece. Please try another piece.");
        Console.ReadKey();
        continue;
    }

    // Display available moves
    Console.WriteLine($"\n🎯 Available Moves for Piece at ({selectedPiecePos.X + 1},{selectedPiecePos.Y + 1}):");
    Console.WriteLine($"  (0) Back");
    for (int i = 0; i < legalMoves.Count; i++)
    {
        Position move = legalMoves[i];
        Console.WriteLine($"  ({i + 1}) Move to ({move.X + 1},{move.Y + 1})");
    }

    // Get move selection
    Console.WriteLine("\nSelect move number:");
    if (!int.TryParse(Console.ReadLine(), out int moveChoice) ||
        moveChoice < 0 || moveChoice > legalMoves.Count)
    {
        Console.WriteLine("Invalid selection. Please try again.");
        Console.ReadKey();
        continue;
    }

    // Check if user wants to go back
    if (moveChoice == 0)
    {
        continue;
    }

    Position destinationPos = legalMoves[moveChoice - 1];

    // Execute move
    gameController.MovePiece(selectedPiece, destinationPos);
    Console.WriteLine("✓ Move executed!");
    Console.ReadKey();
}

Console.WriteLine("\nThanks for playing Checkers!");
Console.ReadKey();
