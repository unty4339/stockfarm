using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 指令メニュー要素。指定タスクをプレイヤーが強制指示するためのUI。
/// ボタンを押すとモードに入り、メニュー要素が非表示になるとモードも解除される
/// </summary>
public class CommandMenuUI : MonoBehaviour
{
    private GameObject _panel;
    private DemolishModeUI _demolishModeUI;

    private void Awake()
    {
        BuildPanel();
    }

    private void Start()
    {
        _demolishModeUI = FindFirstObjectByType<DemolishModeUI>();
    }

    private void BuildPanel()
    {
        _panel = new GameObject("CommandPanel");
        _panel.transform.SetParent(transform, false);
        var rt = _panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(600, 200);
        rt.anchoredPosition = UIHelper.SubMenuPanelAnchoredPosition;
        var img = _panel.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);

        UIHelper.CreateButton(_panel.transform, "解体", new Vector2(-180, 0), 160, 72, OnDemolishPressed, 24);
        UIHelper.CreateButton(_panel.transform, "種付け", new Vector2(0, 0), 160, 72, OnBreedPressed, 24);
        UIHelper.CreateButton(_panel.transform, "搾乳", new Vector2(180, 0), 160, 72, OnMilkPressed, 24);

        _panel.SetActive(false);
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
        _demolishModeUI?.Exit();
        _panel?.SetActive(false);
    }

    private void Update()
    {
        if (!_panel.activeSelf) return;

        // モードがアクティブな間はモード側が入力を処理するためスキップする
        if (_demolishModeUI != null && _demolishModeUI.IsActive) return;

        var mouse = Mouse.current;
        if (mouse == null) return;
        if ((mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame)
            && !UIHelper.IsPointerOverUI())
            Hide();
    }

    /// <summary>
    /// 解体ボタン押下時の処理。解体モードに入る
    /// </summary>
    public void OnDemolishPressed()
    {
        _demolishModeUI?.Enter();
    }

    /// <summary>
    /// 種付けボタン押下時の処理
    /// </summary>
    public void OnBreedPressed()
    {
        Debug.Log("[CommandMenuUI] 種付けモード開始（未実装）");
    }

    /// <summary>
    /// 搾乳ボタン押下時の処理
    /// </summary>
    public void OnMilkPressed()
    {
        Debug.Log("[CommandMenuUI] 搾乳モード開始（未実装）");
    }
}
