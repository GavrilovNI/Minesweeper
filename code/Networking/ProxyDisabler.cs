using Sandbox;

namespace Minesweeper.Networking;

public class ProxyDisabler : Component, Component.INetworkSpawn
{
    public void OnNetworkSpawn(Connection owner)
    {
        if(IsProxy)
            GameObject.Enabled = false;
    }

    protected override void OnEnabled()
    {
        if(IsProxy)
            GameObject.Enabled = false;
    }

    protected override void OnStart()
    {
        if(IsProxy)
            GameObject.Enabled = false;
    }
}
