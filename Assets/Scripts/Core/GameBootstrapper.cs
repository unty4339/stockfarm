using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Play 開始時にゲーム全体を初期化するブートストラップ
/// </summary>
public class GameBootstrapper : MonoBehaviour
{
    private void Start()
    {
        SetupCropManager();
        SetupItemVisualManager();
        SetupStorageManager();
        PlaceInitialEquipments();
        SpawnInitialWorkers();
        SetupAgricultureZone();
    }

    /// <summary>
    /// CropManagerをシーンに生成する
    /// </summary>
    private void SetupCropManager()
    {
        new GameObject("CropManager").AddComponent<CropManager>();
    }

    /// <summary>
    /// ItemVisualManagerをシーンに生成する
    /// </summary>
    private void SetupItemVisualManager()
    {
        new GameObject("ItemVisualManager").AddComponent<ItemVisualManager>();
    }

    /// <summary>
    /// StorageManagerをシーンに生成する
    /// </summary>
    private void SetupStorageManager()
    {
        new GameObject("StorageManager").AddComponent<StorageManager>();
    }

    /// <summary>
    /// 牧場主の初期位置付近に農業ゾーンを作成する
    /// </summary>
    private void SetupAgricultureZone()
    {
        var positions = new List<Vector2Int>();
        for (int x = 17; x <= 20; x++)
            for (int y = 13; y <= 16; y++)
                positions.Add(new Vector2Int(x, y));

        ZoneManager.Instance.CreateZone(ZoneType.Agriculture, positions);
    }

    /// <summary>
    /// 初期設備（給餌桶×2、チェスト×2、ベッド×2）をコスト無しで配置する
    /// </summary>
    private void PlaceInitialEquipments()
    {
        BuildingManager.Instance.PlaceEquipmentFree(EquipmentType.FeedingTrough, new Vector2Int(5, 5));
        BuildingManager.Instance.PlaceEquipmentFree(EquipmentType.FeedingTrough, new Vector2Int(7, 5));
        BuildingManager.Instance.PlaceEquipmentFree(EquipmentType.Chest, new Vector2Int(5, 7));
        BuildingManager.Instance.PlaceEquipmentFree(EquipmentType.Chest, new Vector2Int(7, 7));
        BuildingManager.Instance.PlaceEquipmentFree(EquipmentType.NormalBed, new Vector2Int(10, 5));
        BuildingManager.Instance.PlaceEquipmentFree(EquipmentType.NormalBed, new Vector2Int(10, 7));

        RoomManager.Instance.RefreshRooms();
    }

    /// <summary>
    /// 初期ワーカー（成牛×2・牧場主×1）をスポーンしベッドをアサインする
    /// </summary>
    private void SpawnInitialWorkers()
    {
        var cow1 = SpawnCow("ハナ", new Vector2Int(5, 10));
        var cow2 = SpawnCow("モモ", new Vector2Int(7, 10));

        AssignBedToCow(cow1, new Vector2Int(10, 5));
        AssignBedToCow(cow2, new Vector2Int(10, 7));

        SpawnFarmer(new Vector2Int(15, 15));
    }

    /// <summary>
    /// 指定座標に成牛ワーカーをスポーンする
    /// </summary>
    /// <param name="cowName">牛の名前</param>
    /// <param name="gridPos">初期グリッド座標</param>
    /// <returns>生成した CowWorker</returns>
    private CowWorker SpawnCow(string cowName, Vector2Int gridPos)
    {
        var go = new GameObject($"Cow_{cowName}");
        var cow = go.AddComponent<CowWorker>();
        cow.CowName = cowName;
        cow.GridPosition = gridPos;
        cow.LifecycleStage = CowLifecycleStage.AdultCow;
        return cow;
    }

    /// <summary>
    /// 指定座標に牧場主をスポーンする
    /// </summary>
    /// <param name="gridPos">初期グリッド座標</param>
    private void SpawnFarmer(Vector2Int gridPos)
    {
        var go = new GameObject("Farmer");
        var farmer = go.AddComponent<FarmerWorker>();
        farmer.GridPosition = gridPos;
    }

    /// <summary>
    /// 牛に指定座標のベッドをアサインする
    /// </summary>
    /// <param name="cow">対象の牛ワーカー</param>
    /// <param name="bedPos">ベッドのグリッド座標</param>
    private void AssignBedToCow(CowWorker cow, Vector2Int bedPos)
    {
        var equipment = BuildingManager.Instance.GetEquipment(bedPos);
        if (equipment is CowBed bed)
        {
            bed.Assign(cow);
            cow.AssignedBed = bed;
        }
    }
}
