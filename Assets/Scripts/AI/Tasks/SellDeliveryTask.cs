using UnityEngine;

/// <summary>
/// 売却フラグが立ったアイテムを拾い、納品ボックスへ運搬するタスク
/// </summary>
public class SellDeliveryTask : AITaskBase
{
    private enum Phase { PickingUp, Delivering }

    private readonly GridTile _sourceTile;
    private TileItem _carrying;
    private Phase _phase = Phase.PickingUp;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="owner">タスクを実行するワーカー</param>
    /// <param name="sourceTile">売却フラグ付きアイテムが置かれているタイル</param>
    public SellDeliveryTask(WorkerBase owner, GridTile sourceTile)
    {
        Owner = owner;
        _sourceTile = sourceTile;
        Priority = 3;
        TargetPosition = sourceTile.Position;
    }

    /// <inheritdoc/>
    public override bool CanExecute()
    {
        // Delivering フェーズはアイテム搬送中なので、ソースタイルの状態に関わらず続行する
        if (_phase != Phase.PickingUp) return true;

        return _sourceTile.PlacedItem?.SellFlag == true &&
               DeliveryBox.Instance != null &&
               DeliveryBox.Instance.HasCapacity();
    }

    /// <inheritdoc/>
    public override void Interrupt()
    {
        if (_phase == Phase.PickingUp)
            StorageManager.Instance?.ReleasePickup(_sourceTile.Position);

        if (_carrying != null)
        {
            _carrying.SellFlag = true;
            ItemDropService.DropItem(_carrying, Owner.GridPosition);
            _carrying = null;
        }

        base.Interrupt();
    }

    /// <inheritdoc/>
    protected override void OnExecute()
    {
        switch (_phase)
        {
            case Phase.PickingUp:
                ExecutePickingUp();
                break;
            case Phase.Delivering:
                ExecuteDelivering();
                break;
        }
    }

    private void ExecutePickingUp()
    {
        if (_sourceTile.PlacedItem?.SellFlag != true)
        {
            StorageManager.Instance?.ReleasePickup(_sourceTile.Position);
            Interrupt();
            return;
        }

        _carrying = _sourceTile.PlacedItem;
        _sourceTile.PlacedItem = null;
        ItemVisualManager.Instance?.Refresh(_sourceTile.Position);
        StorageManager.Instance?.ReleasePickup(_sourceTile.Position);

        _phase = Phase.Delivering;
        RestartMovingTo(DeliveryBox.Instance.GridPosition);
    }

    private void ExecuteDelivering()
    {
        if (_carrying == null) return;

        if (DeliveryBox.Instance == null || !DeliveryBox.Instance.TryStore(_carrying))
        {
            _carrying.SellFlag = true;
            ItemDropService.DropItem(_carrying, Owner.GridPosition);
        }

        _carrying = null;
    }

    /// <inheritdoc/>
    protected override bool IsComplete()
    {
        return _phase == Phase.Delivering && _carrying == null;
    }

    /// <inheritdoc/>
    public override string GetActionText() => "販売品を運搬中";
}