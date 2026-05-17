/// <summary>
/// 設備の種別
/// </summary>
public enum EquipmentType
{
    // 建築・構造
    /// <summary>壁</summary>
    Wall,
    /// <summary>床</summary>
    Floor,
    // 柵・境界
    /// <summary>柵</summary>
    Fence,
    /// <summary>ゲート</summary>
    Gate,
    // 給餌・リソース
    /// <summary>給餌桶</summary>
    FeedingTrough,
    /// <summary>食料棚</summary>
    FoodShelf,
    /// <summary>チェスト</summary>
    Chest,
    // 生産・加工
    /// <summary>手動搾乳スタンド</summary>
    ManualMilkingStand,
    /// <summary>自動搾乳スタンド</summary>
    AutoMilkingStand,
    /// <summary>足踏み式攪拌機</summary>
    Churn,
    /// <summary>圧搾プレス機</summary>
    CheesePress,
    /// <summary>ボトル詰め機</summary>
    BottleFiller,
    // 自動化・補助
    /// <summary>自動ブラシ機</summary>
    AutoBrush,
    /// <summary>種付け施設</summary>
    BreedingFacility,
    /// <summary>運搬用パレット</summary>
    TransportPallet,
    /// <summary>藁のベッド</summary>
    StrawBed,
    /// <summary>普通のベッド</summary>
    NormalBed,
    /// <summary>贅沢ベッド</summary>
    LuxuryBed,
    /// <summary>キングベッド</summary>
    KingBed,
    /// <summary>資源置き場（売却ポイント）</summary>
    SellPoint,
    /// <summary>納品ボックス（売却フラグ付きアイテムの集積地点）</summary>
    DeliveryBox,
}
