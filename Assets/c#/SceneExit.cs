using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class SceneExit : MonoBehaviour
{
    [Header("目标房间")]
    public string targetScene;

    [Header("进入目标房间后，使用哪个入口")]
    [FormerlySerializedAs("targetEnteryId")]
    public string targetEntryId;
    private bool loading;

    protected virtual void OnTriggerEnter2D(Collider2D other){
        if(loading) return;

        DafeiyuMovement player=other.GetComponentInParent<DafeiyuMovement>();
        if(player==null) return;

        if(string.IsNullOrWhiteSpace(targetScene) || !Application.CanStreamedLevelBeLoaded(targetScene)){
            Debug.LogError( "场景无法加载，请检查名字和 Scene List：" + targetScene, this);
                return;
        }

        if(string.IsNullOrWhiteSpace(targetEntryId)){
             Debug.LogError("请填写目标入口编号。", this);
            return;
        }

        loading = true;

        SceneEntry.PendingScene=targetScene;
        SceneEntry.PendingEntryId=targetEntryId;

         SceneManager.LoadSceneAsync(targetScene, LoadSceneMode.Single);
    }
}
