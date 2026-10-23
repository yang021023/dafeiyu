using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;


public class PauseMenu : MonoBehaviour
{
    [Header("暂停面板")]
    public GameObject panel;

    [Header("返回菜单场景名字")]
    public string menuSceneName="menu";

    private bool isPause;

    void Start()
    {
        Time.timeScale=1f;
        isPause=false;
        if(panel !=null) panel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(Keyboard.current == null) return;
        if(Keyboard.current.escapeKey.wasPressedThisFrame) Toggle();
    }

    public void Toggle(){
        if(isPause) Resume();
        else Pause();
    }

    public void Pause(){
        if(isPause) return;
        isPause=true;
        if(panel!=null) panel.SetActive(true);
        Time.timeScale=0f;
    }

    public void Resume(){
        isPause = false;
        if(panel !=null) panel.SetActive(false);
        Time.timeScale=1f;
    }

    public void BackToMenu(){
        Time.timeScale=1f;                 // 先恢复时间，否则菜单场景是冻结的
        SceneManager.LoadScene(menuSceneName);
    }
}
