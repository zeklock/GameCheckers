using GameBase.Interfaces;

namespace Checkers.Views
{
    public static class PlayerInfoRenderer
    {
        public static void RenderPlayerPieces(IDictionary<IPlayer, IList<IPiece>> playerPieces)
        {
            foreach (KeyValuePair<IPlayer, IList<IPiece>> playerPiece in playerPieces)
            {
                IPlayer player = playerPiece.Key;
                IList<IPiece> pieces = playerPiece.Value;
                Console.WriteLine($"{player.Name} ({player.Color}): {pieces.Count} piece(s)");
            }

            Console.WriteLine();
        }
    }
}
