using UnityEngine;

/// <summary>
/// プレイヤーの移動とジャンプ（旧 Input Manager 版）。
/// WASD / 矢印キーで移動、Space でジャンプ。進む方向にキャラの向きを合わせる。
/// 物理は CharacterController に任せる（壁に当たって止まる・段差に乗る・isGrounded が取れる）。
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = GameConstants.PlayerMoveSpeed;
    [SerializeField] private float jumpPower = GameConstants.PlayerJumpPower;
    [SerializeField] private float rotateSpeed = 720f;   // 向きを変える速さ（度/秒）

    private CharacterController controller;
    private Animator animator;          // モデルが差し替わっていれば見つかる。無ければ null のまま。
    private float verticalVelocity;     // 上下方向の速度（ジャンプ・重力）
    private bool wasGrounded = true;    // 前のフレームで接地していたか（着地音の判定用）
    private float footstepTimer;        // 次の足音までの残り時間

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        // 見た目のモデルは ModelLoader が Awake で子として生成するので、Start で探す
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (!GameManager.Instance.IsPlaying)
        {
            SetAnimSpeed(0f);
            return;
        }

        // ---- 入力を「進みたい方向」にする ----
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 moveDir = new Vector3(h, 0f, v);
        if (moveDir.magnitude > 1f) moveDir.Normalize();   // 斜め入力で速くならないように

        // ---- 進む方向に体を向ける ----
        if (moveDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
        }

        // ---- ジャンプと重力 ----
        if (controller.isGrounded)
        {
            if (!wasGrounded) SoundLibrary.Play("Land");   // 空中から地面に着いた瞬間だけ鳴る
            verticalVelocity = -1f;     // 地面に軽く押し付けて isGrounded を安定させる
            if (Input.GetKeyDown(KeyCode.Space))
            {
                verticalVelocity = jumpPower;
                SetAnimTrigger("Jump");
                SoundLibrary.Play("Jump");
            }
        }
        wasGrounded = controller.isGrounded;
        verticalVelocity += GameConstants.Gravity * Time.deltaTime;

        // ---- 実際に動かす ----
        Vector3 velocity = moveDir * moveSpeed;
        velocity.y = verticalVelocity;
        controller.Move(velocity * Time.deltaTime);

        SetAnimSpeed(moveDir.magnitude);
        UpdateFootstep(moveDir.magnitude);
    }

    // ---------- 足音（地面を走っている間、一定間隔で鳴らす）----------

    private void UpdateFootstep(float speed)
    {
        if (!controller.isGrounded || speed < 0.1f)
        {
            footstepTimer = 0f;         // 止まった／跳んだら、次に動き出した瞬間に鳴るようにリセット
            return;
        }

        footstepTimer -= Time.deltaTime;
        if (footstepTimer <= 0f)
        {
            SoundLibrary.PlayFootstep();
            footstepTimer = GameConstants.FootstepInterval;
        }
    }

    // ---------- アニメーション（モデルがあるときだけ動く）----------

    private bool HasAnimator()
    {
        return animator != null && animator.runtimeAnimatorController != null;
    }

    private void SetAnimSpeed(float speed)
    {
        if (HasAnimator()) animator.SetFloat("Speed", speed);
    }

    private void SetAnimTrigger(string name)
    {
        if (HasAnimator()) animator.SetTrigger(name);
    }
}
