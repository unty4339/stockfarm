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

    private void Start()
    {
        BuildMenuBar();
    }

    private void BuildMenuBar()
    {
        var bar = CreateBarPanel();

        CreateMenuButton(bar.transform, "指令", new Vector2(-120, 0), ShowCommandMenu);
        CreateMenuButton(bar.transform, "優先順位", new Vector2(0, 0), ShowPriorityMenu);
        CreateMenuButton(bar.transform, "施設", new Vector2(120, 0), ShowFacilityMenu);

        CommandMenuUI = gameObject.AddComponent<CommandMenuUI>();
        PriorityMenuUI = gameObject.AddComponent<PriorityMenuUI>();
        FacilityMenuUI = gameObject.AddComponent<FacilityMenuUI>();
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
    /// 全サブメニューを非表示にする
    /// </summary>
    public void HideAll()
    {
        CommandMenuUI?.Hide();
        PriorityMenuUI?.Hide();
        FacilityMenuUI?.Hide();
    }

    private GameObject CreateBarPanel()
    {
        var go = new GameObject("MenuBar");
        go.transform.SetParent(transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(400, 50);
        rt.anchoredPosition = new Vector2(0, 30);
        var img = go.AddComponent<Image>();
        img.color = new Color(0f, 0f, 0f, 0.7f);
        return go;
    }

    private void CreateMenuButton(Transform parent, string label, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(label + "Btn");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(100, 40);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.25f, 0.25f, 0.25f, 1f);
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        var textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        var textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.offsetMin = Vector2.zero;
        textRt.offsetMax = Vector2.zero;
        var tmp = textGo.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 13;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = Color.white;
    }
}
