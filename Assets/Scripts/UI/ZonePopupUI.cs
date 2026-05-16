using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 選択ゾーンの情報と優先度を設定するポップアップパネル
/// </summary>
public class ZonePopupUI : MonoBehaviour
{
    private GameObject _panel;
    private TextMeshProUGUI _typeLabel;
    private Button _priorityButton;
    private TextMeshProUGUI _priorityLabel;
    private ZoneData _currentZone;

    private void Awake()
    {
        BuildPanel();
        _panel.SetActive(false);
    }

    private void BuildPanel()
    {
        _panel = new GameObject("ZonePopupPanel");
        _panel.transform.SetParent(transform, false);
        var rt = _panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(250, 60);
        rt.anchoredPosition = new Vector2(0, 90);
        var img = _panel.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.75f);

        _typeLabel = UIHelper.CreateText(_panel.transform, "ZoneTypeLabel", "",
            new Vector2(-50, 0), 13, Color.white);
        _typeLabel.rectTransform.sizeDelta = new Vector2(130, 30);

        _priorityButton = UIHelper.CreateButton(_panel.transform, "",
            new Vector2(80, 0), 100, 32, CyclePriority);
        _priorityLabel = _priorityButton.GetComponentInChildren<TextMeshProUGUI>();
    }

    /// <summary>
    /// 指定ゾーンの情報を表示する
    /// </summary>
    /// <param name="zone">表示するゾーン</param>
    public void Show(ZoneData zone)
    {
        _currentZone = zone;
        _panel.SetActive(true);
        RefreshDisplay();
    }

    /// <summary>
    /// パネルを非表示にする
    /// </summary>
    public void Hide()
    {
        _currentZone = null;
        _panel.SetActive(false);
    }

    private void RefreshDisplay()
    {
        if (_currentZone == null) return;

        _typeLabel.text = ZoneTypeToJP(_currentZone.Type);

        bool showPriority = _currentZone.Type == ZoneType.Storage;
        _priorityButton.gameObject.SetActive(showPriority);

        if (showPriority && _priorityLabel != null)
            _priorityLabel.text = $"優先度: {_currentZone.Priority}";
    }

    private void CyclePriority()
    {
        if (_currentZone == null) return;
        _currentZone.Priority = (_currentZone.Priority % 10) + 1;
        if (_priorityLabel != null)
            _priorityLabel.text = $"優先度: {_currentZone.Priority}";
    }

    private static string ZoneTypeToJP(ZoneType type) => type switch
    {
        ZoneType.Storage => "保管ゾーン",
        ZoneType.Agriculture => "農業ゾーン",
        _ => type.ToString(),
    };
}
