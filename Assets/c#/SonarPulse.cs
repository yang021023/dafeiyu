using UnityEngine;

// 声纳：身边一圈常亮 + 外面一圈脉冲扫过 + 一条可见的波纹环
[RequireComponent(typeof(SpriteMask))]
public class SonarPulse : MonoBehaviour
{
    [Header("跟随谁")]
    public Transform target;

    [Header("常亮范围（固定大小）")]
    public SpriteMask lightMask;
    [Min(0.1f)] public float lightRadius = 3f;

    [Header("脉冲（从常亮边缘往外扫）")]
    [Min(0.1f)] public float pulseStartRadius = 3.2f;
    public float maxRadius = 14f;
    [Min(0.1f)] public float duration = 1.5f;
    [Min(0f)] public float interval = 0.6f;
    public bool loop = true;

    [Header("波纹视觉（可选）")]
    [Tooltip("显示圆环的 SpriteRenderer：跟着脉冲放大并淡出")]
    public SpriteRenderer rippleRing;

    private SpriteMask pulseMask;
    private float timer;
    private float pulseBaseDiameter;
    private float lightBaseDiameter;
    private float ringBaseDiameter;

    private void Awake(){
        pulseMask = GetComponent<SpriteMask>();
        pulseBaseDiameter = DiameterOf(pulseMask);
        ResolveTarget();   // 跨场景复制后 target 会是空的，自动找一个

        if (lightMask != null){
            lightBaseDiameter = DiameterOf(lightMask);
            lightMask.transform.localScale = Vector3.one * (lightRadius * 2f / lightBaseDiameter);
        }

        if (rippleRing != null){
            ringBaseDiameter = rippleRing.sprite != null
                ? rippleRing.sprite.bounds.size.x : 1f;
            if (ringBaseDiameter <= 0.01f) ringBaseDiameter = 1f;
        }

        transform.localScale = Vector3.one * (pulseStartRadius * 2f / pulseBaseDiameter);
    }

    private static float DiameterOf(SpriteMask mask){
        if (mask == null || mask.sprite == null) return 1f;
        float d = mask.sprite.bounds.size.x;
        return d > 0.01f ? d : 1f;
    }

    // 从别的场景复制过来时，指向玩家的引用会丢，这里兜底自动找
    private void ResolveTarget(){
        if (target != null) return;
        DafeiyuMovement player = Object.FindFirstObjectByType<DafeiyuMovement>();
        if (player != null) target = player.transform;
    }

    private void LateUpdate(){
        if (target == null){
            ResolveTarget();
            if (target == null) return;
        }

        float x = target.position.x;
        float y = target.position.y;

        // 遮罩跟随
        transform.position = new Vector3(x, y, transform.position.z);
        if (lightMask != null)
            lightMask.transform.position = new Vector3(x, y, lightMask.transform.position.z);

        if (lightMask != null)
            lightMask.transform.localScale = LocalScaleFor(lightMask.transform, lightRadius * 2f, lightBaseDiameter);

        // 脉冲进度
        timer += Time.deltaTime;
        float total = duration + interval;
        float t = loop ? Mathf.Repeat(timer, total) : timer;
        float p = Mathf.Clamp01(t / duration);

        float radius = Mathf.SmoothStep(pulseStartRadius, maxRadius, p);
        transform.localScale = LocalScaleFor(transform, radius * 2f, pulseBaseDiameter);

        // 波纹环：跟脉冲同样大小，越扩越淡
        if (rippleRing != null){
            rippleRing.transform.position = new Vector3(x, y, rippleRing.transform.position.z);
            rippleRing.transform.localScale = LocalScaleFor(rippleRing.transform, radius * 2f, ringBaseDiameter);

            Color c = rippleRing.color;
            c.a = 1f - p;              // p=0 全亮，p=1 全透明
            rippleRing.color = c;
        }
    }

    // 算 localScale：抵消父物体的缩放，保证参数就是"世界单位"，
    // 同时按 X/Y 分别补偿，父物体非均匀缩放也不会变成椭圆
    private static Vector3 LocalScaleFor(Transform t, float worldDiameter, float baseDiameter)
    {
        float k = worldDiameter / Mathf.Max(baseDiameter, 0.0001f);

        Vector3 p = (t != null && t.parent != null) ? t.parent.lossyScale : Vector3.one;
        float px = Mathf.Abs(p.x) > 0.0001f ? Mathf.Abs(p.x) : 1f;
        float py = Mathf.Abs(p.y) > 0.0001f ? Mathf.Abs(p.y) : 1f;

        return new Vector3(k / px, k / py, 1f);
    }
}