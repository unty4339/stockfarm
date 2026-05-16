using UnityEngine;

/// <summary>
/// 床設備。タイル種別をFloorに変更して移動コストを下げる
/// </summary>
public class Floor : EquipmentBase
{
    public override EquipmentType Type => EquipmentType.Floor;
    public override Vector2Int Size => Vector2Int.one;
    public override int BuildCost => 5;
    public override int MoodBonus => 0;
    public override MoodType AffectedMoodType => MoodType.Work;
    protected override Color EquipmentColor => new Color(0.82f, 0.82f, 0.78f);

    /// <summary>
    /// 配置時にタイル種別をFloorに変更する
    /// </summary>
    /// <param name="position">設置グリッド座標</param>
    /// <param name="rotation">配置回転角度（0/90/180/270度）</param>
    public override void Place(Vector2Int position, int rotation = 0)
    {
        base.Place(position, rotation);
        var tile = MapManager.Instance?.GetTileOrNull(position);
        if (tile != null) tile.Type = TileType.Floor;
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
