using UnityEngine;
using UnityEngine.InputSystem; 
using System.Collections.Generic; // <--- This line must be placed here

public class PlayerSword : MonoBehaviour
{
    [Header("Configuration")]
    public Transform attackPoint; 
    public float attackRange = 1.0f;
    public LayerMask enemyLayers;
    
    [Header("Combo Damage")]
    public int attack1Damage = 25;
    public int attack2Damage = 50; 

    private HashSet<Collider> _alreadyHit; 

    private void Awake()
    {
        _alreadyHit = new HashSet<Collider>();
    }

    // This method will be called directly by the Animation Events
    public void ExecuteAttack(int attackID)
    {
        // 1. Clear the HashSet at the start of the swing
        _alreadyHit.Clear();

        int damageToDeal = GetDamageForAttack(attackID);

        // 2. Check for hits in a sphere around the attackPoint
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayers);

        // 3. Loop through all enemies found and deal damage
        foreach (Collider enemy in hitEnemies)
        {
            if (_alreadyHit.Contains(enemy))
            {
                continue;
            }
            
            EnemyHealth hp = enemy.GetComponent<EnemyHealth>();
            if (hp != null)
            {
                hp.TakeDamage(damageToDeal);
                _alreadyHit.Add(enemy); // Add the enemy to the list of hit targets
            }
        }
    }
    
    // Helper function to get the correct damage value
    private int GetDamageForAttack(int attackID)
    {
        switch (attackID)
        {
            case 1:
                return attack1Damage;
            case 2:
                return attack2Damage;
            default:
                Debug.LogWarning("Unknown attack ID used. Defaulting to Attack 1 damage.");
                return attack1Damage;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}