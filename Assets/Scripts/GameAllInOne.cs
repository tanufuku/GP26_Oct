using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

// ゲームのスクリプト。これ1つで全部動く。
// kind に "player" とか "enemy" とか入れて使い分ける
public class GameAllInOne : MonoBehaviour
{
    public static GameAllInOne G;        // manager
    public static bool skipTitle = false;

    public string kind = "player";

    // manager用
    public int state = 0;   // 0 title 1 playing 2 pause 3 gameover 4 clear
    public int score = 0;
    public int highScore = 0;
    public Image hpFill;
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI invText;
    public TextMeshProUGUI msgText;
    public TextMeshProUGUI clearScoreText;
    public GameObject hud;
    public GameObject titlePanel;
    public GameObject continueHint;
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    public GameObject clearPanel;
    public float msgTimer = 0;

    // player用
    public int hp = 10;
    public int maxHp = 10;
    public int atk = 1;
    public int weaponAtk = 0;
    public int def = 0;
    public int keys = 0;
    public string items = "";
    public float speed = 5f;
    public float jump = 7f;
    public Transform model;
    public Transform firePoint;
    public bool invincible = false;
    float vy = 0;
    float cooldown = 0;
    float invTimer = 0;
    float blinkTimer = 0;
    bool visible = true;
    float flinchT = -1;
    Vector3 modelRest;
    CharacterController cc;
    Renderer[] rends;
    List<Material> mats = new List<Material>();
    List<Color> matColors = new List<Color>();
    bool wasGrounded = true;
    bool dead = false;

    // enemy用
    public int enemyType = 0;   // 0 patrol 1 chaser 2 ranged 3 boss
    public string enemyName = "Enemy";
    public int ehp = 3;
    public int eatk = 1;
    public float espeed = 2f;
    public float edetect = 6f;
    public float erange = 1.5f;
    public float einterval = 1.5f;
    public int escore = 100;
    public Color ecolor = Color.white;
    public float escale = 1f;
    public bool isBoss = false;
    public Vector3 wp1;
    public Vector3 wp2;
    public float keepDist = 6f;
    int wpIndex = 0;
    float atkTimer = 0;
    float flashTimer = -1;
    Transform player;
    GameAllInOne playerScript;

    // bullet用
    public Vector3 dir;
    public float bspeed = 16f;
    public int bdmg = 1;
    public float life = 1.5f;

    // item用
    public int itemType = 0;    // 0 sword 1 shield 2 potion
    public string itemName = "Sword";
    public int bonusAtk = 0;
    public int bonusDef = 0;
    public int heal = 0;
    public Color icolor = Color.white;
    public float spin = 90f;

    // door用
    public bool needsKey = false;
    public bool isOpen = false;
    float openT = -1;
    Vector3 doorStart;

    // chest用
    public int contentType = -1;    // -1 nothing 0 sword 1 shield 2 potion
    public int contentKeys = 0;
    public Transform lid;
    bool opened = false;
    float lidT = -1;
    Quaternion lidStart;
    bool contentGiven = false;

    // switch用
    public GameAllInOne targetDoor;
    public Color pressedColor = Color.green;
    bool pressed = false;

    // checkpoint用
    bool activated = false;

    // camera用
    public Transform target;
    public Vector3 offset = new Vector3(0, 10, -7);
    public float smooth = 8f;

    void Awake()
    {
        if (kind == "manager")
        {
            G = this;
            Time.timeScale = 1;
            highScore = PlayerPrefs.GetInt("HighScore", 0);
        }
        if (kind == "player")
        {
            cc = GetComponent<CharacterController>();
            if (model != null) modelRest = model.localPosition;
        }
    }

