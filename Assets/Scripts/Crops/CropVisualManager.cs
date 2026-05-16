using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// タイル上の作物をオレンジ矩形スプライトで表示する
/// </summary>
public class CropVisualManager : MonoBehaviour
{
    private static readonly Color DefaultCropColor = new Color(1f, 0.55f, 0.1f);
    private const float BaseScale = 0.35f;
    private const float GrowthScaleRange = 0.35f;
    private const int SortingOrder = 1;

    private readonly Dictionary<Vector2Int, GameObject> _visuals = new();

    /// <summary>
    /// 作物のビジュアルを表示する
    /// </summary>
    /// <param name="tile">表示対象タイル</param>
    public void Show(GridTile tile)
    {
        if (tile == null || tile.Crop == null) return;

        Hide(tile.Position);

        var go = new GameObject($"Crop_{tile.Position.x}_{tile.Position.y}");
        go.transform.SetParent(transform);
        go.transform.position = new Vector3(tile.Position.x, tile.Position.y, -0.07f);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteHelper.CreateColorSprite(DefaultCropColor);
        sr.sortingOrder = SortingOrder;

        float scale = BaseScale + GrowthScaleRange * tile.Crop.GrowthProgress;
        go.transform.localScale = new Vector3(scale, scale, 1f);

        _visuals[tile.Position] = go;
    }

    /// <summary>
    /// 指定座標の作物ビジュアルを非表示にする
    /// </summary>
    /// <param name="position">タイル座標</param>
    public void Hide(Vector2Int position)
    {
        if (!_visuals.TryGetValue(position, out var go)) return;
        if (go != null) Destroy(go);
        _visuals.Remove(position);
    }

    /// <summary>
    /// 成長進捗に応じてスプライトのスケールを更新する
    /// </summary>
    /// <param name="tile">更新対象タイル</param>
    public void RefreshGrowth(GridTile tile)
    {
        if (tile?.Crop == null) return;
        if (!_visuals.TryGetValue(tile.Position, out var go) || go == null) return;

        float scale = BaseScale + GrowthScaleRange * tile.Crop.GrowthProgress;
        go.transform.localScale = new Vector3(scale, scale, 1f);
    }
}
