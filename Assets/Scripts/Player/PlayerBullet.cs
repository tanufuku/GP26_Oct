using UnityEngine;

/// <summary>
/// プレイヤーが撃つ弾。まっすぐ飛び、敵に当たるとダメージを与えて消える。壁に当たっても消える。
/// 威力は撃った側（PlayerAttack）が Launch で教えてくれる。弾自身は「飛ぶ」「当たる」だけを担当する。
/// </summary>
public class PlayerBullet : MonoBehaviour
{
    [SerializeField] private float speed = GameConstants.PlayerBulletSpeed;
    [SerializeField] private float lifeTime = GameConstants.PlayerBulletLifeTime;

    private Vector3 direction;
    private int damage;

    /// <summary>撃った側が、飛ぶ方向と威力を教える</summary>
    public void Launch(Vector3 dir, int dmg)
    {
        direction = dir.normalized;
        damage = dmg;
        Destroy(gameObject, lifeTime);     // 何にも当たらなくても射程の外で消える
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 敵に当たった：相手の TakeDamage を呼ぶ（敵側で HP 減少・火花・しゃがみモーションが起きる）
        EnemyBase enemy = other.GetComponentInParent<EnemyBase>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            EffectLibrary.Play("Attack", transform.position);   // 着弾の閃光（当たった場所で出す）
            Destroy(gameObject);
            return;
        }

        // スイッチ・アイテム・敵の弾などのトリガーは素通り。壁や扉（普通の Collider）に当たったら消える。
        if (other.isTrigger) return;
        if (other.CompareTag(GameConstants.TagPlayer)) return;

        EffectLibrary.Play("Attack", transform.position);       // 壁に当たったときも小さく光る
        Destroy(gameObject);
    }
}
