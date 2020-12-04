using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{

    private HealthSystem healthSystem;

    
    void Start()
    {
        
        Player player = GetComponent<Player>();
        int healthMax = player.maxHealth;
        healthSystem = new HealthSystem(healthMax);
        healthSystem.OnHealthChanged += HealthSystem_OnHealthChanged;
    }

    private void HealthSystem_OnHealthChanged(object sender, System.EventArgs e)
    {
        Debug.Log("Player health changed");
    }
       
    public int GetPlayerHealth()
    {
        return healthSystem.GetHealth();
    }

    public float GetPlayerHealthPercent()
    {
        return healthSystem.GetHealthPercent();
    }

    public void DoDamage(int damage)
    {
        healthSystem.Damage(damage);
    }

    public void DoHeal(int heal)
    {
        healthSystem.Heal(heal);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
