using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// UI部品の生成を補助する静的ユーティリティ
/// フォントは初回 CreateText 呼び出し時に Addressables から自動キャッシュされる
/// </summary>
public static class UIHelper
{
    private const string FontAddress = "DefaultUIFont.asset";
    private static TMP_FontAsset _font;

    /// <summary>
    /// 現在のマウス位置が UI 要素上かどうかを EventSystem のレイキャストで判定する
    /// </summary>
    /// <returns>UI 上の場合 true</returns>
    public static bool IsPointerOverUI()
    {
        var mouse = Mouse.current;
        if (mouse == null || EventSystem.current == null) return false;

        var eventData = new PointerEventData(EventSystem.current)
        {
            position = mouse.position.ReadValue()
        };
        var results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }

    /// <summary>
    /// フォントをキャッシュ済みでなければ AddressableManager 経由で同期ロードする
    /// </summary>
    private static void EnsureFont()
    {
        if (_font != null) return;
        _font = AddressableManager.Instance.LoadAsset<TMP_FontAsset>(FontAddress);
    }

    /// <summary>
    /// 既存の TextMeshProUGUI にキャッシュ済みフォントを適用する
    /// UIHelper を経由せず直接生成した TMP コンポーネントに使用する
    /// </summary>
    /// <param name="tmp">フォントを適用する TextMeshProUGUI</param>
    public static void ApplyFont(TextMeshProUGUI tmp)
    {
        EnsureFont();
        if (_font != null) tmp.font = _font;
    }

    /// <summary>
    /// 既存の TextMeshPro（ワールド空間）にキャッシュ済みフォントを適用する
    /// </summary>
    /// <param name="tmp">フォントを適用する TextMeshPro</param>
    public static void ApplyFont(TextMeshPro tmp)
    {
        EnsureFont();
        if (_font != null) tmp.font = _font;
    }

    /// <summary>
    /// TextMeshProUGUI コンポーネント付きの子 GameObject を生成する
    /// </summary>
    /// <param name="parent">親 Transform</param>
    /// <param name="name">GameObject 名</param>
    /// <param name="text">初期テキスト</param>
    /// <param name="anchoredPos">アンカー基準の位置</param>
    /// <param name="fontSize">フォントサイズ</param>
    /// <param name="color">文字色</param>
    /// <returns>生成した TextMeshProUGUI</returns>
    public static TextMeshProUGUI CreateText(Transform parent, string name, string text,
        Vector2 anchoredPos, float fontSize, Color color)
    {
        EnsureFont();

        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(190, 30);
        rt.anchoredPosition = anchoredPos;
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        if (_font != null) tmp.font = _font;
        return tmp;
    }

    /// <summary>
    /// ボタン付きの子 GameObject を生成する
    /// </summary>
    /// <param name="parent">親 Transform</param>
    /// <param name="label">ボタンラベル</param>
    /// <param name="anchoredPos">アンカー基準の位置</param>
    /// <param name="width">幅</param>
    /// <param name="height">高さ</param>
    /// <param name="onClick">クリック時のコールバック</param>
    /// <returns>生成した Button</returns>
    public static Button CreateButton(Transform parent, string label, Vector2 anchoredPos,
        float width, float height, UnityEngine.Events.UnityAction onClick)
    {
        EnsureFont();

        var go = new GameObject(label + "Button");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(width, height);
        rt.anchoredPosition = anchoredPos;

        var img = go.AddComponent<Image>();
        img.color = new Color(0.3f, 0.3f, 0.3f, 1f);

        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        var textGo = new GameObject("Label");
        textGo.transform.SetParent(go.transform, false);
        var textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;
        textRt.anchoredPosition = Vector2.zero;
        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 12;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        if (_font != null) tmp.font = _font;

        return btn;
    }
}
