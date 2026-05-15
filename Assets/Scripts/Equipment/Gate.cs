using UnityEngine;

/// <summary>
/// ゲート設備。境界として機能するが移動コスト2で通行可能
/// </summary>
public class Gate : EquipmentBase
{
    public override EquipmentType Type => EquipmentType.Gate;
    public override Vector2Int Size => Vector2Int.one;
    public override int BuildCost => 20;
    public override int MoodBonus => 0;
    public override MoodType AffectedMoodType => MoodType.Work;
    protected override Color EquipmentColor => new Color(0.8f, 0.65f, 0.2f);

    /// <summary>
    /// 配置時にタイル種別をGateに変更する
    /// </summary>
    /// <param name="position">設置グリッド座標</param>
    public override void Place(Vector2Int position)
    {
        base.Place(position);
        var tile = MapManager.Instance?.GetTileOrNull(position);
        if (tile != null) tile.Type = TileType.Gate;
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
