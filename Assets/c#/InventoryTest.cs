using UnityEngine;

public class InventoryTest : MonoBehaviour
{
    public ItemData potion;
    public ItemData key;

    [Header("装备测试")]
    public ItemData sword;
    public ItemData armor;
    public ItemData boots;

    [ContextMenu("1. 加 3 瓶药")]
    private void AddPotion() { Test(GameState.Inventory.Add(potion, 3)); }

    [ContextMenu("2. 加 2 把钥匙")]
    private void AddKey() { Test(GameState.Inventory.Add(key, 2)); }

    [ContextMenu("3. 打印背包")]
    private void Print()
    {
        for (int i = 0; i < GameState.Inventory.slotCount; i++)
        {
            ItemStack s = GameState.Inventory.Get(i);
            if (s.IsEmpty) continue;
            Debug.Log($"[{i}] {s.item.displayName} x{s.count}");
        }
    }

    [ContextMenu("4. 用一个格子")]
    private void UseFirst()
    {
        var inv = GameState.Inventory;
        for (int i = 0; i < inv.slotCount; i++)
        {
            if (!inv.Get(i).IsEmpty) { inv.Remove(i, 1); Debug.Log($"用了 slot {i}"); return; }
        }
    }

    private void Test(int added) { Debug.Log($"实际放入 {added} 个"); }

    // ==================== 往背包里塞装备（测试 UI 用） ====================

    [ContextMenu("0a. 装备包加一把剑")]
    private void AddSwordToBag() { Test(GameState.EquipBag.Add(sword, 1)); }

    [ContextMenu("0b. 装备包加一件衣服")]
    private void AddArmorToBag() { Test(GameState.EquipBag.Add(armor, 1)); }

    [ContextMenu("0c. 装备包加一双鞋")]
    private void AddBootsToBag() { Test(GameState.EquipBag.Add(boots, 1)); }

    // ==================== 装备测试 ====================

    [ContextMenu("5. 穿上剑")]
    private void EquipSword()
    {
        GameState.Equipment.Set(EquipSlot.Weapon, sword);
        Debug.Log($"武器槽 = {(sword != null ? sword.displayName : "空")}");
    }

    [ContextMenu("6. 穿上衣服")]
    private void EquipArmor()
    {
        GameState.Equipment.Set(EquipSlot.Armor, armor);
        Debug.Log($"衣服槽 = {(armor != null ? armor.displayName : "空")}");
    }

    [ContextMenu("7. 穿上鞋子")]
    private void EquipBoots()
    {
        GameState.Equipment.Set(EquipSlot.Shoes, boots);
        Debug.Log($"鞋子槽 = {(boots != null ? boots.displayName : "空")}");
    }

    [ContextMenu("8. 全部卸下")]
    private void UnequipAll()
    {
        GameState.Equipment.ClearAll();
        Debug.Log("装备已全部卸下");
    }

    [ContextMenu("9. 打印当前装备")]
    private void PrintEquip()
    {
        foreach (ItemData item in GameState.Equipment.AllEquipped())
            Debug.Log($"装备中: {item.displayName}  攻击+{item.bonusDamage}  范围+{item.bonusAttackRange}  血+{item.bonusMaxHealth}  速度+{item.bonusSpeed}");
    }
}