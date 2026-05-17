using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 施設設置モード中のUI。GhostPlacerと連携して配置プレビューを管理する。
/// 壁・柵はドラッグで一列まとめて建設できる
/// </summary>
public class FacilityPlacementModeUI : MonoBehaviour, IModeUI
{
    private const float DragThreshold = 5f;

    /// <summary>ゴースト表示コンポーネントの参照</summary>
    public GhostPlacer GhostPlacer { get; private set; }
    /// <summary>施設設置モード中かどうか</summary>
    public bool IsActive { get; private set; }
    /// <summary>配置中の設備種別</summary>
    public EquipmentType CurrentType { get; private set; }
    /// <summary>現在の配置回転角度（0/90/180/270度）</summary>
    public int CurrentRotation { get; private set; }

    private TextMeshProUGUI _modeLabel;
    private Camera _mainCamera;
    private RectTransform _linePreviewRT;
    private Image _linePreviewImage;
    private RectTransform _canvasRT;

    private Vector2 _dragStartScreen;
    private bool _isMouseDown;
    private bool _isDragging;

    private void Awake()
    {
        GhostPlacer = gameObject.AddComponent<GhostPlacer>();
        BuildOverlay();
        BuildLinePreview();
    }

    private void Start()
    {
        _mainCamera = Camera.main;
        _canvasRT = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
    }

