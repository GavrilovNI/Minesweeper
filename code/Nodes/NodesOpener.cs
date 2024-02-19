using Sandbox;

namespace Minesweeper.Nodes;

public class NodesOpener : Component, Component.ITriggerListener
{
    [Property] protected Node Node { get; set; } = null!;

    public void OnTriggerEnter(Collider other)
    {
        if(!other.Tags.Has("player") || Node is null)
            return;
        if(Node.State == NodeState.Closed)
            Node.SetState(NodeState.Opened);
    }

    public void OnTriggerExit(Collider other) {}
}
