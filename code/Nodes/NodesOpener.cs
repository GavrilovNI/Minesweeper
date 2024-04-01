using Minesweeper.Players;
using Sandbox;

namespace Minesweeper.Nodes;

public class NodesOpener : Component, Component.ITriggerListener
{
    [Property] protected Node Node { get; set; } = null!;

    public void OnTriggerEnter(Collider other)
    {
        if(IsProxy)
            return;

        if(!other.Components.Get<Player>().IsValid() || !Node.IsValid())
            return;

        if(Node.State == NodeState.Closed)
            Node.SetState(NodeState.Opened);
    }

    public void OnTriggerExit(Collider other) {}
}
