using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// タスクの優先度評価を行い、実行すべきタスクを決定する
/// </summary>
public class AIDecisionMaker
{
    /// <summary>管理対象のワーカー</summary>
    public WorkerBase Owner { get; }
    /// <summary>タスク種別ごとの優先度設定（1〜7）</summary>
    public Dictionary<Type, int> TaskPriorities { get; } = new Dictionary<Type, int>();

    private AITaskBase _forcedTask;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="owner">管理対象のワーカー</param>
    public AIDecisionMaker(WorkerBase owner)
    {
        Owner = owner;
        InitializeDefaultPriorities();
    }

    private void InitializeDefaultPriorities()
    {
        TaskPriorities[typeof(BirthTask)] = 1;
        TaskPriorities[typeof(SleepTask)] = 2;
        TaskPriorities[typeof(EatTask)] = 3;
        TaskPriorities[typeof(SellDeliveryTask)] = 3;
        TaskPriorities[typeof(MilkWaitTask)] = 4;
        TaskPriorities[typeof(CareTask)] = 5;
        TaskPriorities[typeof(ProcessingAssistTask)] = 5;
        TaskPriorities[typeof(TransportTask)] = 5;
        TaskPriorities[typeof(HarvestTask)] = 4;
        TaskPriorities[typeof(PlantTask)] = 5;
        TaskPriorities[typeof(PickUpTask)] = 4;
        TaskPriorities[typeof(CleanTask)] = 6;
        TaskPriorities[typeof(IdleWalkTask)] = 7;
    }

    /// <summary>
    /// タスクが空の時に次のタスクをアサインする
    /// </summary>
    public void TryAssignNextTask()
    {
        if (Owner.CurrentTask != null) return;

        var task = DecideTask();
        if (task != null)
            Owner.CurrentTask = task;
    }

    /// <summary>
    /// 現在の状態に基づき最適なタスクを決定して返す
    /// </summary>
    /// <returns>実行すべきタスク（なければnull）</returns>
    public AITaskBase DecideTask()
    {
        if (_forcedTask != null && _forcedTask.State == AITaskState.Pending)
        {
            var f = _forcedTask;
            _forcedTask = null;
            return f;
        }

        // 出産は最優先
        if (Owner is CowWorker cow && cow.HasEffect(StatusEffectType.PregnancyLate))
        {
            var birth = new BirthTask(cow);
            if (birth.CanExecute()) return birth;
        }

        // 緊急タスク（スケジュール無視）
        if (Owner.HasEffect(StatusEffectType.Hungry) || Owner.HasEffect(StatusEffectType.Malnutrition))
        {
            var eat = TryCreateEatTask();
            if (eat != null) return eat;
        }

        if (Owner.HasEffect(StatusEffectType.Fatigue) || Owner.Stamina < 20f)
        {
            var sleep = TryCreateSleepTask();
            if (sleep != null) return sleep;
        }

        // 種付けフラグがある牛：牧場主が種付けアイドル待機中なら即向かう
        if (Owner is CowWorker breedCow
            && (breedCow.WantsBreeding || breedCow.WantsBreedingRepeat)
            && !breedCow.HasEffect(StatusEffectType.PregnancyEarly)
            && !breedCow.HasEffect(StatusEffectType.PregnancyLate))
        {
            var idleTask = FindAvailableBreedingIdleTask();
            if (idleTask != null)
            {
                var breedTask = new BreedingTask(breedCow, idleTask);
                if (breedTask.CanExecute()) return breedTask;
                breedTask.Interrupt();
            }
        }

        // スケジュールに従ったタスク
        int slot = GameTimeManager.Instance?.CurrentSlot ?? 8;
        ScheduleSlotType scheduleType = Owner.GetScheduleAt(slot);

        return scheduleType switch
        {
            ScheduleSlotType.Sleep    => TryCreateSleepTask() ?? CreateIdleTask(),
            ScheduleSlotType.Joy      => TryCreateJoyTask() ?? CreateIdleTask(),
            ScheduleSlotType.Work     => TryCreateWorkTask() ?? CreateIdleTask(),
            ScheduleSlotType.Breeding => TryCreateBreedingIdleTask() ?? CreateIdleTask(),
            _                         => CreateIdleTask(),
        };
    }

    /// <summary>
    /// プレイヤー命令によるタスクを強制設定する（スケジュール制限を無視）
    /// </summary>
    /// <param name="task">強制するタスク</param>
    public void ForceTask(AITaskBase task)
    {
        Owner.CurrentTask?.Interrupt();
        _forcedTask = task;
        Owner.CurrentTask = null;
    }

    /// <summary>
    /// タスクの優先度を設定する
    /// </summary>
    /// <param name="taskType">タスク種別</param>
    /// <param name="priority">優先度（1〜7）</param>
    public void SetTaskPriority(Type taskType, int priority)
    {
        TaskPriorities[taskType] = Mathf.Clamp(priority, 1, 7);
    }

