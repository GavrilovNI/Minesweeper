using Minesweeper.GameObjectSystems;
using Sandbox;
using System;

namespace Minesweeper.Networking;

public readonly struct NetComponent
{
    public readonly Guid? ComponentId;

    public NetComponent(Component? component)
    {
        if(component.IsValid())
        {
            ComponentId = component.Id;
            component.Scene.GetCache().Cache(component);
        }
        else
        {
            ComponentId = null;
        }
    }

    public T? GetComponent<T>(Scene scene) where T : Component
    {
        if(!ComponentId.HasValue)
            return null;

        return scene.GetCache().FindAndCacheComponent<T>(ComponentId.Value);
    }

    public T? GetComponent<T>() where T : Component => GetComponent<T>(Game.ActiveScene);

    public static implicit operator Component?(NetComponent netComponent) => netComponent.GetComponent<Component>();
    public static implicit operator NetComponent(Component component) => new(component);

    public override int GetHashCode()
    {
        var component = GetComponent<Component>();
        if(component is null)
            return 0;
        return component.GetHashCode();
    }

    public override bool Equals(object? obj) => obj is NetComponent other && this == other;

    public static bool operator ==(NetComponent left, NetComponent right) => left.ComponentId == right.ComponentId;
    public static bool operator !=(NetComponent left, NetComponent right) => !(left== right);
}
