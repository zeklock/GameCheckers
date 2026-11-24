namespace GameBase.Models;

public readonly record struct Direction(Position Move, Position Jump)
{
    public static readonly Direction TopLeft = new(new Position(-1, -1), new Position(-2, -2));
    public static readonly Direction TopRight = new(new Position(1, -1), new Position(2, -2));
    public static readonly Direction BottomLeft = new(new Position(-1, 1), new Position(-2, 2));
    public static readonly Direction BottomRight = new(new Position(1, 1), new Position(2, 2));
}
