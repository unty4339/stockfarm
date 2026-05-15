using UnityEngine;

/// <summary>
/// 施設配置モード中のプレビュー（ゴースト）表示を担うクラス
/// </summary>
public class GhostPlacer : MonoBehaviour
{
    private GameObject _ghostObject;
    private SpriteRenderer _ghostRenderer;

    private static readonly Color ValidColor = new Color(0f, 1f, 0f, 0.4f);
    private static readonly Color InvalidColor = new Color(1f, 0f, 0f, 0.4f);

    /// <summary>
    /// 指定設備種別・座標にゴーストを表示する
    /// </summary>
    /// <param name="type">設備種別</param>
    /// <param name="position">グリッド座標</param>
    public void ShowGhost(EquipmentType type, Vector2Int position)
    {
        if (_ghostObject == null)
        {
            _ghostObject = new GameObject("Ghost");
            _ghostRenderer = _ghostObject.AddComponent<SpriteRenderer>();
            _ghostRenderer.sortingOrder = 10;
        }

        bool canPlace = CanPlace(type, position);
        _ghostRenderer.sprite = SpriteHelper.CreateColorSprite(canPlace ? ValidColor : InvalidColor);
        _ghostRenderer.color = canPlace ? ValidColor : InvalidColor;

        var size = BuildingManager.GetEquipmentSize(type);
        _ghostObject.transform.position = new Vector3(position.x + size.x * 0.5f - 0.5f, position.y + size.y * 0.5f - 0.5f, -0.1f);
        _ghostObject.transform.localScale = new Vector3(size.x, size.y, 1f);
        _ghostObject.SetActive(true);
    }

    /// <summary>
    /// ゴーストを非表示にする
    /// </summary>
    public void HideGhost()
    {
        if (_ghostObject != null)
            _ghostObject.SetActive(false);
    }

    /// <summary>
    /// 指定位置に設備を配置可能かを判定する
    /// </summary>
    /// <param name="type">設備種別</param>
    /// <param name="position">グリッド座標</param>
    /// <returns>配置可能な場合true</returns>
    public bool CanPlace(EquipmentType type, Vector2Int position)
    {
        if (MapManager.Instance == null) return false;

        var size = BuildingManager.GetEquipmentSize(type);
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                var pos = position + new Vector2Int(x, y);
                var tile = MapManager.Instance.GetTileOrNull(pos);
                if (tile == null || tile.PlacedEquipment != null) return false;
            }
        }
        return true;
    }

    private void OnDestroy()
    {
        if (_ghostObject != null)
            Destroy(_ghostObject);
    }
}
