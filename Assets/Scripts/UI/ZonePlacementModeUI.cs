using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 矩形ドラッグでゾーンを配置するモードのUI。
/// BuildModeUI と同様のガードフラグ IsInZonePlacementMode を持ち、
/// SelectionInputHandler がこのフラグを確認して入力をスキップする
/// </summary>
public class ZonePlacementModeUI : MonoBehaviour
{
    /// <summary>ゾーン配置モード中かどうか</summary>
    public bool IsInZonePlacementMode { get; private set; }
    /// <summary>配置中のゾーン種別</summary>
    public ZoneType SelectedZoneType { get; private set; }

    private TextMeshProUGUI _modeLabel;
    private RectTransform _previewRectRT;
    private Camera _mainCamera;
    private RectTransform _canvasRT;

    private Vector2 _dragStartScreen;
    private bool _isDragging;

    private void Awake()
    {
        BuildOverlay();
        BuildPreviewRect();
    }

    private void Start()
    {
        _mainCamera = Camera.main;
        _canvasRT = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
    }

    private void BuildOverlay()
    {
        var go = new GameObject("ZoneModeLabel");
        go.transform.SetParent(transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(400, 40);
        rt.anchoredPosition = new Vector2(0, -25);
        _modeLabel = go.AddComponent<TextMeshProUGUI>();
        _modeLabel.text = "";
        _modeLabel.fontSize = 14;
        _modeLabel.alignment = TextAlignmentOptions.Center;
        _modeLabel.color = Color.yellow;
        UIHelper.ApplyFont(_modeLabel);
    }

    private void BuildPreviewRect()
    {
        var go = new GameObject("ZonePreviewRect");
        go.transform.SetParent(transform, false);
        _previewRectRT = go.AddComponent<RectTransform>();
        // WorkerSelectionUI と同様にアンカーを中央に固定し、Canvas ローカル座標で位置・サイズを制御する
        _previewRectRT.anchorMin = new Vector2(0.5f, 0.5f);
        _previewRectRT.anchorMax = new Vector2(0.5f, 0.5f);
        _previewRectRT.pivot = Vector2.zero;
        _previewRectRT.sizeDelta = Vector2.zero;
        var img = go.AddComponent<Image>();
        img.color = new Color(1f, 0.85f, 0.2f, 0.25f);
        img.raycastTarget = false;
        go.SetActive(false);
    }

    private void Update()
    {
        if (!IsInZonePlacementMode) return;

        var mouse = Mouse.current;
        if (mouse == null) return;

        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            ExitZonePlacementMode();
            return;
        }

        if (mouse.rightButton.wasPressedThisFrame)
        {
            ExitZonePlacementMode();
            return;
        }

        var screenPos = mouse.position.ReadValue();

        if (mouse.leftButton.wasPressedThisFrame && !UIHelper.IsPointerOverUI())
        {
            _dragStartScreen = screenPos;
            var snappedRect = GetSnappedCanvasRect(screenPos, screenPos);
            _previewRectRT.anchoredPosition = snappedRect.min;
            _previewRectRT.sizeDelta = Vector2.zero;
            _isDragging = true;
            _previewRectRT.gameObject.SetActive(true);
        }

        if (_isDragging && mouse.leftButton.isPressed)
        {
            var snappedRect = GetSnappedCanvasRect(_dragStartScreen, screenPos);
            _previewRectRT.anchoredPosition = snappedRect.min;
            _previewRectRT.sizeDelta = snappedRect.size;
        }

        if (_isDragging && mouse.leftButton.wasReleasedThisFrame)
        {
            _isDragging = false;
            _previewRectRT.gameObject.SetActive(false);
            ConfirmZone(_dragStartScreen, screenPos);
        }
    }

    /// <summary>
    /// 指定ゾーン種別でゾーン配置モードを開始する
    /// </summary>
    /// <param name="type">ゾーン種別</param>
    public void EnterZonePlacementMode(ZoneType type)
    {
        IsInZonePlacementMode = true;
        SelectedZoneType = type;
        _isDragging = false;
        _modeLabel.text = $"{ZoneTypeToJP(type)}配置中  ドラッグで範囲を指定  [ESC]でキャンセル";
    }

    /// <summary>
    /// ゾーン配置モードを終了する
    /// </summary>
    public void ExitZonePlacementMode()
    {
        IsInZonePlacementMode = false;
        _isDragging = false;
        _modeLabel.text = "";
        _previewRectRT.gameObject.SetActive(false);
    }

    /// <summary>
    /// ドラッグ開始・終了のスクリーン座標からグリッドにスナップした矩形をキャンバス座標で返す
    /// </summary>
    private Rect GetSnappedCanvasRect(Vector2 screenStart, Vector2 screenEnd)
    {
        var worldStart = _mainCamera.ScreenToWorldPoint(new Vector3(screenStart.x, screenStart.y, 0f));
        var worldEnd = _mainCamera.ScreenToWorldPoint(new Vector3(screenEnd.x, screenEnd.y, 0f));

        var gridStart = GridHelper.WorldToGrid(worldStart);
        var gridEnd = GridHelper.WorldToGrid(worldEnd);

        int minX = Mathf.Min(gridStart.x, gridEnd.x);
        int maxX = Mathf.Max(gridStart.x, gridEnd.x);
        int minY = Mathf.Min(gridStart.y, gridEnd.y);
        int maxY = Mathf.Max(gridStart.y, gridEnd.y);

        // 選択タイル群のワールド空間上の端に合わせる（タイル1セルは [-0.5, +0.5]）
        var screenRectMin = _mainCamera.WorldToScreenPoint(new Vector3(minX - 0.5f, minY - 0.5f, 0f));
        var screenRectMax = _mainCamera.WorldToScreenPoint(new Vector3(maxX + 0.5f, maxY + 0.5f, 0f));

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRT, screenRectMin, null, out Vector2 canvasMin);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRT, screenRectMax, null, out Vector2 canvasMax);

        var rectMin = Vector2.Min(canvasMin, canvasMax);
        var rectMax = Vector2.Max(canvasMin, canvasMax);
        return new Rect(rectMin, rectMax - rectMin);
    }

    private void ConfirmZone(Vector2 screenStart, Vector2 screenEnd)
    {
        var tiles = GetTilesInRect(screenStart, screenEnd);
        ZoneManager.Instance?.CreateZone(SelectedZoneType, tiles);
    }

    private IEnumerable<Vector2Int> GetTilesInRect(Vector2 screenStart, Vector2 screenEnd)
    {
        var worldStart = _mainCamera.ScreenToWorldPoint(new Vector3(screenStart.x, screenStart.y, 0f));
        var worldEnd = _mainCamera.ScreenToWorldPoint(new Vector3(screenEnd.x, screenEnd.y, 0f));

        var gridStart = GridHelper.WorldToGrid(worldStart);
        var gridEnd = GridHelper.WorldToGrid(worldEnd);

        int minX = Mathf.Min(gridStart.x, gridEnd.x);
        int maxX = Mathf.Max(gridStart.x, gridEnd.x);
        int minY = Mathf.Min(gridStart.y, gridEnd.y);
        int maxY = Mathf.Max(gridStart.y, gridEnd.y);

        var result = new List<Vector2Int>();
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                result.Add(new Vector2Int(x, y));
            }
        }
        return result;
    }

    private static string ZoneTypeToJP(ZoneType type) => type switch
    {
        ZoneType.Storage => "保管ゾーン",
        ZoneType.Agriculture => "農業ゾーン",
        _ => type.ToString(),
    };
}
