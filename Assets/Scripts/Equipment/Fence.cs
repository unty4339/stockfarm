using UnityEngine;

/// <summary>
/// 柵設備。タイル種別をFenceに変更して部屋の境界を形成する
/// </summary>
public class Fence : EquipmentBase
{
    public override EquipmentType Type => EquipmentType.Fence;
    public override Vector2Int Size => Vector2Int.one;
    public override int BuildCost => 15;
    public override int MoodBonus => 0;
    public override MoodType AffectedMoodType => MoodType.Work;
    protected override Color EquipmentColor => new Color(0.6f, 0.42f, 0.2f);

    /// <summary>
    /// 配置時にタイル種別をFenceに変更する
    /// </summary>
    /// <param name="position">設置グリッド座標</param>
    /// <param name="rotation">配置回転角度（0/90/180/270度）</param>
    public override void Place(Vector2Int position, int rotation = 0)
    {
        var tile = MapManager.Instance?.GetTileOrNull(position);
        if (tile != null) tile.Type = TileType.Fence;
        base.Place(position, rotation);
    }

    /// <summary>
    /// 撤去時にタイル種別をGroundに戻す
    /// </summary>
    public override void Remove()
    {
        var tile = MapManager.Instance?.GetTileOrNull(GridPosition);
        if (tile != null) tile.Type = TileType.Ground;
        base.Remove();
    }
}
