using UnityEngine;
using UnityEngine.AI;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 50;
    private int currentHealth;
    private Animator animator;
    private bool isDead = false;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        Debug.Log($"Enemy HP: {currentHealth}");

        // 1. Play "Flinch" Animation
        // (Make sure you have a Trigger named "Damage" in Animator!)
        if (animator != null) animator.SetTrigger("Damage");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        // 1. Disable Brain and Physics so he stops moving
        if (GetComponent<EnemyAI>()) GetComponent<EnemyAI>().enabled = false;
        if (GetComponent<NavMeshAgent>()) GetComponent<NavMeshAgent>().enabled = false;
        if (GetComponent<Collider>()) GetComponent<Collider>().enabled = false;

        // 2. Play Death Animation
        if (animator != null) animator.SetTrigger("Die");

        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.RegisterEnemyDeath();
        }

        // 3. Delete body after 5 seconds to clear memory
        Destroy(gameObject, 5f);
    }
}