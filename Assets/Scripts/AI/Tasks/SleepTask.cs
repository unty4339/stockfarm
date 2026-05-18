using UnityEngine;

/// <summary>
/// 睡眠タスク。割り当て済みのベッドに向かい、スタミナを回復する
/// </summary>
public class SleepTask : AITaskBase
{
    private readonly CowBed _bed;
    private int _executingTicks;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="owner">睡眠するワーカー</param>
    /// <param name="bed">目標ベッド</param>
    public SleepTask(WorkerBase owner, CowBed bed)
    {
        Owner = owner;
        _bed = bed;
        Priority = 2;
        TargetPosition = bed.GridPosition;
    }

    /// <inheritdoc/>
    public override bool CanExecute()
    {
        return _bed != null && _bed.IsAssignedTo(Owner);
    }

    /// <inheritdoc/>
    protected override void OnExecute()
    {
        _executingTicks++;
        if (Owner is FarmerWorker farmer)
        {
            var mood = RoomManager.Instance?.GetMoodAt(Owner.GridPosition);
            farmer.Sleep(mood?.RestMood ?? 0f);
        }
    }

    /// <inheritdoc/>
    protected override bool IsComplete()
    {
        return Owner.Stamina >= 99f ||
               Owner.GetScheduleAt(GameTimeManager.Instance?.CurrentSlot ?? 0) != ScheduleSlotType.Sleep;
    }

    /// <inheritdoc/>
    public override string GetActionText() => "睡眠中";
}