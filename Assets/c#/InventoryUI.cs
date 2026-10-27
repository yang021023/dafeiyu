using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("主面板")]
    public GameObject panel;
    public Transform grid;
    public GameObject slotPrefab;

    [Header("顶部页签")]
    public Button tabItemButton;      // 「物品」
    public Button tabEquipButton;     // 「武器」

    private SlotUI[] slotUIs;

    // 0 = 物品（消耗品背包），1 = 装备（装备背包）
    private int activeTab;

    // 当前页签对应的那个背包
    private InventoryData Bag => activeTab == 0 ? GameState.Inventory : GameState.EquipBag;

    private void Start()
    {
        GameState.Inventory.OnChanged += Refresh;
        GameState.EquipBag.OnChanged  += Refresh;
        GameState.Equipment.OnChanged += Refresh;

        // 两个背包格子数取大的，保证界面够用
        int count = Mathf.Max(GameState.Inventory.slotCount, GameState.EquipBag.slotCount);

        slotUIs = new SlotUI[count];
        for (int i = 0; i < slotUIs.Length; i++)
        {
            GameObject go = Instantiate(slotPrefab, grid);
            slotUIs[i] = go.GetComponent<SlotUI>();
            slotUIs[i].Init(i, this);
        }

        if (tabItemButton != null)  tabItemButton.onClick.AddListener(() => SetTab(0));
        if (tabEquipButton != null) tabEquipButton.onClick.AddListener(() => SetTab(1));

        SetTab(0);
        panel.SetActive(false);
    }

    private void OnDestroy()
    {
        if (GameState.Inventory != null) GameState.Inventory.OnChanged -= Refresh;
        if (GameState.EquipBag != null)  GameState.EquipBag.OnChanged  -= Refresh;
        if (GameState.Equipment != null) GameState.Equipment.OnChanged -= Refresh;
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        // Tab 开关背包
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            bool open = !panel.activeSelf;
            panel.SetActive(open);
            if (open) Refresh();
        }

        // 打开时 Q / E 切页签
        if (panel.activeSelf)
        {
            if (Keyboard.current.qKey.wasPressedThisFrame) SetTab(0);
            if (Keyboard.current.eKey.wasPressedThisFrame) SetTab(1);
        }
    }

    public void SetTab(int tab)
    {
        activeTab = tab;
        Refresh();
    }

    // 刷新：显示当前页签那个背包 + 标记已装备
    public void Refresh()
    {
        InventoryData bag = Bag;

        for (int i = 0; i < slotUIs.Length; i++)
        {
            ItemStack stack = (i < bag.slotCount) ? bag.Get(i) : null;

            bool equipped = false;
            if (activeTab == 1 && stack != null && !stack.IsEmpty)
                equipped = GameState.Equipment.Get(stack.item.equipSlot) == stack.item;

            slotUIs[i].Show(stack, equipped);
        }
    }

    // 格子被点击：物品 → 使用，装备 → 穿/脱
    public void UseSlot(int index)
    {
        ItemStack peek = Bag.Get(index);
        string itemName = (peek != null && !peek.IsEmpty) ? peek.item.displayName : "空";
        Debug.Log($"[背包] 点了第 {index} 格（{itemName}），页签 = {(activeTab == 0 ? "物品" : "武器")}");

        if (activeTab == 0) UseConsumable(index);
        else ToggleEquip(index);
    }

    // ---------- 物品页 ----------
    private void UseConsumable(int index)
    {
        InventoryData bag = GameState.Inventory;
        ItemStack stack = bag.Get(index);
        if (stack == null || stack.IsEmpty) return;

        if (stack.item.healAmount > 0)
        {
            PlayerHealth hp = Object.FindAnyObjectByType<PlayerHealth>();
            if (hp == null) return;

            if (hp.CurrentHealth >= hp.maxHealth)
            {
                Debug.Log("血是满的，先不吃");
                return;
            }

            hp.Heal(stack.item.healAmount);
            bag.Remove(index, 1);
            Debug.Log($"吃了一个 {stack.item.displayName}，回血 {stack.item.healAmount}");
            return;
        }

        Debug.Log($"{stack.item.displayName} 暂时用不了");
    }

    // ---------- 装备页：点一下穿，再点一下脱 ----------
    private void ToggleEquip(int index)
    {
        InventoryData bag = GameState.EquipBag;
        ItemStack stack = bag.Get(index);
        if (stack == null || stack.IsEmpty) return;

        ItemData item = stack.item;
        if (item.category != ItemCategory.Equipment)
        {
            Debug.Log($"{item.displayName} 不是装备");
            return;
        }

        EquipmentData eq = GameState.Equipment;

        // 已经穿着 → 脱下来
        if (eq.Get(item.equipSlot) == item)
        {
            eq.Set(item.equipSlot, null);
            Debug.Log($"卸下了 {item.displayName}");
            return;
        }

        // 没穿 → 穿上（同槽位的会被顶掉，物品本身留在装备背包里）
        eq.Set(item.equipSlot, item);
        Debug.Log($"装备了 {item.displayName}（槽位 {item.equipSlot}）→ 武器 = " +
                  (eq.weapon != null ? eq.weapon.displayName : "空") +
                  "，衣服 = " + (eq.armor != null ? eq.armor.displayName : "空") +
                  "，鞋子 = " + (eq.shoes != null ? eq.shoes.displayName : "空"));
    }
}
