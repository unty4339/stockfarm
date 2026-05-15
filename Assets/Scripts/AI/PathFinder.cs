using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A*アルゴリズムを使用した経路探索クラス
/// </summary>
public static class PathFinder
{
    private static readonly Vector2Int[] Directions = new[]
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
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

                int cost = costProvider.GetCost(neighborPos);
                if (cost == int.MaxValue) continue;

                int newGCost = current.GCost + cost;

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
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
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
