using Sandbox;

namespace Minesweeper.Nodes;

public class NodeColorer : Component
{
    [Property] protected Node Node { get; set; } = null!;
    [Property] protected ModelRenderer ModelRenderer { get; set; } = null!;
    [Property] protected Color ClosedColor { get; set; } = Color.Gray;
    [Property] protected Color BlockedColor { get; set; } = Color.Gray;
    [Property] protected Color OpenedColor { get; set; } = Color.Yellow;
    [Property] protected Color DefaultColor { get; set; } = Color.Gray;


    protected override void OnEnabled()
    {
        Node.StateChanged += OnNodeStateChanged;
        ApplyColor();
    }

    protected override void OnDisabled()
    {
        Node.StateChanged -= OnNodeStateChanged;
    }

    protected virtual void OnNodeStateChanged(Node node, NodeState oldState, NodeState newState)
    {
        ApplyColor();
    }

    protected virtual void ApplyColor()
    {
        ModelRenderer.Tint = GetColor();
    }

    protected virtual Color GetColor()
    {
        return Node.State switch
        {
            NodeState.Closed => ClosedColor,
            NodeState.Blocked => BlockedColor,
            NodeState.Opened => OpenedColor,
            _ => DefaultColor,
        };
    }
}
