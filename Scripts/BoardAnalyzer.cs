using System.Collections.Generic;
using UnityEngine;

public class BoardAnalyzer : MonoBehaviour
{
    private Tile[,] grid;
    private int rows;
    private int columns;
    private GameConfig config;

    // Visited mark system
    private int[,] visitMark;
    private int currentMark = 1;

    // Reused buffers
    private readonly List<Tile> tempGroup = new List<Tile>(256);
    private readonly Stack<Tile> stack = new Stack<Tile>(256);

    public void Setup(Tile[,] gridRef, int r, int c, GameConfig cfg)
    {
        grid = gridRef;
        rows = r;
        columns = c;
        config = cfg;

        if (grid == null)
        {
            Debug.LogError("BoardAnalyzer.Setup: grid is null!");
            return;
        }

        visitMark = new int[rows, columns];
        currentMark = 1;
    }

   
    // PUBLIC API
  
    public List<Tile> GetConnectedGroup(int startX, int startY)
    {
        tempGroup.Clear();

        if (grid == null) return tempGroup;
        if (!InBounds(startX, startY)) return tempGroup;

        Tile start = grid[startY, startX];
        if (start == null) return tempGroup;

        BeginNewMark();

        int targetColor = start.colorIndex;

        stack.Clear();
        stack.Push(start);
        SetVisited(startX, startY);

        while (stack.Count > 0)
        {
            Tile t = stack.Pop();
            if (t == null) continue;

            tempGroup.Add(t);

            TryPushNeighbor(t.x + 1, t.y, targetColor);
            TryPushNeighbor(t.x - 1, t.y, targetColor);
            TryPushNeighbor(t.x, t.y + 1, targetColor);
            TryPushNeighbor(t.x, t.y - 1, targetColor);
        }

        return tempGroup;
    }

    public void UpdateAllGroupIcons()
    {
        if (grid == null || config == null) return;
        if (config.colors == null || config.colors.Count == 0) return;

        BeginNewMark();

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                if (IsVisited(x, y)) continue;

                Tile start = grid[y, x];
                if (start == null) continue;

                List<Tile> group = GetConnectedGroup_Internal(x, y, start.colorIndex);
                int count = group.Count;

                Sprite icon = PickIcon(start.colorIndex, count);

                for (int i = 0; i < group.Count; i++)
                {
                    if (group[i] != null)
                        group[i].SetSprite(icon);
                }
            }
        }
    }

    public bool HasAnyBlastableMove()
    {
        if (grid == null) return false;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                Tile t = grid[y, x];
                if (t == null) continue;

                int c = t.colorIndex;

                // only need check right + down
                if (x + 1 < columns && grid[y, x + 1] != null && grid[y, x + 1].colorIndex == c) return true;
                if (y + 1 < rows && grid[y + 1, x] != null && grid[y + 1, x].colorIndex == c) return true;
            }
        }

        return false;
    }

    
    // INTERNAL GROUP (NO MARK RESET!)
   
    private List<Tile> GetConnectedGroup_Internal(int startX, int startY, int targetColor)
    {
        tempGroup.Clear();
        stack.Clear();

        Tile start = grid[startY, startX];
        if (start == null) return tempGroup;

        stack.Push(start);
        SetVisited(startX, startY);

        while (stack.Count > 0)
        {
            Tile t = stack.Pop();
            if (t == null) continue;

            tempGroup.Add(t);

            TryPushNeighbor(t.x + 1, t.y, targetColor);
            TryPushNeighbor(t.x - 1, t.y, targetColor);
            TryPushNeighbor(t.x, t.y + 1, targetColor);
            TryPushNeighbor(t.x, t.y - 1, targetColor);
        }

        return tempGroup;
    }

    private void TryPushNeighbor(int nx, int ny, int targetColor)
    {
        if (!InBounds(nx, ny)) return;
        if (IsVisited(nx, ny)) return;

        Tile n = grid[ny, nx];
        if (n == null) return;
        if (n.colorIndex != targetColor) return;

        SetVisited(nx, ny);
        stack.Push(n);
    }

    
    // ICON PICKER  A/B/C/D
  
    private Sprite PickIcon(int colorIndex, int groupCount)
    {
        if (config == null || config.colors == null || config.colors.Count == 0)
            return null;

        colorIndex = Mathf.Clamp(colorIndex, 0, config.colors.Count - 1);

        GameConfig.TileColorData data = config.colors[colorIndex];

        Sprite defaultIcon = data.defaultIcon;
        Sprite iconA = data.iconA;
        Sprite iconB = data.iconB;
        Sprite iconC = data.iconC;

        int A = config.thresholdA;
        int B = config.thresholdB;
        int C = config.thresholdC;

        // PDF: more than A => iconA, more than B => iconB, more than C => iconC
        if (groupCount > C) return iconC != null ? iconC : (iconB != null ? iconB : (iconA != null ? iconA : defaultIcon));
        if (groupCount > B) return iconB != null ? iconB : (iconA != null ? iconA : defaultIcon);
        if (groupCount > A) return iconA != null ? iconA : defaultIcon;

        return defaultIcon;
    }


    // VISITED MARK SYSTEM
    
    private void BeginNewMark()
    {
        currentMark++;

        if (currentMark == int.MaxValue)
        {
            currentMark = 1;
            for (int y = 0; y < rows; y++)
                for (int x = 0; x < columns; x++)
                    visitMark[y, x] = 0;
        }
    }

    private bool IsVisited(int x, int y)
    {
        return visitMark[y, x] == currentMark;
    }

    private void SetVisited(int x, int y)
    {
        visitMark[y, x] = currentMark;
    }

    private bool InBounds(int x, int y)
    {
        return x >= 0 && x < columns && y >= 0 && y < rows;
    }
}
