using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 牛・牧場主に共通するステータス・状態異常・スケジュール管理の基底クラス
/// </summary>
public abstract class WorkerBase : MonoBehaviour
{
    private const float HungerIncreasePerTick = 0.05f / 60f;
    private const float HungryThreshold = 70f;
    private const int MalnutritionTicks = 18000;
    private const float StaminaDecreasePerTick = 0.03f / 60f;
    private const float BaseStaminaRecovery = 0.2f / 60f;

    /// <summary>1マス移動に要するtick数（サブクラスでオーバーライド可能）</summary>
    public virtual int MovementTicksPerCell => 60;

    /// <summary>空腹度（0〜100、0が満腹、100が飢餓）</summary>
    public float Hunger { get; protected set; }
    /// <summary>スタミナ（0〜100）</summary>
    public float Stamina { get; protected set; } = 100f;
    /// <summary>幸福度（0〜100）</summary>
    public float Happiness { get; protected set; } = 50f;
    /// <summary>現在有効な状態異常の集合</summary>
    public HashSet<StatusEffectType> ActiveEffects { get; } = new HashSet<StatusEffectType>();
    /// <summary>24スロット分のスケジュール配列</summary>
    public ScheduleSlotType[] Schedule { get; } = new ScheduleSlotType[24];
    /// <summary>現在実行中のタスク（nullなら待機）</summary>
    public AITaskBase CurrentTask { get; set; }
    /// <summary>グリッド上の現在位置</summary>
    public Vector2Int GridPosition { get; set; }
    /// <summary>割り当てられたベッド</summary>
    public CowBed AssignedBed { get; set; }

    private int _hungryTickCount;
    private SpriteRenderer _spriteRenderer;
    private Vector3 _visualPosition;
    private bool _visualInitialized;

    /// <summary>ワーカーの表示色</summary>
    protected abstract Color WorkerColor { get; }

    /// <summary>UIアイコン表示用の色</summary>
    public Color DisplayColor => WorkerColor;

    /// <summary>AIの意思決定コンポーネント</summary>
    public AIDecisionMaker DecisionMaker { get; private set; }

    protected virtual void Awake()
    {
        for (int i = 0; i < 24; i++)
            Schedule[i] = DefaultScheduleFor(i);

        _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        _spriteRenderer.sprite = SpriteHelper.CreateColorSprite(WorkerColor);
        _spriteRenderer.sortingOrder = 3;
        transform.localScale = new Vector3(0.7f, 0.7f, 1f);

        DecisionMaker = new AIDecisionMaker(this);
    }

