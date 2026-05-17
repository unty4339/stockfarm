/// <summary>
/// モードとして機能するUIコンポーネントが実装するインターフェース
/// </summary>
public interface IModeUI
{
    /// <summary>モードがアクティブかどうか</summary>
    bool IsActive { get; }
    /// <summary>モードを終了する</summary>
    void Exit();
}

/// <summary>
/// モードの排他制御を行う静的コーディネータ。
/// いずれかのモードが開始されたとき、既存のモードを先に終了させる
/// </summary>
public static class ModeCoordinator
{
    private static IModeUI _currentMode;

    /// <summary>現在いずれかのモードがアクティブかどうか</summary>
    public static bool IsAnyModeActive => _currentMode != null;

    /// <summary>
    /// モードを開始する。既存のモードがあれば先に終了させる
    /// </summary>
    /// <param name="mode">開始するモード</param>
    public static void Enter(IModeUI mode)
    {
        if (_currentMode != null && _currentMode != mode)
            _currentMode.Exit();
        _currentMode = mode;
    }

    /// <summary>
    /// モードを終了する。指定されたモードが現在アクティブでなければ何もしない
    /// </summary>
    /// <param name="mode">終了するモード</param>
    public static void Exit(IModeUI mode)
    {
        if (_currentMode == mode)
            _currentMode = null;
    }
}
