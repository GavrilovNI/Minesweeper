using Sandbox;
using System.Collections.Generic;

namespace Minesweeper.Networking;

public static class NetListExtensions
{
    public static IEnumerable<T> ToEnumerable<T>(this NetList<T> list)
    {
        foreach(var item in list)
            yield return item;
    }
}
