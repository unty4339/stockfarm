using UnityEngine;

/// <summary>
/// 液体リソース（牛乳）を表すクラス。品質値を持つ
/// </summary>
public class LiquidResource : ResourceBase
{
    /// <summary>品質（0〜100）</summary>
    public float Quality { get; private set; }

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="amount">数量</param>
    /// <param name="quality">品質（0〜100）</param>
    public LiquidResource(int amount, float quality)
    {
        Type = ResourceType.Milk;
        Amount = amount;
        Quality = Mathf.Clamp(quality, 0f, 100f);
    }

    /// <summary>
    /// 別の液体リソースと混合する
    /// 混合後の品質は加重平均、数量は合算される
    /// </summary>
    /// <param name="other">混合する液体リソース</param>
    public void Mix(LiquidResource other)
    {
        if (other == null || other.Amount <= 0) return;

        int totalAmount = Amount + other.Amount;
        float newQuality = (Amount * Quality + other.Amount * other.Quality) / totalAmount;
        Amount = totalAmount;
        Quality = Mathf.Clamp(newQuality, 0f, 100f);
    }
}
