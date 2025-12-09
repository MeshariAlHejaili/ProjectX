using UnityEngine;
using UnityEngine.InputSystem; // REQUIRED for the new system

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
    float nextAttackTime = 0f;

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
    }

    void Update()
    {
        // FIX: Using Mouse.current.leftButton instead of Input.GetButtonDown
        if (Time.time >= nextAttackTime && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Slash();
            nextAttackTime = Time.time + 1f / attackRate;
        }
    }

    void Slash()
    {
        if (animator != null) animator.SetTrigger("Attack");

        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayers);

        foreach (Collider enemy in hitEnemies)
        {
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