using UnityEngine;

/// <summary>
/// タイル上に配置される1スロット分のアイテムを表すデータクラス（MonoBehaviourではない）
/// スタック可否・スタック上限はResourceTypeによって静的に決まる
/// </summary>
public class TileItem
{
    /// <summary>アイテム種別</summary>
    public ResourceType Type { get; }

    /// <summary>現在のスタック数（非スタックアイテムは常に1）</summary>
    public int StackCount { get; private set; }

    /// <summary>品質（非スタックの品質ありアイテムのみ非null）</summary>
    public float? Quality { get; }

    /// <summary>スタック可能かどうか</summary>
    public bool IsStackable => GetStackLimit(Type) > 0;

    /// <summary>スタック上限（非スタックアイテムは0）</summary>
    public int StackLimit => GetStackLimit(Type);

    /// <summary>スタックが上限に達しているか</summary>
    public bool IsStackFull => IsStackable && StackCount >= StackLimit;

    /// <summary>
    /// 品質あり（非スタック）アイテム用コンストラクタ
    /// </summary>
    /// <param name="type">アイテム種別</param>
    /// <param name="quality">品質（0〜100）</param>
    public TileItem(ResourceType type, float quality)
    {
        Type = type;
        StackCount = 1;
        Quality = quality;
    }

    /// <summary>
    /// スタック可能アイテム用コンストラクタ
    /// </summary>
    /// <param name="type">アイテム種別</param>
    /// <param name="stackCount">初期スタック数</param>
    public TileItem(ResourceType type, int stackCount = 1)
    {
        Type = type;
        StackCount = Mathf.Max(1, stackCount);
    }

    /// <summary>
    /// 指定数をスタックに追加する
    /// </summary>
    /// <param name="count">追加したい数</param>
    /// <returns>追加できなかった余剰数</returns>
    public int TryAddStack(int count)
    {
        if (!IsStackable) return count;
        int canAdd = StackLimit - StackCount;
        int added = Mathf.Min(canAdd, count);
        StackCount += added;
        return count - added;
    }

    /// <summary>
    /// 指定数をスタックから取り出す（非スタックアイテムは常に0を返す）
    /// </summary>
    /// <param name="amount">取り出したい数</param>
    /// <returns>実際に取り出した数</returns>
    public int TryTake(int amount)
    {
        if (!IsStackable) return 0;
        int taken = Mathf.Min(amount, StackCount);
        StackCount -= taken;
        return taken;
    }

    /// <summary>
    /// ResourceTypeごとのスタック上限を返す（0の場合はスタック不可）
    /// </summary>
    public static int GetStackLimit(ResourceType type) => type switch
    {
        ResourceType.Wheat => 50,
        _ => 0,
    };
}
