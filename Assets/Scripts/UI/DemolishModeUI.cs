using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 解体モード中の入力・プレビュー描画を管理するUI。
/// クリックで単体売却、ドラッグでグリッド矩形範囲内の設備を一括売却する。
/// ModeCoordinator にアクティブ状態を登録し、SelectionInputHandler はこれを介して入力をスキップする
/// </summary>
public class DemolishModeUI : MonoBehaviour, IModeUI
{
    private const float DragThreshold = 5f;

    /// <summary>解体モード中かどうか</summary>
    public bool IsActive { get; private set; }

    private TextMeshProUGUI _modeLabel;
    private RectTransform _previewRectRT;
    private Camera _mainCamera;
    private RectTransform _canvasRT;

    private Vector2 _dragStartScreen;
    private bool _isMouseDown;
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
        var go = new GameObject("DemolishModeLabel");
        go.transform.SetParent(transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(500, 40);
        rt.anchoredPosition = new Vector2(0, -25);
        _modeLabel = go.AddComponent<TextMeshProUGUI>();
        _modeLabel.text = "";
        _modeLabel.fontSize = 14;
        _modeLabel.alignment = TextAlignmentOptions.Center;
        _modeLabel.color = new Color(1f, 0.4f, 0.4f);
        UIHelper.ApplyFont(_modeLabel);
    }

    private void BuildPreviewRect()
    {
        var go = new GameObject("DemolishPreviewRect");
        go.transform.SetParent(transform, false);
        _previewRectRT = go.AddComponent<RectTransform>();
        _previewRectRT.anchorMin = new Vector2(0.5f, 0.5f);
        _previewRectRT.anchorMax = new Vector2(0.5f, 0.5f);
        _previewRectRT.pivot = Vector2.zero;
        _previewRectRT.sizeDelta = Vector2.zero;
        var img = go.AddComponent<Image>();
        img.color = new Color(1f, 0.2f, 0.2f, 0.3f);
        img.raycastTarget = false;
        go.SetActive(false);
    }

    private void Update()
    {
        if (!IsActive) return;

        var mouse = Mouse.current;
        if (mouse == null) return;

        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame)
        {
            Exit();
            return;
        }

        if (mouse.rightButton.wasPressedThisFrame)
        {
            Exit();
            return;
        }

        var screenPos = mouse.position.ReadValue();

        if (mouse.leftButton.wasPressedThisFrame && !UIHelper.IsPointerOverUI())
        {
            _dragStartScreen = screenPos;
            _isMouseDown = true;
            _isDragging = false;
        }

        if (_isMouseDown && mouse.leftButton.isPressed)
        {
            if (!_isDragging && Vector2.Distance(screenPos, _dragStartScreen) >= DragThreshold)
            {
                _isDragging = true;
                _previewRectRT.gameObject.SetActive(true);
            }

            if (_isDragging)
            {
                var snappedRect = GetSnappedCanvasRect(_dragStartScreen, screenPos);
                _previewRectRT.anchoredPosition = snappedRect.min;
                _previewRectRT.sizeDelta = snappedRect.size;
            }
        }

        if (_isMouseDown && mouse.leftButton.wasReleasedThisFrame)
        {
            _isMouseDown = false;
            _previewRectRT.gameObject.SetActive(false);

            if (_isDragging)
            {
                _isDragging = false;
                DemolishInRect(_dragStartScreen, screenPos);
            }
            else
            {
                DemolishAtScreen(_dragStartScreen);
            }
        }
    }

    /// <summary>
    /// 解体モードを開始する
    /// </summary>
    public void Enter()
    {
        ModeCoordinator.Enter(this);
        IsActive = true;
        _isMouseDown = false;
        _isDragging = false;
        _modeLabel.text = "解体モード  クリック: 単体売却 / ドラッグ: 範囲売却  [ESC]でキャンセル";
    }

    /// <summary>
    /// 解体モードを終了する
    /// </summary>
    public void Exit()
    {
        if (!IsActive) return;
        IsActive = false;
        ModeCoordinator.Exit(this);
        _isMouseDown = false;
        _isDragging = false;
        _modeLabel.text = "";
        _previewRectRT.gameObject.SetActive(false);
    }

    /// <summary>
    /// スクリーン座標の位置にある設備を1つ売却する
    /// </summary>
    private void DemolishAtScreen(Vector2 screenPos)
    {
        var worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
        worldPos.z = 0f;
        var eq = FindEquipmentAtWorld(worldPos);
        if (eq != null)
            BuildingManager.Instance?.RemoveEquipment(eq.GridPosition);
    }

    /// <summary>
    /// スクリーン座標の矩形範囲内にある全設備を売却する
    /// </summary>
    private void DemolishInRect(Vector2 screenStart, Vector2 screenEnd)
    {
        var worldStart = _mainCamera.ScreenToWorldPoint(new Vector3(screenStart.x, screenStart.y, 0f));
        var worldEnd = _mainCamera.ScreenToWorldPoint(new Vector3(screenEnd.x, screenEnd.y, 0f));

        float minX = Mathf.Min(worldStart.x, worldEnd.x);
        float maxX = Mathf.Max(worldStart.x, worldEnd.x);
        float minY = Mathf.Min(worldStart.y, worldEnd.y);
        float maxY = Mathf.Max(worldStart.y, worldEnd.y);

        var toRemove = new List<EquipmentBase>();
        foreach (var eq in FindObjectsByType<EquipmentBase>(FindObjectsSortMode.None))
        {
            if (!eq.IsPlaced) continue;
            var center = eq.transform.position;
            float halfW = eq.Size.x * 0.5f;
            float halfH = eq.Size.y * 0.5f;
            if (center.x + halfW > minX && center.x - halfW < maxX &&
                center.y + halfH > minY && center.y - halfH < maxY)
                toRemove.Add(eq);
        }

        foreach (var eq in toRemove)
            BuildingManager.Instance?.RemoveEquipment(eq.GridPosition);
    }

    private EquipmentBase FindEquipmentAtWorld(Vector3 worldPos)
    {
        foreach (var eq in FindObjectsByType<EquipmentBase>(FindObjectsSortMode.None))
        {
            if (!eq.IsPlaced) continue;
            var center = eq.transform.position;
            float halfW = eq.Size.x * 0.5f;
            float halfH = eq.Size.y * 0.5f;
            if (Mathf.Abs(worldPos.x - center.x) <= halfW && Mathf.Abs(worldPos.y - center.y) <= halfH)
                return eq;
        }
        return null;
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
}
