using UnityEngine;

[RequireComponent(typeof(Camera))]
public class camerafollow : MonoBehaviour
{
    [Header("玩家角色")]
    public Transform target;
    [Header("平滑系数 0~1")]
    [Range(0f, 1f)] public float smoothSpeed = 0.125f;
    [Header("房间边界（使用多边形的外接矩形范围）")]
    [Tooltip("可留空自由跟随。请使用包围整个房间的矩形，不要沿平台轮廓绘制。")]
    public PolygonCollider2D mapBoundsPoly;

    private Camera cam;
    private bool snapOnNextFrame;

    void Awake()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true;
    }

    void OnEnable()
    {
        // 等到所有 Start（包括入口传送）结束后，再确定首次镜头位置。
        snapOnNextFrame = true;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // 相机目标位置（跟随玩家）
        Vector3 desired = target.position;
        desired.z = transform.position.z;
        desired = ClampToRoom(desired);

        // 保留原先 60 帧时的平滑手感，同时避免帧率影响跟随速度。
        float blend = 1f - Mathf.Pow(1f - Mathf.Clamp01(smoothSpeed), Time.deltaTime * 60f);
        Vector3 next = snapOnNextFrame
            ? desired
            : Vector3.Lerp(transform.position, desired, blend);

        // 即使初始相机在边界外，也能立即回到合法位置。
        transform.position = ClampToRoom(next);
        snapOnNextFrame = false;
    }

    private Vector3 ClampToRoom(Vector3 position)
    {
        if (mapBoundsPoly == null || !mapBoundsPoly.isActiveAndEnabled)
            return position;

        Bounds bounds = mapBoundsPoly.bounds;
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        position.x = ClampAxis(position.x, bounds.min.x, bounds.max.x, halfW);
        position.y = ClampAxis(position.y, bounds.min.y, bounds.max.y, halfH);
        return position;
    }

    private static float ClampAxis(float value, float min, float max, float halfView)
    {
        // 房间小于镜头时固定在房间中心，避免上下限反转。
        if (max - min <= halfView * 2f)
            return (min + max) * 0.5f;

        return Mathf.Clamp(value, min + halfView, max - halfView);
    }
}
