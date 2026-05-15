using UnityEngine;

/// <summary>
/// A*経路探索でのタイルコスト提供インタフェース
/// </summary>
public interface IPathCostProvider
{
    /// <summary>
    /// 指定タイルの移動コストを返す（通行不可の場合はint.MaxValueを返す）
    /// </summary>
    /// <param name="position">タイル座標</param>
    /// <returns>移動コスト</returns>
    int GetCost(Vector2Int position);
}