    void Start()
    {
        if (kind == "manager")
        {
            if (skipTitle)
            {
                skipTitle = false;
                if (PlayerPrefs.GetInt("HasSave", 0) == 1) Load();
                SetState(1);
            }
            else
            {
                SetState(0);
            }
            if (scoreText != null) scoreText.text = "SCORE " + score;
        }
        if (kind == "player")
        {
            rends = GetComponentsInChildren<Renderer>();
            foreach (Renderer r in rends)
            {
                foreach (Material m in r.materials)
                {
                    if (m.HasProperty("_BaseColor") || m.HasProperty("_Color")) { mats.Add(m); matColors.Add(m.color); }
                }
            }
            UpdateHpUI();
            UpdateInvUI();
        }
        if (kind == "enemy")
        {
            transform.localScale = Vector3.one * escale;
            GameObject p = GameObject.FindWithTag("Player");
            if (p != null) { player = p.transform; playerScript = p.GetComponent<GameAllInOne>(); }
            rends = GetComponentsInChildren<Renderer>();
            foreach (Renderer r in rends)
            {
                foreach (Material m in r.materials)
                {
                    if (m.HasProperty("_BaseColor") || m.HasProperty("_Color")) { m.color = ecolor; mats.Add(m); matColors.Add(m.color); }
                }
            }
        }
        if (kind == "item")
        {
            Renderer r = GetComponentInChildren<Renderer>();
            if (r != null) r.material.color = icolor;
        }
        if (kind == "door")
        {
            doorStart = transform.position;
        }
        if (kind == "chest")
        {
            if (lid != null) lidStart = lid.localRotation;
        }
    }

    void Update()
    {
        if (kind == "manager")
        {
            // キー入力
            if (state == 0)
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                {
                    PlayerPrefs.DeleteKey("HasSave"); PlayerPrefs.DeleteKey("PosX"); PlayerPrefs.DeleteKey("PosY"); PlayerPrefs.DeleteKey("PosZ");
                    PlayerPrefs.DeleteKey("HP"); PlayerPrefs.DeleteKey("Score"); PlayerPrefs.DeleteKey("Keys"); PlayerPrefs.DeleteKey("Items"); PlayerPrefs.Save();
                    SetState(1);
                }
                else if (Input.GetKeyDown(KeyCode.C) && PlayerPrefs.GetInt("HasSave", 0) == 1)
                {
                    Load();
                    SetState(1);
                }
            }
            else if (state == 1)
            {
                if (Input.GetKeyDown(KeyCode.Escape)) SetState(2);
            }
            else if (state == 2)
            {
                if (Input.GetKeyDown(KeyCode.Escape)) SetState(1);
            }
            else if (state == 3)
            {
                if (Input.GetKeyDown(KeyCode.R)) { skipTitle = true; SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
            }
            else if (state == 4)
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) { skipTitle = false; SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
            }
            // メッセージ
            if (msgTimer > 0)
            {
                msgTimer -= Time.deltaTime;
                if (msgTimer <= 0 && msgText != null) msgText.text = "";
            }
        }
        else if (kind == "player")
        {
            cooldown -= Time.deltaTime;
            // 無敵点滅
            if (invincible)
            {
                invTimer -= Time.deltaTime;
                blinkTimer -= Time.deltaTime;
                if (blinkTimer <= 0)
                {
                    visible = !visible;
                    foreach (Renderer r in rends) if (r != null) r.enabled = visible;
                    blinkTimer = 0.1f;
                }
                if (invTimer <= 0)
                {
                    invincible = false;
                    foreach (Renderer r in rends) if (r != null) r.enabled = true;
                }
            }
            // のけぞり
            if (flinchT >= 0 && model != null)
            {
                flinchT += Time.deltaTime;
                if (flinchT < 0.05f) model.localPosition = Vector3.Lerp(modelRest, modelRest - Vector3.forward * 0.3f, flinchT / 0.05f);
                else if (flinchT < 0.25f) model.localPosition = Vector3.Lerp(modelRest - Vector3.forward * 0.3f, modelRest, (flinchT - 0.05f) / 0.2f);
                else { model.localPosition = modelRest; flinchT = -1; for (int i = 0; i < mats.Count; i++) mats[i].color = matColors[i]; }
            }
            if (G == null || G.state != 1) return;
            if (dead) return;

            float h = Input.GetAxis("Horizontal");
            float v = Input.GetAxis("Vertical");
            Vector3 move = new Vector3(h, 0, v);
            if (move.magnitude > 1) move.Normalize();
            if (move.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(move), 720 * Time.deltaTime);
            }
            if (cc.isGrounded)
            {
                vy = -1;
                if (Input.GetKeyDown(KeyCode.Space)) vy = jump;
            }
            wasGrounded = cc.isGrounded;
            vy += -20f * Time.deltaTime;
            Vector3 vel = move * speed;
            vel.y = vy;
            cc.Move(vel * Time.deltaTime);

