using UnityEngine;

/// <summary>
/// 施設配置モードへの入口となるメニュー。設備種別のボタン一覧を表示する
/// </summary>
public class FacilityMenuUI : MonoBehaviour
{
    private GameObject _panel;
    private BuildModeUI _buildModeUI;

    private void Awake()
    {
        _panel = new GameObject("FacilityPanel");
        _panel.transform.SetParent(transform, false);
        var rt = _panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.sizeDelta = new Vector2(0, 130);
        rt.anchoredPosition = new Vector2(0, 90);
        var img = _panel.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);

        BuildButtons();
        _panel.SetActive(false);
    }

    private void Start()
    {
        _buildModeUI = FindFirstObjectByType<BuildModeUI>();
    }

    private void BuildButtons()
    {
        EquipmentType[] types = {
            EquipmentType.Floor, EquipmentType.Wall, EquipmentType.Fence, EquipmentType.Gate,
            EquipmentType.FeedingTrough, EquipmentType.FoodShelf, EquipmentType.Chest,
            EquipmentType.CowBed, EquipmentType.SellPoint,
        };

        float startX = -((types.Length - 1) * 0.5f) * 90f;
        for (int i = 0; i < types.Length; i++)
        {
            var type = types[i];
            float x = startX + i * 90f;
            UIHelper.CreateButton(_panel.transform, EquipmentTypeToJP(type),
                new Vector2(x, 0), 80, 36, () => OnFacilitySelected(type));
        }
    }

    /// <summary>パネルを表示する</summary>
    public void Show() => _panel?.SetActive(true);
    /// <summary>パネルを非表示にする</summary>
    public void Hide() => _panel?.SetActive(false);

    /// <summary>
    /// 指定設備の配置モードを開始する
    /// </summary>
    /// <param name="type">選択された設備種別</param>
    public void OnFacilitySelected(EquipmentType type)
    {
        Hide();
        _buildModeUI?.EnterBuildMode(type);
    }

    private static string EquipmentTypeToJP(EquipmentType type)
    {
        return type switch
        {
            EquipmentType.Floor => "床",
            EquipmentType.Wall => "壁",
            EquipmentType.Fence => "柵",
            EquipmentType.Gate => "ゲート",
            EquipmentType.FeedingTrough => "給餌桶",
            EquipmentType.FoodShelf => "食料棚",
            EquipmentType.Chest => "チェスト",
            EquipmentType.CowBed => "ベッド",
            EquipmentType.SellPoint => "売場",
            _ => type.ToString(),
        };
    }
}
