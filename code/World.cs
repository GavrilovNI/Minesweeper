using Minesweeper.Nodes;
using Minesweeper.Mth;
using System;
using Sandbox;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using Minesweeper.Networking;

namespace Minesweeper;

public class World : Component
{
    public event Action? OpenedBomb;
    public event Action? OpenedAllSafeNodes;
    public event Action? NodesUpdated;

    [Property] public Vector2IntB Size { get; private set; } = 8;
    [Property] public float NodeSize { get; private set; } = 50;
    [Property] public float NodeScale { get; private set; } = 1;
    [Property] public float BombChance { get; private set; } = 0.2f;

    [Property] public GameObject NodesParent { get; private set; } = null!;
    [Property] public GameObject SafeNodePrefab { get; private set; } = null!;
    [Property] public GameObject BombNodePrefab { get; private set; } = null!;
    [Property] protected int OpenedNodesCount { get; private set; } = 0;
    [Property] protected int SafeNodesCount { get; private set; } = 0;

    [Sync] protected NetDictionary<Vector2IntB, NetComponent> Nodes { get; private set; } = new();

    protected CancellationTokenSource CancellationTokenSource { get; set; } = null!;

    public void Clear()
    {
        NetworkAuthorityException.ThrowIfProxy(this);

        foreach(var position in Nodes.Keys.ToList())
            RemoveNode(position, false);

        OpenedNodesCount = 0;
        SafeNodesCount = 0;
        NodesUpdated?.Invoke();
    }

    protected override void OnAwake()
    {
        NodesParent ??= GameObject;
    }

    protected override void OnEnabled()
    {
        CancellationTokenSource = new();
    }

    protected override void OnDisabled()
    {
        CancellationTokenSource.Cancel();
        CancellationTokenSource.Dispose();
    }

    protected virtual void NotifyNeighborsAboutUpdate(Vector2IntB position)
    {
        NetworkAuthorityException.ThrowIfProxy(this);

        foreach(var direction in Enum.GetValues<Direction>())
        {
            var neighborNode = GetNode(position + direction.ToVector());
            neighborNode?.OnNeighborChanged(direction.GetOpposite());
        }
    }

    protected virtual void ChangeOrAddNode(Node node, bool notifyNeighbors = true)
    {
        NetworkAuthorityException.ThrowIfProxy(this);

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

        NodesUpdated?.Invoke();
    }

    protected virtual bool RemoveNode(Vector2IntB position, bool notifyNeighbors = true)
    {
        NetworkAuthorityException.ThrowIfProxy(this);

        bool removed = Nodes.TryGetValue(position, out var currentNetNode);
        if(removed)
        {
            var currentNode = currentNetNode.GetComponent<Node>()!;

            Nodes.Remove(position);
            currentNode.StateChanged -= OnNodeStateChanged;

            if(currentNode.State == NodeState.Opened)
                OpenedNodesCount--;
            if(currentNode is SafeNode)
                SafeNodesCount--;

            currentNode.GameObject.Destroy();

            if(notifyNeighbors)
                NotifyNeighborsAboutUpdate(position);

            NodesUpdated?.Invoke();
        }

        return removed;
    }

    protected virtual Node SpawnRandomNode(Vector2IntB position, bool enable = true)
    {
        NetworkAuthorityException.ThrowIfProxy(this);

        float random = Game.Random.Float();
        var prefab = random < BombChance ? BombNodePrefab : SafeNodePrefab;

        return SpawnNode(prefab, position, enable);
    }

    public Transform GetNodeWorldTransform(Vector2IntB nodePosition)
    {
        NetworkAuthorityException.ThrowIfProxy(this);

        var localPosition = nodePosition * (NodeSize * NodeScale);
        var transform = new Transform(localPosition, Rotation.Identity, NodeScale);
        transform = Transform.Local.ToWorld(transform);
        return transform;
    }

    protected virtual Node SpawnNode(GameObject nodePrefab, Vector2IntB position, bool enable = true)
    {
        NetworkAuthorityException.ThrowIfProxy(this);

        var transform = GetNodeWorldTransform(position);
        var gameobject = nodePrefab.Clone(transform, NodesParent, false, $"Node {position}");

        var node = gameobject.Components.Get<Node>(true);
        if(node is null)
            throw new InvalidOperationException("Node prefab doesn't have node component");

        node!.Inititialize(this, position);
        gameobject.Enabled = enable;
        node.GameObject.NetworkSpawn();
        return node;
    }

    public virtual async Task SpawnNodes()
    {
        NetworkAuthorityException.ThrowIfProxy(this);

        var token = CancellationTokenSource.Token;

        Clear();
        for(int x = 0; x < Size.x; ++x)
        {
            for(int y = 0; y < Size.y; ++y)
            {
                if(token.IsCancellationRequested)
                    return;

                var position = new Vector2IntB(x, y);
                var node = SpawnRandomNode(position, false);
                ChangeOrAddNode(node, false);

                await Task.Yield();
            }
        }

        foreach(var (_, node) in Nodes)
            node.GetComponent<Node>()!.GameObject.Enabled = true;
    }

    public bool HasNode(Vector2IntB position) => Nodes.ContainsKey(position);


    protected virtual void OnNodeStateChanged(Node node, NodeState oldState, NodeState newState)
    {
        NetworkAuthorityException.ThrowIfProxy(this);

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

    public Node? GetNode(Vector2IntB position)
    {
        if(Nodes.TryGetValue(position, out var nodeComponent))
            return nodeComponent.GetComponent<Node>();
        return null;
    }

    public override int GetHashCode()
    {
        var result = HashCode.Combine(Size, NodeScale);

        foreach(var (_, node) in Nodes)
            result = HashCode.Combine(result, node);

        return result;
    }
}
