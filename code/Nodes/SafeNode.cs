using Minesweeper.Mth;
using Sandbox;
using System;

namespace Minesweeper.Nodes;

public class SafeNode : Node
{
    [Property] public int BombNeighborsCount { get; private set; }

    protected override void OnStateChanged(NodeState oldState, NodeState newState)
    {
        base.OnStateChanged(oldState, newState);
        if(newState == NodeState.Opened)
            RecalculateNeigbors();
    }

    public override void OnNeighborChanged(Direction directionToNeighbor)
    {
        base.OnNeighborChanged(directionToNeighbor);
        RecalculateNeigbors();
    }

    protected virtual void RecalculateNeigbors()
    {
        BombNeighborsCount = CalculateNeighboringBombsCount();
        if(BombNeighborsCount == 0)
            OpenNeighbors();
    }

    protected int CalculateNeighboringBombsCount()
    {
        int result = 0;
        foreach(var direction in Enum.GetValues<Direction>())
        {
            var neighbor = World.GetNode(Position + direction.ToVector());
            if(neighbor is BombNode)
                result++;
        }
        return result;
    }

    protected virtual void OpenNeighbors()
    {
        foreach(var direction in Enum.GetValues<Direction>())
        {
            var nextPosition = Position + direction.ToVector();
            var node = World.GetNode(nextPosition);

            if(node is not null && node.State != NodeState.Opened)
                node.SetState(NodeState.Opened);
        }
    }

    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), BombNeighborsCount);
}
