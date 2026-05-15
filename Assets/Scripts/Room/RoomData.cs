using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 柵・壁・ゲートで囲まれた閉領域（部屋）を表すデータクラス
/// </summary>
public class RoomData
{
    /// <summary>部屋内タイル座標の集合</summary>
    public HashSet<Vector2Int> TilePositions { get; } = new HashSet<Vector2Int>();
    /// <summary>部屋内に配置された設備の一覧</summary>
    public List<EquipmentBase> ContainedEquipments { get; } = new List<EquipmentBase>();
    /// <summary>現在のムード値</summary>
    public MoodData CurrentMood { get; set; } = new MoodData();
    /// <summary>部屋内の廃棄物マーカー一覧</summary>
    public List<WasteMarker> WasteMarkers { get; } = new List<WasteMarker>();

    /// <summary>
    /// 指定座標が部屋内に含まれるかを返す
    /// </summary>
    /// <param name="position">確認する座標</param>
    /// <returns>含まれる場合true</returns>
    public bool Contains(Vector2Int position)
    {
        return TilePositions.Contains(position);
    }

    /// <summary>
    /// 部屋内に指定タイプの設備が存在するかを返す
    /// </summary>
    /// <param name="type">確認する設備タイプ</param>
    /// <returns>存在する場合true</returns>
    public bool HasEquipment(EquipmentType type)
    {
        foreach (var eq in ContainedEquipments)
        {
            if (eq.Type == type) return true;
        }
        return false;
    }

    /// <summary>
    /// 部屋内の指定タイプの設備を全て返す
    /// </summary>
    /// <param name="type">取得する設備タイプ</param>
    /// <returns>設備リスト</returns>
    public List<EquipmentBase> GetEquipments(EquipmentType type)
    {
        var result = new List<EquipmentBase>();
        foreach (var eq in ContainedEquipments)
        {
            if (eq.Type == type) result.Add(eq);
        }
        return result;
    }
}
