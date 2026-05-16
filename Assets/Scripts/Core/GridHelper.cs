using UnityEngine;

/// <summary>
/// グリッド座標とワールド座標の相互変換を行うユーティリティ
/// タイル・スプライトはセル中心が整数座標となる前提
/// </summary>
public static class GridHelper
{
    /// <summary>
    /// ワールド座標を含むグリッドセルの座標を返す
    /// </summary>
    /// <param name="world">ワールド座標</param>
    /// <returns>グリッド座標</returns>
    public static Vector2Int WorldToGrid(Vector2 world)
    {
        return new Vector2Int(
            Mathf.FloorToInt(world.x + 0.5f),
            Mathf.FloorToInt(world.y + 0.5f));
    }

    /// <summary>
    /// グリッド上の矩形（左上基点・占有サイズ）の中心ワールド座標を返す
    /// </summary>
    /// <param name="gridPos">グリッド座標（左上基点）</param>
    /// <param name="size">占有サイズ</param>
    /// <returns>ワールド座標</returns>
    public static Vector2 GridToWorld(Vector2Int gridPos, Vector2Int size)
    {
        return new Vector2(
            gridPos.x + size.x * 0.5f - 0.5f,
            gridPos.y + size.y * 0.5f - 0.5f);
    }
}
