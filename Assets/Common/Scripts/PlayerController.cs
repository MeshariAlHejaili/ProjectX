using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    public float attackCooldown = 1.0f;
    
    [Header("Roll Settings")]
    public float rollSpeed = 10f;    // Faster than running
    public float rollDuration = 0.8f; // How long the roll lasts (match animation length)
    public float rollCooldown = 1.2f; // prevent spamming

    [Header("Combat Settings")]
    [Range(0f, 1f)] public float attackMovementPenalty = 0.3f; 

    [Header("References")]
    private CharacterController _controller;
    private Animator _animator;
    private PlayerControls _input;
    private Camera _mainCamera;

    // State Variables
    private Vector2 _moveInput;
    private Vector2 _mousePosition;
    
    private bool _isAttacking = false;
    private bool _isRolling = false; // NEW FLAG

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _input = new PlayerControls();
        _mainCamera = Camera.main;

        _input.Player.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
        _input.Player.Move.canceled += ctx => _moveInput = Vector2.zero;
        _input.Player.Look.performed += ctx => _mousePosition = ctx.ReadValue<Vector2>();
        
        _input.Player.Attack.performed += ctx => PerformAttack();
        
        // NEW: Roll Listener
        _input.Player.Roll.performed += ctx => PerformRoll();
    }

    private void OnEnable() => _input.Enable();
    private void OnDisable() => _input.Disable();

    private void Update()
    {
        // 1. PRIORITY CHECK: Rolling overrides EVERYTHING.
        if (_isRolling)
        {
            // While rolling, we don't run normal movement or rotation logic.
            // The Roll Coroutine handles the movement.
            return;
        }

        HandleMovement();
        HandleRotation();
        HandleAnimation();
    }

    private void HandleMovement()
    {
        Vector3 move = new Vector3(_moveInput.x, 0, _moveInput.y);
        float currentSpeed = moveSpeed;

        if (_isAttacking)
        {
            currentSpeed *= attackMovementPenalty;
        }

        _controller.Move(move * currentSpeed * Time.deltaTime);
        _controller.Move(Vector3.down * 5f * Time.deltaTime);
    }

    private void HandleRotation()
    {
        Ray ray = _mainCamera.ScreenPointToRay(_mousePosition);
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);

        if (groundPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 lookDir = hitPoint - transform.position;
            lookDir.y = 0;

            if (lookDir != Vector3.zero)
            {
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation, 
                    Quaternion.LookRotation(lookDir), 
                    rotationSpeed * 100 * Time.deltaTime
                );
            }
        }
    }

    private void HandleAnimation()
    {
        Vector3 localMove = transform.InverseTransformDirection(new Vector3(_moveInput.x, 0, _moveInput.y));
        _animator.SetFloat("InputX", localMove.x, 0.1f, Time.deltaTime);
        _animator.SetFloat("InputY", localMove.z, 0.1f, Time.deltaTime);
    }

    private void PerformAttack()
    {
        if (_isAttacking || _isRolling) return; // Cannot attack while rolling

        _animator.SetTrigger("AttackTrigger");
        StartCoroutine(AttackRoutine());
    }

    // --- NEW ROLL LOGIC ---
    private void PerformRoll()
    {
        if (_isAttacking || _isRolling) return; // Cannot roll if busy

        StartCoroutine(RollRoutine());
    }

private IEnumerator RollRoutine()
    {
        _isRolling = true;
        _animator.SetTrigger("RollTrigger");

        // 1. Calculate direction based on WASD
        Vector3 rollDirection = new Vector3(_moveInput.x, 0, _moveInput.y);

        // 2. Handle Rotation
        if (rollDirection != Vector3.zero)
        {
            // If pressing WASD, face that direction immediately!
            // This snaps the character to face where they are rolling.
            transform.rotation = Quaternion.LookRotation(rollDirection);
            rollDirection.Normalize(); 
        }
        else 
        {
            // If NOT pressing WASD, roll forward (towards mouse)
            // No need to rotate, we are already facing the mouse.
            rollDirection = transform.forward; 
        }

        // 3. The Physical Movement Loop
        float timer = 0;
        while (timer < rollDuration)
        {
            // Move strictly forward relative to the character's new facing direction
            // (Since we just rotated them to face the roll, "forward" IS the roll direction)
            _controller.Move(transform.forward * rollSpeed * Time.deltaTime);
            
            // Gravity
            _controller.Move(Vector3.down * 5f * Time.deltaTime);

            timer += Time.deltaTime;
            yield return null; 
        }

        yield return new WaitForSeconds(0.1f); // Short cooldown
        _isRolling = false;
        
        // Once this finishes, _isRolling becomes false.
        // The Update() loop kicks back in, and the character will 
        // instantly snap back to face the mouse cursor.
    }

    private IEnumerator AttackRoutine()
    {
        _isAttacking = true;
        yield return new WaitForSeconds(attackCooldown); 
        _isAttacking = false;
    }
}