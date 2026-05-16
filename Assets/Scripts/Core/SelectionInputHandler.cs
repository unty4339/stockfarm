using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// マウスの左クリック・ドラッグ入力を監視し、ワーカー選択を制御する入力ハンドラ
/// BuildModeUI がアクティブな間はすべての処理をスキップする
/// </summary>
public class SelectionInputHandler : MonoBehaviour
{
    private const float DragThreshold = 5f;
    private const float WorkerPickRadius = 0.5f;

    private BuildModeUI _buildModeUI;
    private WorkerPopupUI _workerPopupUI;
    private WorkerSelectionUI _workerSelectionUI;
    private Camera _mainCamera;

    private Vector2 _mouseDownPos;
    private bool _mouseDownActive;
    private bool _dragStarted;

    private void Start()
    {
        _buildModeUI = FindFirstObjectByType<BuildModeUI>();
        _workerPopupUI = FindFirstObjectByType<WorkerPopupUI>();
        _workerSelectionUI = FindFirstObjectByType<WorkerSelectionUI>();
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        if (_buildModeUI != null && _buildModeUI.IsInBuildMode) return;

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
        if (IsPointerOverUI()) return;

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
            _workerPopupUI?.Show(workers[0]);
        else if (workers.Count > 1)
            _workerSelectionUI?.ShowIconBar(workers);
    }

    private void HandleSingleClick(Vector2 screenPos)
    {
        var worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
        worldPos.z = 0f;

        var worker = FindNearestWorker(worldPos);
        if (worker != null)
        {
            _workerPopupUI?.Show(worker);
        }
        else
        {
            _workerPopupUI?.Hide();
            _workerSelectionUI?.HideIconBar();
        }
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

    private bool IsPointerOverUI()
    {
        var mouse = Mouse.current;
        if (mouse == null || EventSystem.current == null) return false;

        var eventData = new PointerEventData(EventSystem.current)
        {
            position = mouse.position.ReadValue()
        };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }
}
