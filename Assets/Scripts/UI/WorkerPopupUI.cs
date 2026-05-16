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
    private WorkerBase _currentWorker;

    private void Awake()
    {
        BuildPanel();
    }

    private void BuildPanel()
    {
        _panel = new GameObject("WorkerPopup");
        _panel.transform.SetParent(transform, false);
        var rt = _panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0.5f);
        rt.anchorMax = new Vector2(0f, 0.5f);
        rt.sizeDelta = new Vector2(210, 185);
        rt.anchoredPosition = new Vector2(115, 0);
        var img = _panel.AddComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        _nameText = UIHelper.CreateText(_panel.transform, "NameText", "名前",
            new Vector2(0, 68), 14, Color.white);
        _statusText = UIHelper.CreateText(_panel.transform, "StatusText", "",
            new Vector2(0, 15), 10, new Color(0.8f, 0.8f, 0.8f));
        (_statusText.transform as RectTransform).sizeDelta = new Vector2(190, 65);

        UIHelper.CreateButton(_panel.transform, "優先度", new Vector2(0, -50), 70, 24, OnPriorityPressed);
        UIHelper.CreateButton(_panel.transform, "閉じる", new Vector2(0, -75), 70, 24, Hide);
        _panel.SetActive(false);
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
        _currentWorker = worker;
        _panel?.SetActive(true);

        string displayName = worker is CowWorker cow ? cow.CowName : "牧場主";
        _nameText.text = displayName;
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

    private void OnPriorityPressed()
    {
        var priorityUI = FindFirstObjectByType<PriorityMenuUI>();
        if (_currentWorker != null)
            priorityUI?.RefreshList(_currentWorker);
    }
}
