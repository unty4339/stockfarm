/// <summary>
/// 加工補助タスク。加工設備に補助要員として参加する
/// </summary>
public class ProcessingAssistTask : AITaskBase
{
    private readonly ProcessingEquipmentBase _equipment;
    private bool _started;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="owner">補助するワーカー</param>
    /// <param name="equipment">補助対象の加工設備</param>
    public ProcessingAssistTask(WorkerBase owner, ProcessingEquipmentBase equipment)
    {
        Owner = owner;
        _equipment = equipment;
        Priority = 5;
        TargetPosition = equipment.GridPosition;
    }

    /// <inheritdoc/>
    public override bool CanExecute()
    {
        return _equipment.IsPlaced && !_equipment.IsProcessing;
    }

    /// <inheritdoc/>
    protected override void OnExecute()
    {
        if (!_started)
        {
            _equipment.StartCycle(Owner);
            _started = true;
        }
    }

    /// <inheritdoc/>
    protected override bool IsComplete()
    {
        return _started && !_equipment.IsProcessing;
    }
}
