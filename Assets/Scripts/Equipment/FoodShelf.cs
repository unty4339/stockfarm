using UnityEngine;

/// <summary>
/// 食料棚。牧場主が栄養ペレットを取得する設備
/// </summary>
public class FoodShelf : EquipmentBase
{
    public override EquipmentType Type => EquipmentType.FoodShelf;
    public override Vector2Int Size => Vector2Int.one;
    public override int BuildCost => 60;
    public override int MoodBonus => 5;
    public override MoodType AffectedMoodType => MoodType.Work;
    protected override Color EquipmentColor => new Color(0.85f, 0.7f, 0.35f);

    private const int DefaultCapacity = 30;

    /// <summary>現在の保管量</summary>
    public int StoredAmount { get; private set; }
    /// <summary>最大保管量</summary>
    public int Capacity { get; private set; } = DefaultCapacity;

    /// <summary>
    /// 食料を補充する
    /// </summary>
    /// <param name="amount">補充量</param>
    public void Refill(int amount)
    {
        StoredAmount = Mathf.Min(StoredAmount + amount, Capacity);
    }

    /// <summary>
    /// 牧場主が食料を取得する（残量不足の場合はfalseを返す）
    /// </summary>
    /// <param name="amount">取得量</param>
    /// <param name="food">取得した食料リソース</param>
    /// <returns>取得成功でtrue</returns>
    public bool TryTake(int amount, out SolidResource food)
    {
        if (StoredAmount < amount)
        {
            food = null;
            return false;
        }
        StoredAmount -= amount;
        food = new SolidResource(ResourceType.NutritionPellet, amount);
        return true;
    }
}
