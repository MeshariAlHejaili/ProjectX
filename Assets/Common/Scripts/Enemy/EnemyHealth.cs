using UnityEngine;
using UnityEngine.AI; // Needed for disabling NavMeshAgent

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 50;
    private int currentHealth;
    private Animator animator;
    private bool isDead = false;

    private void Awake()
    {
        // Use GetComponent<Animator>() if the animator is on the root object, 
        // or GetComponentInChildren<Animator>() if it's on a child mesh.
        // Assuming your original line is correct:
        animator = GetComponentInChildren<Animator>(); 
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return; // Ignore damage if already dead

        currentHealth -= amount;
        Debug.Log($"Enemy HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
            return; // Exit immediately after calling Die()
        }
        
        // 1. Play "Flinch" Animation only if not dead
        // (Make sure you have a Trigger named "Damage" in Animator!)
        if (animator != null) 
        {
            animator.SetTrigger("Damage");
        }
    }

    private void Die()
    {
        // Set 'isDead' right at the beginning of the death process
        isDead = true;

        // 1. Disable Brain and Physics so he stops moving
        if (GetComponent<EnemyAI>()) GetComponent<EnemyAI>().enabled = false;
        if (GetComponent<NavMeshAgent>()) GetComponent<NavMeshAgent>().enabled = false;
        
        // Disable the character controller or main collider
        Collider mainCollider = GetComponent<Collider>();
        if (mainCollider != null) mainCollider.enabled = false;
        
        // 2. Play Death Animation
        if (animator != null) 
        {
            // IMPORTANT: If you have a death animation, this will play it.
            // Ensure the death animation doesn't loop and ends naturally.
            animator.SetTrigger("Die");
        }

        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.RegisterEnemyDeath();
        }

        // 3. Delete body after 5 seconds to clear memory
        Destroy(gameObject, 5f);
    }
}