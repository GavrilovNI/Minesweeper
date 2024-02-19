using Sandbox;
using System;
using System.Collections.Generic;

namespace Minesweeper.Mth;

public static class LinqExtensions
{
    public static List<T> Shuffle<T>(this IEnumerable<T> enumerable, Random? random = null)
    {
        var list = new List<T>(enumerable);
        random ??= Game.Random;

        int n = list.Count;
        while(n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
        return list;
    }
}
