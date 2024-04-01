using Minesweeper.Mth;
using Minesweeper.Players;
using Sandbox;
using System;
using System.Threading.Tasks;

namespace Minesweeper;

public class GameController : Component
{
    private static GameController? _instance = null;
    public static GameController? Instance
    {
        get
        {
            if(_instance.IsValid())
                return _instance;
            return null;
        }
        private set => _instance = value;
    }

    public delegate void StateChangedDelegate(GameState oldState, GameState newState);
    public static event StateChangedDelegate? StateChanged;

    [Property] public GameState State { get; private set; } = GameState.NotStarted;
    public bool IsChangingState { get; private set; } = false;

    [Property] public World World { get; private set; } = null!;
    [Property] public bool Restart { get; private set; } = false;
    [Property] public float TimeToTeleportPlayers { get; private set; } = 1f;
    [Property] public float RestartingTime { get; private set; } = 2f;

    protected TaskCompletionSource? StartGameTaskCompletionSource { get; private set; } = null;

    protected override void OnEnabled()
    {
        if(Instance.IsValid() && Instance != this)
        {
            Log.Warning($"{nameof(Scene)} {Scene} has to much instances of {nameof(GameController)}. Destroying this...");
            Destroy();
            return;
        }

        if(!Scene.IsEditor)
            Instance = this;

        World.OpenedBomb += OnOpenedBomb;
        World.OpenedAllSafeNodes += OnOpenedAllSafeNodes;
    }

    protected override void OnDisabled()
    {
        World.OpenedBomb -= OnOpenedBomb;
        World.OpenedAllSafeNodes -= OnOpenedAllSafeNodes;
    }

    protected override void OnStart()
    {
        if(IsProxy)
            return;

        SetState(GameState.NotStarted);
        _ = StartGame();
    }

    protected virtual void SetState(GameState state)
    {
        if(IsChangingState)
            throw new InvalidOperationException("Can't change node state when state is in changing process");

        IsChangingState = true;
        var oldState = State;
        State = state;
        StateChanged?.Invoke(oldState, state);
        IsChangingState = false;
    }

    protected override void OnUpdate()
    {
        if(IsProxy)
            return;

        if(Restart)
        {
            Restart = false;
            SetState(GameState.NotStarted);
            _ = StartGame();
        }
    }
    protected virtual void RespawnPlayer(Player player, SpawnPoint spawnPoint)
    {
        player.SetTransform(spawnPoint.Transform.World);
    }

    protected virtual void RespawnPlayers()
    {
        var spawnPoints = Scene.GetAllComponents<SpawnPoint>().Shuffle();
        var players = Scene.GetAllComponents<Player>();

        int i = 0;
        foreach(var player in players)
        {
            var spawnPoint = spawnPoints[i];
            i = (i + 1) % spawnPoints.Count;
            RespawnPlayer(player, spawnPoint);
        }
    }

    protected virtual async Task StartGame()
    {
        if(State == GameState.Starting || State == GameState.Started)
            throw new InvalidOperationException("Game is already started");

        var firstStart = State == GameState.NotStarted;

        SetState(GameState.Starting);
        if(!firstStart)
            await Task.Delay(TimeToTeleportPlayers.CeilToInt() * 1000);
        BlockSpawn();
        RespawnPlayers();
        await Task.Delay(RestartingTime.CeilToInt() * 1000);
        await World.SpawnNodes();
        UnblockSpawn();
        SetState(GameState.Started);
    }

    protected virtual void BlockSpawn()
    {
        foreach(var blocker in Scene.Components.GetAll<SpawnBlocker>())
            blocker.Blocking = true;
    }

    protected virtual void UnblockSpawn()
    {
        foreach(var blocker in Scene.GetAllComponents<SpawnBlocker>())
            blocker.Blocking = false;
    }

    protected virtual void OnOpenedBomb()
    {
        if(State != GameState.Started)
            return;

        SetState(GameState.Finished);
        _ = StartGame();
    }

    protected virtual void OnOpenedAllSafeNodes()
    {
        if(State != GameState.Started)
            return;

        SetState(GameState.Finished);
        _ = StartGame();
    }

}
