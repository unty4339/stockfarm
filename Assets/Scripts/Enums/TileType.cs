/// <summary>
/// グリッドタイルの種別
/// </summary>
public enum TileType
{
    /// <summary>地面（移動コスト2）</summary>
    Ground,
    /// <summary>床（移動コスト1）</summary>
    Floor,
    /// <summary>壁（通行不可）</summary>
    Wall,
    /// <summary>柵（通行不可）</summary>
    Fence,
    /// <summary>ゲート（移動コスト2）</summary>
    Gate,
}
