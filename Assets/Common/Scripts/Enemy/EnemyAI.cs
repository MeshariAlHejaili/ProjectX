using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Setup")]
    public Transform player;
    public float attackRange = 1.5f;
    public float timeBetweenAttacks = 1.5f;
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

        float distance = Vector3.Distance(transform.position, player.position);
        attackTimer += Time.deltaTime;

        if (distance <= attackRange)
        {
            // --- ATTACK MODE ---
            agent.isStopped = true;
            
            // STOP WALKING: We tell the animator we are NOT moving
            animator.SetBool("IsMoving", false); 

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

            // START WALKING: If our speed is > 0.1, we are moving
            // This line fixes your error!
            bool currentlyMoving = agent.velocity.magnitude > 0.1f;
            animator.SetBool("IsMoving", currentlyMoving);
        }
    }

    void AttackPlayer()
    {
        attackTimer = 0f;
        
        // Ensure you have a Trigger named "Attack" (or "AttackTrigger")
        animator.SetTrigger("Attack"); 

        var playerHealth = player.GetComponent<IDamageable>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
        }
    }
}