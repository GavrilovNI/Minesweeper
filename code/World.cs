using Minesweeper.Nodes;
using Minesweeper.Mth;
using System;
using System.Collections.Generic;
using Sandbox;
using System.Linq;

namespace Minesweeper;

public class World : Component
{
    [Property] public Vector2IntB Size { get; private set; } = 8;
    [Property] public float NodeSize { get; private set; } = 50;
    [Property] public float NodeScale { get; private set; } = 1;
    [Property] public float BombChance { get; private set; } = 0.2f;

    [Property] public GameObject NodesParent { get; private set; } = null!;
    [Property] public GameObject SafeNodePrefab { get; private set; } = null!;
    [Property] public GameObject BombNodePrefab { get; private set; } = null!;
    [Property] protected int OpenedNodesCount { get; private set; } = 0;
    [Property] protected int SafeNodesCount { get; private set; } = 0;

    public event Action? OpenedBomb;
    public event Action? OpenedAllSafeNodes;

    protected readonly Dictionary<Vector2IntB, Node> Nodes = new();

    public void Clear()
    {
        foreach(var position in Nodes.Keys.ToList())
            RemoveNode(position, false);

        OpenedNodesCount = 0;
        SafeNodesCount = 0;
    }

    protected override void OnAwake()
    {
        NodesParent ??= GameObject;
    }

    protected virtual void NotifyNeighborsAboutUpdate(Vector2IntB position)
    {
        foreach(var direction in Enum.GetValues<Direction>())
        {
            var neighborNode = GetNode(position + direction.ToVector());
            neighborNode?.OnNeighborChanged(direction.GetOpposite());
        }
    }

    protected virtual void ChangeOrAddNode(Node node, bool notifyNeighbors = true)
    {
        var position = node.Position;
        RemoveNode(position, false);

        Nodes[position] = node;
        node.StateChanged += OnNodeStateChanged;

        if(node.State == NodeState.Opened)
            OpenedNodesCount++;
        if(node is SafeNode)
            SafeNodesCount++;

        if(notifyNeighbors)
            NotifyNeighborsAboutUpdate(position);
    }

    protected virtual bool RemoveNode(Vector2IntB position, bool notifyNeighbors = true)
    {
        bool removed = Nodes.TryGetValue(position, out var currentNode);
        if(removed)
        {
            Nodes.Remove(position);
            currentNode!.StateChanged -= OnNodeStateChanged;

            if(currentNode.State == NodeState.Opened)
                OpenedNodesCount--;
            if(currentNode is SafeNode)
                SafeNodesCount--;

            currentNode.GameObject.Destroy();

            if(notifyNeighbors)
                NotifyNeighborsAboutUpdate(position);
        }

        return removed;
    }

    protected virtual Node SpawnRandomNode(Vector2IntB position, bool enable = true)
    {
        float random = Game.Random.Float();
        var prefab = random < BombChance ? BombNodePrefab : SafeNodePrefab;

        return SpawnNode(prefab, position, enable);
    }

    protected virtual Node SpawnNode(GameObject nodePrefab, Vector2IntB position, bool enable = true)
    {
        var localPosition = position * NodeSize * NodeScale;
        var transform = new Transform(localPosition, Rotation.Identity, NodeScale);
        transform = Transform.Local.ToWorld(transform);
        var gameobject = nodePrefab.Clone(transform, NodesParent, false, $"Node {position}");

        var node = gameobject.Components.Get<Node>(true);
        if(node is null)
            throw new InvalidOperationException("Node prefab doesn't have node component");

        node!.Inititialize(this, position);
        gameobject.Enabled = enable;
        return node;
    }

    public virtual void SpawnNodes()
    {
        Clear();
        for(int x = 0; x < Size.x; ++x)
        {
            for(int y = 0; y < Size.y; ++y)
            {
                var position = new Vector2IntB(x, y);
                var node = SpawnRandomNode(position, false);
                ChangeOrAddNode(node, false);
            }
        }

        /*if(GetNode(Vector2IntB.Zero) is SafeNode)
        {
            var newNode = SpawnNode(BombNodePrefab, Vector2IntB.Zero, false);
            ChangeOrAddNode(newNode);
        }*/

        foreach(var (_, node) in Nodes)
            node.GameObject.Enabled = true;
    }

    public bool HasNode(Vector2IntB position) => Nodes.ContainsKey(position);


    protected virtual void OnNodeStateChanged(Node node, NodeState oldState, NodeState newState)
    {
        var realNode = GetNode(node.Position);
        bool isMineNode = realNode is not null && realNode == node;
        if(!isMineNode)
            return;

        if(newState == NodeState.Opened)
        {
            OpenedNodesCount++;
            if(node is BombNode)
            {
                // prevent first node be the bomb
                if(OpenedNodesCount == 1)
                {
                    var position = node.Position;
                    var newNode = SpawnNode(SafeNodePrefab, position, true);
                    ChangeOrAddNode(newNode);
                    newNode.SetState(NodeState.Opened);
                    return;
                }

                OnOpenedBomb();
                return;
            }

            if(OpenedNodesCount == SafeNodesCount)
            {
                OnOpenedAllSafeNodes();
                return;
            }
        }
    }

    protected virtual void OnOpenedBomb()
    {
        OpenedBomb?.Invoke();
    }

    protected virtual void OnOpenedAllSafeNodes()
    {
        OpenedAllSafeNodes?.Invoke();
    }

    public Node? GetNode(Vector2IntB position) => Nodes!.GetValueOrDefault(position, null);

    public override int GetHashCode() => HashCode.Combine(Nodes);
}
