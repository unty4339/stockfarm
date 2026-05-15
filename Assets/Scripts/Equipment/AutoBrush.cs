using UnityEngine;

/// <summary>
/// 自動ブラシ機。牛のケアタスク対象設備。幸福度を向上させる
/// </summary>
public class AutoBrush : EquipmentBase
{
    public override EquipmentType Type => EquipmentType.AutoBrush;
    public override Vector2Int Size => Vector2Int.one;
    public override int BuildCost => 250;
    public override int MoodBonus => 15;
    public override MoodType AffectedMoodType => MoodType.Rest;
    protected override Color EquipmentColor => new Color(0.75f, 0.55f, 0.85f);

    private const float HappinessGain = 10f;

    /// <summary>使用中フラグ（1頭ずつ利用）</summary>
    public bool IsInUse { get; private set; }

    /// <summary>
    /// ケアを実行し、ownerのHappinessを増加させる
    /// </summary>
    /// <param name="owner">ケアを受ける牛</param>
    public void PerformCare(CowWorker owner)
    {
        if (IsInUse) return;

        IsInUse = true;
        owner.UpdateHappiness(HappinessGain);
        IsInUse = false;
    }
}
