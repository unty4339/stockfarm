using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// マップ上のアイテム運搬と保管ゾーン管理を担うシングルトン
/// </summary>
public class StorageManager : MonoBehaviour
{
    /// <summary>シングルトンインスタンス</summary>
    public static StorageManager Instance { get; private set; }

    private readonly HashSet<Vector2Int> _pickupReserved = new();

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 指定座標から最も近い、保管ゾーン外のアイテム配置タイルを返す
    /// </summary>
    /// <param name="from">探索の基準座標</param>
    /// <returns>最も近い対象タイル、見つからなければnull</returns>
    public GridTile FindNearestPickupTile(Vector2Int from)
    {
        if (MapManager.Instance == null) return null;

        GridTile nearest = null;
        int minDist = int.MaxValue;
        var tiles = MapManager.Instance.Tiles;
        int w = MapManager.Instance.Width;
        int h = MapManager.Instance.Height;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                var tile = tiles[x, y];
                if (tile.PlacedItem == null) continue;
                if (tile.PlacedItem.SellFlag) continue;
                if (tile.Zone?.Type == ZoneType.Storage) continue;
                if (_pickupReserved.Contains(tile.Position)) continue;

                int dist = Mathf.Abs(x - from.x) + Mathf.Abs(y - from.y);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = tile;
                }
            }
        }

        return nearest;
    }

    /// <summary>
    /// 指定座標から最も近い、設備が置かれていない保管ゾーン内タイル座標とゾーンデータを返す
    /// forItemType を指定した場合は、そのアイテムを受け入れ可能なタイルのみを対象とする
    /// （空タイル、または同種スタック可能アイテムが満杯でないタイル）
    /// </summary>
    /// <param name="from">探索の基準座標</param>
    /// <param name="forItemType">フィルタするアイテム種別（nullなら設備なしタイルを全て対象）</param>
    /// <returns>配置先タイル座標とゾーンデータのタプル、存在しなければnull</returns>
    public (Vector2Int pos, ZoneData zone)? FindNearestStorageDestination(Vector2Int from, ResourceType? forItemType = null)
    {
        if (ZoneManager.Instance == null || MapManager.Instance == null) return null;

        (Vector2Int pos, ZoneData zone)? nearest = null;
        int minDist = int.MaxValue;

        foreach (var zone in ZoneManager.Instance.Zones)
        {
            if (zone.Type != ZoneType.Storage) continue;
            foreach (var pos in zone.TilePositions)
            {
                if (!MapManager.Instance.IsValidPosition(pos)) continue;
                var tile = MapManager.Instance.GetTile(pos);
                if (tile.PlacedEquipment != null) continue;

                if (forItemType.HasValue && tile.PlacedItem != null)
                {
                    if (tile.PlacedItem.Type != forItemType.Value) continue;
                    if (tile.PlacedItem.IsStackFull) continue;
                }

                int dist = Mathf.Abs(pos.x - from.x) + Mathf.Abs(pos.y - from.y);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = (pos, zone);
                }
            }
        }

        return nearest;
    }

    /// <summary>
    /// 運搬先ゾーン優先度より劣後する、同種スタック可能アイテムが置かれた収集候補タイルを近い順で返す
    /// 既に予約済みのタイルは除外する
    /// </summary>
    /// <param name="from">探索の基準座標</param>
    /// <param name="type">収集するアイテム種別</param>
    /// <param name="destinationPriority">運搬先ゾーンの優先度</param>
    /// <param name="remainingCapacity">追加で取得できる最大個数</param>
    /// <returns>収集候補タイルのリスト（近い順）</returns>
    public List<GridTile> FindAdditionalPickupTiles(
        Vector2Int from,
        ResourceType type,
        int destinationPriority,
        int remainingCapacity)
    {
        if (MapManager.Instance == null || remainingCapacity <= 0) return new List<GridTile>();

        var candidates = new List<(GridTile tile, int dist)>();
        var tiles = MapManager.Instance.Tiles;
        int w = MapManager.Instance.Width;
        int h = MapManager.Instance.Height;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                var tile = tiles[x, y];
                if (tile.PlacedItem == null) continue;
                if (tile.PlacedItem.Type != type) continue;
                if (tile.PlacedItem.SellFlag) continue;
                if (_pickupReserved.Contains(tile.Position)) continue;

                var zone = tile.Zone;
                if (zone != null && zone.Type == ZoneType.Storage && zone.Priority >= destinationPriority) continue;

                int dist = Mathf.Abs(x - from.x) + Mathf.Abs(y - from.y);
                candidates.Add((tile, dist));
            }
        }

        candidates.Sort((a, b) => a.dist.CompareTo(b.dist));

        var result = new List<GridTile>();
        int accumulated = 0;
        foreach (var (tile, _) in candidates)
        {
            if (accumulated >= remainingCapacity) break;
            result.Add(tile);
            accumulated += tile.PlacedItem.StackCount;
        }

        return result;
    }

    /// <summary>
    /// 指定座標から最も近い、売却フラグ付きアイテムが置かれたタイルを返す
    /// 納品ボックスの占有タイルに置かれているアイテムは対象外
    /// </summary>
    /// <param name="from">探索の基準座標</param>
    /// <returns>最も近い対象タイル、見つからなければnull</returns>
    public GridTile FindNearestSellFlaggedTile(Vector2Int from)
    {
        if (MapManager.Instance == null) return null;

        GridTile nearest = null;
        int minDist = int.MaxValue;
        var tiles = MapManager.Instance.Tiles;
        int w = MapManager.Instance.Width;
        int h = MapManager.Instance.Height;

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                var tile = tiles[x, y];
                if (tile.PlacedItem?.SellFlag != true) continue;
                if (tile.PlacedEquipment is DeliveryBox) continue;
                if (_pickupReserved.Contains(tile.Position)) continue;

                int dist = Mathf.Abs(x - from.x) + Mathf.Abs(y - from.y);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = tile;
                }
            }
        }

        return nearest;
    }

    /// <summary>
    /// 指定座標のアイテム取得を予約する（既に予約済みの場合はfalseを返す）
    /// </summary>
    /// <param name="pos">予約するタイル座標</param>
    /// <returns>予約成功した場合true</returns>
    public bool TryReservePickup(Vector2Int pos)
    {
        if (_pickupReserved.Contains(pos)) return false;
        _pickupReserved.Add(pos);
        return true;
    }

    /// <summary>
    /// 指定座標のアイテム取得予約を解放する
    /// </summary>
    /// <param name="pos">解放するタイル座標</param>
    public void ReleasePickup(Vector2Int pos) => _pickupReserved.Remove(pos);
}
