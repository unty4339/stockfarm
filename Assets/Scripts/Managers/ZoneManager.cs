using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ゾーンの作成・削除とタイル上のビジュアル表示を管理するシングルトン
/// </summary>
public class ZoneManager : MonoBehaviour
{
    /// <summary>シングルトンインスタンス</summary>
    public static ZoneManager Instance { get; private set; }

    private readonly List<ZoneData> _zones = new();
    private readonly Dictionary<Vector2Int, ZoneData> _tileToZone = new();
    private readonly Dictionary<ZoneData, List<GameObject>> _zoneVisuals = new();

    /// <summary>登録済みゾーンの読み取り専用リスト</summary>
    public IReadOnlyList<ZoneData> Zones => _zones;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 指定タイル群にゾーンを作成する
    /// 既にゾーンが割り当てられているタイルが1つでも含まれる場合はキャンセルしてnullを返す
    /// </summary>
    /// <param name="type">ゾーン種別</param>
    /// <param name="tiles">ゾーンを構成するタイル座標</param>
    /// <returns>作成したゾーンデータ、またはキャンセル時null</returns>
    public ZoneData CreateZone(ZoneType type, IEnumerable<Vector2Int> tiles)
    {
        var availableTiles = new List<Vector2Int>();
        foreach (var pos in tiles)
        {
            if (!MapManager.Instance.IsValidPosition(pos)) continue;
            if (_tileToZone.ContainsKey(pos)) return null;
            availableTiles.Add(pos);
        }

        if (availableTiles.Count == 0) return null;

        var zone = new ZoneData(type, availableTiles);
        _zones.Add(zone);

        foreach (var pos in availableTiles)
        {
            _tileToZone[pos] = zone;
            MapManager.Instance.GetTile(pos).Zone = zone;
        }

        BuildVisuals(zone);
        return zone;
    }

    /// <summary>
    /// 指定ゾーンを削除し、ビジュアルとタイル参照をクリアする
    /// </summary>
    /// <param name="zone">削除するゾーン</param>
    public void RemoveZone(ZoneData zone)
    {
        if (!_zones.Contains(zone)) return;

        ClearVisuals(zone);

        foreach (var pos in zone.TilePositions)
        {
            _tileToZone.Remove(pos);
            if (MapManager.Instance.IsValidPosition(pos))
                MapManager.Instance.GetTile(pos).Zone = null;
        }

        _zones.Remove(zone);
    }

    /// <summary>
    /// 指定座標のゾーンを返す（ゾーンがなければnull）
    /// </summary>
    /// <param name="position">タイル座標</param>
    /// <returns>ゾーンデータ、またはnull</returns>
    public ZoneData GetZoneAt(Vector2Int position)
    {
        _tileToZone.TryGetValue(position, out var zone);
        return zone;
    }

    private void BuildVisuals(ZoneData zone)
    {
        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;
        foreach (var pos in zone.TilePositions)
        {
            if (pos.x < minX) minX = pos.x;
            if (pos.y < minY) minY = pos.y;
            if (pos.x > maxX) maxX = pos.x;
            if (pos.y > maxY) maxY = pos.y;
        }

        int w = maxX - minX + 1;
        int h = maxY - minY + 1;
        var center = GridHelper.GridToWorld(new Vector2Int(minX, minY), new Vector2Int(w, h));

        var go = new GameObject($"Zone_{zone.Type}");
        go.transform.position = new Vector3(center.x, center.y, 0f);
        go.transform.localScale = new Vector3(w, h, 1f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteHelper.CreateColorSprite(Color.white);
        sr.color = GetZoneColor(zone.Type);
        sr.sortingOrder = 1;

        _zoneVisuals[zone] = new List<GameObject> { go };
    }

    private void ClearVisuals(ZoneData zone)
    {
        if (!_zoneVisuals.TryGetValue(zone, out var visuals)) return;

        foreach (var go in visuals)
        {
            if (go != null) Destroy(go);
        }

        _zoneVisuals.Remove(zone);
    }

    private static Color GetZoneColor(ZoneType type) => type switch
    {
        ZoneType.Storage => new Color(1f, 0.85f, 0.2f, 0.35f),
        ZoneType.Agriculture => new Color(0.2f, 0.85f, 0.3f, 0.35f),
        _ => new Color(1f, 1f, 1f, 0.35f),
    };
}
