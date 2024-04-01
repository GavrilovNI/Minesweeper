using Minesweeper.GameObjectSystems;
using Sandbox;
using System;

namespace Minesweeper.Networking;

public readonly struct NetGameObject
{
    public readonly Guid? GameObjectId;

    public NetGameObject(GameObject? gameObject)
    {
        if(gameObject.IsValid())
        {
            GameObjectId = gameObject.Id;
            gameObject.Scene.GetCache().Cache(gameObject);
        }
        else
        {
            GameObjectId = null;
        }
    }

    public readonly GameObject? GameObject => GetGameObject();

    public GameObject? GetGameObject(Scene scene)
    {
        if(!GameObjectId.HasValue)
            return null;

        return scene.GetCache().FindAndCacheGameObject(GameObjectId.Value);
    }

    public GameObject? GetGameObject() => GetGameObject(Game.ActiveScene);


    public static implicit operator GameObject?(NetGameObject netGameObject) => netGameObject.GameObject;
    public static implicit operator NetGameObject(GameObject gameObject) => new(gameObject);

    public override int GetHashCode() => GameObjectId.GetHashCode();

    public override bool Equals(object? obj) => obj is NetGameObject other && this == other;

    public static bool operator ==(NetGameObject left, NetGameObject right) => left.GameObjectId == right.GameObjectId;
    public static bool operator !=(NetGameObject left, NetGameObject right) => !(left == right);
}
