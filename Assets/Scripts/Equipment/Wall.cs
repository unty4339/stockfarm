using UnityEngine;

/// <summary>
/// 壁設備。タイル種別をWallに変更して部屋の境界を形成する
/// </summary>
public class Wall : EquipmentBase
{
    public override EquipmentType Type => EquipmentType.Wall;
    public override Vector2Int Size => Vector2Int.one;
    public override int BuildCost => 10;
    public override int MoodBonus => 0;
    public override MoodType AffectedMoodType => MoodType.Work;
    protected override Color EquipmentColor => new Color(0.3f, 0.3f, 0.3f);

    /// <summary>
    /// 配置時にタイル種別をWallに変更する
    /// </summary>
    /// <param name="position">設置グリッド座標</param>
    public override void Place(Vector2Int position)
    {
        base.Place(position);
        SetTileType(position, TileType.Wall);
    }

    /// <summary>
    /// 撤去時にタイル種別をGroundに戻す
    /// </summary>
    public override void Remove()
    {
        SetTileType(GridPosition, TileType.Ground);
        base.Remove();
    }

    private void SetTileType(Vector2Int pos, TileType type)
    {
        var tile = MapManager.Instance?.GetTileOrNull(pos);
        if (tile != null) tile.Type = type;
    }
}
