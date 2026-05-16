using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// タイル上のアイテムを紫色矩形スプライトとスタック数ラベルで表示する
/// </summary>
public class ItemVisualManager : MonoBehaviour
{
    private static readonly Color DefaultItemColor = new Color(0.55f, 0.2f, 0.75f);
    private const float ItemScale = 0.4f;
    private const int SortingOrder = 2;
    private const float StackLabelLocalX = 0.22f;
    private const float StackLabelLocalY = -0.22f;
    private const float StackLabelFontSize = 2.5f;

    /// <summary>シングルトンインスタンス</summary>
    public static ItemVisualManager Instance { get; private set; }

    private readonly Dictionary<Vector2Int, ItemVisualEntry> _visuals = new();

    private void Awake()
    {
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// 指定座標のタイルアイテム表示をデータに合わせて更新する
    /// </summary>
    /// <param name="position">タイル座標</param>
    public void Refresh(Vector2Int position)
    {
        if (MapManager.Instance == null || !MapManager.Instance.IsValidPosition(position))
        {
            Hide(position);
            return;
        }

        var tile = MapManager.Instance.GetTile(position);
        if (tile.PlacedItem == null)
        {
            Hide(position);
            return;
        }

        if (!_visuals.TryGetValue(position, out var entry) || entry.Root == null)
        {
            Show(position, tile.PlacedItem);
            return;
        }

        UpdateStackLabel(entry.StackLabel, tile.PlacedItem);
    }

    /// <summary>
    /// 指定座標のアイテムビジュアルを非表示にする
    /// </summary>
    /// <param name="position">タイル座標</param>
    public void Hide(Vector2Int position)
    {
        if (!_visuals.TryGetValue(position, out var entry)) return;
        if (entry.Root != null) Destroy(entry.Root);
        _visuals.Remove(position);
    }

    /// <summary>
    /// アイテムのビジュアルを新規作成する
    /// </summary>
    /// <param name="position">タイル座標</param>
    /// <param name="item">表示するアイテム</param>
    private void Show(Vector2Int position, TileItem item)
    {
        Hide(position);

        var root = new GameObject($"Item_{position.x}_{position.y}");
        root.transform.SetParent(transform);
        root.transform.position = new Vector3(position.x, position.y, -0.08f);
        root.transform.localScale = new Vector3(ItemScale, ItemScale, 1f);

        var sr = root.AddComponent<SpriteRenderer>();
        sr.sprite = SpriteHelper.CreateColorSprite(DefaultItemColor);
        sr.sortingOrder = SortingOrder;

        var labelGo = new GameObject("StackLabel");
        labelGo.transform.SetParent(root.transform, false);
        labelGo.transform.localPosition = new Vector3(StackLabelLocalX, StackLabelLocalY, 0f);

        var stackLabel = labelGo.AddComponent<TextMeshPro>();
        stackLabel.fontSize = StackLabelFontSize;
        stackLabel.color = Color.white;
        stackLabel.alignment = TextAlignmentOptions.BottomRight;
        stackLabel.rectTransform.sizeDelta = new Vector2(1f, 0.5f);
        UIHelper.ApplyFont(stackLabel);
        stackLabel.renderer.sortingOrder = SortingOrder;

        UpdateStackLabel(stackLabel, item);

        _visuals[position] = new ItemVisualEntry(root, stackLabel);
    }

    /// <summary>
    /// スタック数ラベルの表示を更新する
    /// </summary>
    /// <param name="stackLabel">スタック数表示用 TextMeshPro</param>
    /// <param name="item">表示対象アイテム</param>
    private static void UpdateStackLabel(TextMeshPro stackLabel, TileItem item)
    {
        if (stackLabel == null) return;

        if (item.IsStackable)
        {
            stackLabel.gameObject.SetActive(true);
            stackLabel.text = item.StackCount.ToString();
        }
        else
        {
            stackLabel.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// タイルごとのビジュアル参照を保持する
    /// </summary>
    private sealed class ItemVisualEntry
    {
        /// <summary>ビジュアルルート</summary>
        public GameObject Root { get; }
        /// <summary>スタック数ラベル</summary>
        public TextMeshPro StackLabel { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="root">ビジュアルルート</param>
        /// <param name="stackLabel">スタック数ラベル</param>
        public ItemVisualEntry(GameObject root, TextMeshPro stackLabel)
        {
            Root = root;
            StackLabel = stackLabel;
        }
    }
}
