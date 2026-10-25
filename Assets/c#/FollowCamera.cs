using UnityEngine;

// 让黑幕始终跟着相机、自动铺满整个视野
// 挂在黑幕的 SpriteRenderer 上，不用设父子关系
[RequireComponent(typeof(SpriteRenderer))]
public class FollowCamera : MonoBehaviour
{
    [Tooltip("留空则用 Camera.main")]
    public Camera targetCamera;

    [Tooltip("离相机多远（要比场景物体更靠近相机）")]
    public float distance = 10f;

    [Tooltip("放大一点，保证边缘不漏")]
    [Min(1f)] public float margin = 1.15f;

    private SpriteRenderer sr;
    private float baseWidth = 1f;
    private float baseHeight = 1f;

    private void Awake(){
        sr = GetComponent<SpriteRenderer>();
        if (sr.sprite != null){
            baseWidth  = Mathf.Max(0.01f, sr.sprite.bounds.size.x);
            baseHeight = Mathf.Max(0.01f, sr.sprite.bounds.size.y);
        }
    }

    private void LateUpdate(){
        Camera cam = targetCamera != null ? targetCamera : Camera.main;
        if (cam == null) return;

        Vector3 p = cam.transform.position;
        transform.position = new Vector3(p.x, p.y, p.z + distance);

        float h = cam.orthographicSize * 2f * margin;
        float w = h * cam.aspect;

        transform.localScale = new Vector3(w / baseWidth, h / baseHeight, 1f);
    }
}
