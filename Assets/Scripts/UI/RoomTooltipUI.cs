using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// タイルにカーソルを重ねたとき、そのタイルが部屋内なら部屋情報をカーソル右上に表示するツールチップ
/// </summary>
public class RoomTooltipUI : MonoBehaviour
{
    private static readonly Vector2 CursorOffset = new Vector2(15f, 15f);

    private GameObject _panel;
    private TextMeshProUGUI _sizeText;
    private TextMeshProUGUI _workMoodText;
    private TextMeshProUGUI _restMoodText;
    private TextMeshProUGUI _breedMoodText;

    private Camera _mainCamera;
    private RectTransform _canvasRect;

private void Awake() { }

private void Start()
    {
        _mainCamera = Camera.main;
        var canvas = GetComponentInParent<Canvas>();
        _canvasRect = canvas != null ? canvas.GetComponent<RectTransform>() : null;
        BuildPanel(_canvasRect != null ? _canvasRect : transform);
    }

private void BuildPanel(Transform parent)
    {
        _panel = new GameObject("RoomTooltipPanel");
        _panel.transform.SetParent(parent, false);
        var rt = _panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0f, 0f);
        rt.sizeDelta = new Vector2(200, 130);

        var img = _panel.AddComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.1f, 0.88f);

        _sizeText = UIHelper.CreateText(_panel.transform, "SizeText", "",
            new Vector2(0, 45), 14, Color.white);
        _workMoodText = UIHelper.CreateText(_panel.transform, "WorkText", "",
            new Vector2(0, 15), 13, new Color(0.8f, 0.8f, 0.8f));
        _restMoodText = UIHelper.CreateText(_panel.transform, "RestText", "",
            new Vector2(0, -15), 13, new Color(0.8f, 0.8f, 0.8f));
        _breedMoodText = UIHelper.CreateText(_panel.transform, "BreedText", "",
            new Vector2(0, -45), 13, new Color(0.8f, 0.8f, 0.8f));

        _panel.SetActive(false);
    }

    private void Update()
    {
        var mouse = Mouse.current;
        if (mouse == null || _mainCamera == null || _canvasRect == null)
        {
            _panel.SetActive(false);
            return;
        }

        var screenPos = mouse.position.ReadValue();

        if (UIHelper.IsPointerOverUI())
        {
            _panel.SetActive(false);
            return;
        }

        var worldPos = _mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, 0f));
        worldPos.z = 0f;
        var gridPos = GridHelper.WorldToGrid(worldPos);

        if (MapManager.Instance == null || !MapManager.Instance.IsValidPosition(gridPos) ||
            RoomManager.Instance == null)
        {
            _panel.SetActive(false);
            return;
        }

        var room = RoomDetector.GetRoomAt(gridPos, RoomManager.Instance.Rooms);
        if (room == null)
        {
            _panel.SetActive(false);
            return;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRect, screenPos, null, out var localPos);

        var panelRt = _panel.GetComponent<RectTransform>();
        panelRt.anchoredPosition = localPos + CursorOffset;

        _sizeText.text = $"部屋サイズ: {room.TilePositions.Count} タイル";
        _workMoodText.text = $"作業ムード: {room.CurrentMood.WorkMood:0}";
        _restMoodText.text = $"休憩ムード: {room.CurrentMood.RestMood:0}";
        _breedMoodText.text = $"繁殖ムード: {room.CurrentMood.BreedMood:0}";

        _panel.SetActive(true);
    }
}
