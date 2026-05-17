/// <summary>
/// 地面で睡眠するタスク。ベッドが見つからない場合に現在地で休息する。
/// ベッドより回復効率が低い
/// </summary>
public class GroundSleepTask : AITaskBase
{
    /// <summary>地面睡眠時のスタミナ回復係数</summary>
    public const float GroundRecoveryMultiplier = 0.5f;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="owner">睡眠するワーカー</param>
    public GroundSleepTask(WorkerBase owner)
    {
        Owner = owner;
        Priority = 2;
        TargetPosition = owner.GridPosition;
    }

    /// <inheritdoc/>
    public override bool CanExecute() => true;

    /// <inheritdoc/>
    protected override void OnExecute() { }

    /// <inheritdoc/>
    protected override bool IsComplete()
    {
        // ベッドが割り当てられたら SleepTask に移譲するため完了
        if (Owner.AssignedBed != null) return true;
        return Owner.Stamina >= 99f ||
               Owner.GetScheduleAt(GameTimeManager.Instance?.CurrentSlot ?? 0) != ScheduleSlotType.Sleep;
    }
}
