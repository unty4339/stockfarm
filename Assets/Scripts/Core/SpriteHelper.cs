using UnityEngine;

/// <summary>
/// 実行時に単色スプライトを生成するユーティリティ（Addressables不要の簡易ビジュアル用）
/// </summary>
public static class SpriteHelper
{
    /// <summary>
    /// 指定した色の1x1ピクセルスプライトを生成する
    /// </summary>
    /// <param name="color">スプライトの色</param>
    /// <returns>生成したスプライト</returns>
    public static Sprite CreateColorSprite(Color color)
    {
        var texture = new Texture2D(1, 1);
        texture.filterMode = FilterMode.Point;
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
    }
}
