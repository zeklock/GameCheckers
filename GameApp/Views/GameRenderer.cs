using GameBase;

namespace Checkers.Views;

public static class GameRenderer
{
    public static void RenderGame(GameController gameController)
    {
        BoardRenderer.RenderBoard(gameController.GetBoard());
        PlayerInfoRenderer.RenderPlayerPieces(gameController.GetPlayerPieces());

        Console.ReadKey();
    }
}
