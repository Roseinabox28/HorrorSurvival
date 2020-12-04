using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
   private HealthSystem healthSystem;


    void Start()
    {
        
        BaseEnemy enemy = GetComponent<BaseEnemy>();
        int healthMax = enemy.health;
        healthSystem = new HealthSystem(healthMax);
        healthSystem.OnHealthChanged += HealthSystem_OnHealthChanged;
    }

    private void HealthSystem_OnHealthChanged(object sender, System.EventArgs e)
    {
        Debug.Log("Enemy health changed");
    }
       
    public int GetEnemyHealth()
    {
        return healthSystem.GetHealth();
    }

    public float GetEnemyHealthPercent()
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
