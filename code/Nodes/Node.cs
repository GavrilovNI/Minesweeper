using Minesweeper.Mth;
using Minesweeper.Networking;
using Sandbox;
using System;

namespace Minesweeper.Nodes;

public abstract class Node : Component
{
    [Property] public GameObject FlagPrefab { get; protected set; } = null!;
    [Property] public GameObject FlagSpawnPosition { get; protected set; } = null!;

    private NodeState _state = NodeState.Closed;
    [Property, Sync] public NodeState State
    {
        get => _state;
        set
        {
            if(_state == value)
                return;

            var oldState = _state;
            _state = value;

            if(!IsProxy)
                OnStateChanged(oldState, value);
            StateChanged?.Invoke(this, oldState, value);
        }
    }

    public delegate void StateChangedDelegate(Node node, NodeState oldState, NodeState newState);
    public event StateChangedDelegate? StateChanged;

    [Sync] protected NetGameObject Flag { get; private set; }

    [Sync] public bool Initialized { get; private set; }
    [Sync] public Vector2IntB Position { get; private set; }
    [Sync] public NetComponent World { get; private set; } = null!;
    [Sync] public bool IsChangingState { get; private set; } = false;
    [Sync] private NodeState StartingState { get; set; }

    public void Inititialize(World world, Vector2IntB position, NodeState startingState = NodeState.Closed)
    {
        NetworkAuthorityException.ThrowIfProxy(this);

        if(Initialized)
            throw new InvalidOperationException("Node is already inititialized");
        Initialized = true;
        World = world;
        Position = position;
        OnInititialize();
        this.StartingState = startingState;
    }

    protected sealed override void OnStart()
    {
        if(!Initialized)
            throw new InvalidOperationException("Node should be initialized before OnStart");

        if(!IsProxy)
            Tags.Add("node");

        OnStartInternal();

        if(!IsProxy)
            SetState(StartingState);
    }

    protected virtual void OnStartInternal()
    {

    }

    protected virtual void OnInititialize()
    {
        
    }

    public virtual void OnNeighborChanged(Direction directionToNeighbor)
    {
        NetworkAuthorityException.ThrowIfProxy(this);
    }

    protected virtual GameObject SpawnFlag()
    {
        NetworkAuthorityException.ThrowIfProxy(this);

        var gameobject = FlagPrefab.Clone(FlagSpawnPosition.Transform.Local, GameObject, true, "Flag");
        gameobject.BreakFromPrefab();
        gameobject.NetworkSpawn();
        return gameobject;
    }

    public void SetState(NodeState state)
    {
        NetworkAuthorityException.ThrowIfProxy(this);

        if(State == NodeState.Opened || State == state)
            return;
        if(IsChangingState)
            throw new InvalidOperationException("Can't change node state when state is in changing process");

        IsChangingState = true;
        State = state;
        IsChangingState = false;
    }

    protected virtual void OnStateChanged(NodeState oldState, NodeState newState)
    {
        NetworkAuthorityException.ThrowIfProxy(this);

        if(newState == NodeState.Blocked)
            Flag = SpawnFlag();
        else
            Flag.GameObject?.Destroy();
    }

    public override int GetHashCode() => HashCode.Combine(Initialized, Position, State);
}
