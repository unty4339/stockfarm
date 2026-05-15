using UnityEngine;

/// <summary>
/// 部屋内の設備配置と廃棄物マーカーに基づきムード値を計算する静的クラス
/// </summary>
public static class MoodCalculator
{
    /// <summary>
    /// 部屋内の設備ボーナス合計から廃棄物ペナルティ合計を差し引いたMoodDataを返す
    /// 各ムード値は0〜100にclampされる
    /// </summary>
    /// <param name="room">計算対象の部屋</param>
    /// <returns>計算結果のMoodData</returns>
    public static MoodData Calculate(RoomData room)
    {
        float workBonus = 0f;
        float restBonus = 0f;
        float breedBonus = 0f;

        foreach (var eq in room.ContainedEquipments)
        {
            switch (eq.AffectedMoodType)
            {
                case MoodType.Work:
                    workBonus += eq.MoodBonus;
                    break;
                case MoodType.Rest:
                    restBonus += eq.MoodBonus;
                    break;
                case MoodType.Breed:
                    breedBonus += eq.MoodBonus;
                    break;
            }
        }

        int wastePenalty = 0;
        foreach (var waste in room.WasteMarkers)
        {
            wastePenalty += waste.Severity;
        }

        return new MoodData(
            Mathf.Clamp(workBonus - wastePenalty, 0f, 100f),
            Mathf.Clamp(restBonus - wastePenalty, 0f, 100f),
            Mathf.Clamp(breedBonus - wastePenalty, 0f, 100f)
        );
    }

    /// <summary>
    /// 作業効率を返す: happiness/100 × (1 + workMood/100)
    /// </summary>
    /// <param name="happiness">幸福度（0〜100）</param>
    /// <param name="workMood">作業ムード（0〜100）</param>
    /// <returns>作業効率</returns>
    public static float GetWorkEfficiency(float happiness, float workMood)
    {
        return (happiness / 100f) * (1f + workMood / 100f);
    }

    /// <summary>
    /// スタミナ回復速度を返す: baseRate × bedCoefficient × (1 + restMood/100)
    /// </summary>
    /// <param name="baseRate">基本回復速度</param>
    /// <param name="bedCoefficient">ベッド係数</param>
    /// <param name="restMood">休憩ムード（0〜100）</param>
    /// <returns>スタミナ回復速度</returns>
    public static float GetStaminaRecoveryRate(float baseRate, float bedCoefficient, float restMood)
    {
        return baseRate * bedCoefficient * (1f + restMood / 100f);
    }

    /// <summary>
    /// 繁殖成功率を返す（0〜1にclamp）: clamp(baseProbability × estrusFactor × (1 + breedMood/100), 0, 1)
    /// </summary>
    /// <param name="baseProbability">基本成功確率</param>
    /// <param name="estrusFactor">発情期係数（発情期なら2.0f、通常は1.0f）</param>
    /// <param name="breedMood">繁殖ムード（0〜100）</param>
    /// <returns>繁殖成功率</returns>
    public static float GetBreedSuccessRate(float baseProbability, float estrusFactor, float breedMood)
    {
        return Mathf.Clamp(baseProbability * estrusFactor * (1f + breedMood / 100f), 0f, 1f);
    }
}
