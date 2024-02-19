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
    public static Vector2Int ToVector(this Direction direction)
    {
        return direction switch
        {
            Direction.Up => Vector2Int.Up,
            Direction.Right => Vector2Int.Right,
            Direction.Down => Vector2Int.Down,
            Direction.Left => Vector2Int.Left,
            Direction.UpRight => Vector2Int.Up + Vector2Int.Right,
            Direction.UpLeft => Vector2Int.Up + Vector2Int.Left,
            Direction.DownRight => Vector2Int.Down + Vector2Int.Right,
            Direction.DownLeft => Vector2Int.Down + Vector2Int.Left,
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
