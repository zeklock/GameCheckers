using GameBase.Models;

namespace GameBase.Events;

public class PiecePromotedEventArgs : EventArgs
{
    public readonly Piece PromotedPiece;

    public PiecePromotedEventArgs(Piece promotedPiece)
    {
        PromotedPiece = promotedPiece;
    }
}
