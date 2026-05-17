using System;
using UnityEngine;

/// <summary>
/// 牛の個体を表すクラス。乳蓄積・繁殖・ライフサイクル管理を担う
/// </summary>
public class CowWorker : WorkerBase
{
    private const float MilkAccumulationPerTick = 0.02f / 60f;
    private const int PregnancyEarlyDuration = 180000;
    private const int PregnancyLateDuration = 72000;
    private const int EstrusCycleTicks = 108000;
    private const int EstrusDuration = 18000;
    private const float BaseBreedProbability = 0.5f;

    protected override Color WorkerColor => new Color(0.6f, 0.4f, 0.2f);

    /// <summary>個体識別ID</summary>
    public string CowId { get; private set; }
    /// <summary>個体名</summary>
    public string CowName { get; set; }
    /// <summary>移動基本値</summary>
    public float BaseMovement { get; set; } = 1.0f;
    /// <summary>作業基本値</summary>
    public float BaseWork { get; set; } = 1.0f;
    /// <summary>乳質基本値（0〜100）</summary>
    public float BaseMilkQuality { get; set; } = 70f;
    /// <summary>年齢（日数）</summary>
    public int AgeInDays { get; private set; }
    /// <summary>ライフサイクル段階</summary>
    public CowLifecycleStage LifecycleStage { get; set; } = CowLifecycleStage.Calf;
    /// <summary>現在の乳蓄積量</summary>
    public float MilkAccumulation { get; private set; }
    /// <summary>妊娠経過tick数</summary>
    public int PregnancyProgress { get; private set; }

    /// <summary>子牛誕生イベント（自分自身のWorkerBaseを渡す）</summary>
    public event Action<CowWorker> OnGiveBirth;

    private int _estrusCycleCounter;
    private int _estrusCounter;

    protected override void Awake()
    {
        CowId = Guid.NewGuid().ToString("N")[..8];
        CowName = $"牛{CowId[..4]}";
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnDayChanged += OnDayChanged;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnDayChanged -= OnDayChanged;
    }

    private void OnDayChanged(int day)
    {
        AgeInDays++;
        UpdateLifecycle();
        UpdateEstrus();
    }

    private void UpdateLifecycle()
    {
        if (LifecycleStage == CowLifecycleStage.Calf && AgeInDays >= 30)
            LifecycleStage = CowLifecycleStage.YoungCow;
    }

    private void UpdateEstrus()
    {
        if (LifecycleStage == CowLifecycleStage.Calf) return;
        if (HasEffect(StatusEffectType.PregnancyEarly) || HasEffect(StatusEffectType.PregnancyLate)) return;

        _estrusCycleCounter++;
        if (_estrusCycleCounter >= 3)
        {
            _estrusCycleCounter = 0;
            ApplyEffect(StatusEffectType.Estrus);
            _estrusCounter = 0;
        }

        if (HasEffect(StatusEffectType.Estrus))
        {
            _estrusCounter++;
            if (_estrusCounter >= 1)
                RemoveEffect(StatusEffectType.Estrus);
        }
    }

    /// <summary>
    /// tickごとに乳を蓄積する（泌乳期かつAdultCowのみ有効）
    /// </summary>
    /// <param name="workMood">作業ムード</param>
    public void AccumulateMilk(float workMood)
    {
        if (LifecycleStage != CowLifecycleStage.AdultCow) return;
        if (!HasEffect(StatusEffectType.Lactating)) return;

        float efficiency = GetWorkEfficiency(workMood);
        MilkAccumulation += MilkAccumulationPerTick * efficiency * BaseWork;
    }

    /// <summary>
    /// 搾乳を実行し、蓄積量に応じたLiquidResourceを返す
    /// 搾乳後MilkAccumulationは0にリセットされる
    /// </summary>
    /// <returns>搾乳した牛乳リソース</returns>
    public LiquidResource Milk()
    {
        int amount = Mathf.Max(1, Mathf.FloorToInt(MilkAccumulation));
        float quality = GetMilkQuality();
        MilkAccumulation = 0f;
        return new LiquidResource(amount, quality);
    }

    /// <summary>
    /// 現在の乳質を返す: BaseMilkQuality × (Happiness / 100)
    /// </summary>
    /// <returns>乳質値（0〜100）</returns>
    public float GetMilkQuality()
    {
        return BaseMilkQuality * (Happiness / 100f);
    }

    /// <summary>
    /// 妊娠を開始する（PregnancyEarlyを付与）
    /// </summary>
    public void StartPregnancy()
    {
        PregnancyProgress = 0;
        ApplyEffect(StatusEffectType.PregnancyEarly);
        RemoveEffect(StatusEffectType.Lactating);
    }

    /// <summary>
    /// tickごとに妊娠を進める（GameTimeManagerのOnTickAdvancedから呼ばれる）
    /// </summary>
    public void AdvancePregnancy()
    {
        if (!HasEffect(StatusEffectType.PregnancyEarly) && !HasEffect(StatusEffectType.PregnancyLate)) return;

        PregnancyProgress++;

        if (HasEffect(StatusEffectType.PregnancyEarly) && PregnancyProgress >= PregnancyEarlyDuration)
        {
            RemoveEffect(StatusEffectType.PregnancyEarly);
            ApplyEffect(StatusEffectType.PregnancyLate);
        }
        else if (HasEffect(StatusEffectType.PregnancyLate) && PregnancyProgress >= PregnancyEarlyDuration + PregnancyLateDuration)
        {
            GiveBirth();
        }
    }

    /// <summary>
    /// 出産処理を実行する（状態をリセットし、子牛誕生イベントを発火）
    /// </summary>
    public void GiveBirth()
    {
        RemoveEffect(StatusEffectType.PregnancyLate);
        PregnancyProgress = 0;

        if (LifecycleStage != CowLifecycleStage.AdultCow)
            LifecycleStage = CowLifecycleStage.AdultCow;

        ApplyEffect(StatusEffectType.Lactating);
        OnGiveBirth?.Invoke(this);
    }

    /// <summary>
    /// 繁殖成功率を返す: clamp(基本確率 × 発情期係数 × (1 + breedMood/100), 0, 1)
    /// </summary>
    /// <param name="breedMood">繁殖ムード（0〜100）</param>
    /// <returns>繁殖成功率（0〜1）</returns>
    public float GetBreedSuccessRate(float breedMood)
    {
        float estrusFactor = HasEffect(StatusEffectType.Estrus) ? 2.0f : 1.0f;
        return MoodCalculator.GetBreedSuccessRate(BaseBreedProbability, estrusFactor, breedMood);
    }

    /// <summary>
    /// 1日経過処理（AgeInDaysインクリメントとライフサイクル更新はOnDayChangedで行う）
    /// </summary>
    public void AgeUp() { }
}
