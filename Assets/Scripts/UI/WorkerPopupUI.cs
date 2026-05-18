using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ワーカーをクリックしたときに表示されるポップアップ
/// </summary>
public class WorkerPopupUI : MonoBehaviour
{
    private static readonly Color BlueAccent = new Color(0.22f, 0.47f, 0.90f);
    private static readonly Color WhiteLabel = new Color(0.9f, 0.9f, 0.9f);
    private static readonly Color BarFillColor = new Color(0.22f, 0.47f, 0.90f);
    private static readonly Color BarBgColor = new Color(0.3f, 0.3f, 0.3f);
    private const string BlueHex = "#3878E6";

    private const float PanelW = 400f;
    private const float PanelH = 520f;
    private const float Pad = 12f;
    private const float ContentW = PanelW - Pad * 2f;
    private const float LabelW = 60f;
    private const float BarW = ContentW - LabelW - 4f;
    private const float BarH = 26f;

    private GameObject _panel;
    private TextMeshProUGUI _nameText;
    private GameObject _ageRow;
    private TextMeshProUGUI _ageValue;
    private GameObject _cowStatsRow;
    private TextMeshProUGUI _cowStatsText;
    private Image _hungerFill;
    private Image _staminaFill;
    private Image _happinessFill;
    private GameObject _milkBarRow;
    private Image _milkFill;
    private GameObject _hungryBadge;
    private GameObject _fatigueBadge;
    private GameObject _pregnancyBadge;
    private GameObject _estrusBadge;
    private TextMeshProUGUI _actionText;
    private GameObject _breedingRow;
    private TextMeshProUGUI _breedingBtnLabel;
    private WorkerBase _currentWorker;

    private void Awake()
    {
        PopupCoordinator.OnAnyPopupShown += Hide;
        BuildPanel();
    }

    private void OnDestroy()
    {
        PopupCoordinator.OnAnyPopupShown -= Hide;
    }

    private void BuildPanel()
    {
        _panel = new GameObject("WorkerPopup");
        _panel.transform.SetParent(transform, false);
        var panelRt = _panel.AddComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.zero;
        panelRt.pivot = Vector2.zero;
        panelRt.sizeDelta = new Vector2(PanelW, PanelH);
        panelRt.anchoredPosition = new Vector2(0f, 100f);
        _panel.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        _nameText = MakePanelText("NameText", 221f, ContentW, 54f, 44f, BlueAccent);

        _ageRow = MakeRow("AgeRow", 166f, 40f);
        MakeLeftText(_ageRow.transform, "AgeLabel", "年齢", 0f, LabelW, 30f, WhiteLabel);
        _ageValue = MakeLeftText(_ageRow.transform, "AgeValue", "0", LabelW + 6f, 80f, 30f, BlueAccent);

        _cowStatsRow = MakeRow("CowStatsRow", 119f, 38f);
        _cowStatsText = MakeLeftText(_cowStatsRow.transform, "CowStats", "", 0f, ContentW, 28f, WhiteLabel);

        _hungerFill = MakeBarRow("HungerBar", 74f, "食事");
        _staminaFill = MakeBarRow("StaminaBar", 34f, "睡眠");
        _happinessFill = MakeBarRow("HappinessBar", -6f, "幸福");

        _milkBarRow = MakeRow("MilkBarRow", -50f, 36f);
        _milkFill = MakeBarInRow(_milkBarRow.transform, "乳量");

        BuildBadgesRow(-102f);

        _actionText = MakePanelText("ActionText", -155f, ContentW, 38f, 28f, BlueAccent);

        _breedingRow = BuildBreedingRow(-206f);

        _panel.SetActive(false);
    }

