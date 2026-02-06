using GameBase.Interfaces;
using GameBase.Models;

namespace GameBase.Events;

public class PieceCapturedEventArgs : EventArgs
{
    public readonly IPiece CapturedPiece;
    public readonly Position CapturedPosition;

    public PieceCapturedEventArgs(IPiece capturedPiece, Position capturedPosition)
    {
        CapturedPiece = capturedPiece;
        CapturedPosition = capturedPosition;
    }
}
