/// <summary>
/// リソースを格納・取り出し可能なコンテナのインタフェース
/// </summary>
public interface IContainer
{
    /// <summary>
    /// リソースを格納する（容量超過・型不一致の場合はfalseを返す）
    /// </summary>
    /// <param name="resource">格納するリソース</param>
    /// <returns>格納成功でtrue</returns>
    bool TryStore(ResourceBase resource);

    /// <summary>
    /// 指定種別・数量のリソースを取り出す（不足の場合はfalseを返す）
    /// </summary>
    /// <param name="type">リソース種別</param>
    /// <param name="amount">取り出す数量</param>
    /// <param name="resource">取り出したリソース</param>
    /// <returns>取り出し成功でtrue</returns>
    bool TryTake(ResourceType type, int amount, out ResourceBase resource);

    /// <summary>
    /// 指定種別の現在保管数量を返す
    /// </summary>
    /// <param name="type">リソース種別</param>
    /// <returns>保管数量</returns>
    int GetAmount(ResourceType type);

    /// <summary>
    /// 空き容量を返す
    /// </summary>
    /// <returns>空き容量</returns>
    int GetRemainingCapacity();
}
