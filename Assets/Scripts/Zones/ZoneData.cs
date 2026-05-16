using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ゾーンのデータを保持するクラス（MonoBehaviourではない）
/// </summary>
public class ZoneData
{
    /// <summary>ゾーン種別</summary>
    public ZoneType Type { get; }

    /// <summary>ゾーンを構成するタイル座標の集合</summary>
    public HashSet<Vector2Int> TilePositions { get; }

    /// <summary>
    /// 運搬優先度（1〜10、保管ゾーンのみ使用、数値が大きいほど優先度が高い）
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="type">ゾーン種別</param>
    /// <param name="positions">ゾーンを構成するタイル座標</param>
    public ZoneData(ZoneType type, IEnumerable<Vector2Int> positions)
    {
        Type = type;
        TilePositions = new HashSet<Vector2Int>(positions);
        Priority = 1;
    }
}
