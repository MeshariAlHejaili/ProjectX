using UnityEngine;
using UnityEngine.InputSystem; 
using System.Collections;

public class PlayerSword : MonoBehaviour
{
    [Header("Configuration")]
    public Animator animator;
    public Transform attackPoint;
    public float attackRange = 1.0f;
    public LayerMask enemyLayers;
    public int damageAmount = 25;

    [Header("Cooldown")]
    public float attackRate = 2f;
    [HideInInspector] public float nextAttackTime = 0f; // Made public/hidden for PlayerController access

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
    }

    // --- NEW: Public method for PlayerController to call (Handles cooldown check) ---
    public bool TrySlash()
    {
        if (Time.time >= nextAttackTime)
        {
            Slash();
            nextAttackTime = Time.time + 1f / attackRate;
            return true;
        }
        return false;
    }

    void Slash()
    {
        if (animator != null) animator.SetTrigger("AttackTrigger");

        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider enemy in hitEnemies)
        {
            // Assuming the enemy has a component called EnemyHealth
            EnemyHealth hp = enemy.GetComponent<EnemyHealth>();
            if (hp != null)
            {
                hp.TakeDamage(damageAmount);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}