using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("开始游戏时要加载的场景")]
    public string firstSceneName = "room1";

    // 绑定到“开始游戏”按钮
    public void StartGame()
    {
        SaveSystem.Delete();      // 新游戏覆盖旧存档
        GameState.Clear();
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

    // 绑定到“继续游戏”按钮
    public void ContinueGame()
    {
        SaveData data = SaveSystem.Load();
        if (data == null)
        {
            Debug.Log("没有存档，不能继续");
            return;
        }

        GameState.Apply(data);
        Time.timeScale = 1f;

        if (string.IsNullOrWhiteSpace(data.sceneName) ||
            !Application.CanStreamedLevelBeLoaded(data.sceneName))
        {
            Debug.LogError("存档里的场景无法加载：" + data.sceneName, this);
            return;
        }

        SceneManager.LoadSceneAsync(data.sceneName);
    }

    // 给「继续游戏」按钮判断是否可点
    public bool HasSave() => SaveSystem.HasSave();

    // 绑定到“退出游戏”按钮
    public void QuitGame()
    {
        Debug.Log("退出游戏");
        Application.Quit();
    }
}
