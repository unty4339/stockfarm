using UnityEngine;

/// <summary>
/// 手動搾乳スタンド。牛がここで搾乳待機し、牧場主が補助して搾乳を完了させる
/// </summary>
public class ManualMilkingStand : ProcessingEquipmentBase
{
    public override EquipmentType Type => EquipmentType.ManualMilkingStand;
    public override Vector2Int Size => Vector2Int.one;
    public override int BuildCost => 150;
    public override int MoodBonus => 5;
    public override MoodType AffectedMoodType => MoodType.Work;
    protected override Color EquipmentColor => new Color(0.7f, 0.85f, 0.7f);

    // 搾乳スタンドは牛乳を直接生産するため、加工パラメータは搾乳量に依存
    public override ResourceType InputType => ResourceType.Milk;
    public override int InputAmount => 0;
    public override ResourceType OutputType => ResourceType.Milk;
    public override int OutputAmount => 1;
    public override int CycleTicks => 1800;

    /// <summary>搾乳待機中の牛</summary>
    public CowWorker OccupyingCow { get; private set; }
    /// <summary>搾乳結果を受け取るタンク</summary>
    public new LiquidTank OutputTank { get; private set; }

    private void Awake()
    {
        OutputTank = gameObject.AddComponent<LiquidTank>();
        OutputTank.Capacity = 50;
    }

    /// <summary>
    /// 牛を搾乳スタンドに配置する
    /// </summary>
    /// <param name="cow">搾乳する牛</param>
    public void OccupyCow(CowWorker cow)
    {
        OccupyingCow = cow;
    }

    /// <summary>
    /// 牛を搾乳スタンドから解放する
    /// </summary>
    public void ReleaseCow()
    {
        OccupyingCow = null;
    }
}
