using UnityEngine;

/// <summary>
/// 運搬用パレット。自動運搬タスクの中継地点として使用する
/// </summary>
public class TransportPallet : EquipmentBase
{
    public override EquipmentType Type => EquipmentType.TransportPallet;
    public override Vector2Int Size => Vector2Int.one;
    public override int BuildCost => 50;
    public override int MoodBonus => 5;
    public override MoodType AffectedMoodType => MoodType.Work;
    protected override Color EquipmentColor => new Color(0.9f, 0.82f, 0.5f);
}
