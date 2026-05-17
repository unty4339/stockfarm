using System;

/// <summary>
/// ポップアップ間の排他制御を行う静的コーディネータ
/// いずれかのポップアップが開いたとき、他を閉じるシグナルを送る
/// </summary>
public static class PopupCoordinator
{
    /// <summary>ポップアップが表示されたときに発火するイベント</summary>
    public static event Action OnAnyPopupShown;

    /// <summary>
    /// ポップアップの Show 冒頭で呼び出す。購読中のすべての Hide を実行させる
    /// </summary>
    public static void NotifyShown() => OnAnyPopupShown?.Invoke();
}
