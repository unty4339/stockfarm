using UnityEngine;

/// <summary>
/// 廃棄物（糞尿）のタイルマーカーを表すデータクラス
/// </summary>
public class WasteMarker
{
    /// <summary>マーカーが置かれたタイル座標</summary>
    public Vector2Int TilePosition { get; }
    /// <summary>深刻度（ムード低下量に使用、0で消滅）</summary>
    public int Severity { get; private set; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="tilePosition">配置タイル座標</param>
    /// <param name="severity">初期深刻度</param>
    public WasteMarker(Vector2Int tilePosition, int severity)
    {
        TilePosition = tilePosition;
        Severity = severity;
    }

    /// <summary>
    /// 時間経過による深刻度の減少（0未満にはならない）
    /// </summary>
    /// <param name="amount">減少量</param>
    public void Decay(int amount)
    {
        Severity = Mathf.Max(0, Severity - amount);
    }

    /// <summary>
    /// 消滅済みかどうかを返す
    /// </summary>
    public bool IsGone => Severity <= 0;
}
