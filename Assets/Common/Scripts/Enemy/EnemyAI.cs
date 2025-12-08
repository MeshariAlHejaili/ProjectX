using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float chaseRange = 10f;
    [SerializeField] private float attackRange = 2f; // Must be slightly larger than Stopping Distance
    [SerializeField] private float timeBetweenAttacks = 1.5f;
    [SerializeField] private int attackDamage = 10;

    [Header("Setup")]
    [SerializeField] private string targetTag = "Player"; 

    // Internal Variables
    private NavMeshAgent agent;
    private Animator animator;
    private Transform targetTransform;
    private IDamageable targetHealth;
    private bool alreadyAttacked;

    // Animation Parameter IDs (Faster than using strings)
    private static readonly int IsMovingParam = Animator.StringToHash("IsMoving");
    private static readonly int AttackParam = Animator.StringToHash("Attack");

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        // Auto-find the player
        GameObject playerObj = GameObject.FindGameObjectWithTag(targetTag);
        if (playerObj != null)
        {
            targetTransform = playerObj.transform;
            targetHealth = playerObj.GetComponent<IDamageable>();
        }
        else
        {
            Debug.LogWarning("EnemyAI: Could not find object with tag 'Player'.");
        }
    }

    private void Update()
    {
        if (targetTransform == null) return;

        float distance = Vector3.Distance(transform.position, targetTransform.position);

        if (distance <= attackRange)
        {
            AttackBehavior();
        }
        else if (distance <= chaseRange)
        {
            ChaseBehavior();
        }
        else
        {
            IdleBehavior();
        }
    }

    private void IdleBehavior()
    {
        agent.isStopped = true;
        animator.SetBool(IsMovingParam, false);
    }

    private void ChaseBehavior()
    {
        agent.isStopped = false;
        agent.SetDestination(targetTransform.position);
        animator.SetBool(IsMovingParam, true);
    }

    private void AttackBehavior()
    {
        agent.isStopped = true;
        animator.SetBool(IsMovingParam, false);
        
        // Rotate to face the player smoothly
        Vector3 direction = (targetTransform.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero) 
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }

        if (!alreadyAttacked)
        {
            // 1. Trigger Animation
            animator.SetTrigger(AttackParam);

            // 2. Deal Damage directly (Simple logic)
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(attackDamage);
            }

            // 3. Cooldown
            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }
    
    // Visualize Ranges in Editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}