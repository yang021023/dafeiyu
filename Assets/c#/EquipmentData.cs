using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EquipmentData
{
    public ItemData weapon;
    public ItemData armor;
    public ItemData shoes;

    public event Action OnChanged;

    public ItemData Get(EquipSlot slot)
    {
        switch (slot)
        {
            case EquipSlot.Weapon: return weapon;
            case EquipSlot.Armor:  return armor;
            case EquipSlot.Shoes:  return shoes;
        }
        return null;
    }

    public void Set(EquipSlot slot, ItemData item)
    {
        switch (slot)
        {
            case EquipSlot.Weapon: weapon = item; break;
            case EquipSlot.Armor:  armor  = item; break;
            case EquipSlot.Shoes:  shoes  = item; break;
        }
        OnChanged?.Invoke();
    }

    public IEnumerable<ItemData> AllEquipped()
    {
        if (weapon != null) yield return weapon;
        if (armor  != null) yield return armor;
        if (shoes  != null) yield return shoes;
    }

    public void ClearAll()
    {
        weapon = armor = shoes = null;
        OnChanged?.Invoke();
    }
}