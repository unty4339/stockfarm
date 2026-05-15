using UnityEngine;

/// <summary>
/// ボトル詰め機。牛乳5単位→瓶牛乳1本に加工する（50tick/サイクル）
/// </summary>
public class BottleFiller : ProcessingEquipmentBase
{
    public override EquipmentType Type => EquipmentType.BottleFiller;
    public override Vector2Int Size => Vector2Int.one;
    public override int BuildCost => 200;
    public override int MoodBonus => 5;
    public override MoodType AffectedMoodType => MoodType.Work;
    protected override Color EquipmentColor => new Color(0.6f, 0.75f, 0.9f);

    public override ResourceType InputType => ResourceType.Milk;
    public override int InputAmount => 5;
    public override ResourceType OutputType => ResourceType.BottledMilk;
    public override int OutputAmount => 1;
    public override int CycleTicks => 50;

    private void Awake()
    {
        InputTank = gameObject.AddComponent<LiquidTank>();
        InputTank.Capacity = 30;
    }
}
