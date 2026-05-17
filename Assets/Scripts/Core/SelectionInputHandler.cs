using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// マウスの左クリック・ドラッグ入力を監視し、ワーカー・施設の選択を制御する入力ハンドラ
/// ワーカーが優先され、範囲内にワーカーがいない場合のみ施設を対象にする
/// BuildModeUI がアクティブな間はすべての処理をスキップする
/// </summary>
public class SelectionInputHandler : MonoBehaviour
{
    private const float DragThreshold = 5f;
    private const float WorkerPickRadius = 0.5f;

    private BuildModeUI _buildModeUI;
    private ZonePlacementModeUI _zonePlacementModeUI;
    private DemolishModeUI _demolishModeUI;
    private WorkerPopupUI _workerPopupUI;
    private FacilityPopupUI _facilityPopupUI;
    private ZonePopupUI _zonePopupUI;
    private ItemPopupUI _itemPopupUI;
    private WorkerSelectionUI _workerSelectionUI;
    private Camera _mainCamera;

    private Vector2 _mouseDownPos;
    private bool _mouseDownActive;
    private bool _dragStarted;

    private void Start()
    {
        _buildModeUI = FindFirstObjectByType<BuildModeUI>();
        _zonePlacementModeUI = FindFirstObjectByType<ZonePlacementModeUI>();
        _demolishModeUI = FindFirstObjectByType<DemolishModeUI>();
        _workerPopupUI = FindFirstObjectByType<WorkerPopupUI>();
        _facilityPopupUI = FindFirstObjectByType<FacilityPopupUI>();
        _zonePopupUI = FindFirstObjectByType<ZonePopupUI>();
        _itemPopupUI = FindFirstObjectByType<ItemPopupUI>();
        _workerSelectionUI = FindFirstObjectByType<WorkerSelectionUI>();
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        if (_buildModeUI != null && _buildModeUI.IsInBuildMode) return;
        if (_zonePlacementModeUI != null && _zonePlacementModeUI.IsInZonePlacementMode) return;
        if (_demolishModeUI != null && _demolishModeUI.IsInDemolishMode) return;

        if (mouse.leftButton.wasPressedThisFrame)
            OnMouseDown(mouse.position.ReadValue());

        if (mouse.leftButton.isPressed && _mouseDownActive)
            OnMouseDrag(mouse.position.ReadValue());

        if (mouse.leftButton.wasReleasedThisFrame && _mouseDownActive)
        {
            OnMouseUp(mouse.position.ReadValue());
            _mouseDownActive = false;
            _dragStarted = false;
        }
    }

    private void OnMouseDown(Vector2 screenPos)
    {
        if (UIHelper.IsPointerOverUI()) return;

        _mouseDownPos = screenPos;
        _mouseDownActive = true;
        _dragStarted = false;
    }

    private void OnMouseDrag(Vector2 screenPos)
    {
        if (Vector2.Distance(screenPos, _mouseDownPos) < DragThreshold) return;

        if (!_dragStarted)
        {
            _dragStarted = true;
            _workerSelectionUI?.BeginRect(_mouseDownPos);
        }

        _workerSelectionUI?.UpdateRect(screenPos);
    }

    private void OnMouseUp(Vector2 screenPos)
    {
        if (!_dragStarted)
        {
            HandleSingleClick(_mouseDownPos);
            return;
        }

        _workerSelectionUI?.EndRect();
        var workers = GetWorkersInRect(_mouseDownPos, screenPos);

        if (workers.Count == 1)
        {
            _workerPopupUI?.Show(workers[0]);
        }
        else if (workers.Count > 1)
        {
            _workerSelectionUI?.ShowIconBar(workers);
        }
        else
        {
            // ワーカーが0人のとき施設を確認する
            var equipments = GetEquipmentsInRect(_mouseDownPos, screenPos);
            if (equipments.Count == 1)
                _facilityPopupUI?.Show(equipments[0]);
            else if (equipments.Count > 1)
                _workerSelectionUI?.ShowEquipmentIconBar(equipments);
        }
    }

