using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 部屋の検出結果を保持し、ワーカーへのムード値提供を担うシングルトン
/// </summary>
public class RoomManager : MonoBehaviour
{
    private static RoomManager _instance;
    /// <summary>シングルトン参照</summary>
    public static RoomManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("RoomManager");
                _instance = go.AddComponent<RoomManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }

    /// <summary>現在検出されている全部屋のリスト</summary>
    public List<RoomData> Rooms { get; private set; } = new List<RoomData>();

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
    /// グリッド全体を再走査して部屋リストを更新する
    /// BuildingManagerの設備変更後に呼ぶ
    /// </summary>
    public void RefreshRooms()
    {
        if (MapManager.Instance == null) return;

        Rooms = RoomDetector.DetectRooms(MapManager.Instance.Tiles);

        // 各部屋に設備を紐づけ
        foreach (var room in Rooms)
        {
            room.ContainedEquipments.Clear();
            room.WasteMarkers.Clear();

            foreach (var pos in room.TilePositions)
            {
                var tile = MapManager.Instance.GetTileOrNull(pos);
                if (tile?.PlacedEquipment != null)
                    room.ContainedEquipments.Add(tile.PlacedEquipment);

                if (tile?.WasteMarker != null && !tile.WasteMarker.IsGone)
                    room.WasteMarkers.Add(tile.WasteMarker);
            }

            room.CurrentMood = MoodCalculator.Calculate(room);
        }
    }

    /// <summary>
    /// 指定座標が属する部屋のMoodDataを返す（部屋なしの場合はデフォルト値）
    /// </summary>
    /// <param name="position">確認する座標</param>
    /// <returns>ムードデータ</returns>
    public MoodData GetMoodAt(Vector2Int position)
    {
        var room = RoomDetector.GetRoomAt(position, Rooms);
        return room?.CurrentMood ?? new MoodData();
    }

    /// <summary>
    /// 指定座標が何らかの部屋に属しているかを返す
    /// </summary>
    /// <param name="position">確認する座標</param>
    /// <returns>部屋内の場合true</returns>
    public bool IsInsideRoom(Vector2Int position)
    {
        return RoomDetector.GetRoomAt(position, Rooms) != null;
    }
}
