using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ワーカーごとのタスク優先度を表示・編集するメニュー
/// </summary>
public class PriorityMenuUI : MonoBehaviour
{
    private GameObject _panel;

    /// <summary>現在表示中のワーカー</summary>
    public WorkerBase TargetWorker { get; private set; }

    private void Awake()
    {
        _panel = new GameObject("PriorityPanel");
        _panel.transform.SetParent(transform, false);
        var rt = _panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(300, 150);
        rt.anchoredPosition = new Vector2(0, 90);
        var img = _panel.AddComponent<Image>();
        img.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);
        _panel.SetActive(false);
    }

    /// <summary>パネルを表示する</summary>
    public void Show() => _panel?.SetActive(true);
    /// <summary>パネルを非表示にする</summary>
    public void Hide() => _panel?.SetActive(false);

    /// <summary>
    /// 指定ワーカーのタスク優先度一覧を表示・更新する
    /// </summary>
    /// <param name="worker">対象ワーカー</param>
    public void RefreshList(WorkerBase worker)
    {
        TargetWorker = worker;
        Show();
        // TODO: ワーカーのDecisionMaker.TaskPrioritiesを元に一覧をビルドする
        Debug.Log($"[PriorityMenuUI] {worker.name}の優先度を表示（実装予定）");
    }

    /// <summary>
    /// タスク種別の優先度を変更する
    /// </summary>
    /// <param name="taskType">タスク種別</param>
    /// <param name="priority">新しい優先度（1〜7）</param>
    public void SetPriority(Type taskType, int priority)
    {
        TargetWorker?.DecisionMaker?.SetTaskPriority(taskType, priority);
    }
}
