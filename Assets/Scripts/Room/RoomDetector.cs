using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// フラッドフィルアルゴリズムを使い、グリッドタイルから部屋領域を検出する静的クラス
/// </summary>
public static class RoomDetector
{
    private static readonly Vector2Int[] Directions = new[]
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    /// <summary>
    /// グリッド全体を走査し、全部屋のRoomDataリストを返す
    /// 壁・柵・ゲートを境界として床・地面タイルの連続領域を部屋とみなす
    /// </summary>
    /// <param name="grid">グリッドタイルの二次元配列</param>
    /// <returns>検出された部屋のリスト</returns>
    public static List<RoomData> DetectRooms(GridTile[,] grid)
    {
        var rooms = new List<RoomData>();
        int width = grid.GetLength(0);
        int height = grid.GetLength(1);
        var visited = new bool[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (visited[x, y]) continue;
                var tile = grid[x, y];
                if (tile == null) continue;
                if (IsBoundary(tile.Type))
                {
                    visited[x, y] = true;
                    continue;
                }

                var region = new List<Vector2Int>();
                var queue = new Queue<Vector2Int>();
                bool isEnclosed = true;

                queue.Enqueue(tile.Position);
                visited[x, y] = true;

                while (queue.Count > 0)
                {
                    var pos = queue.Dequeue();
                    region.Add(pos);

                    foreach (var dir in Directions)
                    {
                        var next = pos + dir;
                        if (next.x < 0 || next.x >= width || next.y < 0 || next.y >= height)
                        {
                            isEnclosed = false;
                            continue;
                        }

                        if (visited[next.x, next.y]) continue;

                        var nextTile = grid[next.x, next.y];
                        if (nextTile == null) continue;

                        if (IsBoundary(nextTile.Type))
                        {
                            visited[next.x, next.y] = true;
                            continue;
                        }

                        visited[next.x, next.y] = true;
                        queue.Enqueue(next);
                    }
                }

                if (isEnclosed && region.Count > 0)
                {
                    var room = new RoomData();
                    foreach (var pos in region)
                        room.TilePositions.Add(pos);
                    rooms.Add(room);
                }
            }
        }

        return rooms;
    }

    /// <summary>
    /// 指定座標を含む部屋をリストから検索して返す（見つからなければnull）
    /// </summary>
    /// <param name="position">検索する座標</param>
    /// <param name="rooms">検索対象の部屋リスト</param>
    /// <returns>部屋データまたはnull</returns>
    public static RoomData GetRoomAt(Vector2Int position, List<RoomData> rooms)
    {
        foreach (var room in rooms)
        {
            if (room.Contains(position)) return room;
        }
        return null;
    }

    private static bool IsBoundary(TileType type)
    {
        return type == TileType.Wall || type == TileType.Fence || type == TileType.Gate;
    }
}