    protected virtual void Start()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnTickAdvanced += OnTick;
    }

    protected virtual void OnDestroy()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnTickAdvanced -= OnTick;
    }

    private void Update()
    {
        var targetPos = new Vector3(GridPosition.x, GridPosition.y, -0.1f);
        if (!_visualInitialized)
        {
            _visualPosition = targetPos;
            _visualInitialized = true;
        }
        float timeScale = GameTimeManager.Instance != null ? GameTimeManager.Instance.TimeScale : 1f;
        float cellsPerSecond = (float)GameTimeManager.TicksPerSecond / MovementTicksPerCell * timeScale;
        _visualPosition = Vector3.MoveTowards(_visualPosition, targetPos, cellsPerSecond * Time.deltaTime);
        transform.position = _visualPosition;
    }

    private void OnTick(int tick)
    {
        UpdateHungerLogic();
        UpdateStaminaLogic();
        TickCurrentTask(tick);
        DecisionMaker?.TryAssignNextTask();
    }

    private void UpdateHungerLogic()
    {
        UpdateHunger(HungerIncreasePerTick);

        if (Hunger >= HungryThreshold)
        {
            ApplyEffect(StatusEffectType.Hungry);
            _hungryTickCount++;
            if (_hungryTickCount >= MalnutritionTicks)
                ApplyEffect(StatusEffectType.Malnutrition);
        }
        else
        {
            RemoveEffect(StatusEffectType.Hungry);
            _hungryTickCount = 0;
        }
    }

    private void UpdateStaminaLogic()
    {
        bool isSleeping = CurrentTask is SleepTask;
        if (isSleeping)
        {
            var mood = RoomManager.Instance?.GetMoodAt(GridPosition);
            float bedCoeff = AssignedBed != null
                ? AssignedBed.StaminaRecoveryMultiplier
                : 1.0f;
            float recovery = MoodCalculator.GetStaminaRecoveryRate(BaseStaminaRecovery, bedCoeff, mood?.RestMood ?? 0f);
            UpdateStamina(recovery);
        }
        else
        {
            UpdateStamina(-StaminaDecreasePerTick);
        }

        if (Stamina <= 0f) ApplyEffect(StatusEffectType.Fatigue);
        else if (Stamina >= 20f) RemoveEffect(StatusEffectType.Fatigue);
    }

    private void TickCurrentTask(int tick)
    {
        if (CurrentTask == null) return;
        CurrentTask.Tick();
        if (CurrentTask.State == AITaskState.Completed || CurrentTask.State == AITaskState.Interrupted)
            CurrentTask = null;
    }

    /// <summary>
    /// 空腹度をdeltaだけ変化させる（0〜100にclamp）
    /// </summary>
    /// <param name="delta">変化量</param>
    public void UpdateHunger(float delta)
    {
        Hunger = Mathf.Clamp(Hunger + delta, 0f, 100f);
    }

    /// <summary>
    /// スタミナをdeltaだけ変化させる（0〜100にclamp）
    /// </summary>
    /// <param name="delta">変化量</param>
    public void UpdateStamina(float delta)
    {
        Stamina = Mathf.Clamp(Stamina + delta, 0f, 100f);
    }

    /// <summary>
    /// 幸福度をdeltaだけ変化させる（0〜100にclamp）
    /// </summary>
    /// <param name="delta">変化量</param>
    public void UpdateHappiness(float delta)
    {
        Happiness = Mathf.Clamp(Happiness + delta, 0f, 100f);
    }

    /// <summary>
    /// 状態異常を付与する（重複付与は無視）
    /// </summary>
    /// <param name="type">付与する状態異常</param>
    public void ApplyEffect(StatusEffectType type) => ActiveEffects.Add(type);

    /// <summary>
    /// 状態異常を除去する
    /// </summary>
    /// <param name="type">除去する状態異常</param>
    public void RemoveEffect(StatusEffectType type) => ActiveEffects.Remove(type);

    /// <summary>
    /// 指定の状態異常を持っているか判定する
    /// </summary>
    /// <param name="type">確認する状態異常</param>
    /// <returns>持っている場合true</returns>
    public bool HasEffect(StatusEffectType type) => ActiveEffects.Contains(type);

    /// <summary>
    /// 作業効率を返す: Happiness/100 × (1 + workMood/100)
    /// </summary>
    /// <param name="workMood">作業ムード（0〜100）</param>
    /// <returns>作業効率</returns>
    public virtual float GetWorkEfficiency(float workMood)
    {
        return MoodCalculator.GetWorkEfficiency(Happiness, workMood);
    }

    /// <summary>
    /// スタミナ回復速度を返す
    /// </summary>
    /// <param name="restMood">休憩ムード（0〜100）</param>
    /// <param name="bedCoefficient">ベッド係数</param>
    /// <returns>スタミナ回復速度</returns>
    public float GetStaminaRecoveryRate(float restMood, float bedCoefficient)
    {
        return MoodCalculator.GetStaminaRecoveryRate(BaseStaminaRecovery, bedCoefficient, restMood);
    }

    /// <summary>
    /// 指定スロットのスケジュール種別を取得する
    /// </summary>
    /// <param name="slot">スロット番号（0〜23）</param>
    /// <returns>スケジュール種別</returns>
    public ScheduleSlotType GetScheduleAt(int slot) => Schedule[slot];

    /// <summary>
    /// 指定スロットのスケジュール種別を設定する
    /// </summary>
    /// <param name="slot">スロット番号（0〜23）</param>
    /// <param name="type">スケジュール種別</param>
    public void SetScheduleAt(int slot, ScheduleSlotType type) => Schedule[slot] = type;

    /// <summary>
    /// 指定スロット範囲をまとめて設定する
    /// </summary>
    /// <param name="slotFrom">開始スロット（0〜23）</param>
    /// <param name="slotTo">終了スロット（0〜23）</param>
    /// <param name="type">スケジュール種別</param>
    public void SetScheduleRange(int slotFrom, int slotTo, ScheduleSlotType type)
    {
        for (int i = slotFrom; i <= slotTo; i++)
            Schedule[i % 24] = type;
    }

    private static ScheduleSlotType DefaultScheduleFor(int slot)
    {
        if (slot >= 20 || slot <= 3) return ScheduleSlotType.Sleep;
        if (slot >= 4 && slot <= 7) return ScheduleSlotType.Work;
        return ScheduleSlotType.Work;
    }
}
