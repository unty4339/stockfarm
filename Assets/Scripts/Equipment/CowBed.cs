using UnityEngine;

/// <summary>
/// 牛用ベッドの基底クラス（牧場主も使用可）。1対1でワーカーに割り当てる
/// </summary>
public abstract class CowBed : EquipmentBase
{
    public override int BuildCost => 200;
    public override int MoodBonus => 20;
    public override MoodType AffectedMoodType => MoodType.Rest;

    /// <summary>睡眠時のスタミナ回復倍率（例: 0.9 = 90%）</summary>
    public abstract float StaminaRecoveryMultiplier { get; }

    /// <summary>同時に種付けできる牛の上限数（デフォルト1、サブクラスでオーバーライド可能）</summary>
    public virtual int BreedingCapacity => 1;

    /// <summary>割り当て済みワーカー（nullなら空き）</summary>
    public WorkerBase AssignedWorker { get; private set; }

    /// <summary>
    /// ワーカーを割り当てる（既に割り当てがある場合は例外）
    /// </summary>
    /// <param name="worker">割り当てるワーカー</param>
    public void Assign(WorkerBase worker)
    {
        if (AssignedWorker != null)
            throw new System.InvalidOperationException($"このベッドにはすでに {AssignedWorker.name} が割り当てられています");

        AssignedWorker = worker;
    }

    /// <summary>
    /// 割り当てを解除する
    /// </summary>
    public void Unassign()
    {
        AssignedWorker = null;
    }

    /// <summary>
    /// 指定ワーカーに割り当てられているかを返す
    /// </summary>
    /// <param name="worker">確認するワーカー</param>
    /// <returns>割り当て済みの場合true</returns>
    public bool IsAssignedTo(WorkerBase worker)
    {
        return AssignedWorker == worker;
    }
}
