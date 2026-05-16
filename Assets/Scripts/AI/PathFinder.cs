using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A*アルゴリズムを使用した経路探索クラス
/// </summary>
public static class PathFinder
{
    private const int CardinalCostFactor = 10;
    private const int DiagonalCostFactor = 14; // √2 ≈ 1.414 の整数近似

    private static readonly Vector2Int[] Directions = new[]
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
        new Vector2Int( 1,  1), new Vector2Int(-1,  1),
        new Vector2Int( 1, -1), new Vector2Int(-1, -1),
    };

    /// <summary>
    /// fromからtoまでの経路をグリッド座標リストで返す
    /// 経路が存在しない場合は空のリストを返す
    /// </summary>
    /// <param name="from">出発地点</param>
    /// <param name="to">目標地点</param>
    /// <param name="costProvider">タイルコスト提供者</param>
    /// <returns>経路座標リスト（from含まず、to含む）</returns>
    public static List<Vector2Int> FindPath(Vector2Int from, Vector2Int to, IPathCostProvider costProvider)
    {
        if (from == to) return new List<Vector2Int>();
        if (costProvider.GetCost(to) == int.MaxValue) return new List<Vector2Int>();

        var openSet = new List<PathNode>();
        var closedSet = new HashSet<Vector2Int>();
        var nodeMap = new Dictionary<Vector2Int, PathNode>();

        var startNode = new PathNode(from, 0, Heuristic(from, to), null);
        openSet.Add(startNode);
        nodeMap[from] = startNode;

        while (openSet.Count > 0)
        {
            var current = GetLowestFCost(openSet);
            openSet.Remove(current);
            closedSet.Add(current.Position);

            if (current.Position == to)
                return RetracePath(current);

            foreach (var dir in Directions)
            {
                var neighborPos = current.Position + dir;
                if (closedSet.Contains(neighborPos)) continue;

                bool isDiagonal = dir.x != 0 && dir.y != 0;

                // 斜め移動時のコーナーカット防止：隣接する縦横タイルが両方通行可能な場合のみ許可
                if (isDiagonal)
                {
                    int sideX = costProvider.GetCost(current.Position + new Vector2Int(dir.x, 0));
                    int sideY = costProvider.GetCost(current.Position + new Vector2Int(0, dir.y));
                    if (sideX == int.MaxValue || sideY == int.MaxValue) continue;
                }

                int cost = costProvider.GetCost(neighborPos);
                if (cost == int.MaxValue) continue;

                int newGCost = current.GCost + cost * (isDiagonal ? DiagonalCostFactor : CardinalCostFactor);

                if (nodeMap.TryGetValue(neighborPos, out var existing))
                {
                    if (newGCost < existing.GCost)
                    {
                        existing.GCost = newGCost;
                        existing.Parent = current;
                    }
                }
                else
                {
                    var neighbor = new PathNode(neighborPos, newGCost, Heuristic(neighborPos, to), current);
                    openSet.Add(neighbor);
                    nodeMap[neighborPos] = neighbor;
                }
            }
        }

        return new List<Vector2Int>();
    }

    private static int Heuristic(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        // オクタイル距離（8方向移動に対してadmissibleなヒューリスティック）
        return CardinalCostFactor * Mathf.Max(dx, dy) + (DiagonalCostFactor - CardinalCostFactor) * Mathf.Min(dx, dy);
    }

    private static PathNode GetLowestFCost(List<PathNode> nodes)
    {
        var best = nodes[0];
        for (int i = 1; i < nodes.Count; i++)
        {
            if (nodes[i].FCost < best.FCost ||
                (nodes[i].FCost == best.FCost && nodes[i].HCost < best.HCost))
                best = nodes[i];
        }
        return best;
    }

    private static List<Vector2Int> RetracePath(PathNode endNode)
    {
        var path = new List<Vector2Int>();
        var current = endNode;
        while (current.Parent != null)
        {
            path.Add(current.Position);
            current = current.Parent;
        }
        path.Reverse();
        return path;
    }

    /// <summary>
    /// A*のノード
    /// </summary>
    private class PathNode
    {
        public Vector2Int Position { get; }
        public int GCost { get; set; }
        public int HCost { get; }
        public int FCost => GCost + HCost;
        public PathNode Parent { get; set; }

        public PathNode(Vector2Int position, int gCost, int hCost, PathNode parent)
        {
            Position = position;
            GCost = gCost;
            HCost = hCost;
            Parent = parent;
        }
    }
}
