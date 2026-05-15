#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// エディタメニュー "StockFarm/Setup Scene" からシーンの初期セットアップを行う
/// 冪等: 既に配置済みのオブジェクトは再生成しない
/// </summary>
public static class SceneSetup
{
    /// <summary>
    /// シーンにゲームに必要な全オブジェクトを配置または更新する
    /// </summary>
    [MenuItem("StockFarm/Setup Scene")]
    public static void SetupScene()
    {
        SetupManagers();
        SetupCamera();
        SetupCanvas();

        Debug.Log("[SceneSetup] シーンセットアップが完了しました");
    }

    /// <summary>
    /// 各マネージャー用 GameObject を配置する
    /// </summary>
    private static void SetupManagers()
    {
        EnsureComponent<GameBootstrapper>("GameBootstrapper");
        EnsureComponent<GameTimeManager>("GameTimeManager");
        EnsureComponent<MapManager>("MapManager");
        EnsureComponent<BuildingManager>("BuildingManager");
        EnsureComponent<RoomManager>("RoomManager");
        EnsureComponent<EconomyManager>("EconomyManager");
    }

    /// <summary>
    /// メインカメラを設定する。既存カメラがあれば設定のみ変更する
    /// </summary>
    private static void SetupCamera()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            cam = go.AddComponent<Camera>();
            go.AddComponent<AudioListener>();
        }

        cam.orthographic = true;
        cam.orthographicSize = 17f;
        cam.transform.position = new Vector3(15f, 15f, -10f);
        cam.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
        EditorUtility.SetDirty(cam.gameObject);
    }

    /// <summary>
    /// UI Canvas と各 UI コンポーネントを配置する
    /// </summary>
    private static void SetupCanvas()
    {
        var canvasObjects = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        Canvas canvas = null;
        foreach (var c in canvasObjects)
        {
            if (c.name == "UICanvas")
            {
                canvas = c;
                break;
            }
        }

        if (canvas == null)
        {
            var canvasGo = new GameObject("UICanvas");
            canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGo.AddComponent<CanvasScaler>();
            canvasGo.AddComponent<GraphicRaycaster>();
        }

        EnsureChildComponent<MenuBarUI>(canvas.transform, "MenuBarUI");
        EnsureChildComponent<TimeDisplayUI>(canvas.transform, "TimeDisplayUI");
        EnsureChildComponent<CommandMenuUI>(canvas.transform, "CommandMenuUI");
        EnsureChildComponent<PriorityMenuUI>(canvas.transform, "PriorityMenuUI");
        EnsureChildComponent<FacilityMenuUI>(canvas.transform, "FacilityMenuUI");
        EnsureChildComponent<FacilityPopupUI>(canvas.transform, "FacilityPopupUI");
        EnsureChildComponent<WorkerPopupUI>(canvas.transform, "WorkerPopupUI");
        EnsureChildComponent<BuildModeUI>(canvas.transform, "BuildModeUI");

        EditorUtility.SetDirty(canvas.gameObject);
    }

    /// <summary>
    /// 指定名の GameObject が存在しなければ生成してコンポーネントを追加する
    /// </summary>
    /// <typeparam name="T">追加するコンポーネント型</typeparam>
    /// <param name="goName">GameObject 名</param>
    /// <returns>既存または新規作成した GameObject</returns>
    private static GameObject EnsureComponent<T>(string goName) where T : Component
    {
        var existing = GameObject.Find(goName);
        if (existing != null)
        {
            if (existing.GetComponent<T>() == null)
                existing.gameObject.AddComponent<T>();
            return existing;
        }

        var go = new GameObject(goName);
        go.AddComponent<T>();
        Undo.RegisterCreatedObjectUndo(go, $"Create {goName}");
        return go;
    }

    /// <summary>
    /// 指定 Transform の子に指定名の GameObject が存在しなければ生成してコンポーネントを追加する
    /// </summary>
    /// <typeparam name="T">追加するコンポーネント型</typeparam>
    /// <param name="parent">親 Transform</param>
    /// <param name="goName">GameObject 名</param>
    /// <returns>既存または新規作成した GameObject</returns>
    private static GameObject EnsureChildComponent<T>(Transform parent, string goName) where T : Component
    {
        var existing = parent.Find(goName);
        if (existing != null)
        {
            if (existing.GetComponent<T>() == null)
                existing.gameObject.AddComponent<T>();
            return existing.gameObject;
        }

        var go = new GameObject(goName);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        go.AddComponent<T>();
        Undo.RegisterCreatedObjectUndo(go, $"Create {goName}");
        return go;
    }
}
#endif
