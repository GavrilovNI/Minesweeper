﻿using Minesweeper.Mth;
using Minesweeper.Nodes;
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
    [Property] public GameObject Player { get; private set; } = null!;

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

        World.Lost += OnLost;
        World.Won += OnWon;
    }

    protected override void OnDisabled()
    {
        World.Lost -= OnLost;
        World.Won -= OnWon;
    }

    protected override void OnStart()
    {
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
        if(Restart)
        {
            Restart = false;
            SetState(GameState.NotStarted);
            _ = StartGame();
        }
    }
    protected virtual void RespawnPlayer(Player player, SpawnPoint spawnPoint)
    {
        Player.Transform.World = spawnPoint.Transform.World;
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

        SetState(GameState.Starting);
        RespawnPlayers();
        World.SpawnNodes();
        SetState(GameState.Started);
    }

    protected virtual void OnLost()
    {
        SetState(GameState.Finished);
        _ = StartGame();
    }

    protected virtual void OnWon()
    {
        SetState(GameState.Finished);
        _ = StartGame();
    }

}
