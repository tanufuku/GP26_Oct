using UnityEngine;

/// <summary>
/// 巡回する敵。決めた地点（waypoints）を順番に回り、プレイヤーが射程に入ると殴る。
/// EnemyBase を継承し、Move と Attack だけを自分用に書き換えている（override）。
/// </summary>
public class PatrolEnemy : EnemyBase
{
    [SerializeField] private Transform[] waypoints;     // 巡回する地点（2つ以上）

    private int currentIndex = 0;

    protected override void Move()
    {
        // 近くにプレイヤーがいるときは立ち止まって向き合う
        if (DistanceToPlayer() <= data.attackRange)
        {
            LookAtPlayer();
            SetAnimSpeed(0f);
            return;
        }

        if (waypoints == null || waypoints.Length == 0) return;

        Transform target = waypoints[currentIndex];
        MoveTowards(target.position, data.moveSpeed);

        // 着いたら次の地点へ（最後まで行ったら最初に戻る）
        Vector3 diff = target.position - transform.position;
        diff.y = 0f;
        if (diff.magnitude < 0.3f)
        {
            currentIndex = (currentIndex + 1) % waypoints.Length;
        }
    }

    protected override void Attack()
    {
        playerHealth.TakeDamage(data.attackPower);
    }
}
