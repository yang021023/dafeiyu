using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("开始游戏时要加载的场景")]
    public string firstSceneName = "room1";

    // 绑定到“开始游戏”按钮
    public void StartGame()
    {
        Time.timeScale = 1f;

        if (string.IsNullOrWhiteSpace(firstSceneName))
        {
            Debug.LogError("请在 MainMenu 的 First Scene Name 中填写场景名。", this);
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(firstSceneName))
        {
            Debug.LogError("场景没有加入 Scene List，或场景名写错：" + firstSceneName, this);
            return;
        }

        SceneManager.LoadSceneAsync(firstSceneName);
    }

    // 绑定到“退出游戏”按钮
    public void QuitGame()
    {
        Debug.Log("退出游戏");
        Application.Quit();
    }
}
