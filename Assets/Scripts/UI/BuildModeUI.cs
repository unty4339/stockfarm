using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 施設配置モード中のUI。GhostPlacerと連携して配置プレビューを管理する
/// </summary>
public class BuildModeUI : MonoBehaviour
{
    /// <summary>ゴースト表示コンポーネントの参照</summary>
    public GhostPlacer GhostPlacer { get; private set; }
    /// <summary>配置モード中かどうか</summary>
    public bool IsInBuildMode { get; private set; }
    /// <summary>配置中の設備種別</summary>
    public EquipmentType CurrentType { get; private set; }

    private TextMeshProUGUI _modeLabel;
    private Camera _mainCamera;

    private void Awake()
    {
        GhostPlacer = gameObject.AddComponent<GhostPlacer>();
        BuildOverlay();
    }

    private void Start()
    {
        _mainCamera = Camera.main;
    }

    private void BuildOverlay()
    {
        var go = new GameObject("BuildModeLabel");
        go.transform.SetParent(transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(300, 40);
        rt.anchoredPosition = new Vector2(0, -25);
        _modeLabel = go.AddComponent<TextMeshProUGUI>();
        _modeLabel.text = "";
        _modeLabel.fontSize = 14;
        _modeLabel.alignment = TextAlignmentOptions.Center;
        _modeLabel.color = Color.yellow;
        UIHelper.ApplyFont(_modeLabel);
    }

    private void Update()
    {
        if (!IsInBuildMode) return;

        var mouse = Mouse.current;
        if (mouse == null) return;

        var worldPos = _mainCamera.ScreenToWorldPoint(mouse.position.ReadValue());
        var gridPos = new Vector2Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.y));

        GhostPlacer.ShowGhost(CurrentType, gridPos);

        if (mouse.leftButton.wasPressedThisFrame)
            OnGridClicked(gridPos);

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            ExitBuildMode();
    }

    /// <summary>
    /// 指定設備の配置モードを開始する
    /// </summary>
    /// <param name="type">設備種別</param>
    public void EnterBuildMode(EquipmentType type)
    {
        IsInBuildMode = true;
        CurrentType = type;
        _modeLabel.text = $"配置中: {type}  [ESC]でキャンセル";
    }

    /// <summary>
    /// 配置モードを終了する
    /// </summary>
    public void ExitBuildMode()
    {
        IsInBuildMode = false;
        GhostPlacer.HideGhost();
        _modeLabel.text = "";
    }

    /// <summary>
    /// プレイヤーがグリッドをクリックしたときの処理
    /// </summary>
    /// <param name="position">クリックしたグリッド座標</param>
    public void OnGridClicked(Vector2Int position)
    {
        BuildingManager.Instance?.PlaceEquipment(CurrentType, position);
    }
}
