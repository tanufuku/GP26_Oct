using UnityEngine;

/// <summary>
/// フィールドに置いてある鍵。触れると PlayerInventory の鍵が1本増える。
/// 鍵は「何本持っているか」だけが大事なので ItemData は使わず、数だけ管理する。
/// </summary>
public class KeyPickup : MonoBehaviour
{
    [SerializeField] private float spinSpeed = 120f;

    private void Update()
    {
        transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(GameConstants.TagPlayer)) return;

        // 触れた相手（プレイヤー）から PlayerInventory を取り出して鍵を渡す
        PlayerInventory inventory = other.GetComponent<PlayerInventory>();
        inventory.AddKey(1);

        ScoreManager.Instance.AddScore(GameConstants.ScoreForKey);
        UIManager.Instance.ShowMessage("Got a Key!");
        EffectLibrary.Play("Pickup", transform.position);
        SoundLibrary.Play("Key");
        Destroy(gameObject);
    }
}
