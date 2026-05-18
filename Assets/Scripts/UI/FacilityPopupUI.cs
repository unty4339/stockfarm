using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 施設をクリックしたときに表示されるポップアップ。施設種別に応じてサブ要素を縦に積み上げる
/// </summary>
public class FacilityPopupUI : MonoBehaviour
{
    private static readonly Color DarkBg = new Color(0.1f, 0.1f, 0.1f, 0.9f);
    private static readonly Color SubBg = new Color(0.15f, 0.15f, 0.15f, 1f);
    private static readonly Color WhiteLabel = new Color(0.9f, 0.9f, 0.9f);
    private static readonly Color BlueAccent = new Color(0.22f, 0.47f, 0.90f);
    private static readonly Color BarBgColor = new Color(0.3f, 0.3f, 0.3f);
    private static readonly Color BarFillColor = new Color(0.22f, 0.47f, 0.90f);
    private static readonly Color SlotEmptyColor = new Color(0.2f, 0.2f, 0.2f);

    private const float PanelW = 400f;
    private const float Pad = 12f;
    private const float ContentW = PanelW - Pad * 2f;
    private const float TitleH = 44f;
    private const float LineH = 28f;
    private const float SmallH = 22f;
    private const float BarH = 16f;
    private const float SubPadV = 8f;
    private const float SubPadH = 8f;
    private const float InnerGap = 6f;
    private const float ElemGap = 8f;
    private const float SlotSize = 36f;
    private const float SlotGap = 4f;
    private const int SlotsPerRow = 8;
    private const float BtnW = 84f;
    private const float BtnH = 36f;

    private GameObject _panel;
    private EquipmentBase _currentEquipment;

    private readonly List<(Image fill, Func<float> getRatio)> _dynamicBars = new();
    private readonly List<(Image slot, int slotIndex, Func<int> getTotalStored)> _dynamicSlots = new();
    private readonly List<(TextMeshProUGUI label, Func<string> getText)> _dynamicTexts = new();

    private void Awake()
    {
        PopupCoordinator.OnAnyPopupShown += Hide;
        _panel = new GameObject("FacilityPopup");
        _panel.transform.SetParent(transform, false);
        var rt = _panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.zero;
        rt.pivot = Vector2.zero;
        rt.anchoredPosition = new Vector2(0f, 100f);
        _panel.AddComponent<Image>().color = DarkBg;
        _panel.SetActive(false);
    }

    private void OnDestroy()
    {
        PopupCoordinator.OnAnyPopupShown -= Hide;
    }

    /// <summary>
    /// 指定設備の情報を表示する
    /// </summary>
    /// <param name="equipment">表示する設備</param>
    public void Show(EquipmentBase equipment)
    {
        PopupCoordinator.NotifyShown();
        _currentEquipment = equipment;
        RebuildContent(equipment);
        _panel.SetActive(true);
    }

    /// <summary>
    /// ポップアップを非表示にする
    /// </summary>
    public void Hide()
    {
        _currentEquipment = null;
        _panel?.SetActive(false);
    }

    private void Update()
    {
        if (_panel == null || !_panel.activeSelf) return;

        foreach (var (fill, getRatio) in _dynamicBars)
            SetFill(fill, getRatio());

        foreach (var (slot, slotIndex, getTotalStored) in _dynamicSlots)
            slot.color = slotIndex < getTotalStored() ? BarFillColor : SlotEmptyColor;

        foreach (var (label, getText) in _dynamicTexts)
            label.text = getText();
    }

    private void RebuildContent(EquipmentBase equipment)
    {
        _dynamicBars.Clear();
        _dynamicSlots.Clear();
        _dynamicTexts.Clear();

        for (int i = _panel.transform.childCount - 1; i >= 0; i--)
            Destroy(_panel.transform.GetChild(i).gameObject);

        float y = Pad;

        AddPanelText("Title", equipment.Type.ToString(), 32f, BlueAccent, TitleH, ref y);
        y += ElemGap;

        if (equipment is ProcessingEquipmentBase proc)
        {
            y = BuildWorkableElem(y, proc);
            y += ElemGap;

            if (proc.InputTank != null)
            {
                y = BuildTankElem(y, "入力タンク", proc.InputTank);
                y += ElemGap;
            }
            if (proc.OutputTank != null)
            {
                y = BuildTankElem(y, "出力タンク", proc.OutputTank);
                y += ElemGap;
            }
        }

        if (equipment is Chest chest)
        {
            y = BuildChestElem(y, chest);
            y += ElemGap;
        }

        if (equipment is LiquidTank tank)
        {
            y = BuildTankElem(y, "タンク", tank);
            y += ElemGap;
        }

        if (equipment is FeedingTrough trough)
        {
            y = BuildFoodBarElem(y, trough);
            y += ElemGap;
        }

        // Replace trailing ElemGap with bottom padding
        y = y - ElemGap + Pad;

        _panel.GetComponent<RectTransform>().sizeDelta = new Vector2(PanelW, y);
    }

