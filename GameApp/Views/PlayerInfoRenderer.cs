using GameBase.Interfaces;

namespace Checkers.Views
{
    public static class PlayerInfoRenderer
    {
        public static void RenderPlayerPieces(Dictionary<IPlayer, List<IPiece>> playerPieces)
        {
            foreach (KeyValuePair<IPlayer, List<IPiece>> playerPiece in playerPieces)
            {
                IPlayer player = playerPiece.Key;
                List<IPiece> pieces = playerPiece.Value;
                Console.WriteLine($"{player.Name} ({player.Color}): {pieces.Count} piece(s)");
            }

            Console.WriteLine();
        }
    }
}
