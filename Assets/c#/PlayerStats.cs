using UnityEngine;

// 比 PlayerHealth 先执行 Awake，这样拿到的是"没加成"的原始数值
[DefaultExecutionOrder(-100)]
public class PlayerStats : MonoBehaviour
{
    private PlayerMelee melee;
    private PlayerHealth health;
    private DafeiyuMovement move;

    private int baseDamage;
    private Vector2 baseAttackSize;
    private int baseMaxHealth;

    private void Awake()
    {
        melee  = GetComponent<PlayerMelee>();
        health = GetComponent<PlayerHealth>();
        move   = GetComponent<DafeiyuMovement>();

        if (melee != null)
        {
            baseDamage     = melee.damage;
            baseAttackSize = melee.attackSize;
        }
        if (health != null) baseMaxHealth = health.maxHealth;
    }

    private void OnEnable()
    {
        GameState.Equipment.OnChanged += Apply;
        Apply();
    }

    private void OnDisable()
    {
        GameState.Equipment.OnChanged -= Apply;
    }

    public void Apply()
    {
        int   dmg   = 0;
        float range = 0;
        int   hp    = 0;
        float spd   = 0;

        foreach (ItemData item in GameState.Equipment.AllEquipped())
        {
            dmg   += item.bonusDamage;
            range += item.bonusAttackRange;
            hp    += item.bonusMaxHealth;
            spd   += item.bonusSpeed;
        }

        if (melee != null)
        {
            melee.damage = baseDamage + dmg;
            melee.attackSize = new Vector2(baseAttackSize.x + range, baseAttackSize.y);
        }

        if (health != null)
        {
            int oldMax = health.maxHealth;
            health.maxHealth = baseMaxHealth + hp;

            // 上限提高时，把多出来的血补上
            if (health.maxHealth > oldMax)
                health.Heal(health.maxHealth - oldMax);
            // 上限降低时（卸下衣服），当前血不能超过新上限
            else if (health.maxHealth < oldMax)
                health.ClampToMax();
        }

        if (move != null)
            move.SpeedMultiplier = 1f + spd;
    }
}