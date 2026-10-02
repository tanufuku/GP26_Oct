using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 効果音・BGM を名前で鳴らす小さな道具箱（EffectLibrary の音版）。
///   SoundLibrary.Play("Jump");        // 効果音を 1 回鳴らす
///   SoundLibrary.PlayBgm("BgmMain");  // BGM をループ再生（同じ曲ならそのまま）
/// と書くだけで、Resources/Sounds/<名前>（AudioClip）があれば鳴らす。無ければ何もしない。
///
/// 音のファイルは 2 か所から読まれる（Resources.Load はどちらも見る）：
///   ・Assets/Resources/Sounds/            ← 各自がダウンロードして置く本物の音（Git には入れない：フリー素材は再配布禁止）。名前の一覧は docs/アセット導入手順.md
///   ・Assets/_Generated/Resources/Sounds/ ← メニュー ③ が作る仮の音（Git には入れない）。本物がある名前は作らない
/// 「あれば鳴らす・無ければ黙る」の判断をここに閉じ込めてあるので、呼ぶ側は音の有無を気にしなくてよい。
/// ※ 授業のテーマ（設計）とは関係ない「配布の都合」の裏方スクリプト。Play / PlayBgm の使い方だけ分かればよい。
/// </summary>
public static class SoundLibrary
{
    private const string ResourceFolder = "Sounds/";
    private const int MaxFootsteps = 20;      // Footstep_01 から順に、あるところまで数える

    // 一度読み込んだ AudioClip は覚えておく（毎回 Resources.Load すると遅い）
    private static readonly Dictionary<string, AudioClip> cache = new Dictionary<string, AudioClip>();
    private static int footstepCount = -1;    // -1 = まだ数えていない
    private static AudioSource bgmSource;     // BGM 用（ループ再生する 1 本だけ）
    private static string bgmName;

    // ---------- 効果音 ----------

    /// <summary>名前の効果音を鳴らす。クリップが無ければ何もしない。</summary>
    public static void Play(string name)
    {
        Play(name, 1f);
    }

    /// <summary>音量の倍率付き（足音など、少し小さく鳴らしたいとき用）</summary>
    public static void Play(string name, float volumeScale)
    {
        AudioClip clip = LoadClip(name);
        if (clip == null) return;

        // 使い捨ての AudioSource を作って鳴らし、鳴り終わったら消す。
        // 2D 音（spatialBlend = 0）にして、カメラとの距離に関係なく同じ大きさで聞こえるようにする。
        GameObject go = new GameObject("Sound_" + name);
        AudioSource source = go.AddComponent<AudioSource>();
        source.clip = clip;
        source.spatialBlend = 0f;
        source.volume = GameConstants.SeVolume * volumeScale;
        source.Play();
        Object.Destroy(go, clip.length + 0.1f);
    }

    /// <summary>足音を 1 回鳴らす（Footstep_01, 02, … のうち用意されているものからランダム。無ければ何もしない）</summary>
    public static void PlayFootstep()
    {
        if (footstepCount < 0) footstepCount = CountFootsteps();
        if (footstepCount == 0) return;

        int index = Random.Range(1, footstepCount + 1);
        Play("Footstep_" + index.ToString("00"), 0.5f);
    }

    /// <summary>その名前の音が用意されているか</summary>
    public static bool Has(string name)
    {
        return LoadClip(name) != null;
    }

    // ---------- BGM ----------

    /// <summary>BGM をループ再生する。既に同じ曲が鳴っていればそのまま。クリップが無ければ止めるだけ。</summary>
    public static void PlayBgm(string name)
    {
        if (bgmSource != null && bgmName == name && bgmSource.isPlaying) return;

        AudioClip clip = LoadClip(name);
        if (clip == null)
        {
            StopBgm();
            return;
        }

        if (bgmSource == null)
        {
            GameObject go = new GameObject("BGM");
            bgmSource = go.AddComponent<AudioSource>();
            bgmSource.spatialBlend = 0f;
            bgmSource.loop = true;
        }
        bgmSource.clip = clip;
        bgmSource.volume = GameConstants.BgmVolume;
        bgmSource.Play();
        bgmName = name;
    }

    public static void StopBgm()
    {
        if (bgmSource != null) bgmSource.Stop();
        bgmName = null;
    }

    // ---------- 内部 ----------

    private static AudioClip LoadClip(string name)
    {
        AudioClip clip;
        if (cache.TryGetValue(name, out clip)) return clip;

        clip = Resources.Load<AudioClip>(ResourceFolder + name);
        cache[name] = clip;   // 見つからなかった（null）ことも覚えておき、何度も探しに行かない
        return clip;
    }

    private static int CountFootsteps()
    {
        int count = 0;
        for (int i = 1; i <= MaxFootsteps; i++)
        {
            if (!Has("Footstep_" + i.ToString("00"))) break;
            count++;
        }
        return count;
    }
}
