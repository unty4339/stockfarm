/// <summary>
/// 清掃タスク。廃棄物マーカーのあるタイルへ移動して除去する
/// </summary>
public class CleanTask : AITaskBase
{
    private readonly WasteMarker _target;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="owner">清掃するワーカー</param>
    /// <param name="target">清掃対象の廃棄物マーカー</param>
    public CleanTask(WorkerBase owner, WasteMarker target)
    {
        Owner = owner;
        _target = target;
        Priority = 6;
        TargetPosition = target.TilePosition;
    }

    /// <inheritdoc/>
    public override bool CanExecute()
    {
        return _target != null && !_target.IsGone;
    }

    /// <inheritdoc/>
    protected override void OnExecute()
    {
        _target.Decay(10);
    }

    /// <inheritdoc/>
    protected override bool IsComplete() => _target.IsGone;
}
