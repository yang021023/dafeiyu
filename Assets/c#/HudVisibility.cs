using UnityEngine;
using UnityEngine.SceneManagement;

// 血条只在游戏场景里显示：只要菜单场景还在内存里，就把血条所在的 Canvas 关掉。
//
// 为什么不用「当前激活场景」判断：编辑器里多场景同时 Play 时，激活场景是你在 Hierarchy
// 里选中的那个。调试关卡时常常把 room1 设成激活场景，那样判断就会失效，血条又冒出来。
// 改成「菜单场景是否已加载」判断后，三种情况都对：
//   · 编辑器里 menu + room1 + room2 一起 Play  -> 菜单在内存里 -> 隐藏
//   · 正常从菜单点开始 -> 单场景加载 room1，菜单被卸载 -> 显示
//   · 打包运行 -> 菜单场景根本不在内存里 -> 显示
//
// 不用在场景里挂任何组件：进入 Play 和每次加载/卸载场景后自动生效。
// 想改规则就改 hiddenScenes；不想要这个行为，直接删掉这个文件。
public static class HudVisibility
{
    // 这些场景只要还加载着，血条就隐藏。
    private static readonly string[] hiddenScenes = { "menu" };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Install()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
        Refresh();
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Refresh();
    }

    private static void OnSceneUnloaded(Scene scene)
    {
        Refresh();
    }

    private static void OnActiveSceneChanged(Scene previous, Scene next)
    {
        Refresh();
    }

    private static void Refresh()
    {
        bool hide = IsAnyHiddenSceneLoaded();

        // 血条 Canvas 上挂着 PlayerHUB，用它当作血条的锚点。
        PlayerHUB[] hubs = Object.FindObjectsByType<PlayerHUB>();
        for (int i = 0; i < hubs.Length; i++)
        {
            Canvas canvas = hubs[i].GetComponent<Canvas>();
            if (canvas != null) canvas.enabled = !hide;
        }
    }

    private static bool IsAnyHiddenSceneLoaded()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;

            for (int k = 0; k < hiddenScenes.Length; k++)
                if (scene.name == hiddenScenes[k]) return true;
        }
        return false;
    }
}
