using UnityEngine;
using UnityEngine.InputSystem; // REQUIRED for the new system

public class PlayerAttack : MonoBehaviour
{
    [Header("Settings")]
    public int damageAmount = 10;
    public float attackRange = 3f;
    public float attackRate = 1f;

    [Header("References")]
    public Camera playerCamera;

    private float nextAttackTime = 0f;

    void Update()
    {
        // FIX: Using Mouse.current.leftButton instead of Input.GetButtonDown
        if (Time.time >= nextAttackTime && Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Attack();
            nextAttackTime = Time.time + 1f / attackRate;
        }
    }

    void Attack()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, attackRange))
        {
            EnemyHealth enemy = hit.transform.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damageAmount);
            }
        }
    }
}