using UnityEngine;

/// <summary>
/// 自動搾乳スタンド。牛が入ると自動で搾乳を実行する
/// </summary>
public class AutoMilkingStand : ProcessingEquipmentBase
{
    public override EquipmentType Type => EquipmentType.AutoMilkingStand;
    public override Vector2Int Size => new Vector2Int(2, 2);
    public override int BuildCost => 500;
    public override int MoodBonus => 10;
    public override MoodType AffectedMoodType => MoodType.Work;
    protected override Color EquipmentColor => new Color(0.4f, 0.75f, 0.5f);

    public override ResourceType InputType => ResourceType.Milk;
    public override int InputAmount => 0;
    public override ResourceType OutputType => ResourceType.Milk;
    public override int OutputAmount => 1;
    public override int CycleTicks => 20;

    /// <summary>搾乳待機中の牛</summary>
    public CowWorker OccupyingCow { get; private set; }

    private void Awake()
    {
        OutputTank = gameObject.AddComponent<LiquidTank>();
        OutputTank.Capacity = 100;
    }

    /// <summary>
    /// 牛を自動搾乳スタンドに配置して搾乳サイクルを開始する
    /// </summary>
    /// <param name="cow">搾乳する牛</param>
    public void OccupyCow(CowWorker cow)
    {
        OccupyingCow = cow;
        StartCycle(null);
    }

    /// <summary>
    /// 牛を解放する
    /// </summary>
    public void ReleaseCow()
    {
        OccupyingCow = null;
    }
}
