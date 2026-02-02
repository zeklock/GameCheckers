using GameBase.Models;

namespace GameBase.Events;

public class PieceCapturedEventArgs : EventArgs
{
    public readonly Piece CapturedPiece;

    public PieceCapturedEventArgs(Piece capturedPiece)
    {
        CapturedPiece = capturedPiece;
    }
}
