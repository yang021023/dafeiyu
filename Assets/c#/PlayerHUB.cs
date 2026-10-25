using UnityEngine;
using UnityEngine.UI;

public class PlayerHUB : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public Image healthFill;

    private void Awake(){
        // 兜底：Inspector 里忘了连玩家时，自动在场景里找一个
        if(playerHealth == null){
            playerHealth = Object.FindFirstObjectByType<PlayerHealth>();
            if(playerHealth == null)
                Debug.LogWarning("[HUD] 场景里没找到 PlayerHealth，血条不会更新", this);
            else
                Debug.LogWarning($"[HUD] Player Health 没在 Inspector 里连上，已自动找到 '{playerHealth.name}'。建议手动拖一下。", this);
        }
    }

    void Update()
    {
        if(playerHealth==null || healthFill==null) return;

        float ratio=playerHealth.maxHealth>0?(float) playerHealth.CurrentHealth / playerHealth.maxHealth:0.1f;
        healthFill.fillAmount = Mathf.Clamp01(ratio);
    }
}
