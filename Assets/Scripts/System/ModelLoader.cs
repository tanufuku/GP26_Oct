using UnityEngine;

/// <summary>
/// 見た目（3Dモデル）を、あれば差し替えるスクリプト。キャラクター・アイテム・ステージ装飾で共通に使う。
///
/// このプロジェクトは GitHub で配布するが、Asset Store の素材（キャラクターモデルなど）は
/// ライセンス上 GitHub に含められない。そこで
///   ・シーンには「仮の見た目（カプセルやひし形）」だけを置いておき、
///   ・Resources フォルダに本物の Prefab があれば、起動時にそれと入れ替える
/// という作りにしている。素材が無くてもゲームはそのまま動く。
///
/// 本物の Prefab は docs/アセット導入手順.md の手順で Assets/Resources/ に自分で作って置く
/// （Characters/Player, Characters/Enemy, Items/&lt;アイテム名&gt;, Stage/Decorations）。講師用のメニュー ③ でも同じものを自動生成できる。
/// ※ 授業のテーマ（設計）とは関係ない「配布の都合」の裏方スクリプト。読み飛ばしてよい。
/// </summary>
public class ModelLoader : MonoBehaviour
{
    [SerializeField] private string resourcePath;      // 例: "Characters/Player"（Resources からの相対パス）
    [SerializeField] private GameObject placeholder;   // 本物が見つかったら消す仮モデル

    /// <summary>本物のモデルに差し替わったか</summary>
    public bool ModelLoaded { get; private set; }

    // 他のスクリプトの Start() より前に差し替えを終わらせたいので Awake で行う。
    private void Awake()
    {
        GameObject prefab = Resources.Load<GameObject>(resourcePath);
        if (prefab == null)
        {
            // 本物が無い＝仮モデルのまま。これは正常な状態なのでエラーにはしない。
            return;
        }

        GameObject model = Instantiate(prefab, transform);
        model.name = prefab.name;
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;

        if (placeholder != null)
        {
            // Destroy はフレームの最後に消えるので、先に非表示にして
            // 他のスクリプトの GetComponentsInChildren に拾われないようにする。
            placeholder.SetActive(false);
            Destroy(placeholder);
        }

        ModelLoaded = true;
    }
}
