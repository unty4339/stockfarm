using System;
using UnityEngine;

/// <summary>
/// ゲーム内時間を管理する中枢シングルトンクラス
/// 1日=600tick、1スロット=25tick、1tick=1秒（×1倍速）
/// </summary>
public class GameTimeManager : MonoBehaviour
{
    private static GameTimeManager _instance;
    /// <summary>シングルトン参照</summary>
    public static GameTimeManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("GameTimeManager");
                _instance = go.AddComponent<GameTimeManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private const int TicksPerDay = 600;
    private const int SlotsPerDay = 24;
    private const int TicksPerSlot = TicksPerDay / SlotsPerDay;

    /// <summary>現在のtick（0〜599）</summary>
    public int CurrentTick { get; private set; }
    /// <summary>経過日数</summary>
    public int CurrentDay { get; private set; }
    /// <summary>現在の時間スロット（0〜23）</summary>
    public int CurrentSlot { get; private set; }
    /// <summary>時間倍率（1〜3）</summary>
    public float TimeScale { get; private set; } = 1f;
    /// <summary>一時停止フラグ</summary>
    public bool IsPaused { get; private set; }

    /// <summary>tickが進むたびに発火（引数: currentTick）</summary>
    public event Action<int> OnTickAdvanced;
    /// <summary>スロットが変わるたびに発火（引数: currentSlot）</summary>
    public event Action<int> OnSlotChanged;
    /// <summary>日付が変わるたびに発火（引数: currentDay）</summary>
    public event Action<int> OnDayChanged;

    private float _tickTimer;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (IsPaused) return;

        _tickTimer += Time.deltaTime;
        float tickInterval = 1f / TimeScale;

        while (_tickTimer >= tickInterval)
        {
            _tickTimer -= tickInterval;
            AdvanceTick();
        }
    }

    private void AdvanceTick()
    {
        int prevSlot = CurrentSlot;

        CurrentTick++;
        OnTickAdvanced?.Invoke(CurrentTick);

        CurrentSlot = (CurrentTick % TicksPerDay) / TicksPerSlot;
        if (CurrentSlot != prevSlot)
            OnSlotChanged?.Invoke(CurrentSlot);

        if (CurrentTick % TicksPerDay == 0)
        {
            CurrentDay++;
            CurrentTick = 0;
            OnDayChanged?.Invoke(CurrentDay);
        }
    }

    /// <summary>
    /// 時間倍率を設定する（1〜3にclamp）
    /// </summary>
    /// <param name="scale">倍率</param>
    public void SetTimeScale(float scale)
    {
        TimeScale = Mathf.Clamp(scale, 1f, 3f);
    }

    /// <summary>
    /// 一時停止・再開を切り替える
    /// </summary>
    /// <param name="paused">一時停止ならtrue</param>
    public void SetPaused(bool paused)
    {
        IsPaused = paused;
    }
}
