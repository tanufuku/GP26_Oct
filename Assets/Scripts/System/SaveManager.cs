using UnityEngine;

/// <summary>
/// 進行状況のセーブ・ロード（PlayerPrefs 版）。
/// 保存するもの：プレイヤーの位置・HP・スコア・鍵の数・持っているアイテム。
/// PlayerPrefs は「名前を付けて数値や文字列を保存する小さな引き出し」。
/// 大きなデータには向かないが、この規模のゲームなら十分。
/// </summary>
public class SaveManager : MonoBehaviour
{
    // どこからでも SaveManager.Instance.Save(...) と呼べるようにする入口（GameManager と同じ形）
    public static SaveManager Instance { get; private set; }

    // アイテムは「名前」で保存するので、名前から ItemData に戻すための一覧が必要。
    [SerializeField] private ItemData[] allItems;

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>セーブデータがあるか</summary>
    public bool HasSave()
    {
        return PlayerPrefs.GetInt(GameConstants.SaveKeyHasSave, 0) == 1;
    }

    /// <summary>
    /// 今の状態を保存する。respawnPosition は再開したときに立つ位置（チェックポイントの位置）。
    /// </summary>
    public void Save(Vector3 respawnPosition)
    {
        GameObject player = GameObject.FindWithTag(GameConstants.TagPlayer);
        if (player == null) return;

        PlayerHealth health = player.GetComponent<PlayerHealth>();
        PlayerInventory inventory = player.GetComponent<PlayerInventory>();

        PlayerPrefs.SetInt(GameConstants.SaveKeyHasSave, 1);
        PlayerPrefs.SetFloat(GameConstants.SaveKeyPosX, respawnPosition.x);
        PlayerPrefs.SetFloat(GameConstants.SaveKeyPosY, respawnPosition.y);
        PlayerPrefs.SetFloat(GameConstants.SaveKeyPosZ, respawnPosition.z);
        PlayerPrefs.SetInt(GameConstants.SaveKeyHP, health.CurrentHP);
        PlayerPrefs.SetInt(GameConstants.SaveKeyScore, ScoreManager.Instance.Score);
        PlayerPrefs.SetInt(GameConstants.SaveKeyKeys, inventory.KeyCount);
        PlayerPrefs.SetString(GameConstants.SaveKeyItems, inventory.GetItemNamesJoined(","));
        PlayerPrefs.Save();

        Debug.Log("セーブしました: " + respawnPosition);
    }

    /// <summary>保存した状態をプレイヤーに反映する</summary>
    public void Load()
    {
        if (!HasSave()) return;

        GameObject player = GameObject.FindWithTag(GameConstants.TagPlayer);
        if (player == null) return;

        // ---- 位置 ----
        Vector3 pos = new Vector3(
            PlayerPrefs.GetFloat(GameConstants.SaveKeyPosX),
            PlayerPrefs.GetFloat(GameConstants.SaveKeyPosY),
            PlayerPrefs.GetFloat(GameConstants.SaveKeyPosZ));

        // CharacterController が付いたままだと transform.position の変更が無視されることがあるので、
        // いったん切ってから位置を変え、また入れる。
        CharacterController controller = player.GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;
        player.transform.position = pos;
        if (controller != null) controller.enabled = true;

        // ---- HP ----
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        health.SetHP(PlayerPrefs.GetInt(GameConstants.SaveKeyHP, GameConstants.PlayerMaxHP));

        // ---- スコア ----
        ScoreManager.Instance.SetScore(PlayerPrefs.GetInt(GameConstants.SaveKeyScore, 0));

        // ---- 鍵・アイテム ----
        PlayerInventory inventory = player.GetComponent<PlayerInventory>();
        inventory.Clear();
        inventory.AddKey(PlayerPrefs.GetInt(GameConstants.SaveKeyKeys, 0));

        string itemNames = PlayerPrefs.GetString(GameConstants.SaveKeyItems, "");
        if (itemNames != "")
        {
            foreach (string name in itemNames.Split(','))
            {
                ItemData item = FindItemByName(name);
                if (item != null) inventory.AddItem(item);
            }
        }

        Debug.Log("ロードしました: " + pos);
    }

    /// <summary>セーブデータを消す（ニューゲーム時）。ハイスコアは消さない。</summary>
    public void DeleteSave()
    {
        PlayerPrefs.DeleteKey(GameConstants.SaveKeyHasSave);
        PlayerPrefs.DeleteKey(GameConstants.SaveKeyPosX);
        PlayerPrefs.DeleteKey(GameConstants.SaveKeyPosY);
        PlayerPrefs.DeleteKey(GameConstants.SaveKeyPosZ);
        PlayerPrefs.DeleteKey(GameConstants.SaveKeyHP);
        PlayerPrefs.DeleteKey(GameConstants.SaveKeyScore);
        PlayerPrefs.DeleteKey(GameConstants.SaveKeyKeys);
        PlayerPrefs.DeleteKey(GameConstants.SaveKeyItems);
        PlayerPrefs.Save();
    }

    private ItemData FindItemByName(string name)
    {
        foreach (ItemData item in allItems)
        {
            if (item != null && item.itemName == name) return item;
        }
        return null;
    }
}