    // ── Workable ─────────────────────────────────────────────────────────────

    private float BuildWorkableElem(float yFromTop, ProcessingEquipmentBase proc)
    {
        var workers = proc.GetActiveWorkers();
        int displayRows = Mathf.Max(1, workers.Count);
        float contentH = displayRows * (LineH + InnerGap) + BarH;
        float boxH = SubPadV + contentH + SubPadV;

        var box = MakeElemBox("WorkableElem", yFromTop, boxH);
        float ly = SubPadV;

        if (workers.Count == 0)
        {
            TextInBox(box, "Idle", "待機中", ly, LineH, 26f, WhiteLabel);
            ly += LineH + InnerGap;
        }
        else
        {
            for (int i = 0; i < workers.Count; i++)
            {
                var tmp = TextInBox(box, $"W{i}", "", ly, LineH, 26f, BlueAccent);
                var w = workers[i];
                _dynamicTexts.Add((tmp, () => w is CowWorker c ? c.CowName : "牧場主"));
                ly += LineH + InnerGap;
            }
        }

        var fill = BarInBox(box, "Cycle", ly, BarH);
        _dynamicBars.Add((fill, () => proc.CycleProgress));

        return yFromTop + boxH;
    }

    // ── Tank ─────────────────────────────────────────────────────────────────

    private float BuildTankElem(float yFromTop, string header, LiquidTank tank)
    {
        float contentH = SmallH + InnerGap + LineH + InnerGap + BarH;
        float boxH = SubPadV + contentH + SubPadV;

        var box = MakeElemBox("TankElem", yFromTop, boxH);
        float ly = SubPadV;

        TextInBox(box, "Header", header, ly, SmallH, 22f, WhiteLabel);
        ly += SmallH + InnerGap;

        var statusTmp = TextInBox(box, "Status", "", ly, LineH, 26f, WhiteLabel);
        _dynamicTexts.Add((statusTmp, () =>
        {
            int stored = tank.StoredLiquid?.Amount ?? 0;
            float q = tank.StoredLiquid?.Quality ?? 0f;
            return $"{stored} / {tank.Capacity}  品質 {q:0}";
        }));
        ly += LineH + InnerGap;

        var fill = BarInBox(box, "Tank", ly, BarH);
        _dynamicBars.Add((fill, () =>
            tank.Capacity > 0 ? (float)(tank.StoredLiquid?.Amount ?? 0) / tank.Capacity : 0f));

        return yFromTop + boxH;
    }

    // ── Chest ─────────────────────────────────────────────────────────────────

    private float BuildChestElem(float yFromTop, Chest chest)
    {
        int cap = chest.Capacity;
        int slotRows = Mathf.CeilToInt((float)cap / SlotsPerRow);
        float gridH = slotRows * SlotSize + (slotRows - 1) * SlotGap;
        float contentH = LineH + InnerGap + gridH;
        float boxH = SubPadV + contentH + SubPadV;

        var box = MakeElemBox("ChestElem", yFromTop, boxH);
        float ly = SubPadV;

        var capTmp = TextInBox(box, "Cap", "", ly, LineH, 26f, WhiteLabel);
        _dynamicTexts.Add((capTmp, () => $"{chest.TotalStored} / {chest.Capacity}"));
        ly += LineH + InnerGap;

        int actualPerRow = Mathf.Min(cap, SlotsPerRow);
        float gridTotalW = actualPerRow * SlotSize + (actualPerRow - 1) * SlotGap;
        float gridStartX = (ContentW - SubPadH * 2f - gridTotalW) / 2f + SubPadH;

        for (int i = 0; i < cap; i++)
        {
            int col = i % SlotsPerRow;
            int row = i / SlotsPerRow;

            var slotGo = new GameObject($"Slot{i}");
            slotGo.transform.SetParent(box.transform, false);
            var rt = slotGo.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(SlotSize, SlotSize);
            rt.anchoredPosition = new Vector2(
                gridStartX + col * (SlotSize + SlotGap),
                -(ly + row * (SlotSize + SlotGap)));

            var img = slotGo.AddComponent<Image>();
            img.color = SlotEmptyColor;

            int ci = i;
            _dynamicSlots.Add((img, ci, () => chest.TotalStored));
        }

        return yFromTop + boxH;
    }

