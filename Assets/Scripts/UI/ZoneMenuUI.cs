using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// ゾーン種別の選択サブメニュー。ゾーン設定ボタンから開かれる
/// </summary>
public class ZoneMenuUI : MonoBehaviour
{
    private GameObject _panel;
    private ZonePlacementModeUI _placementModeUI;

    private void Awake()
    {
        _panel = new GameObject("ZonePanel");
        _panel.transform.SetParent(transform, false);
        var rt = _panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.sizeDelta = new Vector2(0, 80);
        rt.anchoredPosition = new Vector2(0, 90);
        var img = _panel.AddComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);

        BuildButtons();
        _panel.SetActive(false);
    }

    private void Start()
    {
        _placementModeUI = FindFirstObjectByType<ZonePlacementModeUI>();
    }

    private void Update()
    {
        if (!_panel.activeSelf) return;
        if (_placementModeUI != null && _placementModeUI.IsInZonePlacementMode) return;

        var mouse = Mouse.current;
        if (mouse == null) return;
        if (mouse.rightButton.wasPressedThisFrame)
            Hide();
    }

    private void BuildButtons()
    {
        UIHelper.CreateButton(_panel.transform, "保管ゾーン",
            new Vector2(-90, 0), 80, 36, () => OnZoneTypeSelected(ZoneType.Storage));

        // TODO: 農業ゾーンは未実装のため非活性表示
        var agriBtn = UIHelper.CreateButton(_panel.transform, "農業ゾーン",
            new Vector2(0, 0), 80, 36, null);
        agriBtn.interactable = false;
    }

    /// <summary>パネルを表示する</summary>
    public void Show() => _panel?.SetActive(true);
    /// <summary>パネルを非表示にする</summary>
    public void Hide() => _panel?.SetActive(false);

    /// <summary>
    /// 指定ゾーン種別の配置モードを開始する
    /// </summary>
    /// <param name="type">選択されたゾーン種別</param>
    private void OnZoneTypeSelected(ZoneType type)
    {
        _placementModeUI?.EnterZonePlacementMode(type);
    }
}
