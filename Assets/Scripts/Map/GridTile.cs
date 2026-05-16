using UnityEngine;

/// <summary>
/// グリッド上の1マスを表すデータクラス
/// </summary>
public class GridTile
{
    /// <summary>タイル座標</summary>
    public Vector2Int Position { get; }
    /// <summary>タイル種別</summary>
    public TileType Type { get; set; }
    /// <summary>設置されている設備（nullなら空き）</summary>
    public EquipmentBase PlacedEquipment { get; set; }
    /// <summary>廃棄物マーカー（nullなら清潔）</summary>
    public WasteMarker WasteMarker { get; set; }
    /// <summary>タイルに置かれているアイテム（nullなら空き）</summary>
    public TileItem PlacedItem { get; set; }
    /// <summary>タイルが属するゾーン（nullなら未設定）</summary>
    public ZoneData Zone { get; set; }
    /// <summary>タイルに植えられている作物データ（nullなら未植付け）</summary>
    public CropData Crop { get; set; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="position">タイル座標</param>
    /// <param name="type">タイル種別</param>
    public GridTile(Vector2Int position, TileType type = TileType.Ground)
    {
        Position = position;
        Type = type;
    }
}
