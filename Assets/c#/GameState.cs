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

}
