using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// チェスト。固体リソース専用の保管設備（容量20）
/// </summary>
public class Chest : EquipmentBase, IContainer
{
    public override EquipmentType Type => EquipmentType.Chest;
    public override Vector2Int Size => Vector2Int.one;
    public override int BuildCost => 100;
    public override int MoodBonus => 0;
    public override MoodType AffectedMoodType => MoodType.Work;
    protected override Color EquipmentColor => new Color(0.55f, 0.38f, 0.18f);

    /// <summary>最大保管数</summary>
    public int Capacity { get; private set; } = 20;
    /// <summary>現在の合計保管数</summary>
    public int TotalStored => Capacity - GetRemainingCapacity();
    /// <summary>フィルター設定（nullなら全種受け入れ）</summary>
    public ResourceType? FilterType { get; set; }
    /// <summary>自動運搬タスク生成時の優先度（1〜7）</summary>
    public int TransportPriority { get; set; } = 4;

    private readonly Dictionary<ResourceType, int> _stored = new Dictionary<ResourceType, int>();

    /// <inheritdoc/>
    public bool TryStore(ResourceBase resource)
    {
        if (resource is LiquidResource) return false;
        if (FilterType.HasValue && resource.Type != FilterType.Value) return false;

        int current = GetAmount(resource.Type);
        int total = GetTotalAmount();
        int canStore = Mathf.Min(resource.Amount, Capacity - total);

        if (canStore <= 0) return false;

        _stored[resource.Type] = current + canStore;
        resource.Amount -= canStore;
        return true;
    }

    /// <inheritdoc/>
    public bool TryTake(ResourceType type, int amount, out ResourceBase resource)
    {
        int current = GetAmount(type);
        if (current < amount)
        {
            resource = null;
            return false;
        }
        _stored[type] = current - amount;
        resource = new SolidResource(type, amount);
        return true;
    }

    /// <inheritdoc/>
    public int GetAmount(ResourceType type)
    {
        return _stored.TryGetValue(type, out int count) ? count : 0;
    }

    /// <inheritdoc/>
    public int GetRemainingCapacity()
    {
        return Capacity - GetTotalAmount();
    }

    private int GetTotalAmount()
    {
        int total = 0;
        foreach (var v in _stored.Values) total += v;
        return total;
    }
}
