using System.Collections;
using UnityEngine;

/// <summary>
/// プレイヤーの攻撃。J キー（またはマウス左クリック）で正面に弾を撃つ。
/// 威力は「素の攻撃力＋武器の攻撃力」を CalcDamage で計算し、弾（PlayerBullet）に渡す。
///
/// 見た目：キャラクター素材に攻撃モーションが無いので、撃つ瞬間に体を少し前に出す動き（コルーチン）で「撃った」感じを出す。
/// 着弾の演出は弾（PlayerBullet）が当たった場所で出す。
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private int baseAttack = GameConstants.PlayerBaseAttack;
    [SerializeField] private PlayerBullet bulletPrefab;     // 撃つ弾
    [SerializeField] private Transform firePoint;           // 弾が出る位置（体の前・胸の高さ）
    [SerializeField] private float lungeDistance = 0.25f;   // 撃つときに体を出す距離（m）

    private PlayerInventory inventory;
    private Animator animator;
    private Transform modelRoot;        // 見た目だけを動かすための親（当たり判定は動かさない）
    private float cooldownTimer;
    private bool isLunging;

    private void Awake()
    {
        inventory = GetComponent<PlayerInventory>();
        modelRoot = transform.Find("ModelRoot");
    }

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        cooldownTimer -= Time.deltaTime;

        if (!GameManager.Instance.IsPlaying) return;
        if (cooldownTimer > 0f) return;

        if (Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0))
        {
            DoAttack();
            cooldownTimer = GameConstants.AttackCooldown;
        }
    }

    /// <summary>
    /// ダメージを計算して返す。
    /// 「素の攻撃力」と「武器の攻撃力」を引数で受け取り、結果を戻り値で返す。
    /// 計算式をここに閉じ込めておけば、あとで「クリティカル」などを足すときもここだけ直せばよい。
    /// </summary>
    public int CalcDamage(int baseAtk, int weaponAtk)
    {
        return baseAtk + weaponAtk;
    }

    private void DoAttack()
    {
        if (bulletPrefab == null) return;

        int damage = CalcDamage(baseAttack, inventory.GetWeaponAttack());

        // 弾を生成して、正面に向けて撃つ。威力は弾に渡す（弾は「飛ぶ・当たる」だけを担当）
        Vector3 spawnPos = (firePoint != null) ? firePoint.position : transform.position + Vector3.up * 1f + transform.forward * 0.5f;
        PlayerBullet bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.LookRotation(transform.forward));
        bullet.Launch(transform.forward, damage);
        SoundLibrary.Play("Shoot");

        // ---- 見た目 ----（着弾の演出は弾側 PlayerBullet が、当たった場所で出す）
        if (animator != null && animator.runtimeAnimatorController != null) animator.SetTrigger("Attack");
        if (!isLunging) StartCoroutine(Lunge());
    }

    /// <summary>体（見た目だけ）を少し前に出して戻す。「一瞬で戻る動き」はコルーチンで書くと素直に読める。</summary>
    private IEnumerator Lunge()
    {
        if (modelRoot == null) yield break;
        isLunging = true;

        Vector3 rest = modelRoot.localPosition;
        Vector3 forward = rest + Vector3.forward * lungeDistance;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.05f;
            modelRoot.localPosition = Vector3.Lerp(rest, forward, t);
            yield return null;
        }
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.12f;
            modelRoot.localPosition = Vector3.Lerp(forward, rest, t);
            yield return null;
        }

        modelRoot.localPosition = rest;
        isLunging = false;
    }
}
