using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 全作物の成長管理を行うシングルトン
/// </summary>
public class CropManager : MonoBehaviour
{
    /// <summary>シングルトンインスタンス</summary>
    public static CropManager Instance { get; private set; }

    private readonly List<GridTile> _activeCrops = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnTickAdvanced += OnTick;
    }

    private void OnDestroy()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnTickAdvanced -= OnTick;
    }

    /// <summary>
    /// tickごとに全作物の成長を進める
    /// </summary>
    private void OnTick(int tick)
    {
        foreach (var tile in _activeCrops.ToList())
        {
            tile.Crop?.AdvanceGrowth();
        }
    }

    /// <summary>
    /// タイルに作物を登録し成長管理リストに追加する
    /// </summary>
    /// <param name="tile">対象タイル</param>
    /// <param name="type">植え付ける作物の種別</param>
    public void RegisterCrop(GridTile tile, CropType type)
    {
        if (tile == null || tile.Crop != null) return;
        tile.Crop = new CropData(type);
        _activeCrops.Add(tile);
    }

    /// <summary>
    /// タイルの作物を除去し成長管理リストから削除する
    /// </summary>
    /// <param name="tile">対象タイル</param>
    public void RemoveCrop(GridTile tile)
    {
        if (tile == null) return;
        tile.Crop = null;
        _activeCrops.Remove(tile);
    }

    /// <summary>
    /// 指定座標から最も近い空き農業ゾーンタイルを返す
    /// </summary>
    /// <param name="from">探索の基準座標</param>
    /// <returns>最も近い空きタイル、見つからなければnull</returns>
    public GridTile FindNearestEmptyAgricultureTile(Vector2Int from)
    {
        if (ZoneManager.Instance == null || MapManager.Instance == null) return null;

        GridTile nearest = null;
        int minDist = int.MaxValue;

        foreach (var zone in ZoneManager.Instance.Zones)
        {
            if (zone.Type != ZoneType.Agriculture) continue;
            foreach (var pos in zone.TilePositions)
            {
                if (!MapManager.Instance.IsValidPosition(pos)) continue;
                var tile = MapManager.Instance.GetTile(pos);
                if (tile.Crop != null) continue;

                int dist = Mathf.Abs(pos.x - from.x) + Mathf.Abs(pos.y - from.y);
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
    /// 指定座標から最も近い収穫可能タイルを返す
    /// </summary>
    /// <param name="from">探索の基準座標</param>
    /// <returns>最も近い収穫可能タイル、見つからなければnull</returns>
    public GridTile FindNearestHarvestReadyTile(Vector2Int from)
    {
        GridTile nearest = null;
        int minDist = int.MaxValue;

        foreach (var tile in _activeCrops)
        {
            if (tile.Crop == null || !tile.Crop.IsReadyToHarvest) continue;

            int dist = Mathf.Abs(tile.Position.x - from.x) + Mathf.Abs(tile.Position.y - from.y);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = tile;
            }
        }

        return nearest;
    }
}
