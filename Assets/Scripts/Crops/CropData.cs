/// <summary>
/// タイル上の作物の成長状態を保持するデータクラス
/// </summary>
public class CropData
{
    /// <summary>作物の種別</summary>
    public CropType Type { get; }

    /// <summary>現在の成長tick数</summary>
    public int GrowthTick { get; private set; }

    /// <summary>収穫可能かどうか</summary>
    public bool IsReadyToHarvest => GrowthTick >= GetMaxGrowthTick(Type);

    /// <summary>成長進捗（0.0〜1.0）</summary>
    public float GrowthProgress => (float)GrowthTick / GetMaxGrowthTick(Type);

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="type">作物の種別</param>
    public CropData(CropType type)
    {
        Type = type;
    }

    /// <summary>
    /// 成長tickを1進める
    /// </summary>
    public void AdvanceGrowth()
    {
        GrowthTick++;
    }

    /// <summary>
    /// 作物種別ごとの最大成長tick数を返す
    /// </summary>
    /// <param name="type">作物の種別</param>
    /// <returns>最大成長tick数</returns>
    public static int GetMaxGrowthTick(CropType type) => type switch
    {
        CropType.Wheat => 10,
        _ => 10,
    };

    /// <summary>
    /// 作物種別ごとの収穫ドロップ情報を返す
    /// </summary>
    /// <param name="type">作物の種別</param>
    /// <returns>ドロップするリソース種別と個数のタプル</returns>
    public static (ResourceType resourceType, int count) GetHarvestDrop(CropType type) => type switch
    {
        CropType.Wheat => (ResourceType.Wheat, 10),
        _ => (ResourceType.Wheat, 10),
    };
}
