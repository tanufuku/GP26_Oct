using UnityEngine;

/// <summary>
/// ゲーム全体で使う定数をまとめた場所。
/// 数値をコードのあちこちに直書き（マジックナンバー）すると、
/// 「この 10 って何？」「どこを直せば変わる？」が分からなくなる。
/// ここに名前を付けて集めておけば、意味が読み取れて、調整も1か所で済む。
/// </summary>
public static class GameConstants
{
    // ---------- プレイヤー ----------
    public const int PlayerMaxHP = 10;          // プレイヤーの最大HP
    public const int PlayerBaseAttack = 1;      // 武器なしの攻撃力
    public const float PlayerMoveSpeed = 5f;    // 移動速度（m/秒）
    public const float PlayerJumpPower = 7f;    // ジャンプの初速
    public const float Gravity = -20f;          // 重力（少し強めにするとキビキビ動く）
    public const float InvincibleTime = 1.0f;   // 被弾後に無敵になる時間（秒）
    public const float BlinkInterval = 0.1f;    // 無敵中の点滅間隔（秒）
    public const float AttackCooldown = 0.35f;  // 攻撃の連打防止（秒）
    public const float PlayerBulletSpeed = 16f; // プレイヤーの弾の速さ（m/秒）
    public const float PlayerBulletLifeTime = 1.5f; // 弾が消えるまでの時間（秒）＝射程 約24m

    // ---------- 敵 ----------
    public const float EnemyBulletSpeed = 8f;      // 遠距離型の敵の弾の速さ（m/秒）
    public const float EnemyBulletLifeTime = 4f;   // 敵の弾が消えるまでの時間（秒）

    // ---------- スコア ----------
    public const int ScoreForItem = 50;         // アイテムを拾ったときのスコア
    public const int ScoreForKey = 50;          // 鍵を拾ったときのスコア
    public const int ScoreForChest = 100;       // 宝箱を開けたときのスコア

    // ---------- ギミック ----------
    public const float DoorOpenTime = 1.5f;     // 扉が開くのにかかる時間（秒）
    public const float ChestOpenTime = 1.0f;    // 宝箱のフタが開く時間（秒）

    // ---------- UI ----------
    public const float MessageDuration = 2.5f;  // 画面中央のメッセージを表示する時間（秒）

    // ---------- 音 ----------
    public const float SeVolume = 0.6f;           // 効果音の音量（0〜1）
    public const float BgmVolume = 0.12f;         // BGM の音量（0〜1）。効果音よりかなり控えめに
    public const float FootstepInterval = 0.35f;  // 走っているときの足音の間隔（秒）

    // ---------- セーブ（PlayerPrefs のキー名）----------
    // キー名を文字列で直書きすると、打ち間違えても気付けない。定数にしておく。
    public const string SaveKeyHasSave = "HasSave";
    public const string SaveKeyPosX = "PosX";
    public const string SaveKeyPosY = "PosY";
    public const string SaveKeyPosZ = "PosZ";
    public const string SaveKeyHP = "HP";
    public const string SaveKeyScore = "Score";
    public const string SaveKeyKeys = "Keys";
    public const string SaveKeyItems = "Items";
    public const string SaveKeyHighScore = "HighScore";

    // ---------- タグ ----------
    public const string TagPlayer = "Player";
    public const string TagEnemy = "Enemy";
    public const string TagEnemyBullet = "EnemyBullet";
}