    private void BuildOverlay()
    {
        var go = new GameObject("FacilityPlacementModeLabel");
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

    private void BuildLinePreview()
    {
        var go = new GameObject("WallDragLinePreview");
        go.transform.SetParent(transform, false);
        _linePreviewRT = go.AddComponent<RectTransform>();
        _linePreviewRT.anchorMin = new Vector2(0.5f, 0.5f);
        _linePreviewRT.anchorMax = new Vector2(0.5f, 0.5f);
        _linePreviewRT.pivot = Vector2.zero;
        _linePreviewRT.sizeDelta = Vector2.zero;
        _linePreviewImage = go.AddComponent<Image>();
        _linePreviewImage.raycastTarget = false;
        go.SetActive(false);
    }

    private void Update()
    {
        if (!IsActive) return;

        var mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.rightButton.wasPressedThisFrame)
        {
            Exit();
            return;
        }

        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.rKey.wasPressedThisFrame && BuildingManager.SupportsRotation(CurrentType))
            {
                CurrentRotation = (CurrentRotation + 90) % 360;
                UpdateModeLabel();
            }
            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                Exit();
                return;
            }
        }

        var screenPos = mouse.position.ReadValue();
        var worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
        var gridPos = GridHelper.WorldToGrid(worldPos);

        if (SupportsDragPlacement(CurrentType))
            HandleDragPlacement(mouse, screenPos, gridPos);
        else
        {
            GhostPlacer.ShowGhost(CurrentType, gridPos, CurrentRotation);
            if (mouse.leftButton.wasPressedThisFrame && !UIHelper.IsPointerOverUI())
                OnGridClicked(gridPos);
        }
    }

    private void HandleDragPlacement(Mouse mouse, Vector2 screenPos, Vector2Int gridPos)
    {
        if (mouse.leftButton.wasPressedThisFrame && !UIHelper.IsPointerOverUI())
        {
            _dragStartScreen = screenPos;
            _isMouseDown = true;
            _isDragging = false;
        }

        if (_isMouseDown && mouse.leftButton.isPressed)
        {
            if (!_isDragging && Vector2.Distance(screenPos, _dragStartScreen) >= DragThreshold)
                _isDragging = true;

            if (_isDragging)
            {
                GhostPlacer.HideGhost();
                UpdateLinePreview(_dragStartScreen, screenPos);
            }
            else
            {
                GhostPlacer.ShowGhost(CurrentType, gridPos, CurrentRotation);
            }
        }
        else if (!_isMouseDown)
        {
            GhostPlacer.ShowGhost(CurrentType, gridPos, CurrentRotation);
        }

        if (_isMouseDown && mouse.leftButton.wasReleasedThisFrame)
        {
            bool wasDragging = _isDragging;
            _isMouseDown = false;
            _isDragging = false;
            _linePreviewRT.gameObject.SetActive(false);

            if (wasDragging)
                PlaceDragLine(_dragStartScreen, screenPos);
            else
                OnGridClicked(gridPos);
        }
    }

    private void UpdateLinePreview(Vector2 screenStart, Vector2 screenEnd)
    {
        var allPositions = GetDragLine(screenStart, screenEnd);
        var placeable = GetActualPlacementPositions(allPositions);
        bool canAfford = placeable.Count > 0 &&
            EconomyManager.Instance.CurrentFunds >= BuildingManager.GetBuildCost(CurrentType) * placeable.Count;
        _linePreviewImage.color = canAfford
            ? new Color(0f, 1f, 0f, 0.4f)
            : new Color(1f, 0f, 0f, 0.4f);

        var canvasRect = GetLineCanvasRect(allPositions);
        _linePreviewRT.anchoredPosition = canvasRect.min;
        _linePreviewRT.sizeDelta = canvasRect.size;
        _linePreviewRT.gameObject.SetActive(true);
    }

    /// <summary>
    /// ドラッグした一列分を一括建設する。既に設備があるマスはスキップし、
    /// 建設対象の合計コストが足りない場合は何もしない
    /// </summary>
    /// <param name="screenStart">ドラッグ開始のスクリーン座標</param>
    /// <param name="screenEnd">ドラッグ終了のスクリーン座標</param>
    private void PlaceDragLine(Vector2 screenStart, Vector2 screenEnd)
    {
        var placeable = GetActualPlacementPositions(GetDragLine(screenStart, screenEnd));
        if (placeable.Count == 0) return;

        int totalCost = BuildingManager.GetBuildCost(CurrentType) * placeable.Count;
        if (EconomyManager.Instance.CurrentFunds < totalCost) return;

        foreach (var pos in placeable)
            BuildingManager.Instance?.PlaceEquipment(CurrentType, pos, CurrentRotation);
    }

    /// <summary>
    /// 指定設備がドラッグ一括配置に対応しているかを返す（壁・柵のみ）
    /// </summary>
    private static bool SupportsDragPlacement(EquipmentType type)
    {
        return type == EquipmentType.Wall || type == EquipmentType.Fence;
    }

    /// <summary>
    /// ドラッグ開始・終了のスクリーン座標から、縦または横の一列グリッド座標リストを返す。
    /// X方向とY方向の変量が同じ場合はX方向優先とする
    /// </summary>
    /// <param name="screenStart">ドラッグ開始のスクリーン座標</param>
    /// <param name="screenEnd">ドラッグ終了のスクリーン座標</param>
    private List<Vector2Int> GetDragLine(Vector2 screenStart, Vector2 screenEnd)
    {
        var worldStart = _mainCamera.ScreenToWorldPoint(new Vector3(screenStart.x, screenStart.y, 0f));
        var worldEnd = _mainCamera.ScreenToWorldPoint(new Vector3(screenEnd.x, screenEnd.y, 0f));

        var gridStart = GridHelper.WorldToGrid(worldStart);
        var gridEnd = GridHelper.WorldToGrid(worldEnd);

        int dx = Mathf.Abs(gridEnd.x - gridStart.x);
        int dy = Mathf.Abs(gridEnd.y - gridStart.y);

        var positions = new List<Vector2Int>();
        if (dx >= dy)
        {
            int minX = Mathf.Min(gridStart.x, gridEnd.x);
            int maxX = Mathf.Max(gridStart.x, gridEnd.x);
            for (int x = minX; x <= maxX; x++)
                positions.Add(new Vector2Int(x, gridStart.y));
        }
        else
        {
            int minY = Mathf.Min(gridStart.y, gridEnd.y);
            int maxY = Mathf.Max(gridStart.y, gridEnd.y);
            for (int y = minY; y <= maxY; y++)
                positions.Add(new Vector2Int(gridStart.x, y));
        }

        return positions;
    }

    /// <summary>
    /// 一列のグリッド座標のうち、実際に建設が必要なマスのみを返す。
    /// 既に設備があるマスやマップ外のマスはスキップする
    /// </summary>
    private List<Vector2Int> GetActualPlacementPositions(List<Vector2Int> allPositions)
    {
        var result = new List<Vector2Int>();
        foreach (var pos in allPositions)
        {
            if (GhostPlacer.CanPlace(CurrentType, pos, CurrentRotation))
                result.Add(pos);
        }
        return result;
    }

    /// <summary>
    /// グリッド座標リストからキャンバス空間の矩形を計算して返す
    /// </summary>
    private Rect GetLineCanvasRect(List<Vector2Int> positions)
    {
        if (positions.Count == 0) return new Rect();

        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;
        foreach (var p in positions)
        {
            if (p.x < minX) minX = p.x;
            if (p.x > maxX) maxX = p.x;
            if (p.y < minY) minY = p.y;
            if (p.y > maxY) maxY = p.y;
        }

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

    /// <summary>
    /// 指定設備の施設設置モードを開始する
    /// </summary>
    /// <param name="type">設備種別</param>
    public void Enter(EquipmentType type)
    {
        ModeCoordinator.Enter(this);
        IsActive = true;
        CurrentType = type;
        CurrentRotation = 0;
        _isMouseDown = false;
        _isDragging = false;
        UpdateModeLabel();
    }

    /// <summary>
    /// 施設設置モードを終了する
    /// </summary>
    public void Exit()
    {
        if (!IsActive) return;
        IsActive = false;
        ModeCoordinator.Exit(this);
        GhostPlacer.HideGhost();
        _isMouseDown = false;
        _isDragging = false;
        _linePreviewRT.gameObject.SetActive(false);
        _modeLabel.text = "";
    }

    /// <summary>
    /// プレイヤーがグリッドをクリックしたときの処理
    /// </summary>
    /// <param name="position">クリックしたグリッド座標</param>
    public void OnGridClicked(Vector2Int position)
    {
        BuildingManager.Instance?.PlaceEquipment(CurrentType, position, CurrentRotation);
    }

    private void UpdateModeLabel()
    {
        if (BuildingManager.SupportsRotation(CurrentType))
            _modeLabel.text = $"配置中: {CurrentType}  回転: {CurrentRotation}°  [R]で回転  [ESC]でキャンセル";
        else
            _modeLabel.text = $"配置中: {CurrentType}  [ESC]でキャンセル";
    }
}
