using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 画面下端のメインメニューバー。各サブメニューを排他表示で管理する
/// </summary>
public class MenuBarUI : MonoBehaviour
{
    /// <summary>指令メニューの参照</summary>
    public CommandMenuUI CommandMenuUI { get; private set; }
    /// <summary>優先順位メニューの参照</summary>
    public PriorityMenuUI PriorityMenuUI { get; private set; }
    /// <summary>スケジュールメニューの参照</summary>
    public ScheduleMenuUI ScheduleMenuUI { get; private set; }
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

        CreateMenuButton(bar.transform, "指令", new Vector2(-480, 0), ShowCommandMenu);
        CreateMenuButton(bar.transform, "優先順位", new Vector2(-240, 0), ShowPriorityMenu);
        CreateMenuButton(bar.transform, "スケジュール", new Vector2(0, 0), ShowScheduleMenu);
        CreateMenuButton(bar.transform, "施設", new Vector2(240, 0), ShowFacilityMenu);
        CreateMenuButton(bar.transform, "ゾーン設定", new Vector2(480, 0), ShowZoneMenu);

        CommandMenuUI = gameObject.AddComponent<CommandMenuUI>();
        PriorityMenuUI = gameObject.AddComponent<PriorityMenuUI>();
        ScheduleMenuUI = gameObject.AddComponent<ScheduleMenuUI>();
        FacilityMenuUI = gameObject.AddComponent<FacilityMenuUI>();
        ZoneMenuUI = gameObject.AddComponent<ZoneMenuUI>();
        gameObject.AddComponent<ZonePlacementModeUI>();
        gameObject.AddComponent<DemolishModeUI>();
    }

    /// <summary>
    /// 指令メニューをトグルする（他メニューは非表示にする）
    /// </summary>
    public void ShowCommandMenu()
    {
        bool wasVisible = CommandMenuUI?.IsVisible ?? false;
        HideAll();
        if (!wasVisible) CommandMenuUI?.Show();
    }

    /// <summary>
    /// 優先順位メニューをトグルする（他メニューは非表示にする）
    /// </summary>
    public void ShowPriorityMenu()
    {
        bool wasVisible = PriorityMenuUI?.IsVisible ?? false;
        HideAll();
        if (!wasVisible) PriorityMenuUI?.Show();
    }

    /// <summary>
    /// スケジュールメニューをトグルする（他メニューは非表示にする）
    /// </summary>
    public void ShowScheduleMenu()
    {
        bool wasVisible = ScheduleMenuUI?.IsVisible ?? false;
        HideAll();
        if (!wasVisible) ScheduleMenuUI?.Show();
    }

    /// <summary>
    /// 施設種別メニューをトグルする（他メニューは非表示にする）
    /// </summary>
    public void ShowFacilityMenu()
    {
        bool wasVisible = FacilityMenuUI?.IsVisible ?? false;
        HideAll();
        if (!wasVisible) FacilityMenuUI?.Show();
    }

    /// <summary>
    /// ゾーン設定メニューをトグルする（他メニューは非表示にする）
    /// </summary>
    public void ShowZoneMenu()
    {
        bool wasVisible = ZoneMenuUI?.IsVisible ?? false;
        HideAll();
        if (!wasVisible) ZoneMenuUI?.Show();
    }

    /// <summary>
    /// 全サブメニューを非表示にする
    /// </summary>
    public void HideAll()
    {
        CommandMenuUI?.Hide();
        PriorityMenuUI?.Hide();
        ScheduleMenuUI?.Hide();
        FacilityMenuUI?.Hide();
        ZoneMenuUI?.Hide();
    }

    private GameObject CreateBarPanel()
    {
        var go = new GameObject("MenuBar");
        go.transform.SetParent(transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.zero;
        rt.pivot = Vector2.zero;
        rt.sizeDelta = new Vector2(1200, 100);
        rt.anchoredPosition = Vector2.zero;
        var img = go.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.7f);
        return go;
    }

    private void CreateMenuButton(Transform parent, string label, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        UIHelper.CreateButton(parent, label, pos, 200, 80, onClick, 24);
    }
}
