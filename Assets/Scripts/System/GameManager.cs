using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ゲーム全体の進行を管理する。
/// 「今どの状態か（GameState）」を1か所で持ち、状態が変わったときに
/// 時間停止や UI の切り替えをまとめて行う。
/// 他のスクリプトは GameManager.Instance.IsPlaying を見て「今動いていいか」を判断する。
/// </summary>
public class GameManager : MonoBehaviour
{
    // どこからでも参照できる入口。シーンに1つだけ置く前提。
    public static GameManager Instance { get; private set; }

    // シーンを読み直したあと「タイトルを飛ばしてすぐ再開するか」。
    // static なのはシーンを読み直しても値が消えないようにするため。
    private static bool skipTitleOnLoad = false;

    [SerializeField] private GameState state = GameState.Title;

    // 起動直後の状態。完成形はタイトルから始める。
    // 授業用の最小シーン（Assets/Scenes/Lessons）ではタイトルを挟まず Playing から始めたいので Inspector で変えられるようにしている。
    [SerializeField] private GameState initialState = GameState.Title;

    /// <summary>現在の状態（外からは読むだけ）</summary>
    public GameState State { get { return state; } }

    /// <summary>プレイ中かどうか。移動・攻撃・敵の行動はこれが true のときだけ動く。</summary>
    public bool IsPlaying { get { return state == GameState.Playing; } }

    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;    // ポーズ中に読み直したときに時間が止まったままにならないように
    }

    private void Start()
    {
        if (skipTitleOnLoad)
        {
            // ゲームオーバーからの再開：セーブがあればチェックポイントから、無ければ最初から
            skipTitleOnLoad = false;
            if (SaveManager.Instance.HasSave())
            {
                SaveManager.Instance.Load();
            }
            ChangeState(GameState.Playing);
        }
        else
        {
            ChangeState(initialState);
        }
    }

    private void Update()
    {
        // 状態ごとに受け付けるキーが違う。switch で分岐すると読みやすい。
        switch (state)
        {
            case GameState.Title:
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                {
                    StartNewGame();
                }
                else if (Input.GetKeyDown(KeyCode.C) && SaveManager.Instance.HasSave())
                {
                    ContinueGame();
                }
                break;

            case GameState.Playing:
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    ChangeState(GameState.Paused);
                }
                break;

            case GameState.Paused:
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    ChangeState(GameState.Playing);
                }
                break;

            case GameState.GameOver:
                if (Input.GetKeyDown(KeyCode.R))
                {
                    RestartFromCheckpoint();
                }
                break;

            case GameState.Clear:
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                {
                    ReturnToTitle();
                }
                break;
        }
    }

    /// <summary>最初から始める（古いセーブは消す）</summary>
    public void StartNewGame()
    {
        SaveManager.Instance.DeleteSave();
        ChangeState(GameState.Playing);
    }

    /// <summary>セーブしたチェックポイントから続きを始める</summary>
    public void ContinueGame()
    {
        SaveManager.Instance.Load();
        ChangeState(GameState.Playing);
    }

    /// <summary>PlayerHealth から「倒れた」と知らせてもらう</summary>
    public void OnPlayerDied()
    {
        ChangeState(GameState.GameOver);
    }

    /// <summary>ボス（EnemyData.isBoss = true の敵）が倒されたら呼ばれる</summary>
    public void OnBossDefeated()
    {
        ScoreManager.Instance.SaveHighScore();
        ChangeState(GameState.Clear);
    }

    /// <summary>シーンを読み直して、タイトルを飛ばしてチェックポイントから再開する</summary>
    public void RestartFromCheckpoint()
    {
        skipTitleOnLoad = true;
        ReloadScene();
    }

    /// <summary>シーンを読み直してタイトルに戻る</summary>
    public void ReturnToTitle()
    {
        skipTitleOnLoad = false;
        ReloadScene();
    }

    /// <summary>
    /// 状態を切り替える。状態変更に伴う処理（時間停止・UI切替）は必ずここを通す。
    /// ばらばらの場所で state を書き換えると、UI だけ古い状態のまま、といったズレが起きる。
    /// </summary>
    private void ChangeState(GameState next)
    {
        GameState previous = state;
        state = next;

        // ポーズ中だけ時間を止める（Update は動くが Time.deltaTime が 0 になる）
        Time.timeScale = (state == GameState.Paused) ? 0f : 1f;

        // 状態が切り替わった瞬間の音（音のファイルが無ければ何も鳴らない）
        switch (state)
        {
            case GameState.Title:
                SoundLibrary.PlayBgm("BgmTitle");
                break;
            case GameState.Playing:
                if (previous == GameState.Title) SoundLibrary.Play("Start");
                SoundLibrary.PlayBgm("BgmMain");        // ポーズ解除で戻ってきたときは同じ曲なので続きから
                break;
            case GameState.Paused:
                SoundLibrary.Play("Pause");
                break;
            case GameState.Clear:
                SoundLibrary.StopBgm();
                SoundLibrary.Play("Clear");
                break;
            case GameState.GameOver:
                SoundLibrary.StopBgm();
                SoundLibrary.Play("GameOver");
                break;
        }

        UIManager.Instance.ShowStatePanel(state);
    }

    private void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
