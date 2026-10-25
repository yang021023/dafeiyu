using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

// 玩家血掉光后弹出失败面板，可以重开本关或回菜单。
// 面板本身由「工具 → 生成 Game Over 面板」自动搭好，不用手动连。
public class GameOverUI : MonoBehaviour
{
    [Header("失败面板（默认隐藏，必须是本对象的子物体）")]
    public GameObject panel;

    [Header("回菜单时加载的场景名")]
    public string menuSceneName = "menu";

    [Tooltip("死亡后延迟多久弹面板，留一点反应时间")]
    [Min(0f)] public float showDelay = 0.6f;

    [Header("调试")]
    [Tooltip("按 K 直接扣光血，用来测试失败流程；发布前关掉")]
    public bool allowDebugKill = true;

    private PlayerHealth player;
    private bool shown;

    private void Start()
    {
        Time.timeScale = 1f;
        shown = false;
        if (panel != null) panel.SetActive(false);
        TryBindPlayer();
    }

    private void Update()
    {
        if (player == null) TryBindPlayer();

        // 调试：按 K 直接死，省得被敌人打五次
        if (allowDebugKill && player != null && !shown &&
            Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame)
        {
            player.TakeDamage(player.maxHealth + 99);
        }
    }

    private void TryBindPlayer()
    {
        player = Object.FindFirstObjectByType<PlayerHealth>();
        if (player == null) return;

        player.Died -= OnPlayerDied;   // 避免重复订阅
        player.Died += OnPlayerDied;
    }

    private void OnDestroy()
    {
        if (player != null) player.Died -= OnPlayerDied;
    }

    private void OnPlayerDied()
    {
        if (shown) return;
        shown = true;
        StartCoroutine(ShowAfterDelay());
    }

    private IEnumerator ShowAfterDelay()
    {
        // 用 Realtime，因为稍后会把 timeScale 设成 0
        yield return new WaitForSecondsRealtime(showDelay);

        if (panel != null) panel.SetActive(true);
        Time.timeScale = 0f;
    }

    // 给「重开本关」按钮用
    public void RestartLevel()
    {
        Time.timeScale = 1f;

        // 用玩家所在的场景名，而不是「激活场景」——
        // 编辑器里多场景同时 Play 时，激活场景可能不是正在玩的那一关。
        string sceneName = player != null ? player.gameObject.scene.name : null;
        if (!string.IsNullOrEmpty(sceneName))
        {
            SceneManager.LoadScene(sceneName);
            return;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // 给「返回菜单」按钮用
    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }
}
