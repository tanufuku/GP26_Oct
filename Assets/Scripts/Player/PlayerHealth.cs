using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーの HP。ダメージ・回復・被弾演出（のけぞり＋赤フラッシュ＋無敵点滅）・死亡を担当する。
/// キャラクター素材に被弾モーションが無いので、体を一瞬後ろに引く動きで「効いた」感じを出す。
/// </summary>
public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHP = GameConstants.PlayerMaxHP;
    [SerializeField] private int startHP = 0;                // 開始時の HP。0 なら満タン（授業用の最小シーンで減らした状態から始めたいとき用）
    [SerializeField] private float flinchDistance = 0.3f;    // 被弾時に体を引く距離（m）

    public int CurrentHP { get; private set; }
    public int MaxHP { get { return maxHP; } }
    public bool IsDead { get { return CurrentHP <= 0; } }

    private bool isInvincible;          // 無敵中は TakeDamage を無視する
    private PlayerInventory inventory;  // 防具の防御力を見るため
    private Animator animator;
    private Transform modelRoot;        // 見た目だけを動かすための親
    private Renderer[] renderers;       // 点滅させる見た目（モデル差し替え後に集める）
    private Material[] tintMaterials;   // 赤フラッシュで色を変えるマテリアル（1 つの Renderer に複数あることが多い）
    private Color[] originalColors;

    private void Awake()
    {
        CurrentHP = (startHP > 0) ? Mathf.Min(startHP, maxHP) : maxHP;
        inventory = GetComponent<PlayerInventory>();
        modelRoot = transform.Find("ModelRoot");
    }

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        renderers = GetComponentsInChildren<Renderer>();

        // 色を持つマテリアル（この Player 専用のコピー）を全部集めて、元の色を覚えておく
        List<Material> list = new List<Material>();
        foreach (Renderer r in renderers)
        {
            foreach (Material m in r.materials) if (HasColor(m)) list.Add(m);
        }
        tintMaterials = list.ToArray();
        originalColors = new Color[tintMaterials.Length];
        for (int i = 0; i < tintMaterials.Length; i++) originalColors[i] = tintMaterials[i].color;

        UIManager.Instance.UpdateHP(CurrentHP, maxHP);
    }

    /// <summary>
    /// ダメージを受ける。amount は「敵の攻撃力」で、防具ぶんを引いた値が実際に減る。
    /// 引数（amount）で「どれだけ」を受け取り、計算はこの中で完結させる、というメソッドの基本形。
    /// </summary>
    public void TakeDamage(int amount)
    {
        if (isInvincible || IsDead) return;

        // 防御力ぶん軽減する。ただし最低1は減るようにして「まったく効かない」状態を作らない。
        int damage = Mathf.Max(1, amount - inventory.GetDefense());
        CurrentHP = Mathf.Max(0, CurrentHP - damage);
        UIManager.Instance.UpdateHP(CurrentHP, maxHP);
        EffectLibrary.Play("PlayerHit", transform.position + Vector3.up * 1f);
        SoundLibrary.Play("PlayerHit");

        if (IsDead)
        {
            Die();
        }
        else
        {
            if (animator != null && animator.runtimeAnimatorController != null) animator.SetTrigger("Damage");
            StartCoroutine(Flinch());
            StartCoroutine(InvincibleBlink());
        }
    }

    /// <summary>回復する（最大HPは超えない）</summary>
    public void Heal(int amount)
    {
        if (IsDead) return;
        CurrentHP = Mathf.Min(maxHP, CurrentHP + amount);
        UIManager.Instance.UpdateHP(CurrentHP, maxHP);
    }

    /// <summary>セーブデータから HP をそのまま入れる</summary>
    public void SetHP(int value)
    {
        CurrentHP = Mathf.Clamp(value, 1, maxHP);
        UIManager.Instance.UpdateHP(CurrentHP, maxHP);
    }

    /// <summary>
    /// 被弾後しばらく無敵にして、その間は見た目を点滅させる。
    /// 「0.1秒待つ → 表示を切り替える」を繰り返す処理は、コルーチンで書くと素直に読める。
    /// </summary>
    private IEnumerator InvincibleBlink()
    {
        isInvincible = true;

        float elapsed = 0f;
        bool visible = false;
        while (elapsed < GameConstants.InvincibleTime)
        {
            SetVisible(visible);
            visible = !visible;
            yield return new WaitForSeconds(GameConstants.BlinkInterval);
            elapsed += GameConstants.BlinkInterval;
        }

        SetVisible(true);
        isInvincible = false;
    }

    /// <summary>のけぞり：見た目を一瞬後ろに引き、同時に赤く光らせてから戻す</summary>
    private IEnumerator Flinch()
    {
        SetTint(new Color(1f, 0.35f, 0.35f));

        if (modelRoot != null)
        {
            Vector3 rest = modelRoot.localPosition;
            Vector3 back = rest - Vector3.forward * flinchDistance;

            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.05f;
                modelRoot.localPosition = Vector3.Lerp(rest, back, t);
                yield return null;
            }
            t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / 0.2f;
                modelRoot.localPosition = Vector3.Lerp(back, rest, t);
                yield return null;
            }
            modelRoot.localPosition = rest;
        }
        else
        {
            yield return new WaitForSeconds(0.15f);
        }

        RestoreTint();
    }

    private void SetVisible(bool visible)
    {
        foreach (Renderer r in renderers)
        {
            if (r != null) r.enabled = visible;
        }
    }

    // 色を持つマテリアルか（URP/Lit は _BaseColor、Standard は _Color。どちらも material.color で読み書きできる）
    private static bool HasColor(Material m)
    {
        return m.HasProperty("_BaseColor") || m.HasProperty("_Color");
    }

    private void SetTint(Color color)
    {
        foreach (Material m in tintMaterials) m.color = color;
    }

    private void RestoreTint()
    {
        for (int i = 0; i < tintMaterials.Length; i++) tintMaterials[i].color = originalColors[i];
    }

    private void Die()
    {
        SetVisible(true);
        RestoreTint();
        GameManager.Instance.OnPlayerDied();
    }
}
