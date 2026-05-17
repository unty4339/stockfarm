using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 施設設置モードへの入口となるメニュー要素。設備種別のボタン一覧を表示する。
/// ボタンを押すと施設設置モードに入り、メニュー要素が非表示になるとモードも解除される
/// </summary>
public class FacilityMenuUI : MonoBehaviour
{
    private GameObject _panel;
    private FacilityPlacementModeUI _placementModeUI;

    private void Awake()
    {
        _panel = new GameObject("FacilityPanel");
        _panel.transform.SetParent(transform, false);
        var rt = _panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.sizeDelta = new Vector2(0, 260);
        rt.anchoredPosition = UIHelper.SubMenuPanelAnchoredPosition;
        var img = _panel.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);

        BuildButtons();
        _panel.SetActive(false);
    }

    private void Start()
    {
        _placementModeUI = FindFirstObjectByType<FacilityPlacementModeUI>();
    }

    private void Update()
    {
        if (!_panel.activeSelf) return;

        // モードがアクティブな間はモード側が入力を処理するためスキップする
        if (_placementModeUI != null && _placementModeUI.IsActive) return;

        var mouse = Mouse.current;
        if (mouse == null) return;
        if (mouse.rightButton.wasPressedThisFrame)
        {
            Hide();
            return;
        }
        if (mouse.leftButton.wasPressedThisFrame && !UIHelper.IsPointerOverUI())
            Hide();
    }

    private void BuildButtons()
    {
        EquipmentType[] types = {
            EquipmentType.Wall, EquipmentType.Fence, EquipmentType.Gate,
            EquipmentType.FeedingTrough,
            EquipmentType.StrawBed, EquipmentType.NormalBed,
            EquipmentType.LuxuryBed, EquipmentType.KingBed,
            EquipmentType.SellPoint,
        };

        float startX = -((types.Length - 1) * 0.5f) * 180f;
        for (int i = 0; i < types.Length; i++)
        {
            var type = types[i];
            float x = startX + i * 180f;
            UIHelper.CreateButton(_panel.transform, EquipmentTypeToJP(type),
                new Vector2(x, 0), 160, 72, () => OnFacilitySelected(type), 24);
        }
    }

    /// <summary>パネルが表示中かどうか</summary>
    public bool IsVisible => _panel != null && _panel.activeSelf;
    /// <summary>パネルを表示する</summary>
    public void Show() => _panel?.SetActive(true);

    /// <summary>
    /// パネルを非表示にする。このメニュー要素から設定されたモードも解除する
    /// </summary>
    public void Hide()
    {
        _placementModeUI?.Exit();
        _panel?.SetActive(false);
    }

    /// <summary>
    /// 指定設備の施設設置モードを開始する
    /// </summary>
    /// <param name="type">選択された設備種別</param>
    public void OnFacilitySelected(EquipmentType type)
    {
        _placementModeUI?.Enter(type);
    }

    private static string EquipmentTypeToJP(EquipmentType type)
    {
        return type switch
        {
            EquipmentType.Floor => "床",
            EquipmentType.Wall => "壁",
            EquipmentType.Fence => "柵",
            EquipmentType.Gate => "ゲート",
            EquipmentType.FeedingTrough => "食事机",
            EquipmentType.StrawBed => "藁ベッド",
            EquipmentType.NormalBed => "普通ベッド",
            EquipmentType.LuxuryBed => "贅沢ベッド",
            EquipmentType.KingBed => "キングベッド",
            EquipmentType.SellPoint => "売場",
            _ => type.ToString(),
        };
    }
}
