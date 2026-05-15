/// <summary>
/// 部屋の3種類のムード値を保持するデータクラス
/// </summary>
public class MoodData
{
    /// <summary>作業ムード（0〜100）</summary>
    public float WorkMood { get; set; }
    /// <summary>休憩ムード（0〜100）</summary>
    public float RestMood { get; set; }
    /// <summary>繁殖ムード（0〜100）</summary>
    public float BreedMood { get; set; }

    /// <summary>
    /// 全ムード0のデフォルトインスタンスを生成する
    /// </summary>
    public MoodData() { }

    /// <summary>
    /// 指定値でインスタンスを生成する
    /// </summary>
    /// <param name="workMood">作業ムード</param>
    /// <param name="restMood">休憩ムード</param>
    /// <param name="breedMood">繁殖ムード</param>
    public MoodData(float workMood, float restMood, float breedMood)
    {
        WorkMood = workMood;
        RestMood = restMood;
        BreedMood = breedMood;
    }
}
