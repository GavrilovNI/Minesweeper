using Minesweeper.Mth;
using Sandbox;
using System;

namespace Minesweeper.Nodes;

public abstract class Node : Component
{
    [Property] public GameObject FlagPrefab { get; protected set; } = null!;
    [Property] public GameObject FlagSpawnPosition { get; protected set; } = null!;

    [Property] public NodeState State { get; private set; } = NodeState.Closed;

    public delegate void StateChangedDelegate(Node node, NodeState oldState, NodeState newState);
    public event StateChangedDelegate? StateChanged;

    protected GameObject? Flag { get; private set; }

    public bool Initialized { get; private set; }
    public Vector2Int Position { get; private set; }
    public World World { get; private set; } = null!;

    public bool IsChangingState { get; private set; } = false;


    public void Inititialize(World world, Vector2Int position)
    {
        if(Initialized)
            throw new InvalidOperationException("Node is already inititialized");
        Initialized = true;
        World = world;
        Position = position;
        OnInititialize();
    }

    protected sealed override void OnStart()
    {
        if(!Initialized)
            throw new InvalidOperationException("Node should be initialized before OnStart");
        Tags.Add("node");
        OnStartInternal();
    }

    protected virtual void OnStartInternal()
    {

    }

    protected virtual void OnInititialize()
    {
        
    }

    public virtual void OnNeighborChanged(Direction directionToNeighbor)
    {

    }

    protected virtual GameObject SpawnFlag()
    {
        var gameobject = FlagPrefab.Clone(FlagSpawnPosition.Transform.Local, GameObject, true, "Flag");
        gameobject.BreakFromPrefab();
        return gameobject;
    }

    public void SetState(NodeState state)
    {
        if(State == NodeState.Opened || State == state)
            return;
        if(IsChangingState)
            throw new InvalidOperationException("Can't change node state when state is in changing process");

        IsChangingState = true;

        var oldState = State;
        State = state;

        OnStateChanged(oldState, State);
        StateChanged?.Invoke(this, oldState, State);
        IsChangingState = false;
    }

    protected virtual void OnStateChanged(NodeState oldState, NodeState newState)
    {
        if(newState == NodeState.Blocked)
            Flag = SpawnFlag();
        else
            Flag?.Destroy();
    }

    public override int GetHashCode() => HashCode.Combine(Initialized, World, Position, State);
}
