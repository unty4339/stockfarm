/// <summary>
/// 固体リソースを表すクラス（栄養ペレット、チーズ、バター、瓶牛乳）
/// </summary>
public class SolidResource : ResourceBase
{
    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="type">リソース種別（固体リソースのみ指定可）</param>
    /// <param name="amount">数量</param>
    public SolidResource(ResourceType type, int amount)
    {
        Type = type;
        Amount = amount;
    }
}
