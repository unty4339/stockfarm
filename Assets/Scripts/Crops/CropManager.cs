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
    private readonly HashSet<Vector2Int> _plantReserved = new();
    private readonly HashSet<Vector2Int> _harvestReserved = new();
    private CropVisualManager _visuals;

    private void Awake()
    {
        Instance = this;
        _visuals = GetComponent<CropVisualManager>();
        if (_visuals == null)
            _visuals = gameObject.AddComponent<CropVisualManager>();
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
            _visuals.RefreshGrowth(tile);
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
        _visuals.Show(tile);
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
        _visuals.Hide(tile.Position);
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
                if (tile.Crop != null || _plantReserved.Contains(pos)) continue;

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
            if (tile.Crop == null || !tile.Crop.IsReadyToHarvest || _harvestReserved.Contains(tile.Position)) continue;

            int dist = Mathf.Abs(tile.Position.x - from.x) + Mathf.Abs(tile.Position.y - from.y);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = tile;
            }
        }

        return nearest;
    }

    /// <summary>
    /// 指定座標の植付けを予約する（既に予約済みの場合はfalseを返す）
    /// </summary>
    /// <param name="pos">予約するタイル座標</param>
    /// <returns>予約成功した場合true</returns>
    public bool TryReservePlanting(Vector2Int pos)
    {
        if (_plantReserved.Contains(pos)) return false;
        _plantReserved.Add(pos);
        return true;
    }

    /// <summary>
    /// 指定座標の植付け予約を解放する
    /// </summary>
    /// <param name="pos">解放するタイル座標</param>
    public void ReleasePlanting(Vector2Int pos) => _plantReserved.Remove(pos);

    /// <summary>
    /// 指定座標の収穫を予約する（既に予約済みの場合はfalseを返す）
    /// </summary>
    /// <param name="pos">予約するタイル座標</param>
    /// <returns>予約成功した場合true</returns>
    public bool TryReserveHarvesting(Vector2Int pos)
    {
        if (_harvestReserved.Contains(pos)) return false;
        _harvestReserved.Add(pos);
        return true;
    }

    /// <summary>
    /// 指定座標の収穫予約を解放する
    /// </summary>
    /// <param name="pos">解放するタイル座標</param>
    public void ReleaseHarvesting(Vector2Int pos) => _harvestReserved.Remove(pos);
}
