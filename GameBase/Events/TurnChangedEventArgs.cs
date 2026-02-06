using GameBase.Interfaces;
using GameBase.Models;

namespace GameBase.Events;

public class TurnChangedEventArgs : EventArgs
{
    public readonly IPlayer CurrentPlayer;

    public TurnChangedEventArgs(IPlayer currentPlayer)
    {
        CurrentPlayer = currentPlayer;
    }
}
