using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ワーカーをクリックしたときに表示されるポップアップ
/// </summary>
public class WorkerPopupUI : MonoBehaviour
{
    private GameObject _panel;
    private TextMeshProUGUI _nameText;
    private TextMeshProUGUI _statusText;
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
        var rt = _panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(0f, 0f);
        rt.pivot = new Vector2(0f, 0f);
        rt.sizeDelta = new Vector2(420, 410);
        rt.anchoredPosition = new Vector2(10, 120);
        var img = _panel.AddComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        _nameText = UIHelper.CreateText(_panel.transform, "NameText", "名前",
            new Vector2(0, 165), 28, Color.white);
        _statusText = UIHelper.CreateText(_panel.transform, "StatusText", "",
            new Vector2(0, 50), 20, new Color(0.8f, 0.8f, 0.8f));
        (_statusText.transform as RectTransform).sizeDelta = new Vector2(380, 130);

        _breedingRow = BuildBreedingRow();

        UIHelper.CreateButton(_panel.transform, "優先度", new Vector2(0, -118), 140, 48, OnPriorityPressed, 24);
        UIHelper.CreateButton(_panel.transform, "閉じる", new Vector2(0, -170), 140, 48, Hide, 24);
        _panel.SetActive(false);
    }

    private GameObject BuildBreedingRow()
    {
        var row = new GameObject("BreedingRow");
        row.transform.SetParent(_panel.transform, false);
        var rowRt = row.AddComponent<RectTransform>();
        rowRt.anchorMin = new Vector2(0.5f, 0.5f);
        rowRt.anchorMax = new Vector2(0.5f, 0.5f);
        rowRt.sizeDelta = new Vector2(400, 36);
        rowRt.anchoredPosition = new Vector2(0, -65);

        var label = UIHelper.CreateText(row.transform, "BreedingLabel", "種付け",
            new Vector2(-100, 0), 22, new Color(0.85f, 0.85f, 0.85f));
        (label.transform as RectTransform).sizeDelta = new Vector2(100, 36);
        label.alignment = TextAlignmentOptions.MidlineLeft;

        var btn = UIHelper.CreateButton(row.transform, "なし",
            new Vector2(80, 0), 210, 36, OnBreedingButtonPressed, 19);
        _breedingBtnLabel = btn.transform.Find("Label").GetComponent<TextMeshProUGUI>();

        return row;
    }

    private void Update()
    {
        if (_panel != null && _panel.activeSelf && _currentWorker != null)
            RefreshStatus();
    }

    /// <summary>
    /// 指定ワーカーの情報を表示する
    /// </summary>
    /// <param name="worker">表示するワーカー</param>
    public void Show(WorkerBase worker)
    {
        PopupCoordinator.NotifyShown();
        _currentWorker = worker;
        _panel?.SetActive(true);

        string displayName = worker is CowWorker cow ? cow.CowName : "牧場主";
        _nameText.text = displayName;

        bool isCow = worker is CowWorker;
        _breedingRow?.SetActive(isCow);
        if (isCow) RefreshBreedingButton();

        RefreshStatus();
    }

    /// <summary>
    /// ポップアップを非表示にする
    /// </summary>
    public void Hide()
    {
        _currentWorker = null;
        _panel?.SetActive(false);
    }

    private void RefreshStatus()
    {
        if (_currentWorker == null) return;
        string effects = _currentWorker.ActiveEffects.Count > 0
            ? string.Join(", ", _currentWorker.ActiveEffects)
            : "なし";
        string taskName = _currentWorker.CurrentTask?.GetType().Name ?? "待機中";
        _statusText.text = $"空腹:{_currentWorker.Hunger:0}  スタミナ:{_currentWorker.Stamina:0}\n" +
                           $"幸福:{_currentWorker.Happiness:0}\n状態: {effects}\n" +
                           $"行動: {taskName}";
    }

    private void RefreshBreedingButton()
    {
        if (_currentWorker is not CowWorker cow || _breedingBtnLabel == null) return;
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
        RefreshBreedingButton();
    }

    private static string GetBreedingLabel(CowWorker cow)
    {
        if (cow.WantsBreedingRepeat) return "予約（繰り返し）";
        if (cow.WantsBreeding) return "予約";
        return "なし";
    }

    private void OnPriorityPressed()
    {
        var priorityUI = FindFirstObjectByType<PriorityMenuUI>();
        if (_currentWorker != null)
            priorityUI?.RefreshList(_currentWorker);
    }
}
