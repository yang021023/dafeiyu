using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SlotUI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI countText;
    public Button button;

    [Tooltip("格子底色（可不填）；填了之后已装备的格子会变色")]
    public Image background;

    [Header("已装备高亮")]
    public Color normalColor = new Color(0.16f, 0.22f, 0.32f, 1f);
    public Color equippedColor = new Color(0.72f, 0.60f, 0.22f, 1f);

    private int index;
    private InventoryUI owner;

    // 不用手动拖：背景图和按钮就是自己身上的组件
    private void Awake()
    {
        if (background == null) background = GetComponent<Image>();
        if (button == null) button = GetComponent<Button>();

        // 子物体没拖的话，自动找一遍，省得报空引用
        if (icon == null) icon = FindChildImage("Icon");
        if (countText == null) countText = GetComponentInChildren<TextMeshProUGUI>(true);
    }

    private Image FindChildImage(string childName)
    {
        Transform t = transform.Find(childName);
        if (t == null) return null;
        return t.GetComponent<Image>();
    }

    private void OnValidate()
    {
        if (!Application.isPlaying && icon == null) icon = FindChildImage("Icon");
    }

    public void Init(int slotIndex, InventoryUI ui)
    {
        index = slotIndex;
        owner = ui;

        if (button == null)
        {
            Debug.LogWarning($"[SlotUI] {name} 没有 Button 组件，点击无效", this);
            return;
        }

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => owner.UseSlot(index));
    }

    public void Show(ItemStack stack, bool equipped = false)
    {
        bool empty = stack == null || stack.IsEmpty;

        if (icon != null) icon.enabled = !empty;
        if (countText != null) countText.enabled = !empty;

        if (background != null)
            background.color = (equipped && !empty) ? equippedColor : normalColor;

        if (empty) return;

        if (icon != null) icon.sprite = stack.item.icon;
        if (countText != null)
            countText.text = stack.count > 1 ? stack.count.ToString() : "";
    }
}
