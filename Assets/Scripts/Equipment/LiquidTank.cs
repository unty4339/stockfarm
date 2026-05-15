using UnityEngine;

/// <summary>
/// 液体リソース専用の保管設備。加工設備に内蔵される
/// </summary>
public class LiquidTank : EquipmentBase, IContainer
{
    public override EquipmentType Type => EquipmentType.Chest;
    public override Vector2Int Size => Vector2Int.one;
    public override int BuildCost => 150;
    public override int MoodBonus => 0;
    public override MoodType AffectedMoodType => MoodType.Work;
    protected override Color EquipmentColor => new Color(0.4f, 0.6f, 0.85f);

    /// <summary>最大保管量</summary>
    public int Capacity { get; set; } = 100;
    /// <summary>現在保管中の液体（nullなら空）</summary>
    public LiquidResource StoredLiquid { get; private set; }

    /// <inheritdoc/>
    public bool TryStore(ResourceBase resource)
    {
        if (resource is not LiquidResource liquid) return false;
        if (GetTotalAmount() + liquid.Amount > Capacity) return false;

        if (StoredLiquid == null)
            StoredLiquid = new LiquidResource(liquid.Amount, liquid.Quality);
        else
            StoredLiquid.Mix(liquid);

        return true;
    }

    /// <inheritdoc/>
    public bool TryTake(ResourceType type, int amount, out ResourceBase resource)
    {
        if (type != ResourceType.Milk || StoredLiquid == null || StoredLiquid.Amount < amount)
        {
            resource = null;
            return false;
        }
        var taken = new LiquidResource(amount, StoredLiquid.Quality);
        StoredLiquid.Amount -= amount;
        if (StoredLiquid.Amount <= 0) StoredLiquid = null;
        resource = taken;
        return true;
    }

    /// <inheritdoc/>
    public int GetAmount(ResourceType type)
    {
        if (type != ResourceType.Milk || StoredLiquid == null) return 0;
        return StoredLiquid.Amount;
    }

    /// <inheritdoc/>
    public int GetRemainingCapacity()
    {
        return Capacity - GetTotalAmount();
    }

    private int GetTotalAmount() => StoredLiquid?.Amount ?? 0;
}
