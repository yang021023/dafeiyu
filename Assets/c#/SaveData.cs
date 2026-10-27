using System.Collections.Generic;
using UnityEngine;

// 存档里的一个物品：只存 id 和数量
[System.Serializable]
public class SaveItem
{
    public string id;
    public int count;
}

// 整个存档
[System.Serializable]
public class SaveData
{
    public int version = 1;          // 存档版本号，以后改结构靠它做迁移

    public string sceneName;         // 存档时所在（或即将进入）的场景
    public int playerHealth;
    public int playerMaxHealth;

    public List<SaveItem> items = new List<SaveItem>();       // 消耗品背包
    public List<SaveItem> equipItems = new List<SaveItem>();  // 装备背包

    public string weaponId;          // 身上穿着的
    public string armorId;
    public string shoesId;

    public bool HasAnyEquip =>
        !string.IsNullOrEmpty(weaponId) ||
        !string.IsNullOrEmpty(armorId) ||
        !string.IsNullOrEmpty(shoesId);
}
