using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 選択ゾーンの情報・優先度設定・削除を行うポップアップパネル
/// 保管/農業ゾーンのクリック時に表示される
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
        PopupCoordinator.OnAnyPopupShown += Hide;
        BuildPanel();
        _panel.SetActive(false);
    }

    private void OnDestroy()
    {
        PopupCoordinator.OnAnyPopupShown -= Hide;
    }

    private void BuildPanel()
    {
        _panel = new GameObject("ZonePopupPanel");
        _panel.transform.SetParent(transform, false);
        var rt = _panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(0f, 0f);
        rt.pivot = new Vector2(0f, 0f);
        rt.sizeDelta = new Vector2(500, 280);
        rt.anchoredPosition = new Vector2(0, 100);
        var img = _panel.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.75f);

        _typeLabel = UIHelper.CreateText(_panel.transform, "ZoneTypeLabel", "",
            new Vector2(0, 100), 26, Color.white);
        _typeLabel.rectTransform.sizeDelta = new Vector2(460, 60);

        _priorityButton = UIHelper.CreateButton(_panel.transform, "",
            new Vector2(0, 20), 400, 64, CyclePriority, 24);
        _priorityLabel = _priorityButton.GetComponentInChildren<TextMeshProUGUI>();

        UIHelper.CreateButton(_panel.transform, "削除", new Vector2(0, -80), 400, 64, OnDeletePressed, 24);
    }

    /// <summary>
    /// 指定ゾーンの情報を表示する
    /// </summary>
    /// <param name="zone">表示するゾーン</param>
    public void Show(ZoneData zone)
    {
        PopupCoordinator.NotifyShown();
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

    /// <summary>
    /// 削除ボタン押下時の処理。農業ゾーンはゾーン内の農作物もすべて削除する
    /// </summary>
    private void OnDeletePressed()
    {
        if (_currentZone == null) return;

        if (_currentZone.Type == ZoneType.Agriculture)
        {
            var cropManager = FindFirstObjectByType<CropManager>();
            if (cropManager != null)
            {
                foreach (var pos in _currentZone.TilePositions)
                {
                    var tile = MapManager.Instance.GetTileOrNull(pos);
                    if (tile?.Crop != null)
                        cropManager.RemoveCrop(tile);
                }
            }
        }

        var zoneManager = FindFirstObjectByType<ZoneManager>();
        zoneManager?.RemoveZone(_currentZone);
        Hide();
    }

    private static string ZoneTypeToJP(ZoneType type) => type switch
    {
        ZoneType.Storage => "保管ゾーン",
        ZoneType.Agriculture => "農業ゾーン",
        _ => type.ToString(),
    };
}