            // 攻撃
            if (cooldown <= 0 && (Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0)))
            {
                cooldown = 0.35f;
                Vector3 pos = firePoint != null ? firePoint.position : transform.position + Vector3.up * 1f + transform.forward * 0.5f;
                GameObject b = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                b.name = "PlayerBullet";
                b.transform.position = pos;
                b.transform.localScale = Vector3.one * 0.6f;
                b.GetComponent<SphereCollider>().isTrigger = true;
                Rigidbody rb = b.AddComponent<Rigidbody>(); rb.isKinematic = true; rb.useGravity = false;
                Material bm = new Material(Shader.Find("Universal Render Pipeline/Lit")); bm.color = new Color(0.4f, 0.9f, 1f); b.GetComponent<Renderer>().material = bm;
                GameAllInOne bs = b.AddComponent<GameAllInOne>();
                bs.kind = "bullet"; bs.dir = transform.forward; bs.bspeed = 16f; bs.bdmg = atk + weaponAtk; bs.life = 1.5f;
            }
        }
        else if (kind == "enemy")
        {
            if (flashTimer >= 0)
            {
                flashTimer -= Time.deltaTime;
                if (flashTimer < 0) for (int i = 0; i < mats.Count; i++) mats[i].color = matColors[i];
            }
            if (dead || player == null) return;
            if (G == null || G.state != 1) return;

            float dist = Vector3.Distance(transform.position, player.position);
            // 移動
            if (enemyType == 0)
            {
                // patrol
                if (dist <= erange)
                {
                    Look(player.position - transform.position);
                }
                else
                {
                    Vector3 tgt = wpIndex == 0 ? wp1 : wp2;
                    MoveTo(tgt, espeed);
                    Vector3 d = tgt - transform.position; d.y = 0;
                    if (d.magnitude < 0.3f) wpIndex = 1 - wpIndex;
                }
            }
            else if (enemyType == 1 || enemyType == 3)
            {
                // chaser / boss
                if (dist > edetect) { }
                else if (dist > erange * 0.8f) MoveTo(player.position, espeed);
                else Look(player.position - transform.position);
            }
            else if (enemyType == 2)
            {
                // ranged
                if (dist <= edetect)
                {
                    Look(player.position - transform.position);
                    Vector3 toP = player.position - transform.position; toP.y = 0; toP.Normalize();
                    if (dist < keepDist - 1) { MoveTo(transform.position - toP * 2, espeed); Look(player.position - transform.position); }
                    else if (dist > keepDist + 1) MoveTo(player.position, espeed);
                }
            }
            // 攻撃
            atkTimer -= Time.deltaTime;
            if (dist <= erange && atkTimer <= 0)
            {
                atkTimer = einterval;
                if (enemyType == 2)
                {
                    Vector3 pos = firePoint != null ? firePoint.position : transform.position + Vector3.up * 1f;
                    Vector3 d = (player.position + Vector3.up * 0.9f) - pos;
                    GameObject b = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    b.name = "EnemyBullet";
                    b.tag = "EnemyBullet";
                    b.transform.position = pos;
                    b.transform.localScale = Vector3.one * 0.4f;
                    b.GetComponent<SphereCollider>().isTrigger = true;
                    Rigidbody rb = b.AddComponent<Rigidbody>(); rb.isKinematic = true; rb.useGravity = false;
                    Material bm = new Material(Shader.Find("Universal Render Pipeline/Lit")); bm.color = new Color(1f, 0.4f, 0.1f); b.GetComponent<Renderer>().material = bm;
                    GameAllInOne bs = b.AddComponent<GameAllInOne>();
                    bs.kind = "ebullet"; bs.dir = d.normalized; bs.bspeed = 8f; bs.bdmg = eatk; bs.life = 4f;
                }
                else
                {
                    if (playerScript != null) playerScript.Damage(eatk);
                }
            }
        }
        else if (kind == "bullet" || kind == "ebullet")
        {
            transform.position += dir * bspeed * Time.deltaTime;
            life -= Time.deltaTime;
            if (life <= 0) Destroy(gameObject);
        }
        else if (kind == "item")
        {
            transform.Rotate(0, spin * Time.deltaTime, 0);
        }
        else if (kind == "key")
        {
            transform.Rotate(0, 120 * Time.deltaTime, 0);
        }
        else if (kind == "door")
        {
            if (openT >= 0)
            {
                openT += Time.deltaTime;
                float t = Mathf.Clamp01(openT / 1.5f);
                transform.position = Vector3.Lerp(doorStart, doorStart + Vector3.down * 2.4f, t);
                if (t >= 1)
                {
                    openT = -1;
                    foreach (Collider c in GetComponents<Collider>()) c.enabled = false;
                }
            }
        }
        else if (kind == "chest")
        {
            if (lidT >= 0)
            {
                lidT += Time.deltaTime;
                float t = Mathf.Clamp01(lidT / 1.0f);
                if (lid != null) lid.localRotation = Quaternion.Slerp(lidStart, lidStart * Quaternion.Euler(-110, 0, 0), t);
                if (t >= 1 && !contentGiven)
                {
                    contentGiven = true;
                    lidT = -1;
                    GameAllInOne p = GameObject.FindWithTag("Player").GetComponent<GameAllInOne>();
                    G.AddScore(100);
                    if (contentKeys > 0) { p.keys += contentKeys; p.UpdateInvUI(); G.Msg("Found a Key in the chest!"); }
                    if (contentType == 2) { p.hp = Mathf.Min(p.maxHp, p.hp + 4); p.UpdateHpUI(); G.Msg("Found Potion!  HP +4"); }
                    else if (contentType == 0) { if (!p.items.Contains("Sword")) { p.items = p.items == "" ? "Sword" : p.items + ",Sword"; p.weaponAtk += 2; p.UpdateInvUI(); G.Msg("Found Sword in the chest!"); } }
                    else if (contentType == 1) { if (!p.items.Contains("Shield")) { p.items = p.items == "" ? "Shield" : p.items + ",Shield"; p.def += 1; p.UpdateInvUI(); G.Msg("Found Shield in the chest!"); } }
                }
            }
        }
    }

    void LateUpdate()
    {
        if (kind == "camera" && target != null)
        {
            transform.position = Vector3.Lerp(transform.position, target.position + offset, smooth * Time.deltaTime);
            transform.LookAt(target.position + Vector3.up * 1f);
        }
    }

    void MoveTo(Vector3 tgt, float spd)
    {
        Vector3 d = tgt - transform.position; d.y = 0;
        if (d.magnitude < 0.1f) return;
        d.Normalize();
        transform.position += d * spd * Time.deltaTime;
        Look(d);
    }

    void Look(Vector3 d)
    {
        d.y = 0;
        if (d.sqrMagnitude < 0.001f) return;
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(d), 540 * Time.deltaTime);
    }

    // プレイヤーがダメージを受ける
    public void Damage(int amount)
    {
        if (invincible || dead) return;
        int d = Mathf.Max(1, amount - def);
        hp = Mathf.Max(0, hp - d);
        UpdateHpUI();
        if (hp <= 0)
        {
            dead = true;
            foreach (Renderer r in rends) if (r != null) r.enabled = true;
            for (int i = 0; i < mats.Count; i++) mats[i].color = matColors[i];
            G.SetState(3);
        }
        else
        {
            for (int i = 0; i < mats.Count; i++) mats[i].color = new Color(1f, 0.35f, 0.35f);
            flinchT = 0;
            invincible = true; invTimer = 1.0f; blinkTimer = 0; visible = true;
        }
    }

    // 敵がダメージを受ける
    public void EnemyDamage(int amount)
    {
        if (dead) return;
        ehp -= amount;
        for (int i = 0; i < mats.Count; i++) mats[i].color = Color.white;
        flashTimer = 0.08f;
        if (ehp <= 0)
        {
            dead = true;
            G.AddScore(escore);
            G.Msg("Defeated " + enemyName + "!  +" + escore);
            if (isBoss)
            {
                if (G.score > G.highScore) { G.highScore = G.score; PlayerPrefs.SetInt("HighScore", G.highScore); PlayerPrefs.Save(); }
                G.SetState(4);
            }
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (kind == "bullet")
        {
            GameAllInOne e = other.GetComponentInParent<GameAllInOne>();
            if (e != null && e.kind == "enemy") { e.EnemyDamage(bdmg); Destroy(gameObject); return; }
            if (other.isTrigger) return;
            if (other.CompareTag("Player")) return;
            Destroy(gameObject);
        }
        else if (kind == "ebullet")
        {
            if (other.CompareTag("Player")) { other.GetComponent<GameAllInOne>().Damage(bdmg); Destroy(gameObject); return; }
            if (other.isTrigger) return;
            GameAllInOne e = other.GetComponentInParent<GameAllInOne>();
            if (e != null && e.kind == "enemy") return;
            Destroy(gameObject);
        }
        else if (kind == "item")
        {
            if (!other.CompareTag("Player")) return;
            GameAllInOne p = other.GetComponent<GameAllInOne>();
            if (itemType == 2)
            {
                p.hp = Mathf.Min(p.maxHp, p.hp + heal); p.UpdateHpUI();
                G.Msg("Used " + itemName + "!  HP +" + heal);
            }
            else
            {
                if (p.items.Contains(itemName)) { G.Msg("You already have " + itemName + "."); return; }
                p.items = p.items == "" ? itemName : p.items + "," + itemName;
                p.weaponAtk += bonusAtk; p.def += bonusDef; p.UpdateInvUI();
                G.Msg("Got " + itemName + "!" + (bonusAtk > 0 ? "  ATK +" + bonusAtk : bonusDef > 0 ? "  DEF +" + bonusDef : ""));
            }
            G.AddScore(50);
            Destroy(gameObject);
        }
        else if (kind == "key")
        {
            if (!other.CompareTag("Player")) return;
            GameAllInOne p = other.GetComponent<GameAllInOne>();
            p.keys += 1; p.UpdateInvUI();
            G.AddScore(50);
            G.Msg("Got a Key!");
            Destroy(gameObject);
        }
        else if (kind == "door")
        {
            if (!needsKey || isOpen) return;
            if (!other.CompareTag("Player")) return;
            GameAllInOne p = other.GetComponent<GameAllInOne>();
            if (p.keys > 0) { p.keys--; p.UpdateInvUI(); G.Msg("Used a Key.  The door is opening..."); OpenDoor(); }
            else G.Msg("Locked.  You need a Key.");
        }
        else if (kind == "chest")
        {
            if (opened) return;
            if (!other.CompareTag("Player")) return;
            GameAllInOne p = other.GetComponent<GameAllInOne>();
            if (needsKey)
            {
                if (p.keys <= 0) { G.Msg("The chest is locked.  You need a Key."); return; }
                p.keys--; p.UpdateInvUI();
            }
            opened = true; lidT = 0;
        }
        else if (kind == "switch")
        {
            if (pressed) return;
            if (!other.CompareTag("Player")) return;
            pressed = true;
            Renderer r = GetComponent<Renderer>();
            if (r != null) r.material.color = pressedColor;
            if (targetDoor != null) { targetDoor.OpenDoor(); G.Msg("Switch ON!  The door is opening..."); }
        }
        else if (kind == "checkpoint")
        {
            if (activated) return;
            if (!other.CompareTag("Player")) return;
            activated = true;
            GameAllInOne p = other.GetComponent<GameAllInOne>();
            Vector3 pos = transform.position + Vector3.up * 1f;
            PlayerPrefs.SetInt("HasSave", 1);
            PlayerPrefs.SetFloat("PosX", pos.x); PlayerPrefs.SetFloat("PosY", pos.y); PlayerPrefs.SetFloat("PosZ", pos.z);
            PlayerPrefs.SetInt("HP", p.hp); PlayerPrefs.SetInt("Score", G.score); PlayerPrefs.SetInt("Keys", p.keys); PlayerPrefs.SetString("Items", p.items);
            PlayerPrefs.Save();
            Renderer r = GetComponent<Renderer>();
            if (r != null) r.material.color = new Color(0.3f, 1f, 0.5f);
            G.Msg("Checkpoint!  Progress saved.");
        }
    }

    public void OpenDoor()
    {
        if (isOpen) return;
        isOpen = true;
        openT = 0;
    }

    public void UpdateHpUI()
    {
        if (G == null) return;
        if (G.hpFill != null) G.hpFill.fillAmount = (float)hp / maxHp;
        if (G.hpText != null) G.hpText.text = "HP " + hp + " / " + maxHp;
    }

    public void UpdateInvUI()
    {
        if (G == null || G.invText == null) return;
        string n = items == "" ? "none" : items.Replace(",", ", ");
        G.invText.text = "Items: " + n + "   Key x" + keys;
    }

    public void AddScore(int a)
    {
        score += a;
        if (scoreText != null) scoreText.text = "SCORE " + score;
    }

    public void Msg(string m)
    {
        if (msgText == null) return;
        msgText.text = m;
        msgTimer = 2.5f;
    }

    public void SetState(int s)
    {
        state = s;
        Time.timeScale = s == 2 ? 0 : 1;
        if (titlePanel != null) titlePanel.SetActive(s == 0);
        if (pausePanel != null) pausePanel.SetActive(s == 2);
        if (gameOverPanel != null) gameOverPanel.SetActive(s == 3);
        if (clearPanel != null) clearPanel.SetActive(s == 4);
        if (hud != null) hud.SetActive(s == 1 || s == 2);
        if (s == 0 && continueHint != null) continueHint.SetActive(PlayerPrefs.GetInt("HasSave", 0) == 1);
        if (s == 4 && clearScoreText != null) clearScoreText.text = "SCORE " + score + "\nHIGH SCORE " + highScore;
    }

    void Load()
    {
        GameObject pl = GameObject.FindWithTag("Player");
        if (pl == null) return;
        GameAllInOne p = pl.GetComponent<GameAllInOne>();
        Vector3 pos = new Vector3(PlayerPrefs.GetFloat("PosX"), PlayerPrefs.GetFloat("PosY"), PlayerPrefs.GetFloat("PosZ"));
        CharacterController c = pl.GetComponent<CharacterController>();
        if (c != null) c.enabled = false;
        pl.transform.position = pos;
        if (c != null) c.enabled = true;
        p.hp = PlayerPrefs.GetInt("HP", 10);
        score = PlayerPrefs.GetInt("Score", 0);
        if (scoreText != null) scoreText.text = "SCORE " + score;
        p.keys = PlayerPrefs.GetInt("Keys", 0);
        p.items = PlayerPrefs.GetString("Items", "");
        p.weaponAtk = 0; p.def = 0;
        if (p.items.Contains("Sword")) p.weaponAtk += 2;
        if (p.items.Contains("Shield")) p.def += 1;
        p.UpdateHpUI();
        p.UpdateInvUI();
    }
}
