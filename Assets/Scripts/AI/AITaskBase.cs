using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 全タスクの基底クラス。タスクのライフサイクル（予約→移動→実行→完了）を管理する
/// </summary>
public abstract class AITaskBase
{
    /// <summary>タスクを実行するワーカー</summary>
    public WorkerBase Owner { get; protected set; }
    /// <summary>タスク優先度（1〜7、低い値が高優先）</summary>
    public int Priority { get; set; } = 4;
    /// <summary>現在の実行状態</summary>
    public AITaskState State { get; protected set; } = AITaskState.Pending;
    /// <summary>移動目標座標</summary>
    public Vector2Int TargetPosition { get; protected set; }

    private List<Vector2Int> _path;
    private int _pathIndex;
    private int _moveTickCounter;

    /// <summary>
    /// 新しい目標座標へ向けて移動を再開する（OnExecute内でフェーズ切替時に使用）
    /// </summary>
    /// <param name="newTarget">次の目標座標</param>
    protected void RestartMovingTo(Vector2Int newTarget)
    {
        TargetPosition = newTarget;
        if (Owner.GridPosition == newTarget)
        {
            State = AITaskState.Executing;
            return;
        }
        var costProvider = new MapPathCostProvider();
        _path = PathFinder.FindPath(Owner.GridPosition, newTarget, costProvider);
        _pathIndex = 0;
        _moveTickCounter = 0;
        State = _path.Count > 0 ? AITaskState.Moving : AITaskState.Interrupted;
    }

    /// <summary>
    /// このタスクが現在実行可能かを判定する
    /// </summary>
    /// <returns>実行可能な場合true</returns>
    public abstract bool CanExecute();

    /// <summary>
    /// タスク固有の実行ロジック（Executing状態のtickごとに呼ばれる）
    /// </summary>
    protected abstract void OnExecute();

    /// <summary>
    /// タスクが完了したかを判定する
    /// </summary>
    /// <returns>完了した場合true</returns>
    protected abstract bool IsComplete();

    /// <summary>
    /// タスクを開始する（Pending → Moving or Executing に遷移）
    /// </summary>
    public void Start()
    {
        if (State != AITaskState.Pending) return;

        if (Owner.GridPosition == TargetPosition)
        {
            State = AITaskState.Executing;
            return;
        }

        var costProvider = new MapPathCostProvider();
        _path = PathFinder.FindPath(Owner.GridPosition, TargetPosition, costProvider);
        _pathIndex = 0;
        _moveTickCounter = 0;

        State = _path.Count > 0 ? AITaskState.Moving : AITaskState.Interrupted;
    }

    /// <summary>
    /// 毎tick呼ばれる更新処理（状態遷移を内部で行う）
    /// Moving・Executing 中に CanExecute が false になった場合は即座に中断する
    /// </summary>
    public void Tick()
    {
        switch (State)
        {
            case AITaskState.Pending:
                Start();
                break;
            case AITaskState.Moving:
                if (!CanExecute()) { Interrupt(); return; }
                TickMoving();
                break;
            case AITaskState.Executing:
                if (!CanExecute()) { Interrupt(); return; }
                OnExecute();
                if (IsComplete())
                    State = AITaskState.Completed;
                break;
        }
    }

    /// <summary>
    /// タスクを中断する
    /// </summary>
    public virtual void Interrupt()
    {
        State = AITaskState.Interrupted;
    }

    private void TickMoving()
    {
        if (_path == null || _pathIndex >= _path.Count)
        {
            State = AITaskState.Executing;
            return;
        }

        _moveTickCounter++;
        if (_moveTickCounter < Owner.MovementTicksPerCell) return;
        _moveTickCounter = 0;

        var nextPos = _path[_pathIndex];
        if (new MapPathCostProvider().GetCost(nextPos) == int.MaxValue)
        {
            RestartMovingTo(TargetPosition);
            return;
        }

        Owner.GridPosition = nextPos;
        _pathIndex++;

        if (_pathIndex >= _path.Count)
            State = AITaskState.Executing;
    }
}

/// <summary>
/// MapManagerのタイル情報を使ったパスコストプロバイダ
/// </summary>
public class MapPathCostProvider : IPathCostProvider
{
    /// <inheritdoc/>
    public int GetCost(Vector2Int position)
    {
        if (MapManager.Instance == null) return int.MaxValue;
        var tile = MapManager.Instance.GetTileOrNull(position);
        if (tile == null) return int.MaxValue;

        return tile.Type switch
        {
            TileType.Floor => 1,
            TileType.Ground => 2,
            TileType.Gate => 2,
            TileType.Wall => int.MaxValue,
            TileType.Fence => int.MaxValue,
            _ => int.MaxValue,
        };
    }
}
