using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 保管ゾーン外のタイルに置かれたアイテムを拾い、保管ゾーンへ運ぶタスク
/// スタック可能アイテムは周辺の同種アイテム（優先度劣後分）をまとめて収集する
/// </summary>
public class PickUpTask : AITaskBase
{
    private enum Phase { PickingUp, AdditionalPickup, Delivering }

    private readonly GridTile _sourceTile;
    private TileItem _carrying;
    private int _destinationPriority;
    private Vector2Int _destinationPos;
    private readonly Queue<GridTile> _additionalTiles = new();
    private GridTile _currentAdditionalTile;
    private Phase _phase = Phase.PickingUp;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="owner">タスクを実行するワーカー</param>
    /// <param name="sourceTile">アイテムが置かれているタイル</param>
    public PickUpTask(WorkerBase owner, GridTile sourceTile)
    {
        Owner = owner;
        _sourceTile = sourceTile;
        Priority = 4;
        TargetPosition = sourceTile.Position;
    }

    /// <inheritdoc/>
    public override bool CanExecute()
    {
        // PickingUp フェーズを過ぎていれば搬送中なので、ソースタイルの状態に関わらず続行する
        if (_phase != Phase.PickingUp) return true;

        return _sourceTile.PlacedItem != null &&
               _sourceTile.Zone?.Type != ZoneType.Storage &&
               StorageManager.Instance != null &&
               StorageManager.Instance.FindNearestStorageDestination(Owner.GridPosition).HasValue;
    }

    /// <inheritdoc/>
    public override void Interrupt()
    {
        switch (_phase)
        {
            case Phase.PickingUp:
                StorageManager.Instance?.ReleasePickup(_sourceTile.Position);
                break;
            case Phase.AdditionalPickup:
                if (_currentAdditionalTile != null)
                    StorageManager.Instance?.ReleasePickup(_currentAdditionalTile.Position);
                break;
        }

        while (_additionalTiles.Count > 0)
            StorageManager.Instance?.ReleasePickup(_additionalTiles.Dequeue().Position);

        if (_carrying != null)
        {
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
            case Phase.AdditionalPickup:
                ExecuteAdditionalPickup();
                break;
            case Phase.Delivering:
                ExecuteDelivering();
                break;
        }
    }

    private void ExecutePickingUp()
    {
        var destResult = StorageManager.Instance?.FindNearestStorageDestination(Owner.GridPosition);
        if (destResult == null || _sourceTile.PlacedItem == null)
        {
            StorageManager.Instance?.ReleasePickup(_sourceTile.Position);
            Interrupt();
            return;
        }

        _destinationPos = destResult.Value.pos;
        _destinationPriority = destResult.Value.zone.Priority;

        _carrying = _sourceTile.PlacedItem;
        _sourceTile.PlacedItem = null;
        ItemVisualManager.Instance?.Refresh(_sourceTile.Position);
        StorageManager.Instance.ReleasePickup(_sourceTile.Position);

        if (_carrying.IsStackable && _carrying.StackCount < _carrying.StackLimit)
        {
            int remaining = _carrying.StackLimit - _carrying.StackCount;
            var additionals = StorageManager.Instance.FindAdditionalPickupTiles(
                Owner.GridPosition, _carrying.Type, _destinationPriority, remaining);
            foreach (var tile in additionals)
            {
                if (StorageManager.Instance.TryReservePickup(tile.Position))
                    _additionalTiles.Enqueue(tile);
            }
        }

        TransitionToNextStop();
    }

    private void ExecuteAdditionalPickup()
    {
        if (_currentAdditionalTile?.PlacedItem != null && _currentAdditionalTile.PlacedItem.Type == _carrying.Type)
        {
            int canTake = _carrying.StackLimit - _carrying.StackCount;
            int taken = _currentAdditionalTile.PlacedItem.TryTake(canTake);
            _carrying.TryAddStack(taken);

            if (_currentAdditionalTile.PlacedItem.StackCount == 0)
            {
                _currentAdditionalTile.PlacedItem = null;
                ItemVisualManager.Instance?.Refresh(_currentAdditionalTile.Position);
            }
        }

        StorageManager.Instance?.ReleasePickup(_currentAdditionalTile.Position);
        TransitionToNextStop();
    }

    private void ExecuteDelivering()
    {
        if (_carrying == null) return;

        if (!_carrying.IsStackable)
        {
            ItemDropService.DropItem(_carrying, Owner.GridPosition);
            _carrying = null;
            return;
        }

        PlaceOnStorageTile(MapManager.Instance.GetTile(Owner.GridPosition));

        if (_carrying == null) return;

        var nextDest = StorageManager.Instance?.FindNearestStorageDestination(Owner.GridPosition, _carrying.Type);
        if (nextDest == null)
        {
            ItemDropService.DropItem(_carrying, Owner.GridPosition);
            _carrying = null;
            return;
        }

        _destinationPos = nextDest.Value.pos;
        RestartMovingTo(_destinationPos);
    }

    private void PlaceOnStorageTile(GridTile tile)
    {
        if (tile.PlacedItem == null)
        {
            int toPlace = Mathf.Min(_carrying.StackLimit, _carrying.StackCount);
            tile.PlacedItem = new TileItem(_carrying.Type, toPlace);
            _carrying.TryTake(toPlace);
        }
        else if (tile.PlacedItem.Type == _carrying.Type && !tile.PlacedItem.IsStackFull)
        {
            int canAdd = tile.PlacedItem.StackLimit - tile.PlacedItem.StackCount;
            int taken = _carrying.TryTake(Mathf.Min(canAdd, _carrying.StackCount));
            tile.PlacedItem.TryAddStack(taken);
        }

        ItemVisualManager.Instance?.Refresh(tile.Position);

        if (_carrying.StackCount == 0)
            _carrying = null;
    }

    private void TransitionToNextStop()
    {
        while (_additionalTiles.Count > 0 && (_carrying == null || _carrying.StackCount >= _carrying.StackLimit))
            StorageManager.Instance?.ReleasePickup(_additionalTiles.Dequeue().Position);

        if (_carrying != null && _carrying.StackCount < _carrying.StackLimit && _additionalTiles.Count > 0)
        {
            _currentAdditionalTile = _additionalTiles.Dequeue();
            _phase = Phase.AdditionalPickup;
            RestartMovingTo(_currentAdditionalTile.Position);
        }
        else
        {
            while (_additionalTiles.Count > 0)
                StorageManager.Instance?.ReleasePickup(_additionalTiles.Dequeue().Position);

            _phase = Phase.Delivering;
            RestartMovingTo(_destinationPos);
        }
    }

    /// <inheritdoc/>
    protected override bool IsComplete()
    {
        return _phase == Phase.Delivering && _carrying == null;
    }
}
