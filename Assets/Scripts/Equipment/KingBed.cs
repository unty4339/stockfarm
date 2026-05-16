using UnityEngine;

/// <summary>
/// キングベッド。サイズ3×3、睡眠回復効率110%
/// </summary>
public class KingBed : CowBed
{
    public override EquipmentType Type => EquipmentType.KingBed;
    public override Vector2Int Size => new Vector2Int(3, 3);
    public override float StaminaRecoveryMultiplier => 1.1f;
    protected override Color EquipmentColor => new Color(0.85f, 0.75f, 0.35f);
}
