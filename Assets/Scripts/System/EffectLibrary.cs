using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 演出（パーティクル）を名前で呼び出す小さな道具箱。
///   EffectLibrary.Play("EnemyDie", transform.position);
/// と書くだけで、Resources/Effects/EnemyDie.prefab があればその場に生成し、少ししたら消す。
///
/// 演出の Prefab は Asset Store の Particle Pack から作るので Git には入っていない
/// （docs/アセット導入手順.md の手順で Assets/Resources/Effects/ に自分で作る。講師用のメニュー ③ でも生成できる）。
/// 無ければ何も起きないだけで、ゲームの進行には影響しない。
/// 「あれば出す・無ければ黙る」の判断をここ 1 か所に閉じ込めておけば、
/// 呼ぶ側（敵・アイテム・プレイヤー）は演出の有無を気にせずに済む。
/// ※ 授業のテーマ（設計）とは関係ない「配布の都合」の裏方スクリプト。Play の使い方だけ分かればよい。
/// </summary>
public static class EffectLibrary
{
    private const string ResourceFolder = "Effects/";
    private const float DefaultLifeTime = 3f;    // パーティクルの長さが分からないときの寿命（秒）

    // 一度読み込んだ Prefab は覚えておく（毎回 Resources.Load すると遅い）
    private static readonly Dictionary<string, GameObject> cache = new Dictionary<string, GameObject>();

    /// <summary>名前の演出をその位置に出す。Prefab が無ければ何もしない。</summary>
    public static void Play(string name, Vector3 position)
    {
        GameObject prefab = LoadPrefab(name);
        if (prefab == null) return;

        GameObject instance = Object.Instantiate(prefab, position, Quaternion.identity);
        Object.Destroy(instance, CalcLifeTime(instance));
    }

    /// <summary>その名前の演出が用意されているか（無いときは仮の見た目で代用したい場合に使う）</summary>
    public static bool Has(string name)
    {
        return LoadPrefab(name) != null;
    }

    private static GameObject LoadPrefab(string name)
    {
        GameObject prefab;
        if (cache.TryGetValue(name, out prefab)) return prefab;

        prefab = Resources.Load<GameObject>(ResourceFolder + name);
        cache[name] = prefab;   // 見つからなかった（null）ことも覚えておき、何度も探しに行かない
        return prefab;
    }

    // パーティクルの「再生時間＋粒の寿命」の最大値を寿命にする
    private static float CalcLifeTime(GameObject instance)
    {
        float life = 0f;
        foreach (ParticleSystem ps in instance.GetComponentsInChildren<ParticleSystem>())
        {
            ParticleSystem.MainModule main = ps.main;
            life = Mathf.Max(life, main.duration + main.startLifetime.constantMax);
        }
        return life > 0f ? life + 0.5f : DefaultLifeTime;
    }
}
