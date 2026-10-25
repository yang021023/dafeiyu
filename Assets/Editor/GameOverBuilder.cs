#if UNITY_EDITOR
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 工具 → 生成 Game Over 面板
//
// 在开着角色（DafeiyuMovement）的每个场景里生成一个失败面板：
//   全屏压暗 + 中间面板 + 「失败」标题 + 重开本关 / 返回菜单 两个按钮
// 面板脚本 GameOverUI 自动挂好、按钮也自动连好，生成后 Ctrl+S 保存即可。
public static class GameOverBuilder
{
    private const string FontPath = "Assets/Font/SourceHanSansSC-Regular SDF.asset";

    private static readonly Vector2 BoxSize = new Vector2(720f, 440f);
    private static readonly Vector2 ButtonSize = new Vector2(460f, 92f);

    [MenuItem("工具/生成 Game Over 面板")]
    private static void Build()
    {
        TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
        if (font == null)
            Debug.LogWarning("[GameOver] 没找到思源黑体 SDF，文字会用 TMP 默认字体（中文可能是方块）。");

        // 找出所有"有角色"的场景
        var scenes = new HashSet<Scene>();
        foreach (DafeiyuMovement player in Object.FindObjectsByType<DafeiyuMovement>(FindObjectsInactive.Include))
        {
            if (player.gameObject.scene.IsValid()) scenes.Add(player.gameObject.scene);
        }

        if (scenes.Count == 0)
        {
            Debug.LogError("[GameOver] 没找到角色。请先打开 room1 或 room2 场景再点。");
            return;
        }

        int built = 0;
        foreach (Scene scene in scenes)
        {
            if (HasGameOverUI(scene))
            {
                Debug.Log($"[GameOver] {scene.name} 已经有失败面板了，跳过。");
                continue;
            }

            BuildIn(scene, font);
            built++;
        }

        Debug.Log(built > 0
            ? $"[GameOver] 已在 {built} 个场景生成失败面板，按 Ctrl+S 保存。"
            : "[GameOver] 没有任何场景需要生成。");
    }

    private static bool HasGameOverUI(Scene scene)
    {
        foreach (GameOverUI ui in Object.FindObjectsByType<GameOverUI>(FindObjectsInactive.Include))
            if (ui.gameObject.scene == scene) return true;
        return false;
    }

    private static void BuildIn(Scene scene, TMP_FontAsset font)
    {
        // ---------- Canvas ----------
        var canvasGo = new GameObject("GameOverCanvas",
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        SceneManager.MoveGameObjectToScene(canvasGo, scene);

        Canvas canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20;   // 压住血条(0)和暂停面板(10)

        CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        // ---------- 全屏压暗 ----------
        RectTransform panel = NewUI("Panel", canvasGo.transform);
        Stretch(panel);
        Image scrim = panel.gameObject.AddComponent<Image>();
        scrim.color = new Color(0f, 0f, 0f, 0.78f);

        // ---------- 中间面板 ----------
        RectTransform box = NewUI("Box", panel);
        box.anchorMin = box.anchorMax = new Vector2(0.5f, 0.5f);
        box.pivot = new Vector2(0.5f, 0.5f);
        box.anchoredPosition = Vector2.zero;
        box.sizeDelta = BoxSize;
        Image boxImage = box.gameObject.AddComponent<Image>();
        boxImage.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        boxImage.type = Image.Type.Sliced;
        boxImage.color = new Color(0.06f, 0.11f, 0.21f, 0.97f);

        // ---------- 标题 ----------
        MakeText(box, "Title", "失 败", font, 76f, Color.white,
            new Vector2(0f, 118f), new Vector2(640f, 110f));
        MakeText(box, "Subtitle", "大肥鱼倒下了", font, 28f, new Color(0.72f, 0.82f, 0.92f, 0.85f),
            new Vector2(0f, 46f), new Vector2(640f, 50f));

        // ---------- 两个按钮 ----------
        Button restart = MakeButton(box, "RestartButton", "重开本关", font, new Vector2(0f, -40f));
        Button toMenu = MakeButton(box, "MenuButton", "返回菜单", font, new Vector2(0f, -160f));

        // ---------- 脚本 + 连线 ----------
        var ui = canvasGo.AddComponent<GameOverUI>();
        ui.panel = panel.gameObject;
        ui.menuSceneName = "menu";
        UnityEventTools.AddPersistentListener(restart.onClick, ui.RestartLevel);
        UnityEventTools.AddPersistentListener(toMenu.onClick, ui.BackToMenu);

        panel.gameObject.SetActive(false);   // 默认隐藏

        EnsureEventSystem(scene);
        EditorSceneManager.MarkSceneDirty(scene);
    }

    private static void EnsureEventSystem(Scene scene)
    {
        foreach (EventSystem es in Object.FindObjectsByType<EventSystem>(FindObjectsInactive.Include))
            if (es.gameObject.scene == scene) return;

        var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        SceneManager.MoveGameObjectToScene(go, scene);
        Debug.Log($"[GameOver] {scene.name} 里没有 EventSystem，已自动补上一个。");
    }

    // ---------------- 小工具 ----------------

    private static RectTransform NewUI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return (RectTransform)go.transform;
    }

    private static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    private static TextMeshProUGUI MakeText(Transform parent, string name, string content,
        TMP_FontAsset font, float size, Color color, Vector2 pos, Vector2 box)
    {
        RectTransform rt = NewUI(name, parent);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = box;

        var text = rt.gameObject.AddComponent<TextMeshProUGUI>();
        if (font != null) text.font = font;
        text.text = content;
        text.fontSize = size;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
    }

    private static Button MakeButton(Transform parent, string name, string label,
        TMP_FontAsset font, Vector2 pos)
    {
        RectTransform rt = NewUI(name, parent);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = ButtonSize;

        var image = rt.gameObject.AddComponent<Image>();
        image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        image.type = Image.Type.Sliced;
        image.color = new Color(0.16f, 0.33f, 0.58f, 0.95f);

        var button = rt.gameObject.AddComponent<Button>();
        button.targetGraphic = image;

        RectTransform labelRt = NewUI("Label", rt);
        Stretch(labelRt);
        var text = labelRt.gameObject.AddComponent<TextMeshProUGUI>();
        if (font != null) text.font = font;
        text.text = label;
        text.fontSize = 38f;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;

        return button;
    }
}
#endif
