using UnityEngine;

/// <summary>
/// 追いかける敵。プレイヤーが detectRange に入ると追いかけ、attackRange まで来たら殴る。
/// ボスもこのクラスを使い、EnemyData（HP・攻撃力・大きさ・isBoss）だけを変えている。
/// 「同じ動き・違うデータ」なら、クラスを増やさずデータで表現する、という例。
/// </summary>
public class ChaserEnemy : EnemyBase
{
    protected override void Move()
    {
        float distance = DistanceToPlayer();

        if (distance > data.detectRange)
        {
            // 気付いていない：その場で待つ
            SetAnimSpeed(0f);
        }
        else if (distance > data.attackRange * 0.8f)
        {
            // 気付いた：追いかける
            MoveTowards(player.position, data.moveSpeed);
        }
        else
        {
            // 射程内：止まって向き合う（殴るのは Attack）
            LookAtPlayer();
            SetAnimSpeed(0f);
        }
    }

    protected override void Attack()
    {
        playerHealth.TakeDamage(data.attackPower);
    }
}
