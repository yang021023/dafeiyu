#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// 工具 → 修复暂停菜单布局
//
// 作用：把 PausePanel 里的遮罩、面板盒子、标题、两个按钮重新摆整齐。
// 只改当前打开的场景里的 PausePanel，改完记得 Ctrl+S。
public static class FixPauseMenuLayout
{
    private static readonly Color ScrimColor  = new Color(0f, 0f, 0f, 0.6f);          // 全屏压暗
    private static readonly Color BoxColor    = new Color(0.06f, 0.11f, 0.21f, 0.96f); // 面板底色
    private static readonly Color ButtonColor = new Color(0.16f, 0.33f, 0.58f, 0.95f); // 按钮底色

    private static readonly Vector2 BoxSize    = new Vector2(720f, 480f);
    private static readonly Vector2 ButtonSize = new Vector2(460f, 92f);

    [MenuItem("工具/修复暂停菜单布局")]
    private static void Apply()
    {
        int count = 0;

        Transform[] all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include);
        foreach (Transform t in all)
        {
            if (t == null || t.name != "PausePanel") continue;
            FixOne(t);
            count++;
        }

        if (count == 0)
        {
            Debug.LogError("[暂停菜单] 没找到 PausePanel。请先打开 room1 或 room2 场景再点。");
            return;
        }

        Debug.Log($"[暂停菜单] 已修好 {count} 个 PausePanel，按 Ctrl+S 保存场景。");
    }

    private static void FixOne(Transform panel)
    {
        Undo.RegisterFullObjectHierarchyUndo(panel.gameObject, "修复暂停菜单布局");

        // 1) PausePanel 本身：铺满全屏，当压暗遮罩
        RectTransform prt = (RectTransform)panel;
        prt.anchorMin = Vector2.zero;
        prt.anchorMax = Vector2.one;
        prt.offsetMin = Vector2.zero;
        prt.offsetMax = Vector2.zero;
        prt.localScale = Vector3.one;

        Image scrim = panel.GetComponent<Image>();
        if (scrim == null) scrim = panel.gameObject.AddComponent<Image>();
        scrim.color = ScrimColor;
        scrim.raycastTarget = true;

        // 2) 中间的面板盒子（子物体叫 Image）
        Transform boxT = panel.Find("Image");
        if (boxT == null)
        {
            GameObject boxGo = new GameObject("Box", typeof(RectTransform), typeof(Image));
            boxGo.transform.SetParent(panel, false);
            boxT = boxGo.transform;
        }

        RectTransform box = (RectTransform)boxT;
        box.anchorMin = new Vector2(0.5f, 0.5f);
        box.anchorMax = new Vector2(0.5f, 0.5f);
        box.pivot = new Vector2(0.5f, 0.5f);
        box.anchoredPosition = Vector2.zero;
        box.sizeDelta = BoxSize;
        box.localScale = Vector3.one;

        Image boxImage = boxT.GetComponent<Image>();
        if (boxImage == null) boxImage = boxT.gameObject.AddComponent<Image>();
        boxImage.color = BoxColor;

        // 3) 标题「暂停」
        foreach (Transform child in panel)
        {
            TextMeshProUGUI tmp = child.GetComponent<TextMeshProUGUI>();
            if (tmp == null || !tmp.text.StartsWith("暂停")) continue;

            RectTransform rt = tmp.rectTransform;
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -48f);
            rt.sizeDelta = new Vector2(640f, 120f);
            rt.localScale = Vector3.one;

            tmp.fontSize = 64f;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.raycastTarget = false;
        }

        // 4) 两个按钮：靠文字区分上下位置
        int fallbackIndex = 0;
        foreach (Transform child in panel)
        {
            Button button = child.GetComponent<Button>();
            if (button == null) continue;

            RectTransform rt = (RectTransform)child;
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = ButtonSize;
            rt.localScale = Vector3.one;

            TextMeshProUGUI label = child.GetComponentInChildren<TextMeshProUGUI>(true);
            string text = label != null ? label.text : string.Empty;

            float y;
            if (text.Contains("返回")) y = -150f;
            else if (text.Contains("继续")) y = -20f;
            else y = fallbackIndex == 0 ? -20f : -150f;
            fallbackIndex++;

            rt.anchoredPosition = new Vector2(0f, y);

            Image buttonImage = child.GetComponent<Image>();
            if (buttonImage != null) buttonImage.color = ButtonColor;

            if (label != null)
            {
                RectTransform lrt = label.rectTransform;
                lrt.anchorMin = Vector2.zero;
                lrt.anchorMax = Vector2.one;
                lrt.offsetMin = Vector2.zero;
                lrt.offsetMax = Vector2.zero;
                lrt.localScale = Vector3.one;

                label.fontSize = 38f;
                label.color = Color.white;
                label.alignment = TextAlignmentOptions.Center;
                label.raycastTarget = false;
            }
        }

        // 5) 让整个暂停 Canvas 压在血条上面
        Canvas canvas = panel.GetComponentInParent<Canvas>();
        if (canvas != null && canvas != panel.GetComponent<Canvas>())
            canvas.sortingOrder = 10;

        // 6) 顺手提醒一下遗留物体
        Transform stray = panel.Find("wall");
        if (stray != null)
            Debug.LogWarning("[暂停菜单] PausePanel 里还挂着一个叫 wall 的子物体，建议手动删掉。");

        EditorSceneManager.MarkSceneDirty(panel.gameObject.scene);
    }
}
#endif
