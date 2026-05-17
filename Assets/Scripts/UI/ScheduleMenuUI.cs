using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// スケジュールセルを識別するマーカーコンポーネント
/// </summary>
public class ScheduleCell : MonoBehaviour
{
    /// <summary>ペイント時のコールバック</summary>
    public Action OnPaint;

    /// <summary>スケジュールを塗り替える</summary>
    public void Paint() => OnPaint?.Invoke();
}

/// <summary>
/// ワーカーの1日24スロットスケジュールを表示・編集するメニュー
/// 行=ワーカー、列=時刻（0〜23時）、区分ボタン選択後にセルをクリックまたはドラッグして設定する
/// </summary>
public class ScheduleMenuUI : MonoBehaviour
{
    private const float NameColumnWidth = 120f;
    private const float HourCellWidth = 32f;
    private const float HeaderRowHeight = 40f;
    private const float RowHeight = 36f;
    private const float CategoryBarHeight = 52f;
    private const float Padding = 12f;
    private const float Border = 2f;

    private static readonly (ScheduleSlotType type, string label, Color color, bool farmerOnly)[] Categories =
    {
        (ScheduleSlotType.Work,     "労働",   new Color(0.80f, 0.65f, 0.20f, 1f), false),
        (ScheduleSlotType.Joy,      "娯楽",   new Color(0.55f, 0.30f, 0.75f, 1f), false),
        (ScheduleSlotType.Sleep,    "睡眠",   new Color(0.20f, 0.30f, 0.60f, 1f), false),
        (ScheduleSlotType.Breeding, "種付け", new Color(0.70f, 0.20f, 0.30f, 1f), true),
    };

    private GameObject _panel;
    private ScheduleSlotType _selectedType = ScheduleSlotType.Work;
    private Image[] _categoryButtonImages;

    private void Awake()
    {
        _panel = new GameObject("SchedulePanel");
        _panel.transform.SetParent(transform, false);
        var rt = _panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        _panel.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        _panel.SetActive(false);
    }

    /// <summary>パネルを表示してテーブルを再構築する</summary>
    public void Show()
    {
        BuildPanel();
        _panel.SetActive(true);
    }

    /// <summary>パネルが表示中かどうか</summary>
    public bool IsVisible => _panel != null && _panel.activeSelf;

    /// <summary>パネルを非表示にする</summary>
    public void Hide() => _panel?.SetActive(false);

    private void Update()
    {
        if (!_panel.activeSelf) return;
        var mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.leftButton.isPressed)
            TryPaintCellUnderMouse(mouse);

