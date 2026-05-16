using UnityEngine;

/// <summary>
/// 普通のベッド。サイズ1×2、睡眠回復効率100%
/// </summary>
public class NormalBed : CowBed
{
    public override EquipmentType Type => EquipmentType.NormalBed;
    public override Vector2Int Size => new Vector2Int(1, 2);
    public override float StaminaRecoveryMultiplier => 1.0f;
    protected override Color EquipmentColor => new Color(0.55f, 0.72f, 0.88f);
}
