/// <summary>
/// 出産タスク。最優先（Priority=1）で発動し、スケジュールを無視する
/// </summary>
public class BirthTask : AITaskBase
{
    private readonly CowWorker _cow;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="cow">出産する牛</param>
    public BirthTask(CowWorker cow)
    {
        _cow = cow;
        Owner = cow;
        Priority = 1;
        TargetPosition = cow.GridPosition;
    }

    /// <inheritdoc/>
    public override bool CanExecute()
    {
        return _cow.HasEffect(StatusEffectType.PregnancyLate);
    }

    /// <inheritdoc/>
    protected override void OnExecute()
    {
        _cow.GiveBirth();
    }

    /// <inheritdoc/>
    protected override bool IsComplete()
    {
        return !_cow.HasEffect(StatusEffectType.PregnancyLate);
    }

    /// <inheritdoc/>
    public override string GetActionText() => "出産中";
}