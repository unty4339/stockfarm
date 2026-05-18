/// <summary>
/// 搾乳待機タスク。搾乳スタンドへ移動し、搾乳操作を待機する
/// </summary>
public class MilkWaitTask : AITaskBase
{
    private readonly CowWorker _cow;
    private readonly ManualMilkingStand _stand;
    private bool _milked;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="cow">搾乳される牛</param>
    /// <param name="stand">目標の搾乳スタンド</param>
    public MilkWaitTask(CowWorker cow, ManualMilkingStand stand)
    {
        _cow = cow;
        _stand = stand;
        Owner = cow;
        Priority = 4;
        TargetPosition = stand.GridPosition;
    }

    /// <inheritdoc/>
    public override bool CanExecute()
    {
        return _cow.LifecycleStage == CowLifecycleStage.AdultCow &&
               _cow.HasEffect(StatusEffectType.Lactating) &&
               _cow.MilkAccumulation >= 1f &&
               _stand.IsPlaced &&
               _stand.OccupyingCow == null;
    }

    /// <inheritdoc/>
    protected override void OnExecute()
    {
        if (!_milked)
        {
            _stand.OccupyCow(_cow);
            var milk = _cow.Milk();
            _stand.OutputTank?.TryStore(milk);
            _stand.ReleaseCow();
            _milked = true;
        }
    }

    /// <inheritdoc/>
    protected override bool IsComplete() => _milked;

    /// <inheritdoc/>
    public override string GetActionText() => "搾乳を待っています";
}