    private void HandleSingleClick(Vector2 screenPos)
    {
        var worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
        worldPos.z = 0f;

        var worker = FindNearestWorker(worldPos);
        if (worker != null)
        {
            _workerPopupUI?.Show(worker);
            return;
        }

        var equipment = FindEquipmentAtWorld(worldPos);
        if (equipment != null)
        {
            _facilityPopupUI?.Show(equipment);
            _zonePopupUI?.Hide();
            _itemPopupUI?.Hide();
            return;
        }

        var gridPos = GridHelper.WorldToGrid(worldPos);
        if (MapManager.Instance != null && MapManager.Instance.IsValidPosition(gridPos))
        {
            var tile = MapManager.Instance.GetTile(gridPos);

            if (tile.PlacedItem != null)
            {
                _itemPopupUI?.Show(tile);
                _workerPopupUI?.Hide();
                _facilityPopupUI?.Hide();
                _zonePopupUI?.Hide();
                _workerSelectionUI?.HideIconBar();
                return;
            }

            if (tile.Zone != null)
            {
                _zonePopupUI?.Show(tile.Zone);
                _workerPopupUI?.Hide();
                _facilityPopupUI?.Hide();
                _itemPopupUI?.Hide();
                _workerSelectionUI?.HideIconBar();
                return;
            }
        }

        _workerPopupUI?.Hide();
        _facilityPopupUI?.Hide();
        _zonePopupUI?.Hide();
        _itemPopupUI?.Hide();
        _workerSelectionUI?.HideIconBar();
    }

    private WorkerBase FindNearestWorker(Vector3 worldPos)
    {
        WorkerBase nearest = null;
        float minDist = WorkerPickRadius;

        foreach (var worker in FindObjectsByType<WorkerBase>(FindObjectsSortMode.None))
        {
            float dist = Vector2.Distance(worker.transform.position, worldPos);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = worker;
            }
        }

        return nearest;
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

    private List<WorkerBase> GetWorkersInRect(Vector2 screenStart, Vector2 screenEnd)
    {
        var worldStart = _mainCamera.ScreenToWorldPoint(new Vector3(screenStart.x, screenStart.y, 0f));
        var worldEnd = _mainCamera.ScreenToWorldPoint(new Vector3(screenEnd.x, screenEnd.y, 0f));

        float minX = Mathf.Min(worldStart.x, worldEnd.x);
        float maxX = Mathf.Max(worldStart.x, worldEnd.x);
        float minY = Mathf.Min(worldStart.y, worldEnd.y);
        float maxY = Mathf.Max(worldStart.y, worldEnd.y);

        var result = new List<WorkerBase>();
        foreach (var worker in FindObjectsByType<WorkerBase>(FindObjectsSortMode.None))
        {
            var p = worker.transform.position;
            if (p.x >= minX && p.x <= maxX && p.y >= minY && p.y <= maxY)
                result.Add(worker);
        }

        return result;
    }

    private List<EquipmentBase> GetEquipmentsInRect(Vector2 screenStart, Vector2 screenEnd)
    {
        var worldStart = _mainCamera.ScreenToWorldPoint(new Vector3(screenStart.x, screenStart.y, 0f));
        var worldEnd = _mainCamera.ScreenToWorldPoint(new Vector3(screenEnd.x, screenEnd.y, 0f));

        float minX = Mathf.Min(worldStart.x, worldEnd.x);
        float maxX = Mathf.Max(worldStart.x, worldEnd.x);
        float minY = Mathf.Min(worldStart.y, worldEnd.y);
        float maxY = Mathf.Max(worldStart.y, worldEnd.y);

        var result = new List<EquipmentBase>();
        foreach (var eq in FindObjectsByType<EquipmentBase>(FindObjectsSortMode.None))
        {
            if (!eq.IsPlaced) continue;
            var center = eq.transform.position;
            float halfW = eq.Size.x * 0.5f;
            float halfH = eq.Size.y * 0.5f;
            // 矩形と施設のAABBが重なっているか判定する
            if (center.x + halfW > minX && center.x - halfW < maxX &&
                center.y + halfH > minY && center.y - halfH < maxY)
                result.Add(eq);
        }

        return result;
    }
}
