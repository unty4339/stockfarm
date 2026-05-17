using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// 優先度セルの左クリック・右クリックを検出するコンポーネント
/// </summary>
public class PriorityCell : MonoBehaviour, IPointerClickHandler
{
    /// <summary>左クリック時のコールバック</summary>
    public Action OnLeftClick;
    /// <summary>右クリック時のコールバック</summary>
    public Action OnRightClick;

    /// <summary>ポインタクリックイベントを左右ボタンに振り分ける</summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            OnLeftClick?.Invoke();
        else if (eventData.button == PointerEventData.InputButton.Right)
            OnRightClick?.Invoke();
    }
}

/// <summary>
/// ワーカーごとのタスク優先度を表示・編集するメニュー
/// 行=ワーカー、列=タスク種別、セルの左/右クリックで優先度を ±1 変更する
/// </summary>
public class PriorityMenuUI : MonoBehaviour
{
    private const float NameColumnWidth = 180f;
    private const float TaskCellWidth = 104f;
    private const float RowHeight = 60f;
    private const float Padding = 16f;
    private const float Border = 4f;

    /// <summary>表示するタスク種別と列ヘッダーラベルの対応表</summary>
    private static readonly (Type taskType, string label)[] TaskColumns =
    {
        (typeof(BirthTask),            "出産"),
        (typeof(SleepTask),            "睡眠"),
        (typeof(EatTask),              "摂食"),
        (typeof(MilkWaitTask),         "搾乳"),
        (typeof(CareTask),             "ケア"),
        (typeof(ProcessingAssistTask), "加工"),
        (typeof(TransportTask),        "運搬"),
        (typeof(HarvestTask),          "収穫"),
        (typeof(PlantTask),            "植付"),
        (typeof(CleanTask),            "清掃"),
    };

    private GameObject _panel;

    /// <summary>現在表示中のワーカー（互換用）</summary>
    public WorkerBase TargetWorker { get; private set; }

    private void Awake()
    {
        _panel = new GameObject("PriorityPanel");
        _panel.transform.SetParent(transform, false);
        var rt = _panel.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        _panel.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        _panel.SetActive(false);
    }

    /// <summary>パネルを表示してテーブルを再構築する</summary>
    public void Show()
    {
        BuildTable();
        _panel.SetActive(true);
    }

    /// <summary>パネルを非表示にする</summary>
    public void Hide() => _panel?.SetActive(false);

    /// <summary>
    /// 指定ワーカーを起点にテーブルを表示する（互換用）
    /// </summary>
    /// <param name="worker">対象ワーカー</param>
    public void RefreshList(WorkerBase worker)
    {
        TargetWorker = worker;
        Show();
    }

    /// <summary>
    /// タスク種別の優先度を変更する（互換用）
    /// </summary>
    /// <param name="taskType">タスク種別</param>
    /// <param name="priority">新しい優先度（1〜7）</param>
    public void SetPriority(Type taskType, int priority)
    {
        TargetWorker?.DecisionMaker?.SetTaskPriority(taskType, priority);
    }

    private void BuildTable()
    {
        foreach (Transform child in _panel.transform)
            Destroy(child.gameObject);

        var workers = FindObjectsOfType<WorkerBase>();
        Array.Sort(workers, (a, b) =>
            string.Compare(GetWorkerName(a), GetWorkerName(b), StringComparison.Ordinal));

        int cols = TaskColumns.Length;
        int rows = workers.Length;

        float tableWidth = NameColumnWidth + cols * TaskCellWidth;
        float tableHeight = (rows + 1) * RowHeight;
        var panelRt = _panel.GetComponent<RectTransform>();
        panelRt.sizeDelta = new Vector2(tableWidth + Padding * 2f, tableHeight + Padding * 2f);
        panelRt.anchoredPosition = UIHelper.SubMenuPanelAnchoredPosition;

        // ヘッダー行
        AddCell("名前", 0, 0, NameColumnWidth, isHeader: true, null, null);
        for (int c = 0; c < cols; c++)
            AddCell(TaskColumns[c].label, c + 1, 0, TaskCellWidth, isHeader: true, null, null);

        // ワーカー行
        for (int r = 0; r < rows; r++)
        {
            var worker = workers[r];
            AddCell(GetWorkerName(worker), 0, r + 1, NameColumnWidth, isHeader: false, null, null);

            for (int c = 0; c < cols; c++)
            {
                var capturedWorker = worker;
                var capturedType = TaskColumns[c].taskType;

                int priority = 0;
                worker.DecisionMaker.TaskPriorities.TryGetValue(capturedType, out priority);

                AddCell(priority.ToString(), c + 1, r + 1, TaskCellWidth, isHeader: false,
                    () => ChangePriority(capturedWorker, capturedType, +1),
                    () => ChangePriority(capturedWorker, capturedType, -1));
            }
        }
    }

    private void ChangePriority(WorkerBase worker, Type taskType, int delta)
    {
        if (!worker.DecisionMaker.TaskPriorities.TryGetValue(taskType, out int current)) return;
        worker.DecisionMaker.SetTaskPriority(taskType, current + delta);
        BuildTable();
    }

    /// <summary>
    /// パネル内に1つのセルを追加する
    /// </summary>
    /// <param name="text">表示テキスト</param>
    /// <param name="col">列インデックス（0=名前列、1以降=タスク列）</param>
    /// <param name="row">行インデックス（0=ヘッダー、1以降=ワーカー行）</param>
    /// <param name="width">セル幅</param>
    /// <param name="isHeader">ヘッダー行かどうか</param>
    /// <param name="onLeft">左クリック時のコールバック（nullで無効）</param>
    /// <param name="onRight">右クリック時のコールバック（nullで無効）</param>
    private void AddCell(string text, int col, int row, float width,
        bool isHeader, Action onLeft, Action onRight)
    {
        float xLeft = Padding + (col == 0 ? 0f : NameColumnWidth + (col - 1) * TaskCellWidth);
        float yTop = Padding + row * RowHeight;
        float cellW = width - Border;
        float cellH = RowHeight - Border;

        var go = new GameObject("Cell");
        go.transform.SetParent(_panel.transform, false);

        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.sizeDelta = new Vector2(cellW, cellH);
        rt.anchoredPosition = new Vector2(xLeft + cellW * 0.5f, -(yTop + cellH * 0.5f));

        var img = go.AddComponent<Image>();
        img.color = isHeader ? new Color(0.2f, 0.3f, 0.5f, 1f) : new Color(0.25f, 0.25f, 0.25f, 1f);

        var textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        var textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = Vector2.zero;
        textRt.anchorMax = Vector2.one;
        textRt.sizeDelta = Vector2.zero;
        textRt.anchoredPosition = Vector2.zero;
        var tmp = textGo.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = 22f;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        UIHelper.ApplyFont(tmp);

        if (onLeft != null || onRight != null)
        {
            var cell = go.AddComponent<PriorityCell>();
            cell.OnLeftClick = onLeft;
            cell.OnRightClick = onRight;
        }
    }

    private static string GetWorkerName(WorkerBase worker)
    {
        if (worker is CowWorker cow) return cow.CowName ?? worker.name;
        return worker.name;
    }
}
