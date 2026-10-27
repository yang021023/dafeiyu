using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "游戏/物品数据库")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemData> allItems=new List<ItemData>();
    private Dictionary<string,ItemData> lookup;

    public ItemData Find(string id){
        if(lookup == null) Build();
        if(string.IsNullOrEmpty(id)) return null;
        return lookup.TryGetValue(id,out ItemData item)? item :null;
    }

    private void Build(){
        lookup = new Dictionary<string,ItemData>();
        foreach(ItemData item in allItems){
            if(item == null || string.IsNullOrEmpty(item.id)) continue;
            if(lookup.ContainsKey(item.id)){
                 Debug.LogError($"[ItemDatabase] id 重复：'{item.id}'（{item.name}），后者被忽略");
                 continue;
            }
            lookup[item.id]=item;
        }
    }
    private void OnValidate(){lookup=null;}
}
