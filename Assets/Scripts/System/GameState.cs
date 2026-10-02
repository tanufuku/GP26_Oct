/// <summary>
/// ゲームの状態。
/// bool を何個も並べる（isTitle, isPaused, isGameOver...）と、
/// 「同時に true になったらどうする？」という矛盾が起きやすい。
/// enum なら「今はこのどれか1つ」と決まるので、状態の管理が安全になる。
/// </summary>
public enum GameState
{
    Title,      // タイトル画面（スタート待ち）
    Playing,    // プレイ中
    Paused,     // ポーズ中（時間停止）
    GameOver,   // プレイヤーが倒れた
    Clear       // ボスを倒してクリア
}
