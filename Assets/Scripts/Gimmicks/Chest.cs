using System.Collections;
using UnityEngine;

/// <summary>
/// 宝箱。プレイヤーが近づくと TryOpen() を試す。
/// needsKey が true なら鍵を1本消費して開く。開くとフタがゆっくり回り、中身が持ち物に入る。
/// 中身はアイテム（ItemData）か鍵（本数）のどちらか、または両方。
/// </summary>
public class Chest : MonoBehaviour
{
    [SerializeField] private bool needsKey = false;
    [SerializeField] private ItemData contentItem;      // 中身のアイテム（無ければ null）
    [SerializeField] private int contentKeys = 0;       // 中身の鍵の本数
    [SerializeField] private Transform lid;             // フタ（回転させる部分）
    [SerializeField] private float lidOpenAngle = -110f;
    [SerializeField] private float openTime = GameConstants.ChestOpenTime;

    public bool IsOpen { get; private set; }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(GameConstants.TagPlayer)) return;
        TryOpen(other.gameObject);
    }

    /// <summary>
    /// 開けようとする。開けられたら true。
    /// 「開けられたかどうか」を戻り値で返すので、呼ぶ側は結果に応じた処理ができる。
    /// </summary>
    public bool TryOpen(GameObject player)
    {
        if (IsOpen) return false;

        PlayerInventory inventory = player.GetComponent<PlayerInventory>();
        PlayerHealth health = player.GetComponent<PlayerHealth>();

        if (needsKey && !inventory.UseKey())
        {
            UIManager.Instance.ShowMessage("The chest is locked.  You need a Key.");
            return false;
        }

        IsOpen = true;
        StartCoroutine(OpenRoutine(inventory, health));
        return true;
    }

    private IEnumerator OpenRoutine(PlayerInventory inventory, PlayerHealth health)
    {
        // 開き始めた瞬間に音（フタが動く間ずっと鳴っているように）
        SoundLibrary.Play("ChestOpen");

        // ---- フタをゆっくり開ける ----
        if (lid != null)
        {
            Quaternion startRot = lid.localRotation;
            Quaternion endRot = startRot * Quaternion.Euler(lidOpenAngle, 0f, 0f);

            float elapsed = 0f;
            while (elapsed < openTime)
            {
                elapsed += Time.deltaTime;
                lid.localRotation = Quaternion.Slerp(startRot, endRot, Mathf.Clamp01(elapsed / openTime));
                yield return null;
            }
            lid.localRotation = endRot;
        }

        // ---- 開き終わってから中身を渡す ----
        EffectLibrary.Play("ChestOpen", transform.position + Vector3.up * 0.8f);
        ScoreManager.Instance.AddScore(GameConstants.ScoreForChest);

        if (contentKeys > 0)
        {
            inventory.AddKey(contentKeys);
            UIManager.Instance.ShowMessage("Found a Key in the chest!");
        }

        if (contentItem != null)
        {
            if (contentItem.itemType == ItemType.Potion)
            {
                health.Heal(contentItem.healAmount);
                UIManager.Instance.ShowMessage("Found " + contentItem.itemName + "!  HP +" + contentItem.healAmount);
            }
            else if (inventory.AddItem(contentItem))
            {
                UIManager.Instance.ShowMessage("Found " + contentItem.itemName + " in the chest!");
            }
        }
    }
}
