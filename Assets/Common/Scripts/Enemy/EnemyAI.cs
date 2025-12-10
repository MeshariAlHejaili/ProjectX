using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Setup")]
    public Transform player;
    public float attackRange = 1.5f;
    public float rotationSpeed = 5f; // New: For turning smoothly
    
    [Header("Combat")]
    public float timeBetweenAttacks = 1.5f;
    public int attackDamage = 10;

    private NavMeshAgent agent;
    private Animator animator;
    private float attackTimer = 0f;
    private bool isAttacking = false; // New: State to prevent movement during attack

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Check for player reference
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) player = playerObj.transform;
        }
        
        // Ensure NavMeshAgent speed matches animator expectations if using a blend tree
        // agent.speed = 3f; // Set a default speed if needed
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
            HandleRotation(); // Face the player while stopped

            if (!isAttacking && attackTimer >= timeBetweenAttacks)
            {
                AttackSequence();
            }
        }
        else
        {
            // --- CHASE MODE ---
            // Only move if we aren't locked in an attack sequence
            if (!isAttacking)
            {
                agent.isStopped = false;
                agent.SetDestination(player.position);
            }
        }
        
        HandleAnimation();
    }
    
    void HandleRotation()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        directionToPlayer.y = 0; // Ignore vertical difference
        
        if (directionToPlayer != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    void HandleAnimation()
    {
        // Use agent velocity for robust animation control
        float speed = agent.velocity.magnitude;
        bool isMoving = speed > 0.1f && agent.remainingDistance > agent.stoppingDistance;
        
        // Use the 'Speed' float parameter (better for blend trees)
        // Set the animator's speed to the NavMeshAgent's current speed/max speed ratio
        animator.SetFloat("Speed", Mathf.Clamp01(speed / agent.speed), 0.1f, Time.deltaTime);

        // Or use the simpler boolean if you prefer your current setup
        animator.SetBool("IsMoving", isMoving); 
    }

    void AttackSequence()
    {
        // Reset timer and set attacking state
        attackTimer = 0f;
        isAttacking = true;
        
        // Lock the agent immediately and trigger the animation
        agent.isStopped = true;
        animator.SetTrigger("Attack"); 
        
        // The animation event will call the DealDamage() function at the exact hit frame
    }
    
    // --- Public Function Called by Animation Event ---
    public void DealDamage()
    {
        // This function should be called at the exact moment the animation hits the player.
        
        // Check distance again to prevent damage if the player dashed away mid-swing
        if (Vector3.Distance(transform.position, player.position) <= attackRange * 1.5f) // Allow slight buffer
        {
            var playerHealth = player.GetComponent<IDamageable>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
            }
        }
    }
    
    // --- Public Function Called by Animation Event ---
    public void ResetAttackState()
    {
        // This must be called at the end of the attack animation (via Animation Event)
        isAttacking = false;
        
        // If the player is still far, immediately start chasing
        if (Vector3.Distance(transform.position, player.position) > attackRange)
        {
             agent.isStopped = false;
        }
    }
}