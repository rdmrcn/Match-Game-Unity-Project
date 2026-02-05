// ===============================
// DeadlockSolver.cs (UPDATED - SMART SHUFFLE)
// ===============================
using System.Collections.Generic;
using UnityEngine;

public class DeadlockSolver
{
    private readonly Tile[,] _grid;
    private readonly int _rows;
    private readonly int _columns;
    private readonly GameConfig _config;

    // Reuse list to avoid GC
    private readonly List<int> _colorPool = new List<int>(256);

    public DeadlockSolver(Tile[,] grid, int rows, int cols, GameConfig cfg)
    {
        _grid = grid;
        _rows = rows;
        _columns = cols;
        _config = cfg;
    }

    /// <summary>
    /// Returns true if deadlock was detected and resolved.
    /// Returns false if there was already a blastable move.
    /// </summary>
    public bool ResolveDeadlock(BoardAnalyzer analyzer)
    {
        if (analyzer == null) return false;

        // ✅ If we already have a move, do nothing.
        if (analyzer.HasAnyBlastableMove())
            return false;

        // ✅ 1) Shuffle colors among existing tiles (tile positions stay same)
        ShuffleColorsKeepTiles();

        // ✅ 2) If shuffle accidentally created a move -> just update icons and finish
        if (analyzer.HasAnyBlastableMove())
        {
            analyzer.UpdateAllGroupIcons();
            return true;
        }

        // ✅ 3) Guarantee a move by forcing an adjacent same-color pair
        ForceCreateBlastablePair();

        // ✅ 4) Recalculate all group icons properly after the change
        analyzer.UpdateAllGroupIcons();

        return true;
    }

    // ---------------------------------------------------------
    // 1) Shuffle colors while keeping tiles in the same places
    // ---------------------------------------------------------
    private void ShuffleColorsKeepTiles()
    {
        if (_grid == null || _config == null) return;

        _colorPool.Clear();

        // collect all tile colors
        for (int y = 0; y < _rows; y++)
        {
            for (int x = 0; x < _columns; x++)
            {
                Tile t = _grid[y, x];
                if (t != null)
                    _colorPool.Add(t.colorIndex);
            }
        }

        if (_colorPool.Count <= 1) return;

        // Fisher-Yates shuffle
        for (int i = _colorPool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (_colorPool[i], _colorPool[j]) = (_colorPool[j], _colorPool[i]);
        }

        // apply shuffled colors back to tiles
        int idx = 0;
        for (int y = 0; y < _rows; y++)
        {
            for (int x = 0; x < _columns; x++)
            {
                Tile t = _grid[y, x];
                if (t == null) continue;

                t.colorIndex = _colorPool[idx++];

                // show default icon for now (group icons will be recalculated later)
                Sprite icon = _config.GetIconForCount(t.colorIndex, 1);
                t.SetSprite(icon);
            }
        }
    }

    // ---------------------------------------------------------
    // 2) Guarantee at least one blastable group (size >= 2)
    // by forcing two adjacent tiles to the same color.
    // ---------------------------------------------------------
    private void ForceCreateBlastablePair()
    {
        if (_grid == null || _config == null) return;

        int safeColorCount = _config.SafeColorCount;
        if (safeColorCount <= 0) return;

        // Choose a random color
        int forcedColor = Random.Range(0, safeColorCount);

        // Find a random adjacent pair of non-null tiles
        if (!TryPickRandomAdjacentPair(out int ax, out int ay, out int bx, out int by))
        {
            // fallback: scan for any adjacent pair
            if (!TryPickFirstAdjacentPair(out ax, out ay, out bx, out by))
                return; // board is too small or invalid
        }

        Tile a = _grid[ay, ax];
        Tile b = _grid[by, bx];
        if (a == null || b == null) return;

        // Force them to same color
        a.colorIndex = forcedColor;
        b.colorIndex = forcedColor;

        // Give them an icon as if group size is 2 (blastable)
        Sprite icon = _config.GetIconForCount(forcedColor, 2);
        a.SetSprite(icon);
        b.SetSprite(icon);
    }

    // ---------------------------------------------------------
    // Helper: pick a random adjacent pair (up/down/left/right)
    // ---------------------------------------------------------
    private bool TryPickRandomAdjacentPair(out int ax, out int ay, out int bx, out int by)
    {
        ax = ay = bx = by = 0;

        // collect candidate pairs
        // (we keep it lightweight: try random attempts)
        const int attempts = 64;

        for (int i = 0; i < attempts; i++)
        {
            int x = Random.Range(0, _columns);
            int y = Random.Range(0, _rows);

            Tile t = _grid[y, x];
            if (t == null) continue;

            // pick random direction
            int dir = Random.Range(0, 4);
            int nx = x, ny = y;

            switch (dir)
            {
                case 0: nx = x + 1; ny = y; break; // right
                case 1: nx = x - 1; ny = y; break; // left
                case 2: nx = x; ny = y + 1; break; // up
                case 3: nx = x; ny = y - 1; break; // down
            }

            if (nx < 0 || nx >= _columns || ny < 0 || ny >= _rows) continue;

            Tile n = _grid[ny, nx];
            if (n == null) continue;

            ax = x; ay = y;
            bx = nx; by = ny;
            return true;
        }

        return false;
    }
    
    // Fallback: scan board to find the first adjacent pair
    private bool TryPickFirstAdjacentPair(out int ax, out int ay, out int bx, out int by)
    {
        ax = ay = bx = by = 0;

        for (int y = 0; y < _rows; y++)
        {
            for (int x = 0; x < _columns; x++)
            {
                Tile t = _grid[y, x];
                if (t == null) continue;

                // right
                if (x + 1 < _columns && _grid[y, x + 1] != null)
                {
                    ax = x; ay = y;
                    bx = x + 1; by = y;
                    return true;
                }

                // up
                if (y + 1 < _rows && _grid[y + 1, x] != null)
                {
                    ax = x; ay = y;
                    bx = x; by = y + 1;
                    return true;
                }
            }
        }

        return false;
    }
}
