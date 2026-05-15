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
    private Button _pauseButton;
    private Button _speed1Button;
    private Button _speed2Button;
    private Button _speed3Button;

    private void Start()
    {
        BuildUI();

        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.OnTickAdvanced += OnTick;
            GameTimeManager.Instance.OnSlotChanged += OnSlotChanged;
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
        var bg = CreatePanel(transform, "TimePanel", new Vector2(220, 80),
            new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-10, 10));
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0, 0, 0, 0.6f);

        _timeText = CreateText(bg.transform, "TimeText", "Day 1 | Slot 8",
            new Vector2(0, 20), 14);
        _fundsText = CreateText(bg.transform, "FundsText", "資金: 2000",
            new Vector2(0, -5), 14);

        CreateSpeedButtons(bg.transform);
    }

    private void CreateSpeedButtons(Transform parent)
    {
        _pauseButton = CreateButton(parent, "PauseBtn", "||", new Vector2(-80, -30), () => OnPausePressed());
        _speed1Button = CreateButton(parent, "Speed1Btn", "×1", new Vector2(-40, -30), () => OnSpeedChanged(1f));
        _speed2Button = CreateButton(parent, "Speed2Btn", "×2", new Vector2(0, -30), () => OnSpeedChanged(2f));
        _speed3Button = CreateButton(parent, "Speed3Btn", "×3", new Vector2(40, -30), () => OnSpeedChanged(3f));
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

    private string SlotToTime(int slot)
    {
        int hour = (slot + 4) % 24;
        return $"{hour:00}:00";
    }

    // UIビルダーヘルパー
    private static GameObject CreatePanel(Transform parent, string name, Vector2 size,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.sizeDelta = size;
        rt.anchoredPosition = anchoredPos;
        return go;
    }

    private static TextMeshProUGUI CreateText(Transform parent, string name, string text, Vector2 pos, float size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(200, 20);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        return tmp;
    }

    private static Button CreateButton(Transform parent, string name, string label, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(36, 20);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        var textGo = new GameObject("Label");
        textGo.transform.SetParent(go.transform, false);
        var textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;
        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 10;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;
        return btn;
    }
}
