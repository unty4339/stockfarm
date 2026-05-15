using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 資源置き場。運搬されたリソースを1日2回（スロット8と20）自動売却する
/// </summary>
public class SellPoint : EquipmentBase
{
    public override EquipmentType Type => EquipmentType.SellPoint;
    public override Vector2Int Size => new Vector2Int(2, 1);
    public override int BuildCost => 0;
    public override int MoodBonus => 0;
    public override MoodType AffectedMoodType => MoodType.Work;
    protected override Color EquipmentColor => new Color(0.95f, 0.85f, 0.2f);

    /// <summary>収集待ちリソースの一時保管リスト</summary>
    public List<ResourceBase> StoredResources { get; } = new List<ResourceBase>();

    private void OnEnable()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnSlotChanged += OnSlotChanged;
    }

    private void OnDisable()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnSlotChanged -= OnSlotChanged;
    }

    private void OnSlotChanged(int slot)
    {
        if (slot == 8 || slot == 20)
            Collect();
    }

    /// <summary>
    /// 保管リソースを全てEconomyManagerへ売却する
    /// </summary>
    public void Collect()
    {
        if (StoredResources.Count == 0) return;
        EconomyManager.Instance?.SellResources(new List<ResourceBase>(StoredResources));
        StoredResources.Clear();
    }

    /// <summary>
    /// リソースを保管リストに追加する
    /// </summary>
    /// <param name="resource">追加するリソース</param>
    public void AddResource(ResourceBase resource)
    {
        StoredResources.Add(resource);
    }
}
