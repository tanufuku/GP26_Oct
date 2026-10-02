using UnityEngine;

/// <summary>
/// アイテム1種類分のデータ（ScriptableObject）。
/// 「剣」「盾」「回復薬」を、それぞれ .asset ファイルとして Assets/Data/Items に作る。
///
/// なぜ ScriptableObject にするのか：
///   ・数値（攻撃力など）をコードから切り離せるので、プログラムを触らずに調整できる
///   ・同じ剣を10個置いても、データは1つを共有するのでムダがない
///   ・Inspector で一覧・比較しやすい
///
/// ※ データを入れるだけのクラスなので、フィールドは public にしている。
///   （動きを持つ MonoBehaviour では [SerializeField] private を使う、という使い分け）
/// </summary>
[CreateAssetMenu(menuName = "GP26/ItemData", fileName = "NewItemData")]
public class ItemData : ScriptableObject
{
    public string itemName = "Item";        // 表示名（セーブにもこの名前を使う）
    public ItemType itemType = ItemType.Weapon;

    [Header("効果（種類に応じて使うものだけ入れる）")]
    public int attackBonus = 0;             // Weapon：攻撃力への加算
    public int defenseBonus = 0;            // Armor：受けるダメージの減少量
    public int healAmount = 0;              // Potion：回復量

    [Header("見た目・スコア")]
    public Color color = Color.white;       // フィールドに置いたときの色（仮モデル用）
    public int scoreValue = GameConstants.ScoreForItem;
}
