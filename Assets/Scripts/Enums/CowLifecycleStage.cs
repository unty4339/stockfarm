/// <summary>
/// 牛のライフサイクル段階
/// </summary>
public enum CowLifecycleStage
{
    /// <summary>子牛（AgeInDays 0〜29）</summary>
    Calf,
    /// <summary>若牛（AgeInDays 30以上、未出産）</summary>
    YoungCow,
    /// <summary>成牛（出産経験あり）</summary>
    AdultCow,
}
