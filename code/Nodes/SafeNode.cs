using Minesweeper.Mth;
using Minesweeper.Networking;
using Sandbox;
using System;

namespace Minesweeper.Nodes;

public class SafeNode : Node
{
    [Property, Sync] public int BombNeighborsCount { get; private set; }

    protected override void OnStateChanged(NodeState oldState, NodeState newState)
    {
        base.OnStateChanged(oldState, newState);
        if(newState == NodeState.Opened)
            RecalculateNeigbors();
    }

    public override void OnNeighborChanged(Direction directionToNeighbor)
    {
        NetworkAuthorityException.ThrowIfProxy(this);
        base.OnNeighborChanged(directionToNeighbor);
        RecalculateNeigbors();
    }

    protected virtual void RecalculateNeigbors()
    {
        NetworkAuthorityException.ThrowIfProxy(this);
        BombNeighborsCount = CalculateNeighboringBombsCount();
        if(BombNeighborsCount == 0)
            OpenNeighbors();
    }

    protected int CalculateNeighboringBombsCount()
    {
        NetworkAuthorityException.ThrowIfProxy(this);

        int result = 0;
        World world = World.GetComponent<World>()!;
        foreach(var direction in Enum.GetValues<Direction>())
        {
            var neighbor = world.GetNode(Position + direction.ToVector());
            if(neighbor is BombNode)
                result++;
        }
        return result;
    }

    protected virtual void OpenNeighbors()
    {
        NetworkAuthorityException.ThrowIfProxy(this);

        World world = World.GetComponent<World>()!;
        foreach(var direction in Enum.GetValues<Direction>())
        {
            var nextPosition = Position + direction.ToVector();
            var node = world.GetNode(nextPosition);

            if(node is not null && node.State != NodeState.Opened)
                node.SetState(NodeState.Opened);
        }
    }

    public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), BombNeighborsCount);
}
