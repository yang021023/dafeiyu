using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy2D : MonoBehaviour
{
    public Transform player;

    public int contactDamage=1;
    public int maxHealth=3;

    public float detectRange=6f;
    public float moveSpeed=2f;
    public float stopDistance=0.7f;

    [Header("巡逻")]
    [Min(0.1f)]
    public float partroHalfDistance=3f;
    private float patrolCenterX;
    private int patrolDirection=1;

    public float knockbackSpeed=5f;
    public float knockbackTime=0.15f;

    private Rigidbody2D body;
    private int health;
    private float knockbackUntil;
    private bool dead;

    [Header("动画")]
    [SerializeField] private Animator animator;

    [Min(0.1f)]
    public float patrolTurnTime = 2f; // 最多朝一个方向走2秒
    private float patrolTimer;

    private static readonly int LeftState=Animator.StringToHash("Base Layer.left");
    private static readonly int RightState=Animator.StringToHash("Base Layer.right");
    private int currentAnimatorState;


    private void Awake(){
        body=GetComponent<Rigidbody2D>();
        body.constraints |=RigidbodyConstraints2D.FreezeRotation;
        health=maxHealth;
        patrolCenterX=body.position.x;

        if(animator==null) animator=GetComponentInChildren<Animator>();
        if(animator !=null) animator.applyRootMotion=false;
    }

    private void FixedUpdate(){
        if(dead || Time.time < knockbackUntil) return;
        float speed =0f;
        bool playerNearby=false;

        if(player != null){
            Vector2 offset=(Vector2)(player.position - transform.position);

            playerNearby=offset.magnitude<=detectRange;
            if(playerNearby){
                if(Mathf.Abs(offset.x)>stopDistance){
                    speed=Mathf.Sign(offset.x)*moveSpeed;
                }
            }
        }

        if (!playerNearby)
        {
            float leftEdge = patrolCenterX - partroHalfDistance;
            float rightEdge = patrolCenterX + partroHalfDistance;

            // 只有巡逻时才计时
            patrolTimer += Time.fixedDeltaTime;

            // 检查当前前进方向对应的边界
            bool reachedEdge = patrolDirection > 0
                ? body.position.x >= rightEdge
                : body.position.x <= leftEdge;

            // 到达边界，或者时间到了，就反向
            if (reachedEdge || patrolTimer >= patrolTurnTime)
            {
                patrolDirection *= -1;
                patrolTimer = 0f;
            }

            float targetX = patrolDirection > 0 ? rightEdge : leftEdge;

            float stepSpeed = Mathf.Min(
                moveSpeed,
                Mathf.Abs(targetX - body.position.x) / Time.fixedDeltaTime
            );

            speed = patrolDirection * stepSpeed;
        }
        else
        {
            // 追踪玩家时清空巡逻计时
            patrolTimer = 0f;
        }
        body.linearVelocity=new Vector2(speed,body.linearVelocity.y);
        UpdateAnimation(speed);
    }

    private void UpdateAnimation(float speed){
        if(animator==null || animator.runtimeAnimatorController==null) return;
        if(Mathf.Abs(speed)<0.01f){
            animator.speed=0f;
            return;
        }
        animator.speed=1f;
        int nextState=speed>0f?RightState:LeftState;
        if(nextState == currentAnimatorState) return;
        if(!animator.HasState(0,nextState)) return;
        animator.Play(nextState,0,0f);
        currentAnimatorState=nextState;
    }

    public void TakeDamage(int damage,Vector2 attackerPosition){
        if(damage <= 0 || dead) return;

        health -=damage;
        Debug.Log($"怪物生命:{health}/{maxHealth}",this);

        if(health<=0){
            dead=true;
            Destroy(gameObject);
            return;
        }

        float direction=transform.position.x >= attackerPosition.x ? 1f : -1f;
        knockbackUntil =Time.time + knockbackTime;
        body.linearVelocity=new Vector2(direction*knockbackSpeed,body.linearVelocity.y);
    }

    private void OnCollisionEnter2D(Collision2D collision){
        DamagePlayer(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision){
        DamagePlayer(collision.collider);
    }

    // 子物体的伤害触发器共用根物体 Rigidbody2D，事件会传到这里。
    private void OnTriggerEnter2D(Collider2D other){
        DamagePlayer(other);
    }

    private void OnTriggerStay2D(Collider2D other){
        DamagePlayer(other);
    }

    private void DamagePlayer(Collider2D other){
        if(dead) return;
        PlayerHealth target=other.GetComponentInParent<PlayerHealth>();
        if(target!=null) target.TakeDamage(contactDamage);
    }
}
