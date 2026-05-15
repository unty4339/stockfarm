using System;
using UnityEngine;

/// <summary>
/// グリッドマップの状態管理と拡張を担うシングルトン
/// </summary>
public class MapManager : MonoBehaviour
{
    private static MapManager _instance;
    /// <summary>シングルトン参照</summary>
    public static MapManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("MapManager");
                _instance = go.AddComponent<MapManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private const int InitialWidth = 30;
    private const int InitialHeight = 30;
    private const int ExpansionSize = 15;
    private const int ExpansionBaseCost = 500;

    /// <summary>現在のマップ幅</summary>
    public int Width { get; private set; }
    /// <summary>現在のマップ高</summary>
    public int Height { get; private set; }
    /// <summary>グリッドタイルの二次元配列</summary>
    public GridTile[,] Tiles { get; private set; }

    private readonly int[] _expansionCounts = new int[3];
    private GameObject _tilesParent;
    private GameObject[,] _tileObjects;

    private static readonly Color GroundColor = new Color(0.76f, 0.60f, 0.42f);
    private static readonly Color FloorColor = new Color(0.85f, 0.85f, 0.82f);
    private static readonly Color WallColor = new Color(0.3f, 0.3f, 0.3f);
    private static readonly Color FenceColor = new Color(0.6f, 0.42f, 0.2f);
    private static readonly Color GateColor = new Color(0.8f, 0.65f, 0.2f);

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
            return;
        }
        InitializeMap(InitialWidth, InitialHeight);
    }

    private void InitializeMap(int width, int height)
    {
        Width = width;
        Height = height;
        Tiles = new GridTile[width, height];
        _tileObjects = new GameObject[width, height];

        if (_tilesParent != null) Destroy(_tilesParent);
        _tilesParent = new GameObject("Tiles");

        var groundSprite = SpriteHelper.CreateColorSprite(GroundColor);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Tiles[x, y] = new GridTile(new Vector2Int(x, y), TileType.Ground);
                CreateTileObject(x, y, groundSprite);
            }
        }
    }

    private void CreateTileObject(int x, int y, Sprite sprite)
    {
        var go = new GameObject($"Tile_{x}_{y}");
        go.transform.parent = _tilesParent.transform;
        go.transform.position = new Vector3(x, y, 0f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 0;
        _tileObjects[x, y] = go;
    }

    /// <summary>
    /// タイルの表示色を更新する
    /// </summary>
    /// <param name="position">更新するタイル座標</param>
    public void RefreshTileVisual(Vector2Int position)
    {
        if (!IsValidPosition(position)) return;
        var tile = Tiles[position.x, position.y];
        var go = _tileObjects[position.x, position.y];
        if (go == null) return;

        var sr = go.GetComponent<SpriteRenderer>();
        if (sr == null) return;

        Color color = tile.Type switch
        {
            TileType.Ground => GroundColor,
            TileType.Floor => FloorColor,
            TileType.Wall => WallColor,
            TileType.Fence => FenceColor,
            TileType.Gate => GateColor,
            _ => GroundColor,
        };
        sr.sprite = SpriteHelper.CreateColorSprite(color);
    }

    /// <summary>
    /// 指定方向にマップを拡張する
    /// 拡張費用: 500×3^(N-1)、Nはその方向の拡張回数
    /// </summary>
    /// <param name="direction">拡張方向</param>
    /// <returns>拡張成功でtrue</returns>
    public bool TryExpand(MapExpandDirection direction)
    {
        int cost = GetExpansionCost(direction);
        if (!EconomyManager.Instance.TrySpendFunds(cost)) return false;

        _expansionCounts[(int)direction]++;
        ExpandMap(direction);
        return true;
    }

    /// <summary>
    /// 指定方向の次回拡張費用を返す: 500×3^(N-1)
    /// </summary>
    /// <param name="direction">拡張方向</param>
    /// <returns>拡張費用</returns>
    public int GetExpansionCost(MapExpandDirection direction)
    {
        int n = _expansionCounts[(int)direction] + 1;
        return Mathf.RoundToInt(ExpansionBaseCost * Mathf.Pow(3f, n - 1));
    }

    /// <summary>
    /// 指定座標のGridTileを返す（範囲外の場合は例外）
    /// </summary>
    /// <param name="position">タイル座標</param>
    /// <returns>タイルデータ</returns>
    public GridTile GetTile(Vector2Int position)
    {
        if (!IsValidPosition(position))
            throw new ArgumentOutOfRangeException(nameof(position), $"マップ範囲外の座標: {position}");
        return Tiles[position.x, position.y];
    }

    /// <summary>
    /// 指定座標のGridTileを返す（範囲外の場合はnull）
    /// </summary>
    /// <param name="position">タイル座標</param>
    /// <returns>タイルデータまたはnull</returns>
    public GridTile GetTileOrNull(Vector2Int position)
    {
        if (!IsValidPosition(position)) return null;
        return Tiles[position.x, position.y];
    }

    /// <summary>
    /// 指定座標がマップ範囲内かを返す
    /// </summary>
    /// <param name="position">確認する座標</param>
    /// <returns>範囲内の場合true</returns>
    public bool IsValidPosition(Vector2Int position)
    {
        return position.x >= 0 && position.x < Width && position.y >= 0 && position.y < Height;
    }

    private void ExpandMap(MapExpandDirection direction)
    {
        // TODO: 実際の拡張処理（グリッド配列のリサイズとビジュアル生成）
        Debug.Log($"[MapManager] マップを{direction}方向に拡張（未実装）");
    }
}
