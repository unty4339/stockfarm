using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 全設備の基底クラス。配置・撤去・ムード補正を共通管理する
/// </summary>
public abstract class EquipmentBase : MonoBehaviour
{
    /// <summary>設備種別</summary>
    public abstract EquipmentType Type { get; }
    /// <summary>グリッド上の配置座標（左上基点）</summary>
    public Vector2Int GridPosition { get; private set; }
    /// <summary>占有グリッドサイズ（例: 1×1, 2×2）</summary>
    public abstract Vector2Int Size { get; }
    /// <summary>建設コスト（資金）</summary>
    public abstract int BuildCost { get; }
    /// <summary>ムード補正値（正: 上昇、負: 低下）</summary>
    public abstract int MoodBonus { get; }
    /// <summary>補正対象のムード種別</summary>
    public abstract MoodType AffectedMoodType { get; }
    /// <summary>現在設置済みかどうか</summary>
    public bool IsPlaced { get; private set; }

    /// <summary>設備の表示色</summary>
    protected abstract Color EquipmentColor { get; }

    /// <summary>UIアイコン表示用の色</summary>
    public Color DisplayColor => EquipmentColor;

    private SpriteRenderer _spriteRenderer;

    /// <summary>
    /// 指定座標に設置する（BuildingManagerから呼ばれる）
    /// </summary>
    /// <param name="position">設置グリッド座標</param>
    public virtual void Place(Vector2Int position)
    {
        GridPosition = position;
        IsPlaced = true;

        if (_spriteRenderer == null)
            _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

        _spriteRenderer.sprite = SpriteHelper.CreateColorSprite(EquipmentColor);
        _spriteRenderer.sortingOrder = 1;

        var worldPos = GridHelper.GridToWorld(position, Size);
        transform.position = new Vector3(worldPos.x, worldPos.y, -0.05f);
        transform.localScale = new Vector3(Size.x * 0.9f, Size.y * 0.9f, 1f);

        if (MapManager.Instance != null)
        {
            foreach (var pos in GetOccupiedPositions())
            {
                var tile = MapManager.Instance.GetTileOrNull(pos);
                if (tile != null) tile.PlacedEquipment = this;
            }
            RoomManager.Instance?.RefreshRooms();
        }
    }

    /// <summary>
    /// 設置を解除し、BuildCostの50%をEconomyManagerに返還する
    /// </summary>
    public virtual void Remove()
    {
        if (!IsPlaced) return;

        if (MapManager.Instance != null)
        {
            foreach (var pos in GetOccupiedPositions())
            {
                var tile = MapManager.Instance.GetTileOrNull(pos);
                if (tile != null) tile.PlacedEquipment = null;
            }
        }

        EconomyManager.Instance?.AddFunds(BuildCost / 2);
        IsPlaced = false;
        RoomManager.Instance?.RefreshRooms();
        Destroy(gameObject);
    }

    /// <summary>
    /// 設備が占有しているグリッド座標の一覧を返す
    /// </summary>
    /// <returns>占有座標リスト</returns>
    public List<Vector2Int> GetOccupiedPositions()
    {
        var positions = new List<Vector2Int>();
        for (int x = 0; x < Size.x; x++)
        {
            for (int y = 0; y < Size.y; y++)
            {
                positions.Add(GridPosition + new Vector2Int(x, y));
            }
        }
        return positions;
    }

}
