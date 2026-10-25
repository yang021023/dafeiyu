using UnityEngine;

// 屏幕震动：任何地方调用 CameraShake.Shake(强度, 时长) 即可
[DefaultExecutionOrder(1000)]   // 保证在 camerafollow 之后执行
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    private float shakeUntil;
    private float strength;     // ← 之前漏声明了

    private void Awake(){
        Instance = this;
    }

    private void OnDestroy(){
        if (Instance == this) Instance = null;   // ← 之前写成了 ==
    }

    /// <param name="amount">强度（世界单位）：0.05 很轻，0.3 很猛</param>
    public static void Shake(float amount, float duration = 0.12f)
    {
        if (Instance == null)
        {
            Camera cam = Camera.main;
            if (cam != null) Instance = cam.GetComponent<CameraShake>();
            if (Instance == null && cam != null)
                Instance = cam.gameObject.AddComponent<CameraShake>();
        }
        if (Instance == null) return;

        Instance.strength = Mathf.Max(Instance.strength, amount);
        Instance.shakeUntil = Time.time + duration;   // ← 之前是 tiime
    }

    private void LateUpdate(){
        if (Time.time >= shakeUntil){
            strength = 0f;
            return;
        }

        Vector2 offset = Random.insideUnitCircle * strength;
        transform.position += new Vector3(offset.x, offset.y, 0f);
    }
}
