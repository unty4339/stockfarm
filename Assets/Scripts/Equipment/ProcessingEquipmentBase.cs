using System;
using UnityEngine;

/// <summary>
/// 加工設備の共通基底クラス。1サイクルの入出力とtick管理を担う
/// </summary>
public abstract class ProcessingEquipmentBase : EquipmentBase
{
    /// <summary>入力リソース種別</summary>
    public abstract ResourceType InputType { get; }
    /// <summary>1サイクルあたりの入力量</summary>
    public abstract int InputAmount { get; }
    /// <summary>出力リソース種別</summary>
    public abstract ResourceType OutputType { get; }
    /// <summary>1サイクルあたりの出力量</summary>
    public abstract int OutputAmount { get; }
    /// <summary>1サイクルの所要tick数</summary>
    public abstract int CycleTicks { get; }

    /// <summary>加工中フラグ</summary>
    public bool IsProcessing { get; private set; }
    /// <summary>加工補助中のワーカー（nullなら補助なし）</summary>
    public WorkerBase AssistantWorker { get; private set; }
    /// <summary>入力用タンク（液体リソース使用設備のみ）</summary>
    public LiquidTank InputTank { get; protected set; }
    /// <summary>出力用タンク（液体リソース使用設備のみ）</summary>
    public LiquidTank OutputTank { get; protected set; }

    /// <summary>1サイクルが完了したときに発火</summary>
    public event Action OnCycleCompleted;

    private int _currentTick;

    /// <summary>
    /// 加工サイクルを開始する
    /// assistantWorkerがnullでない場合、作業効率を加味してサイクル時間を短縮する
    /// </summary>
    /// <param name="assistantWorker">加工補助ワーカー（nullなら補助なし）</param>
    public void StartCycle(WorkerBase assistantWorker)
    {
        if (IsProcessing) return;

        IsProcessing = true;
        AssistantWorker = assistantWorker;
        _currentTick = 0;

        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnTickAdvanced += OnTick;
    }

    /// <summary>
    /// tickごとに進捗を更新する
    /// </summary>
    /// <param name="currentTick">現在のtick値</param>
    public void OnTick(int currentTick)
    {
        if (!IsProcessing) return;

        float efficiency = 1f;
        if (AssistantWorker != null)
        {
            var mood = RoomManager.Instance?.GetMoodAt(GridPosition);
            efficiency = AssistantWorker.GetWorkEfficiency(mood?.WorkMood ?? 0f);
        }

        _currentTick += Mathf.Max(1, Mathf.RoundToInt(efficiency));

        if (_currentTick >= CycleTicks)
            CompleteCycle();
    }

    private void CompleteCycle()
    {
        IsProcessing = false;
        AssistantWorker = null;

        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnTickAdvanced -= OnTick;

        OnCycleCompleted?.Invoke();
    }

    private void OnDestroy()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnTickAdvanced -= OnTick;
    }
}
