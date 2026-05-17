using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// アイテムをクリックしたときに表示されるポップアップ
/// アイテムの名称・数・品質・売却フラグを表示し、売却ボタンでフラグを切り替える
/// </summary>
public class ItemPopupUI : MonoBehaviour
{
    private GameObject _panel;
    private TextMeshProUGUI _nameText;
    private TextMeshProUGUI _stackText;
    private TextMeshProUGUI _qualityText;
    private TextMeshProUGUI _sellFlagText;
    private TextMeshProUGUI _sellButtonLabel;
    private GridTile _currentTile;

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
        _panel = new GameObject("ItemPopup");
        _panel.transform.SetParent(transform, false);
        var rt = _panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(0f, 0f);
        rt.pivot = new Vector2(0f, 0f);
        rt.sizeDelta = new Vector2(400, 350);
        rt.anchoredPosition = new Vector2(0, 100);
        var img = _panel.AddComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        _nameText = UIHelper.CreateText(_panel.transform, "NameText", "", new Vector2(0, 136), 28, Color.white);
        _stackText = UIHelper.CreateText(_panel.transform, "StackText", "", new Vector2(0, 84), 22, new Color(0.8f, 0.8f, 0.8f));
        _qualityText = UIHelper.CreateText(_panel.transform, "QualityText", "", new Vector2(0, 40), 22, new Color(0.8f, 0.8f, 0.8f));
        _sellFlagText = UIHelper.CreateText(_panel.transform, "SellFlagText", "", new Vector2(0, -10), 22, new Color(1f, 0.85f, 0.2f));

        var sellBtn = UIHelper.CreateButton(_panel.transform, "売却する", new Vector2(0, -70), 180, 52, OnSellButtonClicked, 24);
        _sellButtonLabel = sellBtn.GetComponentInChildren<TextMeshProUGUI>();

        UIHelper.CreateButton(_panel.transform, "閉じる", new Vector2(0, -136), 140, 48, Hide, 24);

        _panel.SetActive(false);
    }

    /// <summary>
    /// 指定タイルのアイテム情報を表示する
    /// </summary>
    /// <param name="tile">アイテムが置かれているタイル</param>
    public void Show(GridTile tile)
    {
        PopupCoordinator.NotifyShown();
        _currentTile = tile;
        _panel?.SetActive(true);
        Refresh();
    }

    /// <summary>
    /// ポップアップを非表示にする
    /// </summary>
    public void Hide()
    {
        _panel?.SetActive(false);
        _currentTile = null;
    }

    private void Refresh()
    {
        if (_currentTile?.PlacedItem == null)
        {
            Hide();
            return;
        }

        var item = _currentTile.PlacedItem;
        _nameText.text = item.Type.ToString();

        if (item.IsStackable)
        {
            _stackText.text = $"数: {item.StackCount}";
            _stackText.gameObject.SetActive(true);
        }
        else
        {
            _stackText.gameObject.SetActive(false);
        }

        if (item.Quality.HasValue)
        {
            _qualityText.text = $"品質: {item.Quality.Value:F1}";
            _qualityText.gameObject.SetActive(true);
        }
        else
        {
            _qualityText.gameObject.SetActive(false);
        }

        bool isSell = item.SellFlag;
        _sellFlagText.text = isSell ? "売却: ON" : "売却: OFF";
        if (_sellButtonLabel != null)
            _sellButtonLabel.text = isSell ? "売却解除" : "売却する";
    }

    private void OnSellButtonClicked()
    {
        if (_currentTile?.PlacedItem == null) return;
        _currentTile.PlacedItem.SellFlag = !_currentTile.PlacedItem.SellFlag;
        Refresh();
    }
}
