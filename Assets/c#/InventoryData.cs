using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ItemStack{
    public ItemData item;
    public int count;

    public bool IsEmpty => item==null || count <=0;
    public int StackLimit => item!=null ? item.StackLimit :0;

    public void Clear(){item=null; count=0;}
}

[Serializable]
public class InventoryData{
    public int slotCount=12;
    public List<ItemStack> slots=new List<ItemStack>();

    public event Action OnChanged;

    public InventoryData():this(12){}

    public InventoryData(int count)
    {
        slotCount = Mathf.Max(1,count);
        slots.Clear();
        for(int i=0;i<slotCount;i++){
            slots.Add(new ItemStack());
        }

    }

    public ItemStack Get(int index)
        =>(index>=0 && index <slots.Count)?slots[index]:null;

    public int Add(ItemData item,int amount = 1){
        if(item ==null || amount <=0) return 0;
        while(slots.Count <slotCount) slots.Add(new ItemStack());

        int remaining = amount;

        if(item.stackable){
            foreach(ItemStack slot in slots){
                if(remaining <=0) break;
                if(slot.item !=item || slot.count >= slot.StackLimit) continue;

                int put = Mathf.Min(remaining,slot.StackLimit - slot.count);
                slot.count +=put;
                remaining -=put;
            }
        }

        foreach (ItemStack slot in slots){
            if(remaining <=0) break;
            if(!slot.IsEmpty) continue;

            slot.item = item;
            slot.count=Mathf.Min(remaining,slot.StackLimit);
            remaining-=slot.count;
        }

        int added=amount - remaining;
        if(added > 0 ) OnChanged?.Invoke();
        return added;
    }


    public bool Remove(int index, int amount = 1)
    {
        ItemStack slot = Get(index);
        if (slot == null || slot.IsEmpty || amount <= 0 || slot.count < amount) return false;

        slot.count -= amount;
        if (slot.count <= 0) slot.Clear();

        OnChanged?.Invoke();
        return true;
    }

    public int CountOf(ItemData item)
    {
        int total = 0;
        foreach (ItemStack slot in slots)
            if (slot.item == item) total += slot.count;
        return total;
    }

    public void ClearAll()
    {
        foreach (ItemStack slot in slots) slot.Clear();
        OnChanged?.Invoke();
    }
}