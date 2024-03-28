using System;

namespace Minesweeper.Mth;

public enum Direction
{
    Up,
    Right,
    Down,
    Left,

    UpRight,
    UpLeft,
    DownRight,
    DownLeft,
}

public static class DirectionExtensions
{
    public static Vector2IntB ToVector(this Direction direction)
    {
        return direction switch
        {
            Direction.Up => Vector2IntB.Up,
            Direction.Right => Vector2IntB.Right,
            Direction.Down => Vector2IntB.Down,
            Direction.Left => Vector2IntB.Left,
            Direction.UpRight => Vector2IntB.Up + Vector2IntB.Right,
            Direction.UpLeft => Vector2IntB.Up + Vector2IntB.Left,
            Direction.DownRight => Vector2IntB.Down + Vector2IntB.Right,
            Direction.DownLeft => Vector2IntB.Down + Vector2IntB.Left,
            _ => throw new InvalidOperationException($"Unknown direction {direction}")
        };
    }

    public static Direction GetOpposite(this Direction direction)
    {
        return direction switch
        {
            Direction.Up => Direction.Down,
            Direction.Right => Direction.Left,
            Direction.Down => Direction.Up,
            Direction.Left => Direction.Right,
            Direction.UpRight => Direction.DownLeft,
            Direction.UpLeft => Direction.DownRight,
            Direction.DownRight => Direction.UpLeft,
            Direction.DownLeft => Direction.UpRight,
            _ => throw new InvalidOperationException($"Unknown direction {direction}")
        };
    }
}
