using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Setup")]
    public Transform player;
    public float attackRange = 1.5f; // How close to get before attacking
    public float timeBetweenAttacks = 1.5f; // Cooldown in seconds
    public int attackDamage = 10;

    private NavMeshAgent agent;
    private Animator animator;
    private float attackTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        // 1. Calculate distance to player
        float distance = Vector3.Distance(transform.position, player.position);

        // 2. Manage the cooldown timer
        attackTimer += Time.deltaTime;

        // 3. DECISION: Attack or Chase?
        if (distance <= attackRange)
        {
            // --- ATTACK MODE ---
            // Stop moving so he doesn't push the player
            agent.isStopped = true; 
            // Reset speed animation to 0 so he doesn't "moonwalk"
            animator.SetFloat("Speed", 0); 

            if (attackTimer >= timeBetweenAttacks)
            {
                AttackPlayer();
            }
        }
        else
        {
            // --- CHASE MODE ---
            agent.isStopped = false;
            agent.SetDestination(player.position);
            animator.SetFloat("Speed", agent.velocity.magnitude);
        }
    }

    void AttackPlayer()
    {
        // Reset timer
        attackTimer = 0f;

        // A. Play Animation
        animator.SetTrigger("Attack");

        // B. Deal Damage
        // This looks for a "Health" script or "IDamageable" on the player
        // Adjust "IDamageable" to whatever script holds your Player's HP!
        var playerHealth = player.GetComponent<IDamageable>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
        }
        else
        {
            Debug.Log("Attacking Player! (But Player has no IDamageable script)");
        }
    }
}