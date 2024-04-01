using Sandbox;

namespace Minesweeper.Players;

public class Player : Component
{
    [Broadcast(NetPermission.HostOnly)]
    public void SetTransform(Transform transform)
    {
        Transform.World = transform;
    }
}
