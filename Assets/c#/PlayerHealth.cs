using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5;
    public float invincibleTime=0.8f;

    [Header("掉出地图")]
    [Tooltip("角色的 Y 低于这个值就判定掉出地图，直接死亡")]
    public float fallDeathY = -20f;

    public int CurrentHealth{get;private set;}
    public bool IsDead => CurrentHealth <=0;

    // 血掉光时触发一次；GameOverUI 订阅它来弹失败面板。
    public event System.Action Died;

    private float invincibleUnitil;
    private bool deathHandled;

    [Header("被击退")]
    [Min(0f)] public float knockbackSpeed = 6f;
    [Min(0f)] public float knockbackTime = 0.15f;

    private void Awake(){
        if(GameState.PlayerHealth>0){
            if(GameState.PlayerMaxHealth>0) maxHealth=GameState.PlayerMaxHealth;
            SetHealth(GameState.PlayerHealth);
            Debug.Log($"[血量] 进入 {gameObject.scene.name}，从存档恢复 {CurrentHealth}/{maxHealth}",this);
        }else{
            SetHealth(maxHealth);
            Debug.Log($"[血量] 进入 {gameObject.scene.name}，新开局 {CurrentHealth}/{maxHealth}",this);
        }
    }

    private void SetHealth(int value){
        CurrentHealth=Mathf.Clamp(value,0,maxHealth);
        GameState.PlayerHealth=CurrentHealth;
        GameState.PlayerMaxHealth=maxHealth;
    }

    // 掉出地图判定：Y 低于阈值直接死
    private void Update(){
        if(!IsDead && transform.position.y < fallDeathY) Die();
    }

    public void TakeDamage(int damage){
    TakeDamage(damage, transform.position + Vector3.right);
    }

    public void TakeDamage(int damage, Vector2 sourcePosition){
        if(damage <= 0 || IsDead || Time.time < invincibleUnitil) return;

        SetHealth(CurrentHealth - damage);
        invincibleUnitil = Time.time + invincibleTime;

        Debug.Log($"玩家生命:{CurrentHealth}/{maxHealth}",this);

        if(IsDead){
            Die();
            return;
        }

        // 没死才击退
        CameraShake.Shake(0.18f, 0.18f);
        DafeiyuMovement movement = GetComponent<DafeiyuMovement>();
        if(movement != null) movement.ApplyKnockback(sourcePosition, knockbackSpeed, knockbackTime);
    }

    // 治疗：不会超过上限；死了就不能治
    public void Heal(int amount){
        if(amount <= 0 || IsDead) return;

        SetHealth(CurrentHealth + amount);
        Debug.Log($"玩家回血:{CurrentHealth}/{maxHealth}",this);
    }

    // 最大血量变小后（比如卸下加血装备）调用：当前血不能超过上限
    public void ClampToMax(){
        SetHealth(Mathf.Min(CurrentHealth, maxHealth));
    }

    // 直接死亡：掉出地图、被陷阱秒杀都走这里
    public void Die(){
        if(deathHandled) return;
        deathHandled = true;
        SetHealth(0);      // 存档里也变成 0，下次进场景走"满血"分支

        DafeiyuMovement movement=GetComponent<DafeiyuMovement>();
        if(movement!=null) movement.enabled=false;

        Debug.Log("玩家死亡",this);
        Died?.Invoke();
    }
}
