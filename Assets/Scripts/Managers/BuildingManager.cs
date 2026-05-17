using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 設備の設置・撤去を一元管理するシングルトン
/// </summary>
public class BuildingManager : MonoBehaviour
{
    private static BuildingManager _instance;
    /// <summary>シングルトン参照</summary>
    public static BuildingManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("BuildingManager");
                _instance = go.AddComponent<BuildingManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    private readonly List<EquipmentBase> _allEquipments = new List<EquipmentBase>();

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
    /// 指定設備を設置する
    /// EconomyManager.TrySpendFunds()で資金消費し、成功した場合のみ設置する
    /// </summary>
    /// <param name="type">設備種別</param>
    /// <param name="position">設置グリッド座標</param>
    /// <param name="rotation">配置回転角度（0/90/180/270度）</param>
    /// <returns>設置成功でtrue</returns>
    public bool PlaceEquipment(EquipmentType type, Vector2Int position, int rotation = 0)
    {
        var prefab = CreateEquipmentObject(type);
        if (prefab == null) return false;

        if (!EconomyManager.Instance.TrySpendFunds(prefab.BuildCost))
        {
            Destroy(prefab.gameObject);
            return false;
        }

        prefab.Place(position, rotation);
        _allEquipments.Add(prefab);
        return true;
    }

    /// <summary>
    /// 資金消費なしで設備を強制設置する（初期配置用）
    /// </summary>
    /// <param name="type">設備種別</param>
    /// <param name="position">設置グリッド座標</param>
    /// <param name="rotation">配置回転角度（0/90/180/270度）</param>
    /// <returns>設置した設備</returns>
    public EquipmentBase PlaceEquipmentFree(EquipmentType type, Vector2Int position, int rotation = 0)
    {
        var eq = CreateEquipmentObject(type);
        if (eq == null) return null;
        eq.Place(position, rotation);
        _allEquipments.Add(eq);
        return eq;
    }

    /// <summary>
    /// 指定座標の設備を撤去する（50%返金）
    /// </summary>
    /// <param name="position">撤去するグリッド座標</param>
    public void RemoveEquipment(Vector2Int position)
    {
        var eq = GetEquipment(position);
        if (eq == null) return;
        _allEquipments.Remove(eq);
        eq.Remove();
    }

    /// <summary>
    /// 指定座標に設置されている設備を返す（なければnull）
    /// </summary>
    /// <param name="position">検索するグリッド座標</param>
    /// <returns>設備またはnull</returns>
    public EquipmentBase GetEquipment(Vector2Int position)
    {
        if (MapManager.Instance == null) return null;
        var tile = MapManager.Instance.GetTileOrNull(position);
        return tile?.PlacedEquipment;
    }

    /// <summary>
    /// 指定部屋内に含まれる全設備を返す
    /// </summary>
    /// <param name="room">対象の部屋</param>
    /// <returns>設備リスト</returns>
    public List<EquipmentBase> GetEquipmentsInRoom(RoomData room)
    {
        return room.ContainedEquipments;
    }

    /// <summary>
    /// 全設置済み設備を返す
    /// </summary>
    /// <returns>設備リスト</returns>
    public List<EquipmentBase> GetAllEquipments()
    {
        _allEquipments.RemoveAll(e => e == null);
        return new List<EquipmentBase>(_allEquipments);
    }

    /// <summary>
    /// 設備種別からGridサイズを返す（GhostPlacer用）
    /// </summary>
    /// <param name="type">設備種別</param>
    /// <returns>占有グリッドサイズ</returns>
    public static Vector2Int GetEquipmentSize(EquipmentType type)
    {
        return type switch
        {
            EquipmentType.AutoMilkingStand => new Vector2Int(2, 2),
            EquipmentType.BreedingFacility => new Vector2Int(2, 2),
            EquipmentType.DeliveryBox => new Vector2Int(2, 2),
            EquipmentType.CheesePress => new Vector2Int(2, 1),
            EquipmentType.SellPoint => new Vector2Int(2, 1),
            EquipmentType.StrawBed or EquipmentType.NormalBed => new Vector2Int(1, 2),
            EquipmentType.LuxuryBed => new Vector2Int(2, 2),
            EquipmentType.KingBed => new Vector2Int(3, 3),
            _ => Vector2Int.one,
        };
    }

    /// <summary>
    /// 回転を適用したGridサイズを返す（90°/270°時はx/yを入れ替える）
    /// </summary>
    /// <param name="type">設備種別</param>
    /// <param name="rotation">回転角度（0/90/180/270度）</param>
    /// <returns>回転後の占有グリッドサイズ</returns>
    public static Vector2Int GetEffectiveSize(EquipmentType type, int rotation)
    {
        var size = GetEquipmentSize(type);
        return (rotation == 90 || rotation == 270) ? new Vector2Int(size.y, size.x) : size;
    }

    /// <summary>
    /// 設備が回転に対応しているかを返す（非正方形サイズの設備のみ対応）
    /// </summary>
    /// <param name="type">設備種別</param>
    /// <returns>回転対応であればtrue</returns>
    public static bool SupportsRotation(EquipmentType type)
    {
        var size = GetEquipmentSize(type);
        return size.x != size.y;
    }

    private EquipmentBase CreateEquipmentObject(EquipmentType type)
    {
        var go = new GameObject(type.ToString());
        return type switch
        {
            EquipmentType.Wall => go.AddComponent<Wall>(),
            EquipmentType.Floor => go.AddComponent<Floor>(),
            EquipmentType.Fence => go.AddComponent<Fence>(),
            EquipmentType.Gate => go.AddComponent<Gate>(),
            EquipmentType.FeedingTrough => go.AddComponent<FeedingTrough>(),
            EquipmentType.FoodShelf => go.AddComponent<FoodShelf>(),
            EquipmentType.Chest => go.AddComponent<Chest>(),
            EquipmentType.ManualMilkingStand => go.AddComponent<ManualMilkingStand>(),
            EquipmentType.AutoMilkingStand => go.AddComponent<AutoMilkingStand>(),
            EquipmentType.Churn => go.AddComponent<Churn>(),
            EquipmentType.CheesePress => go.AddComponent<CheesePress>(),
            EquipmentType.BottleFiller => go.AddComponent<BottleFiller>(),
            EquipmentType.AutoBrush => go.AddComponent<AutoBrush>(),
            EquipmentType.BreedingFacility => go.AddComponent<BreedingFacility>(),
            EquipmentType.TransportPallet => go.AddComponent<TransportPallet>(),
            EquipmentType.StrawBed => go.AddComponent<StrawBed>(),
            EquipmentType.NormalBed => go.AddComponent<NormalBed>(),
            EquipmentType.LuxuryBed => go.AddComponent<LuxuryBed>(),
            EquipmentType.KingBed => go.AddComponent<KingBed>(),
            EquipmentType.SellPoint => go.AddComponent<SellPoint>(),
            EquipmentType.DeliveryBox => go.AddComponent<DeliveryBox>(),
            _ => null,
        };
    }
}
