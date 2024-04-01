using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Minesweeper.GameObjectSystems;

public class CacheSystem : GameObjectSystem
{
    public static CacheSystem Active => Game.ActiveScene.GetCache();

    private readonly Dictionary<Guid, Component> _components = new();
    private readonly Dictionary<Guid, GameObject> _gameObjects = new();

    public CacheSystem(Scene scene) : base(scene)
    {
    }

    public void Cache(Component component)
    {
        if(component.Scene != Scene)
            throw new InvalidOperationException($"{nameof(Component)}'s scene is not equal to {nameof(CacheSystem)}'s scene");

        _components[component.Id] = component;
    }

    public bool TryFindAndCacheComponent<T>(Guid componentId, out T component) where T : Component
    {
        component = FindAndCacheComponent<T>(componentId)!;
        return component is not null;
    }

    public T? FindAndCacheComponent<T>(Guid componentId) where T : Component
    {
        if(TryGetComponent<T>(componentId, out var component))
            return component;

        var foundComponent = Scene.Components.GetAll<Component>(FindMode.EverythingInSelfAndDescendants).FirstOrDefault(c => c!.Id.Equals(componentId), null);
        if(!foundComponent.IsValid())
            return null!;

        Cache(foundComponent);
        return foundComponent as T;
    }

    public bool RemoveComponent(Guid componentId) => _components.Remove(componentId);

    public T GetComponent<T>(Guid componentId) where T : Component
    {
        if(!TryGetComponent<T>(componentId, out var component))
            throw new InvalidOperationException($"{nameof(Component)} with id {componentId} wasn't cached");
        return component;
    }

    public bool TryGetComponent<T>(Guid componentId, out T component) where T : Component
    {
        if(_components.TryGetValue(componentId, out var cachedComponent))
        {
            var isValid = cachedComponent.IsValid && cachedComponent.Scene == Scene;
            if(!isValid)
                _components.Remove(componentId);

            component = (cachedComponent as T)!;
            return isValid && component is not null;
        }

        component = null!;
        return false;
    }


    public void Cache(GameObject gameObject)
    {
        if(gameObject.Scene != Scene)
            throw new InvalidOperationException($"{nameof(GameObject)}'s scene is not equal to {nameof(CacheSystem)}'s scene");

        _gameObjects[gameObject.Id] = gameObject;
    }

    public bool TryFindAndCacheGameObject(Guid gameObjectId, out GameObject gameObject)
    {
        gameObject = FindAndCacheGameObject(gameObjectId);
        return gameObject is not null;
    }

    public GameObject FindAndCacheGameObject(Guid gameObjectId)
    {
        if(TryGetGameObject(gameObjectId, out var gameObject))
            return gameObject;

        gameObject = Scene.GetAllObjects(false).FirstOrDefault(o => o!.Id.Equals(gameObjectId), null);
        if(!gameObject.IsValid())
            return null!;

        Cache(gameObject);
        return gameObject;
    }

    public bool RemoveGameObject(Guid gameObjectId) => _gameObjects.Remove(gameObjectId);

    public GameObject GetGameObject(Guid gameObjectId)
    {
        if(!TryGetGameObject(gameObjectId, out var gameObject))
            throw new InvalidOperationException($"{nameof(GameObject)} with id {gameObjectId} wasn't cached");
        return gameObject;
    }

    public bool TryGetGameObject(Guid gameObjectId, out GameObject gameObject)
    {
        if(_gameObjects.TryGetValue(gameObjectId, out gameObject!))
        {
            var isValid = gameObject.IsValid && gameObject.Scene == Scene;
            if(!isValid)
                _gameObjects.Remove(gameObjectId);
            return isValid;
        }
        return false;
    }
}

public static class CacheSystemSceneExtensions
{
    public static CacheSystem GetCache(this Scene scene) => scene.GetSystem<CacheSystem>();
}
