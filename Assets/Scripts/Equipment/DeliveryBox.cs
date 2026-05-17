using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 売却フラグ付きアイテムを収集する2×2の納品ボックス
/// マップ右端中央に初期配置される
/// </summary>
public class DeliveryBox : EquipmentBase
{
    /// <summary>シングルトン参照（シーン内に1つのみ想定）</summary>
    public static DeliveryBox Instance { get; private set; }

    public override EquipmentType Type => EquipmentType.DeliveryBox;
    public override Vector2Int Size => new Vector2Int(2, 2);
    public override int BuildCost => 0;
    public override int MoodBonus => 0;
    public override MoodType AffectedMoodType => MoodType.Work;
    protected override Color EquipmentColor => new Color(0.2f, 0.35f, 0.65f);

    private readonly List<TileItem> _items = new List<TileItem>();

    /// <summary>最大保管数</summary>
    public int Capacity { get; } = 40;

    /// <summary>保管中のアイテム一覧</summary>
    public IReadOnlyList<TileItem> Items => _items;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// アイテムを納品ボックスに保管する
    /// </summary>
    /// <param name="item">保管するアイテム</param>
    /// <returns>保管に成功した場合true</returns>
    public bool TryStore(TileItem item)
    {
        if (_items.Count >= Capacity) return false;
        _items.Add(item);
        return true;
    }

    /// <summary>
    /// 保管容量に余裕があるか
    /// </summary>
    /// <returns>余裕がある場合true</returns>
    public bool HasCapacity() => _items.Count < Capacity;
}
