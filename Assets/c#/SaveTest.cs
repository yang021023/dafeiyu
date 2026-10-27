using UnityEngine;
using UnityEngine.SceneManagement;

// 存档测试：挂在场景里任意对象上，右键组件标题即可点菜单
public class SaveTest : MonoBehaviour
{
    [ContextMenu("1. 保存当前进度")]
    private void SaveNow()
    {
        string scene = SceneManager.GetActiveScene().name;
        SaveSystem.Save(GameState.Snapshot(scene));
    }

    [ContextMenu("2. 读取存档（不切场景）")]
    private void LoadNow()
    {
        SaveData data = SaveSystem.Load();
        if (data == null) return;

        GameState.Apply(data);
        Debug.Log($"读档完成：场景={data.sceneName}，血={data.playerHealth}");
    }

    [ContextMenu("3. 打印存档内容")]
    private void PrintSave()
    {
        SaveData data = SaveSystem.Load();
        if (data == null) { Debug.Log("没有存档"); return; }

        string text = $"[存档] 场景={data.sceneName}  血={data.playerHealth}/{data.playerMaxHealth}\n消耗品：";
        foreach (SaveItem it in data.items) text += $"{it.id} x{it.count}  ";

        text += "\n装备包：";
        foreach (SaveItem it in data.equipItems) text += $"{it.id} x{it.count}  ";

        text += $"\n穿着：武器={data.weaponId}  衣服={data.armorId}  鞋={data.shoesId}";
        Debug.Log(text);
    }

    [ContextMenu("4. 删除存档")]
    private void DeleteSave()
    {
        SaveSystem.Delete();
    }

    [ContextMenu("5. 显示存档路径")]
    private void ShowPath()
    {
        Debug.Log($"存档路径：{SaveSystem.FilePath}\n存在：{SaveSystem.HasSave()}");
    }
}
