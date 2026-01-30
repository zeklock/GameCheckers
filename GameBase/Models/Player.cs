namespace GameBase.Models;

public class Player : IPlayer
{
    public Color Color { get; set; }
    public string Name { get; set; }

    public Player(Color color, string name)
    {
        Color = color;
        Name = name;
    }
}
