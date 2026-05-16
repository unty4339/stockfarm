using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 矩形ドラッグ選択のオーバーレイと、複数ワーカー選択時のアイコンバーを管理するUI
/// </summary>
public class WorkerSelectionUI : MonoBehaviour
{
    private const float IconSize = 50f;
    private const float IconGap = 10f;

    private RectTransform _canvasRT;
    private WorkerPopupUI _workerPopupUI;

    private GameObject _rectPanel;
    private RectTransform _rectRT;

    private GameObject _iconBarPanel;
    private RectTransform _iconBarRT;
    private readonly List<GameObject> _iconButtons = new();

    private Vector2 _dragStartCanvas;

    private void Awake()
    {
        _canvasRT = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
        _workerPopupUI = FindFirstObjectByType<WorkerPopupUI>();
        BuildRectOverlay();
        BuildIconBar();
    }

    private void BuildRectOverlay()
    {
        _rectPanel = new GameObject("SelectionRect");
        _rectPanel.transform.SetParent(transform, false);
        _rectRT = _rectPanel.AddComponent<RectTransform>();
        // アンカーを中央に合わせることで ScreenPointToLocalPointInRectangle の
        // 座標系（Canvas ローカル、原点=中央）と anchoredPosition の基準点を一致させる
        _rectRT.anchorMin = new Vector2(0.5f, 0.5f);
        _rectRT.anchorMax = new Vector2(0.5f, 0.5f);
        _rectRT.pivot = Vector2.zero;

        var img = _rectPanel.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.12f);

        var outline = _rectPanel.AddComponent<Outline>();
        outline.effectColor = new Color(1f, 1f, 1f, 0.6f);
        outline.effectDistance = new Vector2(1.5f, 1.5f);

        _rectPanel.SetActive(false);
    }

    private void BuildIconBar()
    {
        _iconBarPanel = new GameObject("WorkerIconBar");
        _iconBarPanel.transform.SetParent(transform, false);
        _iconBarRT = _iconBarPanel.AddComponent<RectTransform>();
        _iconBarRT.anchorMin = new Vector2(0.5f, 0f);
        _iconBarRT.anchorMax = new Vector2(0.5f, 0f);
        _iconBarRT.sizeDelta = new Vector2(0f, 60f);
        _iconBarRT.anchoredPosition = new Vector2(0f, 70f);

        var img = _iconBarPanel.AddComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);

        _iconBarPanel.SetActive(false);
    }

    /// <summary>
    /// ドラッグ開始。矩形オーバーレイをアクティブ化する
    /// </summary>
    /// <param name="screenPos">ドラッグ開始スクリーン座標</param>
    public void BeginRect(Vector2 screenPos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRT, screenPos, null, out _dragStartCanvas);
        _rectRT.anchoredPosition = _dragStartCanvas;
        _rectRT.sizeDelta = Vector2.zero;
        _rectPanel.SetActive(true);
    }

    /// <summary>
    /// ドラッグ中。矩形を現在のカーソル位置まで更新する
    /// </summary>
    /// <param name="screenPos">現在のカーソルスクリーン座標</param>
    public void UpdateRect(Vector2 screenPos)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _canvasRT, screenPos, null, out Vector2 current);
        Vector2 min = Vector2.Min(_dragStartCanvas, current);
        Vector2 max = Vector2.Max(_dragStartCanvas, current);
        _rectRT.anchoredPosition = min;
        _rectRT.sizeDelta = max - min;
    }

    /// <summary>
    /// ドラッグ終了。矩形を非表示にする
    /// </summary>
    public void EndRect()
    {
        _rectPanel.SetActive(false);
    }

    /// <summary>
    /// 指定ワーカー一覧のアイコンバーを画面下部に生成・表示する
    /// </summary>
    /// <param name="workers">表示するワーカーの一覧</param>
    public void ShowIconBar(List<WorkerBase> workers)
    {
        ClearIconButtons();

        float totalWidth = workers.Count * IconSize + (workers.Count - 1) * IconGap + IconGap * 2f;
        _iconBarRT.sizeDelta = new Vector2(totalWidth, 60f);

        float startX = -(totalWidth / 2f) + IconGap + IconSize / 2f;

        for (int i = 0; i < workers.Count; i++)
        {
            var worker = workers[i];
            float xPos = startX + i * (IconSize + IconGap);

            var btn = UIHelper.CreateButton(
                _iconBarPanel.transform,
                "",
                new Vector2(xPos, 0f),
                IconSize, IconSize,
                () => OnIconClicked(worker));

            btn.GetComponent<Image>().color = worker.DisplayColor;

            UIHelper.CreateText(
                btn.transform,
                "IconLabel",
                GetShortName(worker),
                new Vector2(0f, -IconSize / 2f - 10f),
                9f,
                Color.white);

            _iconButtons.Add(btn.gameObject);
        }

        _iconBarPanel.SetActive(true);
    }

    /// <summary>
    /// アイコンバーを非表示にし、動的生成したボタンを破棄する
    /// </summary>
    public void HideIconBar()
    {
        _iconBarPanel.SetActive(false);
        ClearIconButtons();
    }

    private void OnIconClicked(WorkerBase worker)
    {
        _workerPopupUI ??= FindFirstObjectByType<WorkerPopupUI>();
        HideIconBar();
        _workerPopupUI?.Show(worker);
    }

    private void ClearIconButtons()
    {
        foreach (var btn in _iconButtons)
            Destroy(btn);
        _iconButtons.Clear();
    }

    private static string GetShortName(WorkerBase worker)
    {
        if (worker is CowWorker cow)
            return cow.CowName;
        if (worker is FarmerWorker)
            return "農主";
        return worker.name;
    }
}
