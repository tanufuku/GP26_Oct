using UnityEngine;

/// <summary>
/// チェックポイント。プレイヤーが通ると、その場所を再開地点として進行を保存する。
/// 保存の中身（何を PlayerPrefs に書くか）は SaveManager が知っていて、ここは「ここで保存して」と頼むだけ。
/// </summary>
public class Checkpoint : MonoBehaviour
{
    [SerializeField] private Color activatedColor = new Color(0.3f, 1f, 0.5f);

    private bool isActivated;

    private void OnTriggerEnter(Collider other)
    {
        if (isActivated) return;
        if (!other.CompareTag(GameConstants.TagPlayer)) return;

        isActivated = true;

        // 再開位置はチェックポイントの少し上（床に埋まらないように）
        Vector3 respawnPosition = transform.position + Vector3.up * 1f;
        SaveManager.Instance.Save(respawnPosition);

        Renderer r = GetComponent<Renderer>();
        if (r != null) r.material.color = activatedColor;

        UIManager.Instance.ShowMessage("Checkpoint!  Progress saved.");
        EffectLibrary.Play("Checkpoint", transform.position);
        SoundLibrary.Play("Checkpoint");
    }
}
