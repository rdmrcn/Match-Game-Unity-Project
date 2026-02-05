using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "MatchItems/Game Config")]
public class GameConfig : ScriptableObject
{
    [Header("Board Size")]
    [Range(2, 10)] public int rows = 8;
    [Range(2, 10)] public int columns = 8;

    [Header("Colors")]
    [Range(1, 6)] public int colorCount = 6;

    [Header("Group Thresholds")]
    public int thresholdA = 4;
    public int thresholdB = 7;
    public int thresholdC = 9;

    [Header("Tile Settings")]
    public float tileSize = 120f;
    public GameObject tilePrefab;

    [Header("Tile Color Data")]
    public List<TileColorData> colors = new List<TileColorData>();

    [System.Serializable]
    public class TileColorData
    {
        public string colorName;
        public Sprite defaultIcon;
        public Sprite iconA;
        public Sprite iconB;
        public Sprite iconC;
    }

    public int SafeColorCount { get; set; }

    public Sprite GetIconFor(int colorIndex, int groupCount)
    {
        if (colors == null || colors.Count == 0) return null;

        int safeIndex = Mathf.Clamp(colorIndex, 0, colors.Count - 1);
        var data = colors[safeIndex];

        if (groupCount > thresholdC && data.iconC != null) return data.iconC;
        if (groupCount > thresholdB && data.iconB != null) return data.iconB;
        if (groupCount > thresholdA && data.iconA != null) return data.iconA;

        return data.defaultIcon;
    }

    public Sprite GetIconForCount(int tColorIndex, int count)
    {
        throw new System.NotImplementedException();
    }
}