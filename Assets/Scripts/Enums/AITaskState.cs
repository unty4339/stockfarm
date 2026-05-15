/// <summary>
/// AIタスクの実行状態
/// </summary>
public enum AITaskState
{
    /// <summary>予約済み・未開始</summary>
    Pending,
    /// <summary>目標地点へ移動中</summary>
    Moving,
    /// <summary>実行中</summary>
    Executing,
    /// <summary>正常完了</summary>
    Completed,
    /// <summary>中断済み</summary>
    Interrupted,
}
