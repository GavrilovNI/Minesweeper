using Sandbox;
using System.Linq;

namespace Minesweeper;

public class SpawnBlocker : Component
{
    private bool _blocking = true;
    [Property, Sync] public bool Blocking
    {
        get => _blocking;
        set
        {
            if(_blocking == value)
                return;

            _blocking = value;
            UpdateBlocking(value);
        }
    }

    protected override void OnAwake()
    {
        if(!IsProxy)
            Blocking = Components.GetAll<Collider>(FindMode.EverythingInSelfAndDescendants).Any(c => c.Enabled);
    }

    protected override void OnStart() => UpdateBlocking(Blocking);
    protected override void OnEnabled() => UpdateBlocking(Blocking);

    protected void UpdateBlocking(bool blocking)
    {
        foreach(var collider in Components.GetAll<Collider>(FindMode.EverythingInSelfAndDescendants))
            collider.Enabled = blocking;
    }
}
