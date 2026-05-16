/// <summary>
/// 植付けタスク。農業ゾーンの空きタイルへ移動して作物を植え付ける
/// </summary>
public class PlantTask : AITaskBase
{
    private const int WorkTicksRequired = 3;

    private readonly GridTile _tile;
    private int _workTicksElapsed;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="owner">植付けを行うワーカー</param>
    /// <param name="tile">植付け対象のタイル</param>
    public PlantTask(WorkerBase owner, GridTile tile)
    {
        Owner = owner;
        _tile = tile;
        Priority = 5;
        TargetPosition = tile.Position;
    }

    /// <inheritdoc/>
    public override bool CanExecute()
    {
        return _tile != null
            && _tile.Zone != null
            && _tile.Zone.Type == ZoneType.Agriculture
            && _tile.Crop == null;
    }

    /// <inheritdoc/>
    protected override void OnExecute()
    {
        _workTicksElapsed++;
        if (_workTicksElapsed >= WorkTicksRequired)
            CropManager.Instance.RegisterCrop(_tile, CropType.Wheat);
    }

    /// <inheritdoc/>
    protected override bool IsComplete() => _workTicksElapsed >= WorkTicksRequired;
}
