using UnityEngine;

/// <summary>
/// 給餌桶。牛が食料を消費する設備
/// </summary>
public class FeedingTrough : EquipmentBase
{
    public override EquipmentType Type => EquipmentType.FeedingTrough;
    public override Vector2Int Size => Vector2Int.one;
    public override int BuildCost => 80;
    public override int MoodBonus => 10;
    public override MoodType AffectedMoodType => MoodType.Work;
    protected override Color EquipmentColor => new Color(0.9f, 0.75f, 0.3f);

    private const int DefaultMaxFood = 50;

    /// <summary>現在の食料残量</summary>
    public int CurrentFood { get; private set; }
    /// <summary>最大食料量</summary>
    public int MaxFood { get; private set; } = DefaultMaxFood;
    /// <summary>自動補充フラグ（Chestから自動運搬タスクを生成）</summary>
    public bool AutoReload { get; set; } = true;

    /// <summary>
    /// 食料を補充する（MaxFoodを超えない範囲で加算）
    /// </summary>
    /// <param name="amount">補充量</param>
    public void Refill(int amount)
    {
        CurrentFood = Mathf.Min(CurrentFood + amount, MaxFood);
    }

    /// <summary>
    /// 牛が食料を消費する（残量不足の場合はfalseを返す）
    /// </summary>
    /// <param name="amount">消費量</param>
    /// <returns>消費成功でtrue</returns>
    public bool TryConsume(int amount)
    {
        if (CurrentFood < amount) return false;
        CurrentFood -= amount;
        return true;
    }
}
