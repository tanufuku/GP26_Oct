using UnityEngine;

/// <summary>
/// スコアとハイスコアを管理する。
/// ハイスコアだけは PlayerPrefs に保存して、ゲームを終了しても残るようにする。
/// </summary>
public class ScoreManager : MonoBehaviour
{
    // どこからでも ScoreManager.Instance.AddScore(...) と呼べるようにする入口（GameManager と同じ形）
    public static ScoreManager Instance { get; private set; }

    /// <summary>今回のプレイのスコア</summary>
    public int Score { get; private set; }

    /// <summary>過去最高のスコア（PlayerPrefs から読み込む）</summary>
    public int HighScore { get; private set; }

    private void Awake()
    {
        Instance = this;
        Score = 0;
        HighScore = PlayerPrefs.GetInt(GameConstants.SaveKeyHighScore, 0);
    }

    private void Start()
    {
        UIManager.Instance.UpdateScore(Score);
    }

    /// <summary>スコアを加算する（敵を倒した・アイテムを拾った、など）</summary>
    public void AddScore(int amount)
    {
        Score += amount;
        UIManager.Instance.UpdateScore(Score);
    }

    /// <summary>セーブデータから読み込んだスコアをそのまま入れる</summary>
    public void SetScore(int value)
    {
        Score = value;
        UIManager.Instance.UpdateScore(Score);
    }

    /// <summary>今回のスコアがハイスコアを超えていたら保存する（クリア時に呼ぶ）</summary>
    public void SaveHighScore()
    {
        if (Score > HighScore)
        {
            HighScore = Score;
            PlayerPrefs.SetInt(GameConstants.SaveKeyHighScore, HighScore);
            PlayerPrefs.Save();
        }
    }
}
