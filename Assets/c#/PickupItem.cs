using UnityEngine;

// 地上的可拾取物：玩家碰到就进背包
[RequireComponent(typeof(Collider2D))]
public class PickupItem : MonoBehaviour
{
    [Header("物品")]
    public ItemData item;
    [Min(1)] public int amount = 1;

    [Header("表现")]
    [Tooltip("上下浮动的幅度，0 = 不动")]
    public float bobHeight = 0.12f;
    public float bobSpeed = 2f;

    private Vector3 startPos;

    private void Reset()
    {
        // 加组件时自动把碰撞体设成触发器
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        // 自动用物品的图标当显示图
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && item != null) sr.sprite = item.icon;
    }

    private void OnValidate()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && item != null && item.icon != null) sr.sprite = item.icon;
    }

    private void Start()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        if (bobHeight <= 0f) return;
        float y = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = startPos + new Vector3(0f, y, 0f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 只认玩家
        if (other.GetComponentInParent<PlayerHealth>() == null) return;

        // 按分类进不同的背包
        InventoryData bag = (item != null && item.category == ItemCategory.Equipment)
            ? GameState.EquipBag
            : GameState.Inventory;

        int added = bag.Add(item, amount);
        if (added <= 0) return;              // 背包满了，东西还留在地上

        if (added >= amount)
        {
            Destroy(gameObject);             // 全部收下
        }
        else
        {
            amount -= added;                 // 只收下一部分，剩下的继续留在地上
        }
    }

    // 供 LootDropper 调用：运行时决定掉什么、掉几个
    public void Setup(ItemData newItem, int newAmount)
    {
        item = newItem;
        amount = Mathf.Max(1, newAmount);

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && item != null) sr.sprite = item.icon;
    }
}