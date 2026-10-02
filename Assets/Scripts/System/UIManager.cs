using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 画面の表示をまとめて担当する。
/// HP バー・スコア・持ち物・メッセージ・状態ごとのパネル（タイトル/ポーズ/ゲームオーバー/クリア）。
///
/// ゲームのロジック側（PlayerHealth など）は「HP が変わった」と UIManager に伝えるだけで、
/// 「どの Text にどう書くか」はここだけが知っている。
/// こうしておくと UI のデザインを変えてもロジック側を触らなくて済む。
/// </summary>
public class UIManager : MonoBehaviour
{
    // どこからでも UIManager.Instance.ShowMessage(...) と呼べるようにする入口（GameManager と同じ形）
    public static UIManager Instance { get; private set; }

    [Header("HUD（プレイ中に常に見える表示）")]
    [SerializeField] private GameObject hudRoot;
    [SerializeField] private Image hpFill;                  // HP バーの中身（fillAmount で長さを変える）
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI inventoryText;
    [SerializeField] private TextMeshProUGUI messageText;   // 画面中央に一時的に出るメッセージ

    [Header("状態ごとのパネル")]
    [SerializeField] private GameObject titlePanel;
    [SerializeField] private GameObject continueHint;       // タイトルの「C で続きから」（セーブがあるときだけ表示）
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject clearPanel;
    [SerializeField] private TextMeshProUGUI clearScoreText;

    private Coroutine messageRoutine;

    private void Awake()
    {
        Instance = this;
        if (messageText != null) messageText.text = "";
    }

    // ---------- HUD ----------

    public void UpdateHP(int current, int max)
    {
        if (hpFill != null) hpFill.fillAmount = (float)current / max;
        if (hpText != null) hpText.text = "HP " + current + " / " + max;
    }

    public void UpdateScore(int score)
    {
        if (scoreText != null) scoreText.text = "SCORE " + score;
    }

    public void UpdateInventory(PlayerInventory inventory)
    {
        if (inventoryText == null) return;

        // 持ち物の名前を「, 」でつなぐ（例 "Sword, Shield"）。何も無ければ "none"
        string names = inventory.GetItemNamesJoined(", ");
        if (names == "") names = "none";

        inventoryText.text = "Items: " + names + "   Key x" + inventory.KeyCount;
    }

    /// <summary>画面中央に少しの間メッセージを出す（前のメッセージは上書き）</summary>
    public void ShowMessage(string message)
    {
        if (messageText == null) return;

        if (messageRoutine != null) StopCoroutine(messageRoutine);
        messageRoutine = StartCoroutine(MessageRoutine(message));
    }

    private IEnumerator MessageRoutine(string message)
    {
        messageText.text = message;
        yield return new WaitForSeconds(GameConstants.MessageDuration);
        messageText.text = "";
    }

    // ---------- 状態パネル ----------

    /// <summary>GameManager から状態が変わるたびに呼ばれる。状態に合うパネルだけを表示する。</summary>
    public void ShowStatePanel(GameState state)
    {
        SetActiveSafe(titlePanel, state == GameState.Title);
        SetActiveSafe(pausePanel, state == GameState.Paused);
        SetActiveSafe(gameOverPanel, state == GameState.GameOver);
        SetActiveSafe(clearPanel, state == GameState.Clear);

        // HUD はプレイ中とポーズ中だけ
        SetActiveSafe(hudRoot, state == GameState.Playing || state == GameState.Paused);

        if (state == GameState.Title)
        {
            SetActiveSafe(continueHint, SaveManager.Instance.HasSave());
        }

        if (state == GameState.Clear && clearScoreText != null)
        {
            clearScoreText.text = "SCORE " + ScoreManager.Instance.Score
                                + "\nHIGH SCORE " + ScoreManager.Instance.HighScore;
        }
    }

    // Inspector で未設定でも落ちないようにする小さな工夫
    private void SetActiveSafe(GameObject go, bool active)
    {
        if (go != null) go.SetActive(active);
    }
}
