using UnityEngine;

/// <summary>
/// 圧搾プレス機。牛乳20単位→チーズ1個に加工する（9000tick/サイクル）
/// </summary>
public class CheesePress : ProcessingEquipmentBase
{
    public override EquipmentType Type => EquipmentType.CheesePress;
    public override Vector2Int Size => new Vector2Int(2, 1);
    public override int BuildCost => 400;
    public override int MoodBonus => 5;
    public override MoodType AffectedMoodType => MoodType.Work;
    protected override Color EquipmentColor => new Color(0.85f, 0.72f, 0.45f);

    public override ResourceType InputType => ResourceType.Milk;
    public override int InputAmount => 20;
    public override ResourceType OutputType => ResourceType.Cheese;
    public override int OutputAmount => 1;
    public override int CycleTicks => 9000;

    private void Awake()
    {
        InputTank = gameObject.AddComponent<LiquidTank>();
        InputTank.Capacity = 80;
    }
}
