using Minesweeper.Networking;
using Sandbox;

namespace Minesweeper.Nodes;

public class NodeBlocker : Component
{
    [Property] protected CameraComponent Camera { get; set; } = null!;

    protected virtual Vector2 MousePosition => UI.MousePointerUI.MousePosition;

    protected override void OnUpdate()
    {
        if(Input.Pressed("attack1"))
            ClickOnNode();
    }

    protected virtual void ClickOnNode()
    {
        var ray = Camera.ScreenPixelToRay(MousePosition);
        var traceResult = Scene.Trace.Ray(ray, 100000f).WithTag("node").Run();

        bool hit = traceResult.Hit;

        if(hit)
        {
            var node = traceResult.GameObject.Components.Get<Node>();
            if(!node.IsValid())
                return;

            SwitchNodeBlocking(node);
        }
    }

    [Broadcast(NetPermission.OwnerOnly)]
    protected virtual void SwitchNodeBlocking(NetComponent nodeComponent)
    {
        var node = nodeComponent.GetComponent<Node>();
        if(!node.IsValid() || node.IsProxy)
            return;

        if(node.State == NodeState.Blocked)
            node.SetState(NodeState.Closed);
        else if(node.State == NodeState.Closed)
            node.SetState(NodeState.Blocked);
    }
}