        if ((mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame)
            && !UIHelper.IsPointerOverUI())
            Hide();
    }

    private void TryPaintCellUnderMouse(Mouse mouse)
    {
        if (EventSystem.current == null) return;
        var eventData = new PointerEventData(EventSystem.current)
        {
            position = mouse.position.ReadValue()
        };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        foreach (var result in results)
        {
            var cell = result.gameObject.GetComponent<ScheduleCell>();
            if (cell != null)
            {
                cell.Paint();
                break;
            }
        }
    }

    private void BuildPanel()
    {
        foreach (Transform child in _panel.transform)
            Destroy(child.gameObject);

        var workers = FindObjectsOfType<WorkerBase>();
        Array.Sort(workers, (a, b) =>
            string.Compare(GetWorkerName(a), GetWorkerName(b), StringComparison.Ordinal));

        float tableWidth = NameColumnWidth + 24 * HourCellWidth;
        float totalWidth = tableWidth + Padding * 2f;
        float totalHeight = CategoryBarHeight + HeaderRowHeight + workers.Length * RowHeight + Padding * 2f;

        var panelRt = _panel.GetComponent<RectTransform>();
        panelRt.sizeDelta = new Vector2(totalWidth, totalHeight);
        panelRt.anchoredPosition = UIHelper.SubMenuPanelAnchoredPosition;

        BuildCategoryBar();
        BuildGrid(workers);
    }

    private void BuildCategoryBar()
    {
        _categoryButtonImages = new Image[Categories.Length];
        float btnWidth = 100f;
        float spacing = 8f;

        for (int i = 0; i < Categories.Length; i++)
        {
            var cat = Categories[i];
            int capturedI = i;
            float btnH = CategoryBarHeight - 8f;
            float x = Padding + i * (btnWidth + spacing) + btnWidth * 0.5f;
            float y = -(Padding + btnH * 0.5f);

            var go = new GameObject($"CategoryBtn_{cat.label}");
            go.transform.SetParent(_panel.transform, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(btnWidth, btnH);
            rt.anchoredPosition = new Vector2(x, y);

            var img = go.AddComponent<Image>();
            _categoryButtonImages[i] = img;

            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => SelectCategory(capturedI));

            var textGo = new GameObject("Label");
            textGo.transform.SetParent(go.transform, false);
            var textRt = textGo.AddComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.sizeDelta = Vector2.zero;
            textRt.anchoredPosition = Vector2.zero;
            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text = cat.label;
            tmp.fontSize = 20f;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            UIHelper.ApplyFont(tmp);
        }

        RefreshCategoryButtons();
    }

    private void SelectCategory(int index)
    {
        _selectedType = Categories[index].type;
        RefreshCategoryButtons();
    }

    private void RefreshCategoryButtons()
    {
        if (_categoryButtonImages == null) return;
        for (int i = 0; i < Categories.Length; i++)
        {
            bool selected = Categories[i].type == _selectedType;
            var c = Categories[i].color;
            _categoryButtonImages[i].color = selected
                ? c
                : new Color(c.r * 0.45f, c.g * 0.45f, c.b * 0.45f, 1f);
        }
    }

    private void BuildGrid(WorkerBase[] workers)
    {
        float gridTop = Padding + CategoryBarHeight;

        AddHeaderCell("名前", 0, gridTop, NameColumnWidth);
        for (int h = 0; h < 24; h++)
            AddHeaderCell($"{h}", h + 1, gridTop, HourCellWidth);

        for (int r = 0; r < workers.Length; r++)
        {
            var worker = workers[r];
            float rowY = gridTop + HeaderRowHeight + r * RowHeight;
            AddNameCell(GetWorkerName(worker), rowY, worker.DisplayColor);
            for (int h = 0; h < 24; h++)
                AddScheduleCell(worker, h, rowY);
        }
    }

    private void AddHeaderCell(string text, int col, float topY, float width)
    {
        float x = Padding + (col == 0 ? 0f : NameColumnWidth + (col - 1) * HourCellWidth);
        float cellW = width - Border;
        float cellH = HeaderRowHeight - Border;

        var go = new GameObject("HeaderCell");
        go.transform.SetParent(_panel.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(cellW, cellH);
        rt.anchoredPosition = new Vector2(x + cellW * 0.5f, -(topY + cellH * 0.5f));

        go.AddComponent<Image>().color = new Color(0.15f, 0.15f, 0.25f, 1f);

        var textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        var textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;
        textRt.anchoredPosition = Vector2.zero;
        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = col == 0 ? 18f : 13f;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        UIHelper.ApplyFont(tmp);
    }

    private void AddNameCell(string name, float topY, Color workerColor)
    {
        float cellW = NameColumnWidth - Border;
        float cellH = RowHeight - Border;

        var go = new GameObject("NameCell");
        go.transform.SetParent(_panel.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(cellW, cellH);
        rt.anchoredPosition = new Vector2(Padding + cellW * 0.5f, -(topY + cellH * 0.5f));

        go.AddComponent<Image>().color = new Color(0.18f, 0.18f, 0.18f, 1f);

        var indicatorGo = new GameObject("Indicator");
        indicatorGo.transform.SetParent(go.transform, false);
        var indicatorRt = indicatorGo.AddComponent<RectTransform>();
        indicatorRt.anchorMin = new Vector2(0f, 0.1f);
        indicatorRt.anchorMax = new Vector2(0f, 0.9f);
        indicatorRt.sizeDelta = new Vector2(5f, 0f);
        indicatorRt.anchoredPosition = new Vector2(5f, 0f);
        indicatorGo.AddComponent<Image>().color = workerColor;

        var textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        var textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = new Vector2(-14f, 0f);
        textRt.anchoredPosition = new Vector2(7f, 0f);
        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = name;
        tmp.fontSize = 15f;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        UIHelper.ApplyFont(tmp);
    }

    private void AddScheduleCell(WorkerBase worker, int hour, float topY)
    {
        float x = Padding + NameColumnWidth + hour * HourCellWidth;
        float cellW = HourCellWidth - Border;
        float cellH = RowHeight - Border;

        var go = new GameObject($"ScheduleCell_{hour}");
        go.transform.SetParent(_panel.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(cellW, cellH);
        rt.anchoredPosition = new Vector2(x + cellW * 0.5f, -(topY + cellH * 0.5f));

        var img = go.AddComponent<Image>();
        img.color = GetScheduleColor(worker.GetScheduleAt(hour));

        var cell = go.AddComponent<ScheduleCell>();
        cell.OnPaint = () =>
        {
            if (IsFarmerOnly(_selectedType) && !(worker is FarmerWorker)) return;
            worker.SetScheduleAt(hour, _selectedType);
            img.color = GetScheduleColor(_selectedType);
        };
    }

    private static Color GetScheduleColor(ScheduleSlotType type)
    {
        foreach (var cat in Categories)
            if (cat.type == type) return cat.color;
        return Color.gray;
    }

    private static bool IsFarmerOnly(ScheduleSlotType type)
    {
        foreach (var cat in Categories)
            if (cat.type == type) return cat.farmerOnly;
        return false;
    }

    private static string GetWorkerName(WorkerBase worker)
    {
        if (worker is CowWorker cow) return cow.CowName ?? worker.name;
        return worker.name;
    }
}
