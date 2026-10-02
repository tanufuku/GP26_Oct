using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敵の共通部分（基底クラス）。
/// HP・ダメージ・死亡・プレイヤーの検索・攻撃のタイミング管理はどの敵も同じなのでここに書く。
/// 「どう動くか（Move）」「どう攻撃するか（Attack）」だけが敵ごとに違うので abstract にして、
/// 子クラス（PatrolEnemy / ChaserEnemy / RangedEnemy）に override させる。
///
/// abstract クラスなので、このクラスをそのまま GameObject に付けることはできない。
/// </summary>
public abstract class EnemyBase : MonoBehaviour
{
    [SerializeField] protected EnemyData data;      // ステータス（ScriptableObject）
    [SerializeField] private TMPro.TextMeshPro hpLabel;   // 頭上に HP を出す 3D テキスト（授業用の最小シーンだけ使う。無ければ何もしない）

    protected int currentHP;
    protected Transform player;
    protected PlayerHealth playerHealth;
    protected Animator animator;

    private const float TurnSpeed = 540f;   // 向きを変える速さ（度/秒）

    private float attackTimer;      // 次に攻撃できるまでの残り時間
    protected bool isDead;          // 倒された後か。子クラスが Die() を上書きして「倒れる演出の間に二重に死なない」ようにするため protected
    private Material[] tintMaterials;   // 色を付けるマテリアル（この敵専用のコピー。1 つのモデルに複数あることが多い）
    private Color[] originalColors;

    // 子クラスで Start を上書きしたいときは base.Start() を呼ぶこと
    protected virtual void Start()
    {
        if (data == null)
        {
            // Inspector の Data 欄が空のまま置かれた敵。null のまま進むと分かりにくいエラーになるので、ここで止める。
            Debug.LogError(name + ": EnemyData が設定されていません。Inspector の Data 欄に Assets/Data/Enemies のデータを入れてください。", this);
            enabled = false;
            return;
        }

        currentHP = data.maxHP;
        transform.localScale = Vector3.one * data.modelScale;

        // プレイヤーはシーンに1人なので、タグで探しておく（毎フレーム探さない）
        GameObject playerObj = GameObject.FindWithTag(GameConstants.TagPlayer);
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerHealth = playerObj.GetComponent<PlayerHealth>();
        }

        animator = GetComponentInChildren<Animator>();
        ApplyTint(data.tintColor);
        UpdateHpLabel("");
    }

    protected virtual void Update()
    {
        if (isDead || player == null) return;
        if (GameManager.Instance.IsPlaying) return;

        Move();

        // 攻撃のタイミングは共通ルール：射程内にいて、間隔が空いていれば Attack()
        attackTimer -= Time.deltaTime;
        if (DistanceToPlayer() <= data.attackRange && attackTimer <= 0f)
        {
            Attack();
            attackTimer = data.attackInterval;
        }
    }

    /// <summary>毎フレームの移動。敵の種類ごとに違うので子クラスが必ず実装する。</summary>
    protected abstract void Move();

    /// <summary>射程内・間隔OKのときに呼ばれる攻撃。敵の種類ごとに違うので子クラスが必ず実装する。</summary>
    protected abstract void Attack();

    /// <summary>ダメージを受ける（プレイヤーの弾 PlayerBullet から呼ばれる）</summary>
    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHP -= amount;
        UpdateHpLabel("  -" + amount);      // 受けたダメージも表示（引数 amount がそのまま見える）
        StartCoroutine(HitFlash());
        EffectLibrary.Play("EnemyHit", transform.position + Vector3.up * 1f * data.modelScale);
        SoundLibrary.Play("EnemyHit");
        if (animator != null && animator.runtimeAnimatorController != null) animator.SetTrigger("Damage");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    /// <summary>倒されたとき。スコア加算とボス判定。</summary>
    protected virtual void Die()
    {
        isDead = true;
        ScoreManager.Instance.AddScore(data.scoreValue);
        UIManager.Instance.ShowMessage("Defeated " + data.enemyName + "!  +" + data.scoreValue);

        // 爆発の演出（Particle Pack があるときだけ出る）
        SoundLibrary.Play(data.isBoss ? "BossDie" : "EnemyDie");
        EffectLibrary.Play(data.isBoss ? "BossDie" : "EnemyDie",transform.position + Vector3.up * 0.9f * data.modelScale);

        if (data.isBoss)
        {
            GameManager.Instance.OnBossDefeated();
        }

        Destroy(gameObject);
    }

    // ---------- 子クラスから使う便利メソッド ----------

    protected float DistanceToPlayer()
    {
        return Vector3.Distance(transform.position, player.position);
    }

    /// <summary>目的地に向かって水平に進む（高さは変えない）。着いていれば止まる。</summary>
    protected void MoveTowards(Vector3 target, float speed)
    {
        Vector3 dir = target - transform.position;
        dir.y = 0f;

        if (dir.magnitude < 0.1f)
        {
            SetAnimSpeed(0f);
            return;
        }

        dir.Normalize();
        transform.position += dir * speed * Time.deltaTime;
        LookToward(dir);
        SetAnimSpeed(1f);
    }

    /// <summary>その方向をなめらかに向く</summary>
    protected void LookToward(Vector3 dir)
    {
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, TurnSpeed * Time.deltaTime);
    }

    protected void LookAtPlayer()
    {
        LookToward(player.position - transform.position);
    }

    protected void SetAnimSpeed(float speed)
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            animator.SetFloat("Speed", speed);
        }
    }

    // 頭上の HP 表示（hpLabel が割り当てられている最小シーンだけ）。suffix は "  -2" のような追記。
    private void UpdateHpLabel(string suffix)
    {
        if (hpLabel == null) return;
        hpLabel.text = data.enemyName + "\nHP " + Mathf.Max(currentHP, 0) + " / " + data.maxHP + suffix;
    }

    // ---------- 見た目 ----------

    // 色を持つマテリアルか（URP/Lit は _BaseColor、Standard は _Color。どちらも material.color で読み書きできる）
    private static bool HasColor(Material m)
    {
        return m.HasProperty("_BaseColor") || m.HasProperty("_Color");
    }

    // 種類ごとの色を付ける（同じモデルを使い回しても見分けられるように）
    // モデルは 1 つの Renderer に複数のマテリアル（体・腕・脚など）を持つことがあるので、全部に色を付ける。
    private void ApplyTint(Color tint)
    {
        List<Material> list = new List<Material>();
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            foreach (Material m in r.materials)     // materials（sharedMaterials ではない）＝この敵専用のコピー
            {
                if (!HasColor(m)) continue;
                m.color = tint;
                list.Add(m);
            }
        }

        tintMaterials = list.ToArray();
        originalColors = new Color[tintMaterials.Length];
        for (int i = 0; i < tintMaterials.Length; i++) originalColors[i] = tintMaterials[i].color;
    }

    // ダメージを受けた瞬間、一瞬白く光らせる
    private IEnumerator HitFlash()
    {
        foreach (Material m in tintMaterials) m.color = Color.white;

        yield return new WaitForSeconds(0.08f);

        for (int i = 0; i < tintMaterials.Length; i++) tintMaterials[i].color = originalColors[i];
    }
}
