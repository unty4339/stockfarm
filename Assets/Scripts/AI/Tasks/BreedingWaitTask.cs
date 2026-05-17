/// <summary>
/// 種付けスケジュール中にベッドが空くのを待つタスク。空きが出た時点で完了し、
/// AIDecisionMaker が BreedingIdleTask へ切り替えるのを促す
/// </summary>
public class BreedingWaitTask : AITaskBase
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="owner">待機するワーカー</param>
    public BreedingWaitTask(WorkerBase owner)
    {
        Owner = owner;
        Priority = 3;
        TargetPosition = owner.GridPosition;
    }

    /// <inheritdoc/>
    public override bool CanExecute() => true;

    /// <inheritdoc/>
    protected override void OnExecute() { }

    /// <inheritdoc/>
    protected override bool IsComplete()
    {
        int slot = GameTimeManager.Instance?.CurrentSlot ?? 0;
        if (Owner.GetScheduleAt(slot) != ScheduleSlotType.Breeding) return true;
        return HasAvailableBed();
    }

    private bool HasAvailableBed()
    {
        if (BuildingManager.Instance == null) return false;
        foreach (var eq in BuildingManager.Instance.GetAllEquipments())
        {
            if (eq is CowBed bed && bed.AssignedWorker == null) return true;
        }
        return false;
    }
}
