using UnityEngine;

/// <summary>
/// 遠距離攻撃の敵。プレイヤーと一定の距離を保ちながら弾を撃つ。
/// 近づかれたら下がり、離れすぎたら寄る。攻撃は「殴る」ではなく「弾を生成する」。
/// Attack の中身をまるごと変えられるのが override の便利なところ。
/// </summary>
public class RangedEnemy : EnemyBase
{
    [SerializeField] private EnemyBullet bulletPrefab;
    [SerializeField] private Transform firePoint;           // 弾が出る位置（無ければ体の中心）
    [SerializeField] private float keepDistance = 6f;       // 保ちたい距離（m）

    protected override void Move()
    {
        float distance = DistanceToPlayer();

        if (distance > data.detectRange)
        {
            SetAnimSpeed(0f);
            return;
        }

        LookAtPlayer();

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        toPlayer.Normalize();

        if (distance < keepDistance - 1f)
        {
            // 近すぎる：後ろに下がる
            MoveTowards(transform.position - toPlayer * 2f, data.moveSpeed);
            LookAtPlayer();     // 下がりながらもプレイヤーの方を向く
        }
        else if (distance > keepDistance + 1f)
        {
            // 遠すぎる：近づく
            MoveTowards(player.position, data.moveSpeed);
        }
        else
        {
            SetAnimSpeed(0f);
        }
    }

    protected override void Attack()
    {
        if (bulletPrefab == null) return;

        Vector3 spawnPos = (firePoint != null) ? firePoint.position : transform.position + Vector3.up * 1f;
        Vector3 dir = (player.position + Vector3.up * 0.9f) - spawnPos;

        EnemyBullet bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.LookRotation(dir));
        bullet.Launch(dir.normalized, data.attackPower);
        SoundLibrary.Play("EnemyShoot");
    }
}
