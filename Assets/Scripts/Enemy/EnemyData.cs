using UnityEngine;

/// <summary>
/// 敵1種類分のステータス（ScriptableObject）。
/// 「巡回」「追跡」「遠距離」「ボス」を、それぞれ .asset ファイルとして Assets/Data/Enemies に作る。
///
/// 第16回では HP・攻撃力・速度を敵クラスのフィールドとして直接持っていた。
/// それを「データだけ」ここへ切り出したのがこのクラス。
/// 同じ種類の敵が何体いてもデータは1つ、調整も1か所で済む。
/// </summary>
[CreateAssetMenu(menuName = "GP26/EnemyData", fileName = "NewEnemyData")]
public class EnemyData : ScriptableObject
{
    public string enemyName = "Enemy";

    [Header("基本ステータス")]
    public int maxHP = 3;
    public int attackPower = 1;
    public float moveSpeed = 2f;

    [Header("行動の距離（m）")]
    public float detectRange = 8f;      // この距離までプレイヤーが近づくと気付く
    public float attackRange = 1.5f;    // この距離まで近づくと攻撃する（遠距離型は大きめ）
    public float attackInterval = 1.5f; // 攻撃と攻撃の間隔（秒）

    [Header("その他")]
    public int scoreValue = 100;        // 倒したときのスコア
    public Color tintColor = Color.white;   // 見た目の色（種類の見分け用）
    public float modelScale = 1f;       // 大きさ（ボスは大きくする）
    public bool isBoss = false;         // true の敵を倒すとゲームクリア
}
