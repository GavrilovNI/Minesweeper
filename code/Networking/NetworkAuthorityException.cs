using Sandbox;
using System;

namespace Minesweeper.Networking;

public class NetworkAuthorityException : Exception
{
    public enum NetworkStatus
    {
        NotProxy,
        Proxy,
    }

    public readonly NetworkStatus Status;

    public NetworkAuthorityException(NetworkStatus status) : base($"Called by client that doesn't fit {status} status.")
    {
        Status = status;
    }

    public static void ThrowIfProxy(Component component)
    {
        if(component.IsProxy)
            throw new NetworkAuthorityException(NetworkStatus.Proxy);
    }
    public static void ThrowIfProxy(GameObject gameObject)
    {
        if(gameObject.IsProxy)
            throw new NetworkAuthorityException(NetworkStatus.Proxy);
    }

    public static void ThrowIfNotProxy(Component component)
    {
        if(!component.IsProxy)
            throw new NetworkAuthorityException(NetworkStatus.NotProxy);
    }
    public static void ThrowIfNotProxy(GameObject gameObject)
    {
        if(!gameObject.IsProxy)
            throw new NetworkAuthorityException(NetworkStatus.NotProxy);
    }

    public static void ThrowIfDoesNotFitNetworkStatus(GameObject gameObject, NetworkStatus status)
    {
        if(GetStatus(gameObject.Network) != status)
            throw new NetworkAuthorityException(status);
    }
    public static void ThrowIfDoesNotFitNetworkStatus(Component component, NetworkStatus status)
    {
        if(GetStatus(component.Network) != status)
            throw new NetworkAuthorityException(status);
    }

    protected static NetworkStatus GetStatus(GameObject.NetworkAccessor network) =>
        network.IsProxy ? NetworkStatus.Proxy : NetworkStatus.NotProxy;
}
