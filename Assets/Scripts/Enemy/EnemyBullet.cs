using UnityEngine;

/// <summary>
/// 遠距離型の敵が撃つ弾。まっすぐ飛び、プレイヤーに当たるとダメージ、壁に当たると消える。
/// </summary>
public class EnemyBullet : MonoBehaviour
{
    [SerializeField] private float speed = GameConstants.EnemyBulletSpeed;
    [SerializeField] private float lifeTime = GameConstants.EnemyBulletLifeTime;   // 何にも当たらなくてもこの秒数で消える

    private Vector3 direction;
    private int damage;

    /// <summary>撃った側（RangedEnemy）が、飛ぶ方向と威力を教える</summary>
    public void Launch(Vector3 dir, int dmg)
    {
        direction = dir;
        damage = dmg;
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(GameConstants.TagPlayer))
        {
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null) health.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // 敵自身・他の弾・スイッチ等のトリガーは無視。壁や床（普通の Collider）に当たったら消える。
        if (other.isTrigger) return;
        if (other.GetComponentInParent<EnemyBase>() != null) return;

        Destroy(gameObject);
    }
}
