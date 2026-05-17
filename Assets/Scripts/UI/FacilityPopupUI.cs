using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 施設をクリックしたときに表示されるポップアップ
/// </summary>
public class FacilityPopupUI : MonoBehaviour
{
    private GameObject _panel;
    private TextMeshProUGUI _nameText;
    private TextMeshProUGUI _infoText;

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
        _panel = new GameObject("FacilityPopup");
        _panel.transform.SetParent(transform, false);
        var rt = _panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(0f, 0f);
        rt.pivot = new Vector2(0f, 0f);
        rt.sizeDelta = new Vector2(400, 240);
        rt.anchoredPosition = new Vector2(0, 100);
        var img = _panel.AddComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        _nameText = UIHelper.CreateText(_panel.transform, "NameText", "設備名",
            new Vector2(0, 80), 28, Color.white);
        _infoText = UIHelper.CreateText(_panel.transform, "InfoText", "",
            new Vector2(0, 0), 22, new Color(0.8f, 0.8f, 0.8f));

        UIHelper.CreateButton(_panel.transform, "閉じる", new Vector2(0, -84), 140, 48, Hide, 24);
        _panel.SetActive(false);
    }

    /// <summary>
    /// 指定設備の情報を表示する
    /// </summary>
    /// <param name="equipment">表示する設備</param>
    public void Show(EquipmentBase equipment)
    {
        PopupCoordinator.NotifyShown();
        _panel?.SetActive(true);
        _nameText.text = equipment.Type.ToString();
        _infoText.text = $"コスト: {equipment.BuildCost}\nムード: {equipment.AffectedMoodType} +{equipment.MoodBonus}";
    }

    /// <summary>
    /// ポップアップを非表示にする
    /// </summary>
    public void Hide() => _panel?.SetActive(false);
}
