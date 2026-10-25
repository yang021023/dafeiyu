using UnityEngine;

// 治疗区域：待在触发器里，每隔一段时间回一次血
public class recover : MonoBehaviour
{
    [Tooltip("每次回复多少血")]
    public int healAmount = 1;

    [Tooltip("每隔几秒回一次")]
    [Min(0.1f)] public float healInterval = 1f;

    [Tooltip("进入后多久开始第一次治疗，0 = 立刻")]
    [Min(0f)] public float firstHealDelay = 0f;

    [Tooltip("玩家身上的治疗粒子：只要还没满血就持续播放")]
    public ParticleSystem healFx;

    private int insideCount;      // 有几个碰撞体在里面
    private PlayerHealth target;  // 要治疗谁
    private float nextHealAt;
    private bool lastNeedHeal;    // 只用于日志，避免刷屏

    // 在 Inspector 里点 Reset 或重新加组件时，自动把碰撞体勾成 Trigger
    private void Reset(){
        Collider2D col = GetComponent<Collider2D>();
        if(col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other){ SetInside(other, true); }
    private void OnTriggerExit2D(Collider2D other){ SetInside(other, false); }

    private void SetInside(Collider2D other, bool entering){
        PlayerHealth hp = other.GetComponentInParent<PlayerHealth>();
        if(hp == null) return;   // 不是玩家就忽略

        Debug.Log($"[Recover] {(entering ? "进入" : "离开")}触发器：{other.name}", this);

        if(entering){
            insideCount++;
            target = hp;
            nextHealAt = Time.time + firstHealDelay;
        } else {
            insideCount = Mathf.Max(0, insideCount - 1);
            if(insideCount == 0) target = null;
        }
    }

    private void Update(){
        if(Time.timeScale <= 0f) return;                 // 暂停不治疗
        UpdateHealFx();
        if(insideCount <= 0 || target == null) return;   // 没人在里面
        if(Time.time < nextHealAt) return;               // 还没到下一次

        nextHealAt = Time.time + healInterval;
        target.Heal(healAmount);
    }

    // 在范围内 + 还没满血 = 一直冒；离开或回满了就停
    private void UpdateHealFx(){
        if(healFx == null) return;

        bool needHeal = insideCount > 0 && target != null
                        && target.CurrentHealth < target.maxHealth;

        // 只在状态切换时打日志，方便排查又不会刷屏
        if(needHeal != lastNeedHeal){
            lastNeedHeal = needHeal;
            string hp = target != null ? target.CurrentHealth + "/" + target.maxHealth : "没有目标";
            Debug.Log($"[Recover] 粒子{(needHeal ? "开始" : "停止")}｜在区域内={(insideCount > 0)}｜血量={hp}｜粒子引用={(healFx != null ? "已连" : "未连")}", this);
        }

        if(needHeal){
            if(!healFx.isPlaying) healFx.Play();
        } else {
            if(healFx.isPlaying) healFx.Stop();
        }
    }
}
