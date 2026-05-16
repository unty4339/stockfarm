using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 画面右下の時刻・倍率表示エリア
/// </summary>
public class TimeDisplayUI : MonoBehaviour
{
    private TextMeshProUGUI _timeText;
    private TextMeshProUGUI _fundsText;

    private void Start()
    {
        BuildUI();

        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.OnTickAdvanced += OnTick;
            GameTimeManager.Instance.OnSlotChanged += OnSlotChanged;
            UpdateDisplay(GameTimeManager.Instance.CurrentSlot, GameTimeManager.Instance.TimeScale);
        }

        if (EconomyManager.Instance != null)
            EconomyManager.Instance.OnFundsChanged += OnFundsChanged;
    }

    private void OnDestroy()
    {
        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.OnTickAdvanced -= OnTick;
            GameTimeManager.Instance.OnSlotChanged -= OnSlotChanged;
        }

        if (EconomyManager.Instance != null)
            EconomyManager.Instance.OnFundsChanged -= OnFundsChanged;
    }

    private void BuildUI()
    {
        var bg = new GameObject("TimePanel");
        bg.transform.SetParent(transform, false);
        var bgRt = bg.AddComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(1f, 0f);
        bgRt.anchorMax = new Vector2(1f, 0f);
        bgRt.pivot = new Vector2(1f, 0f);
        bgRt.sizeDelta = new Vector2(220, 80);
        bgRt.anchoredPosition = new Vector2(-10, 10);
        bg.AddComponent<Image>().color = new Color(0, 0, 0, 0.6f);

        _timeText = UIHelper.CreateText(bg.transform, "TimeText", "Day 1 | 10:00",
            new Vector2(0, 22), 13, Color.white);
        _fundsText = UIHelper.CreateText(bg.transform, "FundsText", "資金: 2000",
            new Vector2(0, 2), 13, Color.white);

        UIHelper.CreateButton(bg.transform, "||", new Vector2(-80, -22), 36, 20, OnPausePressed);
        UIHelper.CreateButton(bg.transform, "×1", new Vector2(-40, -22), 36, 20, () => OnSpeedChanged(1f));
        UIHelper.CreateButton(bg.transform, "×2", new Vector2(0, -22), 36, 20, () => OnSpeedChanged(2f));
        UIHelper.CreateButton(bg.transform, "×3", new Vector2(40, -22), 36, 20, () => OnSpeedChanged(3f));
    }

    private void OnTick(int tick)
    {
        if (GameTimeManager.Instance == null) return;
        int day = GameTimeManager.Instance.CurrentDay + 1;
        int slot = GameTimeManager.Instance.CurrentSlot;
        _timeText.text = $"Day {day} | {SlotToTime(slot)}";
    }

    private void OnSlotChanged(int slot)
    {
        if (GameTimeManager.Instance == null) return;
        UpdateDisplay(slot, GameTimeManager.Instance.TimeScale);
    }

    private void OnFundsChanged(int funds)
    {
        _fundsText.text = $"資金: {funds}";
    }

    /// <summary>
    /// 時刻表示と倍率表示を更新する
    /// </summary>
    /// <param name="currentSlot">現在スロット</param>
    /// <param name="timeScale">時間倍率</param>
    public void UpdateDisplay(int currentSlot, float timeScale)
    {
        if (_timeText == null) return;
        int day = GameTimeManager.Instance?.CurrentDay + 1 ?? 1;
        _timeText.text = $"Day {day} | {SlotToTime(currentSlot)} ×{timeScale:0.0}";
    }

    /// <summary>
    /// 一時停止ボタン押下時の処理
    /// </summary>
    public void OnPausePressed()
    {
        bool paused = !(GameTimeManager.Instance?.IsPaused ?? false);
        GameTimeManager.Instance?.SetPaused(paused);
    }

    /// <summary>
    /// 速度変更ボタン押下時の処理
    /// </summary>
    /// <param name="scale">倍率</param>
    public void OnSpeedChanged(float scale)
    {
        GameTimeManager.Instance?.SetTimeScale(scale);
    }

    /// <summary>
    /// スロット番号（0〜23）を時刻文字列に変換する
    /// </summary>
    /// <param name="slot">時間スロット</param>
    /// <returns>HH:00 形式の時刻</returns>
    private static string SlotToTime(int slot)
    {
        return $"{slot:00}:00";
    }
}
