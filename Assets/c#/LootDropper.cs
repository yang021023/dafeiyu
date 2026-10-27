using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LootEntry
{
    public ItemData item;
    [Min(0)] public int minCount = 1;
    [Min(0)] public int maxCount = 1;

    [Tooltip("掉这个物品的概率，1 = 必掉")]
    [Range(0f, 1f)] public float chance = 1f;
}

public class LootDropper : MonoBehaviour
{
    [Header("掉落表")]
    public List<LootEntry> drops = new List<LootEntry>();

    [Header("拾取物 prefab")]
    [Tooltip("必须是带 PickupItem 的 prefab")]
    public GameObject pickupPrefab;

    [Tooltip("每件东西随机散开的半径")]
    public float scatter = 0.35f;

    private bool dropped;

    private void OnDestroy()
    {
        if (dropped || pickupPrefab == null) return;

        // 切场景 / 退出游戏时场景正在卸载，不掉落
        if (!gameObject.scene.isLoaded) return;

        dropped = true;
        Drop();
    }

    private void Drop()
    {
        foreach (LootEntry entry in drops)
        {
            if (entry.item == null) continue;
            if (Random.value > entry.chance) continue;

            int count = Random.Range(entry.minCount, entry.maxCount + 1);
            if (count <= 0) continue;

            Spawn(entry.item, count);
        }
    }

    private void Spawn(ItemData item, int count)
    {
        Vector2 offset = Random.insideUnitCircle * scatter;
        Vector3 pos = transform.position + (Vector3)offset;

        GameObject go = Instantiate(pickupPrefab, pos, Quaternion.identity);

        PickupItem pickup = go.GetComponent<PickupItem>();
        if (pickup != null) pickup.Setup(item, count);
    }
}