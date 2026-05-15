using UnityEngine;

/// <summary>
/// 運搬タスク。2フェーズ（取出→納品）を内部状態で管理する
/// </summary>
public class TransportTask : AITaskBase
{
    private enum TransportPhase { TakingOut, Delivering }

    private readonly IContainer _source;
    private readonly IContainer _destination;
    private readonly ResourceType _resourceType;
    private readonly int _amount;
    private TransportPhase _phase = TransportPhase.TakingOut;
    private ResourceBase _carrying;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="owner">運搬するワーカー</param>
    /// <param name="source">取出元</param>
    /// <param name="destination">納品先</param>
    /// <param name="resourceType">運搬するリソース種別</param>
    /// <param name="amount">運搬量</param>
    public TransportTask(WorkerBase owner, IContainer source, IContainer destination, ResourceType resourceType, int amount)
    {
        Owner = owner;
        _source = source;
        _destination = destination;
        _resourceType = resourceType;
        _amount = amount;
        Priority = 5;

        if (source is EquipmentBase srcEq)
            TargetPosition = srcEq.GridPosition;
    }

    /// <inheritdoc/>
    public override bool CanExecute()
    {
        return _source.GetAmount(_resourceType) >= _amount &&
               _destination.GetRemainingCapacity() >= _amount;
    }

    /// <inheritdoc/>
    protected override void OnExecute()
    {
        switch (_phase)
        {
            case TransportPhase.TakingOut:
                if (_source.TryTake(_resourceType, _amount, out _carrying))
                {
                    _phase = TransportPhase.Delivering;
                    if (_destination is EquipmentBase dstEq)
                        TargetPosition = dstEq.GridPosition;
                }
                break;
            case TransportPhase.Delivering:
                if (_carrying != null)
                    _destination.TryStore(_carrying);
                _carrying = null;
                break;
        }
    }

    /// <inheritdoc/>
    protected override bool IsComplete()
    {
        return _phase == TransportPhase.Delivering && _carrying == null;
    }
}
