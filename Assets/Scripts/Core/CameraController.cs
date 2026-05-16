using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// WASDキーまたは中クリックドラッグでカメラを平行移動し、スクロールホイールで拡大縮小するカメラコントローラ
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour
{
    [SerializeField] private float _panSpeed = 10f;
    [SerializeField] private float _zoomSpeed = 2f;
    [SerializeField] private float _minOrthoSize = 3f;
    [SerializeField] private float _maxOrthoSize = 30f;

    private Camera _camera;
    private Vector2 _lastMousePosition;
    private bool _isDragging;

    /// <summary>
    /// カメラコンポーネントをキャッシュする
    /// </summary>
    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void Update()
    {
        HandleKeyboardPan();
        HandleMiddleClickPan();
        HandleScrollZoom();
    }

    /// <summary>
    /// WASDキー入力でカメラを平行移動する
    /// </summary>
    private void HandleKeyboardPan()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        var direction = Vector2.zero;
        if (keyboard.wKey.isPressed) direction.y += 1f;
        if (keyboard.sKey.isPressed) direction.y -= 1f;
        if (keyboard.aKey.isPressed) direction.x -= 1f;
        if (keyboard.dKey.isPressed) direction.x += 1f;

        if (direction == Vector2.zero) return;

        // ズームレベルに比例した移動速度でスケールを保つ
        float speed = _panSpeed * _camera.orthographicSize * Time.deltaTime;
        transform.position += (Vector3)(direction.normalized * speed);
    }

    /// <summary>
    /// 中クリックドラッグでカメラをドラッグ移動する
    /// </summary>
    private void HandleMiddleClickPan()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        if (mouse.middleButton.wasPressedThisFrame)
        {
            _lastMousePosition = mouse.position.ReadValue();
            _isDragging = true;
        }
        else if (mouse.middleButton.wasReleasedThisFrame)
        {
            _isDragging = false;
        }

        if (!_isDragging || !mouse.middleButton.isPressed) return;

        Vector2 currentMousePosition = mouse.position.ReadValue();
        Vector2 delta = currentMousePosition - _lastMousePosition;
        _lastMousePosition = currentMousePosition;

        // スクリーンピクセル差をワールド座標差に変換
        float screenToWorld = _camera.orthographicSize * 2f / Screen.height;
        Vector3 move = new Vector3(-delta.x, -delta.y, 0f) * screenToWorld;
        transform.position += move;
    }

    /// <summary>
    /// スクロールホイールでカメラの orthographicSize を変更して拡大縮小する
    /// </summary>
    private void HandleScrollZoom()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        float scroll = mouse.scroll.ReadValue().y;
        if (scroll == 0f) return;

        _camera.orthographicSize = Mathf.Clamp(
            _camera.orthographicSize - scroll * _zoomSpeed,
            _minOrthoSize,
            _maxOrthoSize
        );
    }
}
