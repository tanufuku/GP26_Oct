using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーの持ち物（装備アイテムと鍵）。
/// 剣・盾は「持っているだけで効果がある」ので、List に入れておき、
/// 攻撃力・防御力を聞かれたら合計して返す。
/// </summary>
public class PlayerInventory : MonoBehaviour
{
    // 持っているアイテムの一覧。外から勝手に Add/Remove されないよう private にして、
    // 追加は AddItem()、確認は HasItem() / ItemCount を通す。
    private readonly List<ItemData> items = new List<ItemData>();

    /// <summary>持っているアイテムの数</summary>
    public int ItemCount { get { return items.Count; } }

    /// <summary>持っている鍵の数</summary>
    public int KeyCount { get; private set; }

    private void Start()
    {
        NotifyUI();
    }

    /// <summary>アイテムを加える。同じものを既に持っていたら false（拾わない）。</summary>
    public bool AddItem(ItemData item)
    {
        if (items.Contains(item)) return false;

        items.Add(item);
        NotifyUI();
        return true;
    }

    public bool HasItem(ItemData item)
    {
        return items.Contains(item);
    }

    public void AddKey(int count)
    {
        KeyCount += count;
        NotifyUI();
    }

    /// <summary>鍵を1本使う。持っていなければ false。</summary>
    public bool UseKey()
    {
        if (KeyCount <= 0) return false;

        KeyCount--;
        NotifyUI();
        return true;
    }

    /// <summary>持っている武器の攻撃力の合計</summary>
    public int GetWeaponAttack()
    {
        int total = 0;
        foreach (ItemData item in items)
        {
            total += item.attackBonus;
        }
        return total;
    }

    /// <summary>持っている防具の防御力の合計</summary>
    public int GetDefense()
    {
        int total = 0;
        foreach (ItemData item in items)
        {
            total += item.defenseBonus;
        }
        return total;
    }

    /// <summary>
    /// アイテム名を区切り文字でつないだ文字列にする（例 "Sword,Shield"）。
    /// UI の表示（区切りは ", "）と、セーブ（区切りは ","）の両方で使う。
    /// </summary>
    public string GetItemNamesJoined(string separator)
    {
        string result = "";
        for (int i = 0; i < items.Count; i++)
        {
            if (i > 0) result += separator;     // 2つ目以降の前に区切りを入れる
            result += items[i].itemName;
        }
        return result;
    }

    /// <summary>ロード前に空にする</summary>
    public void Clear()
    {
        items.Clear();
        KeyCount = 0;
        NotifyUI();
    }

    private void NotifyUI()
    {
        UIManager.Instance.UpdateInventory(this);
    }
}
