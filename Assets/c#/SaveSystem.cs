using System.IO;
using UnityEngine;

// 存档读写：只负责"把 SaveData 变成文件 / 从文件变回来"
public static class SaveSystem
{
    // Windows 上大致是：
    // C:\Users\你\AppData\LocalLow\DefaultCompany\大肥鱼冒险之旅\save.json
    public static string FilePath =>
        Path.Combine(Application.persistentDataPath, "save.json");

    public static bool HasSave() => File.Exists(FilePath);

    public static void Save(SaveData data)
    {
        if (data == null)
        {
            Debug.LogError("[存档] data 为 null，取消保存");
            return;
        }

        try
        {
            string json = JsonUtility.ToJson(data, true);   // true = 带缩进，方便肉眼看
            File.WriteAllText(FilePath, json);
            Debug.Log($"[存档] 已保存 → {FilePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[存档] 保存失败：{e.Message}");
        }
    }

    public static SaveData Load()
    {
        if (!HasSave())
        {
            Debug.Log("[存档] 没有存档文件");
            return null;
        }

        try
        {
            string json = File.ReadAllText(FilePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            if (data == null)
            {
                Debug.LogError("[存档] 解析失败，文件可能损坏");
                return null;
            }

            Migrate(data);
            return data;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[存档] 读取失败：{e.Message}");
            return null;
        }
    }

    public static void Delete()
    {
        if (!HasSave()) return;

        try
        {
            File.Delete(FilePath);
            Debug.Log("[存档] 已删除");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[存档] 删除失败：{e.Message}");
        }
    }

    // 旧版本存档升级
    private static void Migrate(SaveData data)
    {
        if (data.version < 1)
        {
            // 占位：以后改结构时在这里补默认值
            data.version = 1;
        }
    }
}
