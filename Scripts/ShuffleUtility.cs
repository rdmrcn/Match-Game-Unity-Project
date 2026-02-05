using System.Collections.Generic;
using UnityEngine;

public static class ShuffleUtility
{
    public static void FisherYatesShuffle<T>(IList<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
//shuffle colors
    public static void ApplyColorIndices(List<Tile> tiles, List<int> colorPool)
    {
        int count = Mathf.Min(tiles.Count, colorPool.Count);
        for (int i = 0; i < count; i++)
        {
            tiles[i].colorIndex = colorPool[i];
        }
    }

    public static void BuildColorPoolFromTiles(List<Tile> tiles, List<int> poolOut)
    {
        poolOut.Clear();
        for (int i = 0; i < tiles.Count; i++)
            poolOut.Add(tiles[i].colorIndex);
    }
}