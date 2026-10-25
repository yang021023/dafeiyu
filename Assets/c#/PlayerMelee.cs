using UnityEngine;
using System.Collections.Generic;
[RequireComponent(typeof(DafeiyuMovement))]
public class PlayerMelee : MonoBehaviour
{
    public LayerMask enemyLayer;
    public int damage=1;

    public float forwardOffset=1.2f;
    public float heightOffSet=0f;
    public Vector2 attackSize=new Vector2(2f,2f);

    private DafeiyuMovement movement;

    private readonly HashSet<Enemy2D> hitEnemies=new HashSet<Enemy2D>();

    private void Awake(){
        movement=GetComponent<DafeiyuMovement>();
    }

    private Vector2 AttackCenter(int direction){
        return (Vector2)transform.position+new Vector2(forwardOffset*direction,heightOffSet);
    }

    private void LateUpdate(){
        if(movement == null || !movement.isActiveAndEnabled || !movement.IsAttackActive){
            hitEnemies.Clear();
            return;
        }
    Vector2 center = AttackCenter(movement.AttackFacing);

    Collider2D[] hits=Physics2D.OverlapBoxAll(center,attackSize,0f,enemyLayer);

    foreach(Collider2D hit in hits){
        Enemy2D enemy=hit.GetComponentInParent<Enemy2D>();
        if(enemy!=null && hitEnemies.Add(enemy)){
            enemy.TakeDamage(damage,transform.position);
            CameraShake.Shake(0.05f, 0.18f);
        }
    }

    }

    private void OnDrawGizmosSelected(){
        DafeiyuMovement controller=GetComponent<DafeiyuMovement>();
        int direction=1;
        if(Application.isPlaying && controller!=null){
            direction=controller.IsAttacking?controller.AttackFacing:controller.Facing;
        }
        Gizmos.color=Color.red;
        Gizmos.DrawWireCube(AttackCenter(direction),new Vector3(attackSize.x,attackSize.y,0f));
    }

  
}
