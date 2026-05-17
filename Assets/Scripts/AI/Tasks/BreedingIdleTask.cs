using UnityEngine;

/// <summary>
/// 牧場主の種付けアイドルタスク。割り当てベッドへ移動し、種付けスケジュール中かつ
/// 種付け中の牛がいる間は待機を続ける
/// </summary>
public class BreedingIdleTask : AITaskBase
{
    private readonly CowBed _bed;
    private int _activeCowCount;

    /// <summary>現在種付け中の牛の数</summary>
    public int ActiveCowCount => _activeCowCount;

    /// <summary>まだ受け入れ可能な牛がいるか</summary>
    public bool HasCapacity => _activeCowCount < _bed.BreedingCapacity;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="owner">種付けアイドルする牧場主</param>
    /// <param name="bed">目標ベッド</param>
    public BreedingIdleTask(FarmerWorker owner, CowBed bed)
    {
        Owner = owner;
        _bed = bed;
        Priority = 3;
        TargetPosition = bed.GridPosition;
    }

    /// <inheritdoc/>
    public override bool CanExecute() => _bed != null && _bed.IsAssignedTo(Owner);

    /// <inheritdoc/>
    protected override void OnExecute() { }

    /// <inheritdoc/>
    protected override bool IsComplete()
    {
        int slot = GameTimeManager.Instance?.CurrentSlot ?? 0;
        bool isBreedingSlot = Owner.GetScheduleAt(slot) == ScheduleSlotType.Breeding;
        return !isBreedingSlot && _activeCowCount == 0;
    }

    /// <summary>
    /// 種付け開始時に牛をカウントに登録する
    /// </summary>
    public void RegisterCow() => _activeCowCount++;

    /// <summary>
    /// 種付け終了時に牛をカウントから除外する
    /// </summary>
    public void UnregisterCow() => _activeCowCount = Mathf.Max(0, _activeCowCount - 1);
}
