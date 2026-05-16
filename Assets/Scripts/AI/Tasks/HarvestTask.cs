/// <summary>
/// 収穫タスク。収穫待ちの作物があるタイルへ移動して収穫し、アイテムをドロップする
/// </summary>
public class HarvestTask : AITaskBase
{
    private const int WorkTicksRequired = 3;

    private readonly GridTile _tile;
    private int _workTicksElapsed;
    private bool _harvested;

    /// <summary>
    /// コンストラクタ。対象タイルを収穫予約する
    /// </summary>
    /// <param name="owner">収穫を行うワーカー</param>
    /// <param name="tile">収穫対象のタイル</param>
    public HarvestTask(WorkerBase owner, GridTile tile)
    {
        Owner = owner;
        _tile = tile;
        Priority = 4;
        TargetPosition = tile.Position;
        CropManager.Instance?.TryReserveHarvesting(tile.Position);
    }

    /// <inheritdoc/>
    public override bool CanExecute()
    {
        return _tile != null && _tile.Crop != null && _tile.Crop.IsReadyToHarvest;
    }

    /// <inheritdoc/>
    protected override void OnExecute()
    {
        if (_harvested) return;

        _workTicksElapsed++;
        if (_workTicksElapsed < WorkTicksRequired) return;

        if (_tile.Crop != null)
        {
            var drop = CropData.GetHarvestDrop(_tile.Crop.Type);
            ItemDropService.DropItem(new TileItem(drop.resourceType, drop.count), _tile.Position);
            CropManager.Instance?.RemoveCrop(_tile);
        }
        CropManager.Instance?.ReleaseHarvesting(_tile.Position);
        _harvested = true;
    }

    /// <inheritdoc/>
    protected override bool IsComplete() => _harvested;

    /// <inheritdoc/>
    public override void Interrupt()
    {
        CropManager.Instance?.ReleaseHarvesting(_tile.Position);
        base.Interrupt();
    }
}
