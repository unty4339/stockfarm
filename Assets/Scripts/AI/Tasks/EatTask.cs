using UnityEngine;

/// <summary>
/// 摂食タスク。給餌桶または食料棚へ移動して食料を消費する
/// </summary>
public class EatTask : AITaskBase
{
    private readonly EquipmentBase _targetFood;
    private const int ConsumeAmount = 1;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="owner">摂食するワーカー</param>
    /// <param name="targetFood">目標の給餌桶または食料棚</param>
    public EatTask(WorkerBase owner, EquipmentBase targetFood)
    {
        Owner = owner;
        _targetFood = targetFood;
        Priority = 3;
        TargetPosition = targetFood.GridPosition;
    }

    /// <inheritdoc/>
    public override bool CanExecute()
    {
        return _targetFood != null && _targetFood.IsPlaced;
    }

    /// <inheritdoc/>
    protected override void OnExecute()
    {
        switch (_targetFood)
        {
            case FeedingTrough trough when Owner is CowWorker cow:
                if (trough.TryConsume(ConsumeAmount))
                {
                    cow.UpdateHunger(-50f);
                    cow.RemoveEffect(StatusEffectType.Hungry);
                    cow.RemoveEffect(StatusEffectType.Malnutrition);
                    cow.UpdateHappiness(5f);
                }
                break;
            case FoodShelf shelf when Owner is FarmerWorker farmer:
                farmer.Eat(shelf);
                break;
        }
    }

    /// <inheritdoc/>
    protected override bool IsComplete()
    {
        return !Owner.HasEffect(StatusEffectType.Hungry) || Owner.Hunger <= 30f;
    }

    /// <inheritdoc/>
    public override string GetActionText() => "食事中";
}