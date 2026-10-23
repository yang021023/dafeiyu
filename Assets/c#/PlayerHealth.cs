using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5;
    public float invincibleTime=0.8f;

    public int CurrentHealth{get;private set;}
    public bool IsDead => CurrentHealth <=0;

    private float invincibleUnitil;

   private void Awake(){
        CurrentHealth = maxHealth;
   }

    public void TakeDamage(int damage){
        if(damage <= 0 || IsDead || Time.time < invincibleUnitil) return;

        CurrentHealth = Mathf.Max(0,CurrentHealth-damage);
        invincibleUnitil = Time.time + invincibleTime;

        Debug.Log($"玩家生命:{CurrentHealth}/{maxHealth}",this);

        if(IsDead){
            DafeiyuMovement movement=GetComponent<DafeiyuMovement>();

            if(movement!=null) movement.enabled=false;
            Debug.Log("玩家死亡",this);
        }
    }
}
