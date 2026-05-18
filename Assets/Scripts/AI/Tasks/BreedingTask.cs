/// <summary>
/// 牛の種付けタスク。牧場主がアイドル中のベッドへ移動し、種付けを行う
/// 完了後に妊娠状態になる。「種付け（繰り返し）」でない場合はフラグも解除する
/// </summary>
public class BreedingTask : AITaskBase
{
    private readonly BreedingIdleTask _farmerTask;
    private const int BreedingDurationTicks = 3000;
    private int _executingTicks;
    private bool _completed;
    private bool _registered;

    /// <summary>
    /// コンストラクタ。生成時点で牧場主タスクに牛を登録する
    /// </summary>
    /// <param name="owner">種付けを行う牛</param>
    /// <param name="farmerTask">対象牧場主の種付けアイドルタスク</param>
    public BreedingTask(CowWorker owner, BreedingIdleTask farmerTask)
    {
        Owner = owner;
        _farmerTask = farmerTask;
        Priority = 3;
        TargetPosition = farmerTask.TargetPosition;
        _farmerTask.RegisterCow();
        _registered = true;
    }

    /// <inheritdoc/>
    public override bool CanExecute()
    {
        return _farmerTask != null && _farmerTask.State == AITaskState.Executing;
    }

    /// <inheritdoc/>
    protected override void OnExecute()
    {
        if (_completed) return;
        _executingTicks++;
        if (_executingTicks < BreedingDurationTicks) return;

        _completed = true;
        var cow = (CowWorker)Owner;
        cow.StartPregnancy();
        if (!cow.WantsBreedingRepeat)
            cow.WantsBreeding = false;
        Unregister();
    }

    /// <inheritdoc/>
    protected override bool IsComplete() => _completed;

    /// <inheritdoc/>
    public override void Interrupt()
    {
        base.Interrupt();
        Unregister();
    }

    private void Unregister()
    {
        if (_registered)
        {
            _farmerTask?.UnregisterCow();
            _registered = false;
        }
    }

    /// <inheritdoc/>
    public override string GetActionText() => "種付け中";
}