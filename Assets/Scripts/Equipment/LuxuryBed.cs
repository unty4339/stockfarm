using UnityEngine;

/// <summary>
/// 贅沢ベッド。サイズ2×2、睡眠回復効率110%
/// </summary>
public class LuxuryBed : CowBed
{
    public override EquipmentType Type => EquipmentType.LuxuryBed;
    public override Vector2Int Size => new Vector2Int(2, 2);
    public override float StaminaRecoveryMultiplier => 1.1f;
    protected override Color EquipmentColor => new Color(0.65f, 0.45f, 0.85f);
}