    private TextMeshProUGUI MakePanelText(string name, float y, float width, float height, float fontSize, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(_panel.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(width, height);
        rt.anchoredPosition = new Vector2(0f, y);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        UIHelper.ApplyFont(tmp);
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        return tmp;
    }

    private GameObject MakeRow(string name, float y, float height)
    {
        var go = new GameObject(name);
        go.transform.SetParent(_panel.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(ContentW, height);
        rt.anchoredPosition = new Vector2(0f, y);
        return go;
    }

    /// <summary>
    /// 行の左端から xOffset の位置にテキストを配置する（行は anchorMin/Max=(0.5,0.5) を前提とする）
    /// </summary>
    private TextMeshProUGUI MakeLeftText(Transform row, string name, string text,
        float xOffset, float width, float fontSize, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(row, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0.5f);
        rt.anchorMax = new Vector2(0f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.sizeDelta = new Vector2(width, 40f);
        rt.anchoredPosition = new Vector2(xOffset, 0f);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        UIHelper.ApplyFont(tmp);
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        return tmp;
    }

    private Image MakeBarRow(string name, float y, string labelStr)
    {
        var row = MakeRow(name, y, 36f);
        MakeLeftText(row.transform, "Label", labelStr, 0f, LabelW, 26f, WhiteLabel);

        var bgGo = new GameObject("Bg");
        bgGo.transform.SetParent(row.transform, false);
        var bgRt = bgGo.AddComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0f, 0.5f);
        bgRt.anchorMax = new Vector2(0f, 0.5f);
        bgRt.pivot = new Vector2(0f, 0.5f);
        bgRt.sizeDelta = new Vector2(BarW, BarH);
        bgRt.anchoredPosition = new Vector2(LabelW + 4f, 0f);
        bgGo.AddComponent<Image>().color = BarBgColor;

        return AddFill(bgGo.transform);
    }

    private Image MakeBarInRow(Transform row, string labelStr)
    {
        MakeLeftText(row, "Label", labelStr, 0f, LabelW, 26f, WhiteLabel);

        var bgGo = new GameObject("Bg");
        bgGo.transform.SetParent(row, false);
        var bgRt = bgGo.AddComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0f, 0.5f);
        bgRt.anchorMax = new Vector2(0f, 0.5f);
        bgRt.pivot = new Vector2(0f, 0.5f);
        bgRt.sizeDelta = new Vector2(BarW, BarH);
        bgRt.anchoredPosition = new Vector2(LabelW + 4f, 0f);
        bgGo.AddComponent<Image>().color = BarBgColor;

        return AddFill(bgGo.transform);
    }

    private Image AddFill(Transform bg)
    {
        var fillGo = new GameObject("Fill");
        fillGo.transform.SetParent(bg, false);
        var rt = fillGo.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
        var img = fillGo.AddComponent<Image>();
        img.color = BarFillColor;
        return img;
    }

    private void BuildBadgesRow(float y)
    {
        var row = MakeRow("BadgesRow", y, 52f);
        var hlg = row.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 8f;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        hlg.padding = new RectOffset(0, 0, 0, 0);

        _hungryBadge = MakeBadge(row.transform, "空腹");
        _fatigueBadge = MakeBadge(row.transform, "疲労");
        _pregnancyBadge = MakeBadge(row.transform, "妊娠");
        _estrusBadge = MakeBadge(row.transform, "発情");
    }

    private GameObject MakeBadge(Transform parent, string label)
    {
        var go = new GameObject(label + "Badge");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(72f, 46f);
        go.AddComponent<Image>().color = new Color(0.22f, 0.22f, 0.22f);
        var ol = go.AddComponent<Outline>();
        ol.effectColor = new Color(0.5f, 0.5f, 0.5f);
        ol.effectDistance = new Vector2(1.5f, 1.5f);

        var textGo = new GameObject("Label");
        textGo.transform.SetParent(go.transform, false);
        var textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;
        textRt.anchoredPosition = Vector2.zero;
        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        UIHelper.ApplyFont(tmp);
        tmp.text = label;
        tmp.fontSize = 24f;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        return go;
    }

    private GameObject BuildBreedingRow(float y)
    {
        var row = MakeRow("BreedingRow", y, 48f);
        MakeLeftText(row.transform, "BreedingLabel", "種付け", 0f, 88f, 28f, WhiteLabel);
        var btn = UIHelper.CreateButton(row.transform, "なし", new Vector2(10f, 0f), 180f, 44f, OnBreedingButtonPressed, 26);
        _breedingBtnLabel = btn.transform.Find("Label").GetComponent<TextMeshProUGUI>();
        return row;
    }

    private void Update()
    {
        if (_panel != null && _panel.activeSelf && _currentWorker != null)
            RefreshAll();
    }

    /// <summary>
    /// 指定ワーカーの情報を表示する
    /// </summary>
    /// <param name="worker">表示するワーカー</param>
    public void Show(WorkerBase worker)
    {
        PopupCoordinator.NotifyShown();
        _currentWorker = worker;

        bool isCow = worker is CowWorker;
        _ageRow?.SetActive(isCow);
        _cowStatsRow?.SetActive(isCow);
        _milkBarRow?.SetActive(isCow);
        _breedingRow?.SetActive(isCow);

        RefreshAll();
        _panel?.SetActive(true);
    }

    /// <summary>
    /// ポップアップを非表示にする
    /// </summary>
    public void Hide()
    {
        _currentWorker = null;
        _panel?.SetActive(false);
    }

    private void RefreshAll()
    {
        if (_currentWorker == null) return;

        _nameText.text = _currentWorker is CowWorker c ? c.CowName : "牧場主";

        if (_currentWorker is CowWorker cow)
        {
            _ageValue.text = cow.AgeInDays.ToString();
            _cowStatsText.text =
                $"運動 <color={BlueHex}>{(int)(cow.BaseMovement * 100)}</color>  " +
                $"作業 <color={BlueHex}>{(int)(cow.BaseWork * 100)}</color>  " +
                $"乳質 <color={BlueHex}>{(int)cow.BaseMilkQuality}</color>";
            SetFill(_milkFill, cow.MilkAccumulation / CowWorker.MaxMilkAccumulation);
        }

        SetFill(_hungerFill, 1f - _currentWorker.Hunger / 100f);
        SetFill(_staminaFill, _currentWorker.Stamina / 100f);
        SetFill(_happinessFill, _currentWorker.Happiness / 100f);

        _hungryBadge?.SetActive(_currentWorker.HasEffect(StatusEffectType.Hungry));
        _fatigueBadge?.SetActive(_currentWorker.HasEffect(StatusEffectType.Fatigue));
        _pregnancyBadge?.SetActive(
            _currentWorker.HasEffect(StatusEffectType.PregnancyEarly) ||
            _currentWorker.HasEffect(StatusEffectType.PregnancyLate));
        _estrusBadge?.SetActive(_currentWorker.HasEffect(StatusEffectType.Estrus));

        _actionText.text = _currentWorker.CurrentTask?.GetActionText() ?? "待機中";

        if (_currentWorker is CowWorker breedCow)
            RefreshBreedingButton(breedCow);
    }

    private static void SetFill(Image fill, float ratio)
    {
        if (fill == null) return;
        (fill.transform as RectTransform).sizeDelta = new Vector2(BarW * Mathf.Clamp01(ratio), 0f);
    }

    private void RefreshBreedingButton(CowWorker cow)
    {
        if (_breedingBtnLabel == null) return;
        _breedingBtnLabel.text = GetBreedingLabel(cow);
    }

    private void OnBreedingButtonPressed()
    {
        if (_currentWorker is not CowWorker cow) return;
        if (cow.WantsBreedingRepeat)
        {
            cow.WantsBreeding = false;
            cow.WantsBreedingRepeat = false;
        }
        else if (cow.WantsBreeding)
        {
            cow.WantsBreeding = false;
            cow.WantsBreedingRepeat = true;
        }
        else
        {
            cow.WantsBreeding = true;
        }
        RefreshBreedingButton(cow);
    }

    private static string GetBreedingLabel(CowWorker cow)
    {
        if (cow.WantsBreedingRepeat) return "予約（繰り返し）";
        if (cow.WantsBreeding) return "予約";
        return "なし";
    }
}
