/// <summary>
/// 全リソースの基底クラス（MonoBehaviourではないデータオブジェクト）
/// </summary>
public abstract class ResourceBase
{
    /// <summary>リソース種別</summary>
    public ResourceType Type { get; protected set; }
    /// <summary>数量</summary>
    public int Amount { get; set; }
}