    private AITaskBase TryCreateSleepTask()
    {
        var bed = Owner.AssignedBed;
        if (bed == null) return null;
        var task = new SleepTask(Owner, bed);
        return task.CanExecute() ? task : null;
    }

    private AITaskBase TryCreateEatTask()
    {
        if (Owner is FarmerWorker farmer)
        {
            var shelf = FindNearest<FoodShelf>();
            if (shelf == null) return null;
            var task = new EatTask(farmer, shelf);
            return task.CanExecute() ? task : null;
        }
        if (Owner is CowWorker)
        {
            var trough = FindNearest<FeedingTrough>();
            if (trough == null) return null;
            var task = new EatTask(Owner, trough);
            return task.CanExecute() ? task : null;
        }
        return null;
    }

    private AITaskBase TryCreateJoyTask()
    {
        if (Owner is CowWorker)
        {
            var brush = FindNearest<AutoBrush>();
            if (brush != null && !brush.IsInUse)
            {
                var care = new CareTask(Owner as CowWorker, brush);
                if (care.CanExecute()) return care;
            }
        }
        return null;
    }

    private AITaskBase TryCreateWorkTask()
    {
        if (Owner is FarmerWorker)
        {
            var sellTile = StorageManager.Instance?.FindNearestSellFlaggedTile(Owner.GridPosition);
            if (sellTile != null && StorageManager.Instance.TryReservePickup(sellTile.Position))
            {
                var sell = new SellDeliveryTask(Owner, sellTile);
                if (sell.CanExecute()) return sell;
                StorageManager.Instance.ReleasePickup(sellTile.Position);
            }

            var pickupTile = StorageManager.Instance?.FindNearestPickupTile(Owner.GridPosition);
            if (pickupTile != null && StorageManager.Instance.TryReservePickup(pickupTile.Position))
            {
                var pickUp = new PickUpTask(Owner, pickupTile);
                if (pickUp.CanExecute()) return pickUp;
                StorageManager.Instance.ReleasePickup(pickupTile.Position);
            }
        }

        var harvestTile = CropManager.Instance?.FindNearestHarvestReadyTile(Owner.GridPosition);
        if (harvestTile != null)
        {
            var harvest = new HarvestTask(Owner, harvestTile);
            if (harvest.CanExecute()) return harvest;
        }

        var plantTile = CropManager.Instance?.FindNearestEmptyAgricultureTile(Owner.GridPosition);
        if (plantTile != null)
        {
            var plant = new PlantTask(Owner, plantTile);
            if (plant.CanExecute()) return plant;
        }

        if (Owner is CowWorker cowWorker && cowWorker.MilkAccumulation >= 5f)
        {
            var stand = FindNearest<ManualMilkingStand>();
            if (stand != null && stand.OccupyingCow == null)
            {
                var milkTask = new MilkWaitTask(cowWorker, stand);
                if (milkTask.CanExecute()) return milkTask;
            }
        }

        var processing = FindNearest<ProcessingEquipmentBase>();
        if (processing != null && !processing.IsProcessing)
        {
            var assistTask = new ProcessingAssistTask(Owner, processing);
            if (assistTask.CanExecute()) return assistTask;
        }

        var wasteTarget = FindWasteMarker();
        if (wasteTarget != null)
        {
            var cleanTask = new CleanTask(Owner, wasteTarget);
            if (cleanTask.CanExecute()) return cleanTask;
        }

        return null;
    }

    private AITaskBase CreateIdleTask()
    {
        return new IdleWalkTask(Owner);
    }

    private AITaskBase TryCreateBreedingIdleTask()
    {
        if (Owner is not FarmerWorker farmer) return null;
        var bed = Owner.AssignedBed;
        if (bed == null) return null;
        var task = new BreedingIdleTask(farmer, bed);
        return task.CanExecute() ? task : null;
    }

    private BreedingIdleTask FindAvailableBreedingIdleTask()
    {
        foreach (var worker in UnityEngine.Object.FindObjectsByType<FarmerWorker>(FindObjectsSortMode.None))
        {
            if (worker.CurrentTask is BreedingIdleTask idleTask
                && idleTask.State == AITaskState.Executing
                && idleTask.HasCapacity)
                return idleTask;
        }
        return null;
    }

    private T FindNearest<T>() where T : EquipmentBase
    {
        if (BuildingManager.Instance == null) return null;
        T nearest = null;
        int minDist = int.MaxValue;
        foreach (var eq in BuildingManager.Instance.GetAllEquipments())
        {
            if (eq is T typed)
            {
                int dist = Mathf.Abs(eq.GridPosition.x - Owner.GridPosition.x) +
                           Mathf.Abs(eq.GridPosition.y - Owner.GridPosition.y);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = typed;
                }
            }
        }
        return nearest;
    }

    private WasteMarker FindWasteMarker()
    {
        if (RoomManager.Instance == null) return null;
        foreach (var room in RoomManager.Instance.Rooms)
        {
            foreach (var waste in room.WasteMarkers)
            {
                if (!waste.IsGone) return waste;
            }
        }
        return null;
    }
}
