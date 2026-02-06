using GameBase.Models;

namespace GameBase.Interfaces;

public interface IPlayer
{
    public Color Color { get; set; }
    public string Name { get; set; }
}
