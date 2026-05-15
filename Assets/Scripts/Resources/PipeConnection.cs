using System.Collections.Generic;

/// <summary>
/// 液体リソースを一方向に輸送するパイプの接続情報
/// </summary>
public class PipeConnection
{
    /// <summary>送り元設備</summary>
    public EquipmentBase Source { get; }
    /// <summary>送り先設備</summary>
    public EquipmentBase Destination { get; }
    /// <summary>パイプの色分け識別</summary>
    public PipeColor Color { get; }
    /// <summary>一方通行フラグ（常にtrue）</summary>
    public bool IsOneWay => true;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="source">送り元設備</param>
    /// <param name="destination">送り先設備</param>
    /// <param name="color">パイプ色</param>
    public PipeConnection(EquipmentBase source, EquipmentBase destination, PipeColor color)
    {
        Source = source;
        Destination = destination;
        Color = color;
    }
}

/// <summary>
/// 液体を複数の出力先に分配するクラス
/// </summary>
public class PipeDistributor
{
    /// <summary>出力先のパイプ接続リスト</summary>
    public List<PipeConnection> Outputs { get; } = new List<PipeConnection>();

    /// <summary>
    /// 入力液体リソースを各出力先に均等分配する
    /// </summary>
    /// <param name="resource">分配する液体リソース</param>
    public void Distribute(LiquidResource resource)
    {
        if (Outputs.Count == 0 || resource == null || resource.Amount <= 0) return;

        int perOutput = resource.Amount / Outputs.Count;
        if (perOutput <= 0) return;

        foreach (var pipe in Outputs)
        {
            if (pipe.Destination is IContainer container)
            {
                var portion = new LiquidResource(perOutput, resource.Quality);
                container.TryStore(portion);
            }
        }
    }
}
