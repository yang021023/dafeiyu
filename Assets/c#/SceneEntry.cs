using UnityEngine;
public class SceneEntry : MonoBehaviour
{
    [Header("入口编号，例如 FromRoom01")]
    public string entryId;
    public static string PendingScene;
    public static string PendingEntryId;
    void Start()
    {
        if(string.IsNullOrEmpty(PendingEntryId) ) return;

        if(gameObject.scene.name!=PendingScene || entryId!=PendingEntryId) return;

        DafeiyuMovement player=Object.FindAnyObjectByType<DafeiyuMovement>();
        if(player==null){
            Debug.LogError("新房间没有找到角色",this);
            return;
        }
        Vector3 position=transform.position;
        position.z=player.transform.position.z;
        player.transform.position=position;
        Rigidbody2D body=player.GetComponent<Rigidbody2D>();

        if(body!=null){
            body.position=new Vector2(position.x,position.y);
            body.linearVelocity=Vector2.zero;
            body.angularVelocity=0f;
        }
        PendingScene=null;
        PendingEntryId=null;

    }

    private void OnDrawGizmos(){
    Gizmos.color=Color.green;
    Gizmos.DrawWireSphere(transform.position,0.25f);
   }
}
