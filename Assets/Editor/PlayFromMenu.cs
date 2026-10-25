#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// 让「Play」永远从 menu 场景开始，忽略 Hierarchy 里同时打开的其他场景。
//
// 用途：你可以把 room1 / room2 / room0 都开着方便编辑，
//       但按 Play 时只加载 menu，点「开始游戏」才会单场景进 room1。
//
// 改起点：把下面的 ScenePath 换成别的场景。
// 想恢复默认（按打开的场景跑）：删掉这个文件。
[InitializeOnLoad]
public static class PlayFromMenu
{
    private const string ScenePath = "Assets/Scenes/menu.unity";

    static PlayFromMenu()
    {
        SceneAsset menu = AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath);
        if (menu == null)
        {
            Debug.LogWarning($"[PlayFromMenu] 找不到场景 {ScenePath}，Play 仍然按打开的场景运行。");
            return;
        }

        EditorSceneManager.playModeStartScene = menu;
    }
}
#endif
