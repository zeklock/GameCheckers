using GameBase.Enums;
using GameBase.Interfaces;

namespace GameBase.Models;

public class Player(Color color, string name) : IPlayer
{
    public Color Color { get; set; } = color;
    public string Name { get; set; } = name;
}
