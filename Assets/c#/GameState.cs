using System.Collections.Generic;
using UnityEngine;

// 跨场景保存的玩家数据。
// static 属于整个游戏进程，切场景不会清空，所以 room1 → room2 血量能接着走。
public static class GameState
{
    public static int PlayerHealth=-1;
    public static int PlayerMaxHealth=-1;
    public static EquipmentData Equipment = new EquipmentData();

    // 两个独立背包，都跟着 GameState 跨场景存在
    public static InventoryData Inventory = new InventoryData(12);   // 消耗品（药水等）
    public static InventoryData EquipBag  = new InventoryData(12);   // 装备（剑/衣服/鞋子）

    public static void Clear(){
        PlayerHealth=-1;
        PlayerMaxHealth=-1;
        Inventory = new InventoryData(12);   // 新游戏要把背包清空
        EquipBag  = new InventoryData(12);
        Equipment = new EquipmentData();
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay(){
        Clear();
    }

    // ==================== 存档 ====================

    // 把当前状态打包成存档
    public static SaveData Snapshot(string sceneName)
    {
        SaveData data = new SaveData
        {
            version = 1,
            sceneName = sceneName,
            playerHealth = PlayerHealth,
            playerMaxHealth = PlayerMaxHealth,
            weaponId = Equipment.weapon != null ? Equipment.weapon.id : "",
            armorId  = Equipment.armor  != null ? Equipment.armor.id  : "",
            shoesId  = Equipment.shoes  != null ? Equipment.shoes.id  : ""
        };

        Collect(Inventory, data.items);
        Collect(EquipBag,  data.equipItems);

        return data;
    }

    private static void Collect(InventoryData bag, List<SaveItem> target)
    {
        foreach (ItemStack slot in bag.slots)
        {
            if (slot.IsEmpty) continue;
            target.Add(new SaveItem { id = slot.item.id, count = slot.count });
        }
    }

    // 用存档覆盖当前状态
    public static void Apply(SaveData data)
    {
        if (data == null)
        {
            Debug.LogError("[存档] Apply 收到 null");
            return;
        }

        Clear();        // 先清空，避免旧数据叠加

        PlayerHealth = data.playerHealth > 0 ? data.playerHealth : -1;
        PlayerMaxHealth = data.playerMaxHealth > 0 ? data.playerMaxHealth : -1;

        Fill(Inventory, data.items);
        Fill(EquipBag,  data.equipItems);

        Equipment.Set(EquipSlot.Weapon, FindItem(data.weaponId));
        Equipment.Set(EquipSlot.Armor,  FindItem(data.armorId));
        Equipment.Set(EquipSlot.Shoes,  FindItem(data.shoesId));

        Debug.Log($"[存档] 已读取：场景={data.sceneName}，血={data.playerHealth}，" +
                  $"消耗品 {data.items.Count} 种，装备 {data.equipItems.Count} 件");
    }

    private static void Fill(InventoryData bag, List<SaveItem> source)
    {
        if (source == null) return;

        foreach (SaveItem entry in source)
        {
            ItemData item = FindItem(entry.id);
            if (item == null)
            {
                Debug.LogWarning($"[存档] 未知物品 id：{entry.id}，已跳过");
                continue;
            }
            bag.Add(item, Mathf.Max(1, entry.count));
        }
    }

    private static ItemData FindItem(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        ItemDatabase db = ItemDatabase.Instance;
        return db != null ? db.Find(id) : null;
    }
}
