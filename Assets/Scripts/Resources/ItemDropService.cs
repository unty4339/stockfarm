using UnityEngine;

/// <summary>
/// アイテムをタイルに落とす処理を担う静的サービスクラス
/// </summary>
public static class ItemDropService
{
    /// <summary>
    /// 時計回りの8方向（上から時計回り順）
    /// </summary>
    private static readonly Vector2Int[] ClockwiseDirections =
    {
        new(0,  1),  // 上
        new(1,  1),  // 右上
        new(1,  0),  // 右
        new(1, -1),  // 右下
        new(0, -1),  // 下
        new(-1,-1),  // 左下
        new(-1, 0),  // 左
        new(-1, 1),  // 左上
    };

    /// <summary>
    /// 指定座標にアイテムを落とす
    /// スタック不可または占有済みの場合は時計回りに隣接タイルを探して落とす
    /// </summary>
    /// <param name="item">落とすアイテム</param>
    /// <param name="targetPosition">対象タイル座標</param>
    public static void DropItem(TileItem item, Vector2Int targetPosition)
    {
        DropInternal(item, targetPosition);
    }

    /// <summary>
    /// アイテム落下処理の本体
    /// </summary>
    private static void DropInternal(TileItem item, Vector2Int position)
    {
        var mapManager = MapManager.Instance;
        if (!mapManager.IsValidPosition(position))
        {
            DropOnEmptyNeighbor(item, position);
            return;
        }

        var tile = mapManager.GetTile(position);

        if (item.IsStackable)
        {
            // 同種スタック可能アイテムが存在しスタック未満の場合は追加
            if (tile.PlacedItem != null && tile.PlacedItem.Type == item.Type && !tile.PlacedItem.IsStackFull)
            {
                int remaining = tile.PlacedItem.TryAddStack(item.StackCount);
                ItemVisualManager.Instance?.Refresh(position);
                if (remaining > 0)
                {
                    DropOnEmptyNeighbor(new TileItem(item.Type, remaining), position);
                }
                return;
            }

            // タイルが空きなら新規配置
            if (IsTileEmpty(tile))
            {
                AssignPlacedItem(tile, item, position);
                return;
            }
        }
        else
        {
            // 非スタックアイテム: 空きタイルなら配置
            if (IsTileEmpty(tile))
            {
                AssignPlacedItem(tile, item, position);
                return;
            }
        }

        // タイルが占有済み（設備あり・別アイテムあり・スタック満杯）→ 隣接タイルを探す
        DropOnEmptyNeighbor(item, position);
    }

    /// <summary>
    /// 指定座標を起点に時計回りで最初の空きタイルを探してアイテムを配置する
    /// </summary>
    /// <param name="item">落とすアイテム</param>
    /// <param name="fromPosition">探索の起点座標</param>
    private static void DropOnEmptyNeighbor(TileItem item, Vector2Int fromPosition)
    {
        var mapManager = MapManager.Instance;
        foreach (var dir in ClockwiseDirections)
        {
            var neighborPos = fromPosition + dir;
            if (!mapManager.IsValidPosition(neighborPos)) continue;
            var tile = mapManager.GetTile(neighborPos);
            if (IsTileEmpty(tile))
            {
                AssignPlacedItem(tile, item, neighborPos);
                return;
            }
        }
        // TODO: 隣接8マスが全て埋まっている場合はより広い範囲での探索が必要
        Debug.LogWarning($"ItemDropService: 空きタイルが見つからず、アイテム {item.Type} を破棄しました（起点: {fromPosition}）");
    }

    /// <summary>
    /// タイルにアイテムを配置しビジュアルを更新する
    /// </summary>
    /// <param name="tile">配置先タイル</param>
    /// <param name="item">配置するアイテム</param>
    /// <param name="position">タイル座標</param>
    private static void AssignPlacedItem(GridTile tile, TileItem item, Vector2Int position)
    {
        tile.PlacedItem = item;
        ItemVisualManager.Instance?.Refresh(position);
    }

    /// <summary>
    /// タイルがアイテム配置可能な空き状態かを返す
    /// </summary>
    private static bool IsTileEmpty(GridTile tile)
    {
        return tile.PlacedItem == null && tile.PlacedEquipment == null;
    }
}
