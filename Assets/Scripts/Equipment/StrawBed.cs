using UnityEngine;

/// <summary>
/// 藁のベッド。サイズ1×2、睡眠回復効率90%
/// </summary>
public class StrawBed : CowBed
{
    public override EquipmentType Type => EquipmentType.StrawBed;
    public override Vector2Int Size => new Vector2Int(1, 2);
    public override float StaminaRecoveryMultiplier => 0.9f;
    protected override Color EquipmentColor => new Color(0.75f, 0.65f, 0.35f);
}
