using UnityEngine;

/// <summary>
/// 足踏み式攪拌機。牛乳10単位→バター1個に加工する（6000tick/サイクル）
/// </summary>
public class Churn : ProcessingEquipmentBase
{
    public override EquipmentType Type => EquipmentType.Churn;
    public override Vector2Int Size => Vector2Int.one;
    public override int BuildCost => 300;
    public override int MoodBonus => 5;
    public override MoodType AffectedMoodType => MoodType.Work;
    protected override Color EquipmentColor => new Color(0.7f, 0.55f, 0.35f);

    public override ResourceType InputType => ResourceType.Milk;
    public override int InputAmount => 10;
    public override ResourceType OutputType => ResourceType.Butter;
    public override int OutputAmount => 1;
    public override int CycleTicks => 6000;

    private void Awake()
    {
        InputTank = gameObject.AddComponent<LiquidTank>();
        InputTank.Capacity = 50;
    }
}
