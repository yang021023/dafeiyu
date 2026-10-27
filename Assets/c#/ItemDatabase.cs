using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "游戏/物品数据库")]
public class ItemDatabase : ScriptableObject
{
    // 必须放在 Assets/Resources/ItemDatabase.asset 才能被 Resources.Load 找到
    public const string ResourcePath = "ItemDatabase";

    public List<ItemData> allItems = new List<ItemData>();

    private Dictionary<string, ItemData> lookup;

    // 全局访问入口（读档时靠它把 id 变回 ItemData）
    private static ItemDatabase instance;

    public static ItemDatabase Instance
    {
        get
        {
            if (instance == null)
                instance = Resources.Load<ItemDatabase>(ResourcePath);

            if (instance == null)
                Debug.LogError($"[ItemDatabase] 找不到 Assets/Resources/{ResourcePath}.asset");

            return instance;
        }
    }

    public ItemData Find(string id)
    {
        if (lookup == null) Build();
        if (string.IsNullOrEmpty(id)) return null;

        return lookup.TryGetValue(id, out ItemData item) ? item : null;
    }

    private void Build()
    {
        lookup = new Dictionary<string, ItemData>();

        foreach (ItemData item in allItems)
        {
            if (item == null || string.IsNullOrEmpty(item.id)) continue;

            if (lookup.ContainsKey(item.id))
            {
                Debug.LogError($"[ItemDatabase] id 重复：'{item.id}'（{item.name}），后者被忽略");
                continue;
            }

            lookup[item.id] = item;
        }
    }

    private void OnValidate()
    {
        lookup = null;
        instance = this;      // 编辑器里自动登记，省得 Resources.Load 失败
    }

    // 每次进入 Play 都清掉静态缓存，避免用到上次的引用
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
    }
}
