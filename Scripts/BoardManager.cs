using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private GameConfig config;

    [Header("Parents")]
    [SerializeField] private RectTransform tilesParent; // Board_root

    [Header("Systems")]
    [SerializeField] private BoardAnalyzer analyzer;

    [Header("Blast Settings")]
    [SerializeField] private float popDelay = 0.08f; // pop anim bekleme

    [Header("Sound Settings")]
    [SerializeField] private bool enableSfx = true; // SFX aç/kapat

    public event Action<int> BlastExecuted;

    private Tile[,] grid;
    private int rows;
    private int columns;
    private float tileSize;

    private bool inputLocked;

    private GameController cachedGameController;

    private void Awake()
    {
        cachedGameController = FindObjectOfType<GameController>();
    }

    private void Start()
    {
        if (config == null)
        {
            Debug.LogError("BoardManager: Config is NULL!");
            enabled = false;
            return;
        }

        if (tilesParent == null)
        {
            Debug.LogError("BoardManager: tilesParent is NULL! (Board_root ver)");
            enabled = false;
            return;
        }

        rows = config.rows;
        columns = config.columns;
        tileSize = config.tileSize;

        grid = new Tile[rows, columns];

        GenerateBoard();

        if (analyzer != null)
        {
            analyzer.Setup(grid, rows, columns, config);
            analyzer.UpdateAllGroupIcons();
        }
    }

    // BOARD GENERATION
    private void GenerateBoard()
    {
        float offsetX = ((columns - 1) * tileSize) * 0.5f;
        float offsetY = ((rows - 1) * tileSize) * 0.5f;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                CreateTileAt(x, y, offsetX, offsetY);
            }
        }
    }

    private void CreateTileAt(int x, int y, float offsetX, float offsetY)
    {
        int colorIndex = UnityEngine.Random.Range(0, config.colorCount);
        GameConfig.TileColorData data = config.colors[colorIndex];

        GameObject obj = Instantiate(config.tilePrefab, tilesParent);
        obj.name = $"Tile_{x}_{y}";

        Tile tile = obj.GetComponent<Tile>();
        if (tile == null)
        {
            Debug.LogError("Tile prefab üzerinde Tile scripti yok!");
            Destroy(obj);
            return;
        }

        tile.Init(x, y, colorIndex, data.defaultIcon, this);

        // direkt doğru pozisyon koyma
        tile.SetAnchoredPositionImmediate(GridToAnchoredPos(x, y, offsetX, offsetY));

        grid[y, x] = tile;
    }

    // POSITION HELPERS
    private Vector2 GridToAnchoredPos(int x, int y)
    {
        float offsetX = ((columns - 1) * tileSize) * 0.5f;
        float offsetY = ((rows - 1) * tileSize) * 0.5f;
        return GridToAnchoredPos(x, y, offsetX, offsetY);
    }

    private Vector2 GridToAnchoredPos(int x, int y, float offsetX, float offsetY)
    {
        // UI anchor mantığı: +y yukarı, biz gridde y arttıkça aşağı gitsin istiyoruz
        float px = x * tileSize - offsetX;
        float py = -(y * tileSize - offsetY);
        return new Vector2(px, py);
    }

    // CLICK ENTRY POINT
    public void HandleTileClick(Tile tile)
    {
        if (inputLocked) return;
        if (tile == null) return;
        if (analyzer == null) return;

        // grup bul
        List<Tile> group = analyzer.GetConnectedGroup(tile.x, tile.y);

        // ✅ Group < 2 ise patlama yok -> Move/Life düşsün + ERROR SFX
        if (group == null || group.Count < 2)
        {
            // ERROR SFX (di-dir / X)
            if (enableSfx)
                Sounds.Instance?.PlayError();

            if (cachedGameController == null)
                cachedGameController = FindObjectOfType<GameController>();

            if (cachedGameController != null)
                cachedGameController.ConsumeLife();

            return;
        }

        StartCoroutine(BlastRoutine(group));
    }

    // BLAST + COLLAPSE + REFILL
    private IEnumerator BlastRoutine(List<Tile> group)
    {
        inputLocked = true;

        int removedCount = group.Count;

        // POP SFX (pit / pop) - 1 kere
        if (enableSfx)
            Sounds.Instance?.PlayPop();

        // 1) Grid'den sil + Pop destroy
        for (int i = 0; i < group.Count; i++)
        {
            Tile t = group[i];
            if (t == null) continue;

            int gx = t.x;
            int gy = t.y;

            if (InBounds(gx, gy) && grid[gy, gx] == t)
                grid[gy, gx] = null;

            t.PlayPopAndDestroy(popDelay);
        }

        yield return new WaitForSeconds(popDelay);

        // 2) Collapse
        CollapseBoard();

        // 3) settle bekle
        yield return new WaitUntil(AllTilesSettled);

        // 4) Refill
        RefillBoard();

        // 5) settle bekle
        yield return new WaitUntil(AllTilesSettled);

        // 6) ikon güncelle
        if (analyzer != null)
            analyzer.UpdateAllGroupIcons();

        // 7) event (score/moves)
        BlastExecuted?.Invoke(removedCount);

        inputLocked = false;
    }

    private void CollapseBoard()
    {
        // her column için aşağı doğru doldur
        for (int x = 0; x < columns; x++)
        {
            int writeY = rows - 1;

            for (int y = rows - 1; y >= 0; y--)
            {
                Tile t = grid[y, x];
                if (t == null) continue;

                if (y != writeY)
                {
                    // grid taşı
                    grid[writeY, x] = t;
                    grid[y, x] = null;

                    // tile update
                    t.SetGridPosition(x, writeY);
                    t.SetTargetAnchoredPosition(GridToAnchoredPos(x, writeY));
                }

                writeY--;
            }

            // üst kalanlar null olsun
            for (int y = writeY; y >= 0; y--)
                grid[y, x] = null;
        }
    }

    private void RefillBoard()
    {
        float offsetX = ((columns - 1) * tileSize) * 0.5f;
        float offsetY = ((rows - 1) * tileSize) * 0.5f;

        for (int x = 0; x < columns; x++)
        {
            int missing = 0;
            for (int y = 0; y < rows; y++)
                if (grid[y, x] == null) missing++;

            if (missing == 0) continue;

            int spawnIndex = 0;

            // yukarıdan aşağı boşlara doldur
            for (int y = 0; y < rows; y++)
            {
                if (grid[y, x] != null) continue;

                int colorIndex = UnityEngine.Random.Range(0, config.colorCount);
                GameConfig.TileColorData data = config.colors[colorIndex];

                GameObject obj = Instantiate(config.tilePrefab, tilesParent);
                obj.name = $"Tile_{x}_{y}_NEW";

                Tile tile = obj.GetComponent<Tile>();
                if (tile == null)
                {
                    Destroy(obj);
                    continue;
                }

                tile.Init(x, y, colorIndex, data.defaultIcon, this);

                // spawn yukarıdan gelsin (NEGATIVE grid y)
                int spawnY = -1 - spawnIndex;
                spawnIndex++;

                Vector2 startPos = GridToAnchoredPos(x, spawnY, offsetX, offsetY);
                Vector2 targetPos = GridToAnchoredPos(x, y, offsetX, offsetY);

                tile.SetAnchoredPositionImmediate(startPos);
                tile.SetTargetAnchoredPosition(targetPos);

                grid[y, x] = tile;
            }
        }
    }

    private bool AllTilesSettled()
    {
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < columns; x++)
            {
                Tile t = grid[y, x];
                if (t != null && !t.IsSettled())
                    return false;
            }
        }
        return true;
    }

    private bool InBounds(int x, int y)
    {
        return x >= 0 && x < columns && y >= 0 && y < rows;
    }

    // RESET (GameController için)
    public void ResetBoard()
    {
        StopAllCoroutines();
        inputLocked = false;

        // temizle
        if (grid != null)
        {
            for (int y = 0; y < rows; y++)
            {
                for (int x = 0; x < columns; x++)
                {
                    if (grid[y, x] != null)
                    {
                        Destroy(grid[y, x].gameObject);
                        grid[y, x] = null;
                    }
                }
            }
        }

        // yeniden üret
        GenerateBoard();

        if (analyzer != null)
        {
            analyzer.Setup(grid, rows, columns, config);
            analyzer.UpdateAllGroupIcons();
        }
    }

    // GETTERS (Analyzer için debug gerekirse)
    public Tile[,] GetGridRef() => grid;
    public int GetRows() => rows;
    public int GetColumns() => columns;
}