    // ── Food bar ─────────────────────────────────────────────────────────────

    private float BuildFoodBarElem(float yFromTop, FeedingTrough trough)
    {
        float contentH = LineH + InnerGap + BarH;
        float boxH = SubPadV + contentH + SubPadV;

        var box = MakeElemBox("FoodElem", yFromTop, boxH);
        float ly = SubPadV;

        var foodTmp = TextInBox(box, "Food", "", ly, LineH, 26f, WhiteLabel);
        _dynamicTexts.Add((foodTmp, () => $"食料 {trough.CurrentFood} / {trough.MaxFood}"));
        ly += LineH + InnerGap;

        var fill = BarInBox(box, "Food", ly, BarH);
        _dynamicBars.Add((fill, () =>
            trough.MaxFood > 0 ? (float)trough.CurrentFood / trough.MaxFood : 0f));

        return yFromTop + boxH;
    }

    // ── Primitive builders ────────────────────────────────────────────────────

    /// <summary>
    /// パネル上端から y の距離にテキストを配置し、y を height 分進める
    /// </summary>
    private void AddPanelText(string name, string text, float fontSize, Color color,
        float height, ref float y)
    {
        var go = new GameObject(name);
        go.transform.SetParent(_panel.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(ContentW, height);
        rt.anchoredPosition = new Vector2(0f, -y);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        UIHelper.ApplyFont(tmp);
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        y += height;
    }

    /// <summary>
    /// パネル上端から yFromTop の距離にサブ要素ボックスを生成する
    /// </summary>
    private GameObject MakeElemBox(string name, float yFromTop, float height)
    {
        var go = new GameObject(name);
        go.transform.SetParent(_panel.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(ContentW, height);
        rt.anchoredPosition = new Vector2(0f, -yFromTop);
        go.AddComponent<Image>().color = SubBg;
        return go;
    }

    /// <summary>
    /// ボックス内上端から ly の距離に左揃えテキストを配置する
    /// </summary>
    private TextMeshProUGUI TextInBox(GameObject box, string name, string text,
        float ly, float height, float fontSize, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(box.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(ContentW - SubPadH * 2f, height);
        rt.anchoredPosition = new Vector2(SubPadH, -ly);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        UIHelper.ApplyFont(tmp);
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        return tmp;
    }

    /// <summary>
    /// ボックス内上端から ly の距離にプログレスバーを配置し、Fillの Image を返す
    /// </summary>
    private Image BarInBox(GameObject box, string name, float ly, float height)
    {
        float barW = ContentW - SubPadH * 2f;

        var bgGo = new GameObject($"{name}Bg");
        bgGo.transform.SetParent(box.transform, false);
        var bgRt = bgGo.AddComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0f, 1f);
        bgRt.anchorMax = new Vector2(0f, 1f);
        bgRt.pivot = new Vector2(0f, 1f);
        bgRt.sizeDelta = new Vector2(barW, height);
        bgRt.anchoredPosition = new Vector2(SubPadH, -ly);
        bgGo.AddComponent<Image>().color = BarBgColor;

        var fillGo = new GameObject($"{name}Fill");
        fillGo.transform.SetParent(bgGo.transform, false);
        var fillRt = fillGo.AddComponent<RectTransform>();
        fillRt.anchorMin = new Vector2(0f, 0f);
        fillRt.anchorMax = new Vector2(0f, 1f);
        fillRt.pivot = new Vector2(0f, 0.5f);
        fillRt.anchoredPosition = Vector2.zero;
        fillRt.sizeDelta = Vector2.zero;
        var img = fillGo.AddComponent<Image>();
        img.color = BarFillColor;
        return img;
    }

    private static void SetFill(Image fill, float ratio)
    {
        if (fill == null) return;
        var parent = fill.transform.parent as RectTransform;
        if (parent == null) return;
        (fill.transform as RectTransform).sizeDelta =
            new Vector2(parent.sizeDelta.x * Mathf.Clamp01(ratio), 0f);
    }
}
