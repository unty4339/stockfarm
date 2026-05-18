/// <summary>
/// ケア利用タスク。自動ブラシ機を使用して幸福度を向上させる
/// </summary>
public class CareTask : AITaskBase
{
    private readonly CowWorker _cow;
    private readonly AutoBrush _brush;
    private bool _done;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="cow">ケアを受ける牛</param>
    /// <param name="brush">目標の自動ブラシ機</param>
    public CareTask(CowWorker cow, AutoBrush brush)
    {
        _cow = cow;
        _brush = brush;
        Owner = cow;
        Priority = 5;
        TargetPosition = brush.GridPosition;
    }

    /// <inheritdoc/>
    public override bool CanExecute()
    {
        return _brush.IsPlaced && !_brush.IsInUse;
    }

    /// <inheritdoc/>
    protected override void OnExecute()
    {
        if (!_done)
        {
            _brush.PerformCare(_cow);
            _done = true;
        }
    }

    /// <inheritdoc/>
    protected override bool IsComplete() => _done;

    /// <inheritdoc/>
    public override string GetActionText() => "手入れを受けています";
}