using UnityEngine;

/// <summary>
/// アイドル散歩タスク。やることがない時にランダムな近隣タイルを歩く
/// </summary>
public class IdleWalkTask : AITaskBase
{
    private static readonly Vector2Int[] Dirs = new[]
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
        new Vector2Int(1, 1), new Vector2Int(-1, 1), new Vector2Int(1, -1), new Vector2Int(-1, -1),
    };

    private bool _arrived;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="owner">散歩するワーカー</param>
    public IdleWalkTask(WorkerBase owner)
    {
        Owner = owner;
        Priority = 7;
        TargetPosition = PickRandomTarget(owner.GridPosition);
    }

    /// <inheritdoc/>
    public override bool CanExecute() => true;

    /// <inheritdoc/>
    protected override void OnExecute() => _arrived = true;

    /// <inheritdoc/>
    protected override bool IsComplete() => _arrived;

    private static Vector2Int PickRandomTarget(Vector2Int current)
    {
        int radius = Random.Range(1, 4);
        int attempts = 10;
        while (attempts-- > 0)
        {
            int dx = Random.Range(-radius, radius + 1);
            int dy = Random.Range(-radius, radius + 1);
            var candidate = current + new Vector2Int(dx, dy);

            if (MapManager.Instance == null) return candidate;
            var tile = MapManager.Instance.GetTileOrNull(candidate);
            if (tile != null && tile.Type != TileType.Wall && tile.Type != TileType.Fence)
                return candidate;
        }
        return current;
    }

    /// <inheritdoc/>
    public override string GetActionText() => "散歩中";
}