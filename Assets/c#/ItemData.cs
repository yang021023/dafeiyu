using UnityEngine;

// 物品分类：消耗品（药水）和装备（剑 / 衣服 / 鞋子）
public enum ItemCategory { Consumable, Equipment }

// 装备槽位
public enum EquipSlot { Weapon, Armor, Shoes }

// 每种物品一个资源文件：右键 → 创建 → 游戏 → 物品数据
[CreateAssetMenu(fileName = "NewItem", menuName = "游戏/物品数据")]
public class ItemData : ScriptableObject
{
    [Tooltip("唯一 ID，存档靠它。用过一次就别再改")]
    public string id = "item_id";

    public string displayName = "新物品";

    public Sprite icon;

    [Header("分类")]
    public ItemCategory category = ItemCategory.Consumable;

    [Tooltip("只有 category = Equipment 时才用这个")]
    public EquipSlot equipSlot = EquipSlot.Weapon;

    [Header("堆叠")]
    public bool stackable = true;
    [Min(1)] public int maxStack = 9;

    [Header("使用效果")]
    [Tooltip("大于 0 表示能吃，回这么多血")]
    public int healAmount = 0;

    [Header("装备加成")]
    [Tooltip("剑：加攻击力")]
    public int bonusDamage;
    [Tooltip("剑：加攻击距离（世界单位）")]
    public float bonusAttackRange;
    [Tooltip("衣服：加最大血量")]
    public int bonusMaxHealth;
    [Tooltip("鞋子：加速度，0.3 = 快 30%")]
    public float bonusSpeed;

    // 给 UI 和背包用：这一格最多放几个
    // 注意：名字故意不叫 MaxStack，避免和字段 maxStack 只差大小写
    public int StackLimit => stackable ? Mathf.Max(1, maxStack) : 1;
}
