using UnityEngine;

/// <summary>
/// 種付け施設。繁殖の実行起点となる設備
/// </summary>
public class BreedingFacility : EquipmentBase
{
    public override EquipmentType Type => EquipmentType.BreedingFacility;
    public override Vector2Int Size => new Vector2Int(2, 2);
    public override int BuildCost => 600;
    public override int MoodBonus => 30;
    public override MoodType AffectedMoodType => MoodType.Breed;
    protected override Color EquipmentColor => new Color(0.9f, 0.55f, 0.75f);

    /// <summary>使用中フラグ（同時に1頭のみ）</summary>
    public bool IsInUse { get; private set; }

    /// <summary>
    /// 繁殖を試みる
    /// 成功すれば cow.StartPregnancy() を呼ぶ
    /// </summary>
    /// <param name="cow">種付けを行う牛</param>
    /// <param name="breedMood">現在の繁殖ムード</param>
    /// <returns>繁殖成功でtrue</returns>
    public bool TryBreed(CowWorker cow, float breedMood)
    {
        if (IsInUse) return false;
        if (cow.LifecycleStage == CowLifecycleStage.Calf) return false;
        if (cow.HasEffect(StatusEffectType.PregnancyEarly) || cow.HasEffect(StatusEffectType.PregnancyLate)) return false;

        IsInUse = true;
        float successRate = cow.GetBreedSuccessRate(breedMood);
        bool success = Random.value <= successRate;

        if (success)
            cow.StartPregnancy();

        IsInUse = false;
        return success;
    }
}
