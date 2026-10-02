using UnityEngine;

/// <summary>
/// フィールドに置いてあるアイテム。プレイヤーが触れると拾える。
/// 「何のアイテムか」は ItemData（ScriptableObject）を Inspector で差すだけで決まる。
/// </summary>
public class ItemPickup : MonoBehaviour
{
    [SerializeField] private ItemData itemData;
    [SerializeField] private float spinSpeed = 90f;     // くるくる回して目立たせる（度/秒）

    private void Start()
    {
        // 仮モデル（ひし形）のときだけ、データの色を付けて種類を見分けられるようにする。
        // 本物のモデル（Asset Store 素材）に差し替わっているときは、その見た目をそのまま使う。
        ModelLoader loader = GetComponentInChildren<ModelLoader>();
        bool usingPlaceholder = (loader == null || !loader.ModelLoaded);

        Renderer r = GetComponentInChildren<Renderer>();
        if (usingPlaceholder && r != null && itemData != null) r.material.color = itemData.color;
    }

    private void Update()
    {
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(GameConstants.TagPlayer)) return;

        PlayerInventory inventory = other.GetComponent<PlayerInventory>();
        PlayerHealth health = other.GetComponent<PlayerHealth>();

        if (itemData.itemType == ItemType.Potion)
        {
            // 回復薬はその場で使う
            health.Heal(itemData.healAmount);
            UIManager.Instance.ShowMessage("Used " + itemData.itemName + "!  HP +" + itemData.healAmount);
        }
        else
        {
            // 装備品は持ち物に入れる。同じものを持っていたら拾わない。
            if (!inventory.AddItem(itemData))
            {
                UIManager.Instance.ShowMessage("You already have " + itemData.itemName + ".");
                return;
            }
            UIManager.Instance.ShowMessage("Got " + itemData.itemName + "!" + DescribeEffect());
        }

        ScoreManager.Instance.AddScore(itemData.scoreValue);
        EffectLibrary.Play("Pickup", transform.position);
        SoundLibrary.Play("Pickup");
        Destroy(gameObject);
    }

    private string DescribeEffect()
    {
        if (itemData.attackBonus > 0) return "  ATK +" + itemData.attackBonus;
        if (itemData.defenseBonus > 0) return "  DEF +" + itemData.defenseBonus;
        return "";
    }
}
