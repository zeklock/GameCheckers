namespace GameBase.Models;

public readonly struct Direction
{
    public readonly Position Move;
    public readonly Position Jump;

    private Direction(Position move, Position jump)
    {
        Move = move;
        Jump = jump;
    }

    public static readonly Direction TopLeft = new Direction(new Position(-1, -1), new Position(-2, -2));
    public static readonly Direction TopRight = new Direction(new Position(1, -1), new Position(2, -2));
    public static readonly Direction BottomLeft = new Direction(new Position(-1, 1), new Position(-2, 2));
    public static readonly Direction BottomRight = new Direction(new Position(1, 1), new Position(2, 2));
}
