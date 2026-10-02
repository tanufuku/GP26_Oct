using UnityEngine;

/// <summary>
/// 床のスイッチ。プレイヤーが踏むと、つながっている Door の Open() を呼ぶ。
/// 「別のオブジェクトのメソッドを呼ぶ」一番シンプルな形：
/// Inspector で相手（targetDoor）を差しておき、その相手のメソッドを呼ぶ。
/// </summary>
public class Switch : MonoBehaviour
{
    [SerializeField] private Door targetDoor;                       // このスイッチで開く扉
    [SerializeField] private Color pressedColor = Color.green;      // 踏んだあとの色

    private bool isPressed;

    private void OnTriggerEnter(Collider other)
    {
        if (isPressed) return;
        if (!other.CompareTag(GameConstants.TagPlayer)) return;

        isPressed = true;
        SoundLibrary.Play("Switch");

        // 見た目を変えて「押された」と分かるようにする
        Renderer r = GetComponent<Renderer>();
        if (r != null) r.material.color = pressedColor;

        if (targetDoor != null)
        {
            targetDoor.Open();
            UIManager.Instance.ShowMessage("Switch ON!  The door is opening...");
        }
    }
}
