using UnityEngine;
using UnityEngine.UI;

public class PlayerHUB : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public Image healthFill;   
   

    void Update()
    {
        if(playerHealth==null || healthFill==null) return;

        float ratio=playerHealth.maxHealth>0?(float) playerHealth.CurrentHealth / playerHealth.maxHealth:0.1f;
        healthFill.fillAmount = Mathf.Clamp01(ratio);
    }
}
