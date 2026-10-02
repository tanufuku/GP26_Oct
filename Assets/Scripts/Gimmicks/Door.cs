using System.Collections;
using UnityEngine;

/// <summary>
/// 扉。Open() を呼ばれると、ゆっくり床の下に沈んで通れるようになる。
/// needsKey が true の扉は、プレイヤーが近づいたときに鍵を持っていれば自動で開く。
///
/// この GameObject には Collider が2つ付いている：
///   ・通れなくする普通の Collider（壁の役）
///   ・少し大きい isTrigger の Collider（「近づいた」を検知する役）
/// </summary>
public class Door : MonoBehaviour
{
    [SerializeField] private bool needsKey = false;
    [SerializeField] private float openDistance = 2.4f;     // どれだけ沈むか（扉の高さ以上にする）
    [SerializeField] private float openTime = GameConstants.DoorOpenTime;

    public bool IsOpen { get; private set; }

    /// <summary>扉を開ける（Switch から呼ばれる／鍵で開く）</summary>
    public void Open()
    {
        if (IsOpen) return;

        IsOpen = true;
        SoundLibrary.Play("Door");
        StartCoroutine(OpenRoutine());
    }

    /// <summary>
    /// 一気に動かすのではなく、毎フレーム少しずつ位置を変えて「ゆっくり開く」を作る。
    /// 「時間をかけて何かをする」処理はコルーチンが得意。
    /// </summary>
    private IEnumerator OpenRoutine()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.down * openDistance;

        float elapsed = 0f;
        while (elapsed < openTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / openTime);
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;      // 1フレーム待つ
        }

        transform.position = endPos;

        // 床の下に沈んだ扉に引っかからないよう、当たり判定を全部切る
        foreach (Collider col in GetComponents<Collider>())
        {
            col.enabled = false;
        }
    }

    // 鍵付きの扉：近づいたプレイヤーが鍵を持っていれば開く
    private void OnTriggerEnter(Collider other)
    {
        if (!needsKey || IsOpen) return;
        if (!other.CompareTag(GameConstants.TagPlayer)) return;

        PlayerInventory inventory = other.GetComponent<PlayerInventory>();
        if (inventory.UseKey())
        {
            UIManager.Instance.ShowMessage("Used a Key.  The door is opening...");
            Open();
        }
        else
        {
            UIManager.Instance.ShowMessage("Locked.  You need a Key.");
            SoundLibrary.Play("Locked");
        }
    }
}
