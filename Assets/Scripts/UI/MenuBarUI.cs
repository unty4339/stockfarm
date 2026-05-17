using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 画面下端のメインメニューバー。3つのサブメニューを排他表示で管理する
/// </summary>
public class MenuBarUI : MonoBehaviour
{
    /// <summary>指令メニューの参照</summary>
    public CommandMenuUI CommandMenuUI { get; private set; }
    /// <summary>優先順位メニューの参照</summary>
    public PriorityMenuUI PriorityMenuUI { get; private set; }
    /// <summary>施設種別メニューの参照</summary>
    public FacilityMenuUI FacilityMenuUI { get; private set; }
    /// <summary>ゾーン設定メニューの参照</summary>
    public ZoneMenuUI ZoneMenuUI { get; private set; }

    private void Start()
    {
        BuildMenuBar();
    }

    private void BuildMenuBar()
    {
        var bar = CreateBarPanel();

        CreateMenuButton(bar.transform, "指令", new Vector2(-360, 0), ShowCommandMenu);
        CreateMenuButton(bar.transform, "優先順位", new Vector2(-120, 0), ShowPriorityMenu);
        CreateMenuButton(bar.transform, "施設", new Vector2(120, 0), ShowFacilityMenu);
        CreateMenuButton(bar.transform, "ゾーン設定", new Vector2(360, 0), ShowZoneMenu);

        CommandMenuUI = gameObject.AddComponent<CommandMenuUI>();
        PriorityMenuUI = gameObject.AddComponent<PriorityMenuUI>();
        FacilityMenuUI = gameObject.AddComponent<FacilityMenuUI>();
        ZoneMenuUI = gameObject.AddComponent<ZoneMenuUI>();
        gameObject.AddComponent<ZonePlacementModeUI>();
        gameObject.AddComponent<ZonePopupUI>();
    }

    /// <summary>
    /// 指令メニューを表示する（他メニューは非表示にする）
    /// </summary>
    public void ShowCommandMenu()
    {
        HideAll();
        CommandMenuUI?.Show();
    }

    /// <summary>
    /// 優先順位メニューを表示する（他メニューは非表示にする）
    /// </summary>
    public void ShowPriorityMenu()
    {
        HideAll();
        PriorityMenuUI?.Show();
    }

    /// <summary>
    /// 施設種別メニューを表示する（他メニューは非表示にする）
    /// </summary>
    public void ShowFacilityMenu()
    {
        HideAll();
        FacilityMenuUI?.Show();
    }

    /// <summary>
    /// ゾーン設定メニューを表示する（他メニューは非表示にする）
    /// </summary>
    public void ShowZoneMenu()
    {
        HideAll();
        ZoneMenuUI?.Show();
    }

    /// <summary>
    /// 全サブメニューを非表示にする
    /// </summary>
    public void HideAll()
    {
        CommandMenuUI?.Hide();
        PriorityMenuUI?.Hide();
        FacilityMenuUI?.Hide();
        ZoneMenuUI?.Hide();
    }

    private GameObject CreateBarPanel()
    {
        var go = new GameObject("MenuBar");
        go.transform.SetParent(transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(1040, 100);
        rt.anchoredPosition = new Vector2(0, 60);
        var img = go.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.7f);
        return go;
    }

    private void CreateMenuButton(Transform parent, string label, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        UIHelper.CreateButton(parent, label, pos, 200, 80, onClick, 24);
    }
}
