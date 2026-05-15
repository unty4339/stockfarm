using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 資金の管理と資源売却を担うシングルトン
/// </summary>
public class EconomyManager : MonoBehaviour
{
    private static EconomyManager _instance;
    /// <summary>シングルトン参照</summary>
    public static EconomyManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("EconomyManager");
                _instance = go.AddComponent<EconomyManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    /// <summary>現在の資金量</summary>
    public int CurrentFunds { get; private set; } = 2000;

    /// <summary>資金が変化したときに発火（引数: 変化後の残高）</summary>
    public event Action<int> OnFundsChanged;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 資金を加算する
    /// </summary>
    /// <param name="amount">加算量</param>
    public void AddFunds(int amount)
    {
        CurrentFunds += amount;
        OnFundsChanged?.Invoke(CurrentFunds);
    }

    /// <summary>
    /// 資金を消費する（残高不足の場合はfalseを返す）
    /// </summary>
    /// <param name="amount">消費量</param>
    /// <returns>消費成功でtrue</returns>
    public bool TrySpendFunds(int amount)
    {
        if (CurrentFunds < amount) return false;
        CurrentFunds -= amount;
        OnFundsChanged?.Invoke(CurrentFunds);
        return true;
    }

    /// <summary>
    /// 指定リソースリストを売却し、資金に換算する
    /// </summary>
    /// <param name="resources">売却するリソースリスト</param>
    public void SellResources(List<ResourceBase> resources)
    {
        int total = 0;
        foreach (var r in resources)
            total += GetSellPrice(r);
        AddFunds(total);
    }

    /// <summary>
    /// リソースの売却価格を返す
    /// 瓶牛乳: 40×(Quality/100), バター: 80×(Quality/100), チーズ: 120×(Quality/100)
    /// </summary>
    /// <param name="resource">売却するリソース</param>
    /// <returns>売却価格</returns>
    public int GetSellPrice(ResourceBase resource)
    {
        float quality = resource is LiquidResource liq ? liq.Quality : 100f;
        float factor = quality / 100f;

        return resource.Type switch
        {
            ResourceType.BottledMilk => Mathf.RoundToInt(40f * factor) * resource.Amount,
            ResourceType.Butter => Mathf.RoundToInt(80f * factor) * resource.Amount,
            ResourceType.Cheese => Mathf.RoundToInt(120f * factor) * resource.Amount,
            ResourceType.Milk => Mathf.RoundToInt(10f * factor) * resource.Amount,
            _ => 0,
        };
    }
}
