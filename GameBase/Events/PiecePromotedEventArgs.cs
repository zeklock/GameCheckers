using GameBase.Interfaces;
using GameBase.Models;

namespace GameBase.Events;

public class PiecePromotedEventArgs : EventArgs
{
    public readonly IPiece PromotedPiece;
    public readonly Position PromotedPosition;

    public PiecePromotedEventArgs(IPiece promotedPiece, Position promotedPosition)
    {
        PromotedPiece = promotedPiece;
        PromotedPosition = promotedPosition;
    }
}
