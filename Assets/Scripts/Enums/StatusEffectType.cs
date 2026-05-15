/// <summary>
/// ワーカーに付与される状態異常の種別（複数同時に重複可能）
/// </summary>
public enum StatusEffectType
{
    /// <summary>疲労（スタミナ0で付与）</summary>
    Fatigue,
    /// <summary>栄養失調（空腹状態が継続で付与）</summary>
    Malnutrition,
    /// <summary>空腹（Hungerが閾値以上で付与）</summary>
    Hungry,
    /// <summary>妊娠前期（種付け成功から一定tick）牛専用</summary>
    PregnancyEarly,
    /// <summary>妊娠後期（前期経過後）牛専用</summary>
    PregnancyLate,
    /// <summary>泌乳期（出産後）牛専用</summary>
    Lactating,
    /// <summary>発情期（周期的に発生）牛専用</summary>
    Estrus,
}
