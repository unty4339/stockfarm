# 積み残しタスク一覧

クラス実装フェーズで対応しきれなかった未実装・暫定実装の一覧。
コード中の `// TODO:` コメントはここに対応するエントリが存在する。

---

## 高優先度

### ワーカー・設備のクリック検出
**対象:** `WorkerBase`、`EquipmentBase`、`WorkerPopupUI`、`FacilityPopupUI`

`WorkerPopupUI.Show(worker)` と `FacilityPopupUI.Show(equipment)` は実装済みだが、
ゲームオブジェクトから呼び出すクリック検出の仕組みが存在しない。

- `WorkerBase.Awake()` に `Physics2DRaycaster` またはマウス入力でのクリック検出を追加する
- `EquipmentBase.Place()` 生成時に同様の処理を追加する
- 代替案: `GameBootstrapper` や新規 `InputManager` が `Mouse.current.leftButton.wasPressedThisFrame` で
  ワールド座標を取得し、該当するワーカー・設備を探す

---

### CommandMenuUI のボタン連携
**対象:** `Assets/Scripts/UI/CommandMenuUI.cs`

解体・種付け・搾乳の各ボタンが `Debug.Log` のスタブのみ。

| ボタン | 必要な実装 |
|--------|-----------|
| 解体   | クリックした設備を選択し `BuildingManager.RemoveEquipment()` を呼ぶ「解体モード」の実装 |
| 種付け | 選択した牛に `BreedingFacility` へ向かわせる `ForceTask` の発行 |
| 搾乳   | 選択した牛に `MilkWaitTask` を `ForceTask` で割り当てる |

---

### PriorityMenuUI のタスク一覧表示
**対象:** `Assets/Scripts/UI/PriorityMenuUI.cs:43`

```
// TODO: ワーカーのDecisionMaker.TaskPrioritiesを元に一覧をビルドする
```

`RefreshList(WorkerBase)` が呼ばれたとき、`worker.DecisionMaker.TaskPriorities`
をもとにタスク名と優先度スライダー/ボタンを動的に生成する処理が未実装。

---

## 中優先度

### MapManager.ExpandMap の実装
**対象:** `Assets/Scripts/Managers/MapManager.cs:183`

```
// TODO: 実際の拡張処理（グリッド配列のリサイズとビジュアル生成）
```

`TryExpand(MapExpandDirection)` で資金消費まで実装済みだが、
グリッド配列のリサイズ・新規タイルの `GridTile` 生成・タイル GameObject の生成が未実装。

---

### マップ拡張 UI
**対象:** 新規クラス（例: `MapExpandUI`）

`MapManager.TryExpand()` を呼ぶ UI ボタンが存在しない。
費用表示（`MapManager.GetExpansionCost()`）と方向選択も必要。

---


## 低優先度

### パイプ接続 UI
**対象:** `Assets/Scripts/Resources/PipeConnection.cs`、`PipeDistributor`

データ構造と `Distribute()` は実装済みだが、
ゲーム内で接続を設定・解除するための UI が存在しない。
`LiquidTank` 同士を視覚的に繋ぐ操作フローが必要。

---

### スケジュール編集 UI
**対象:** `WorkerBase.Schedule`（24スロット配列）、新規クラス（例: `ScheduleEditorUI`）

`WorkerBase.SetScheduleRange()` は実装済みだが、
プレイヤーがスロットを Work / Sleep / Joy に切り替えるUIが存在しない。

---

### TransportTask の発生元・配送先の自動解決
**対象:** `Assets/Scripts/AI/Tasks/TransportTask.cs`

`TransportTask` は 2 フェーズ実装済みだが、
`AIDecisionMaker.TryCreateWorkTask()` から `TransportTask` を生成するロジックが未接続。
どの `Chest` からどの `Chest` へ運ぶかを決定するルールが必要。

---

### FarmerWorker の作業スケジュール既定値
**対象:** `Assets/Scripts/Workers/WorkerBase.cs`、`FarmerWorker`

`WorkerBase.DefaultScheduleFor(int slot)` の実装内容に応じて、
牧場主用のデフォルトスケジュール（例: 昼間 Work、夜 Sleep）を明示的に設定する。
現状は牛と同じ既定スケジュールが使われている可能性がある。

---

## スコープ外（今後の機能として）

以下は設計フェーズで意図的に対象外とされたもの。

- **Addressables スプライト**: 外部アート素材が揃ったタイミングで `SpriteHelper` を置き換える
- **AutoMilkingStand / AutoBrush の自律サイクル**: データ構造は実装済み、AIタスクとの自動連携が未接続
- **売上ログ UI**: `SellPoint.Collect()` は実装済みだが、売却履歴を表示するパネルがない
