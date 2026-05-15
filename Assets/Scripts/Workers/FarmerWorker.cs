using UnityEngine;

/// <summary>
/// プレイヤーが操作する牧場主の個体クラス
/// WorkerBaseの共通ステータスに加え、睡眠・摂食の特殊処理を持つ
/// </summary>
public class FarmerWorker : WorkerBase
{
    private const float SleepHappinessGain = 10f;
    private const float EatHappinessGain = 5f;
    private const float HungerRecovery = 80f;

    protected override Color WorkerColor => new Color(0.2f, 0.4f, 0.8f);

    /// <summary>栄養失調による活動不能状態フラグ</summary>
    public bool IsIncapacitated => HasEffect(StatusEffectType.Malnutrition);

    /// <summary>
    /// 睡眠を実行する
    /// restMood >= 50の場合Happiness += 10
    /// </summary>
    /// <param name="restMood">休憩ムード（0〜100）</param>
    public void Sleep(float restMood)
    {
        if (restMood >= 50f)
            UpdateHappiness(SleepHappinessGain);
    }

    /// <summary>
    /// 食料棚から摂食する（空腹度-80、Malnutrition/Hungry解除）
    /// </summary>
    /// <param name="shelf">食料棚</param>
    public void Eat(FoodShelf shelf)
    {
        if (!shelf.TryTake(1, out _)) return;

        UpdateHunger(-HungerRecovery);
        RemoveEffect(StatusEffectType.Malnutrition);
        RemoveEffect(StatusEffectType.Hungry);
        UpdateHappiness(EatHappinessGain);
    }

    /// <summary>
    /// 作業効率を返す（WorkerBaseをoverride）: Happiness/100 × (1 + workMood/100)
    /// </summary>
    /// <param name="workMood">作業ムード（0〜100）</param>
    /// <returns>作業効率</returns>
    public override float GetWorkEfficiency(float workMood)
    {
        return MoodCalculator.GetWorkEfficiency(Happiness, workMood);
    }
}
