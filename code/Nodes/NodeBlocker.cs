using Sandbox;

namespace Minesweeper.Nodes;

public class NodeBlocker : Component
{
    [Property] protected CameraComponent Camera { get; set; } = null!;

    protected override void OnUpdate()
    {
        if(Input.Pressed("attack1"))
            ClickOnNode();
    }

    protected virtual void ClickOnNode()
    {
        var ray = Camera.ScreenPixelToRay(Mouse.Position);
        var traceResult = Scene.Trace.Ray(ray, 100000f).WithTag("node").Run();

        bool hit = traceResult.Hit;
        Gizmo.Draw.Color = hit ? Color.Green : Color.Red;
        Gizmo.Draw.Line(traceResult.StartPosition, traceResult.EndPosition);

        if(hit)
        {
            var node = traceResult.GameObject.Components.Get<Node>();
            if(!node.IsValid())
                return;

            if(node.State == NodeState.Blocked)
                node.SetState(NodeState.Closed);
            else if(node.State == NodeState.Closed)
                node.SetState(NodeState.Blocked);

        }
    }
}
