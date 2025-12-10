using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 10f;
    
    [Header("Attack Settings")]
    public float attack1Duration = 0.5f;   // Duration of the first attack clip
    public float comboInputWindow = 0.3f;  // Time the player has to press the second attack
    public float actionCooldown = 1.5f;    

    [Header("Dash Settings")] 
    public float dashSpeed = 10f;    
    public float dashDuration = 0.3f; 
    public float dashCooldown = 1.2f; 

    [Header("Combat Settings")]
    [Range(0f, 1f)] public float attackMovementPenalty = 0.3f; 

    [Header("References")]
    private CharacterController _controller;
    private Animator _animator;
    private PlayerControls _input;
    private Camera _mainCamera; // Camera is no longer used for rotation, but kept for setup

    // State Variables
    private Vector2 _moveInput;
    private Vector2 _mousePosition; // No longer used for rotation
    
    private bool _isAttacking = false;
    private bool _isDashing = false; 
    private bool _canDash = true;    
    
    private bool _comboWindowOpen = false; 
    private bool _comboInputReceived = false; 

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _input = new PlayerControls();
        _mainCamera = Camera.main; // Still useful if you use the camera for movement direction relative to camera

        // Input Listeners
        _input.Player.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
        _input.Player.Move.canceled += ctx => _moveInput = Vector2.zero;
        // _input.Player.Look.performed += ctx => _mousePosition = ctx.ReadValue<Vector2>(); // MOUSE LOOK REMOVED
        
        _input.Player.Attack.performed += ctx => PerformAttack();
        _input.Player.Roll.performed += ctx => PerformDash(); 
    }

    private void OnEnable() => _input.Enable();
    private void OnDisable() => _input.Disable();

    private void Update()
    {
        if (_isDashing)
        {
            return;
        }

        HandleMovement();
        HandleRotation(); // NOW HANDLES MOVEMENT-BASED ROTATION
        HandleAnimation();
    }

    private void HandleMovement()
    {
        // Vector in World Space based on WASD keys
        Vector3 move = new Vector3(_moveInput.x, 0, _moveInput.y);
        float currentSpeed = moveSpeed;

        if (_isAttacking)
        {
            currentSpeed *= attackMovementPenalty;
        }

        // Apply movement and gravity
        _controller.Move(move * currentSpeed * Time.deltaTime);
        _controller.Move(Vector3.down * 5f * Time.deltaTime);
    }

    // --- MODIFIED: ROTATE CHARACTER TO FACE MOVEMENT DIRECTION ---
    private void HandleRotation()
    {
        // Don't rotate if locked in attack or dash
        if (_isAttacking || _isDashing) return; 

        // 1. Get the direction vector from input (ignoring Y-axis for 2D movement)
        Vector3 inputDirection = new Vector3(_moveInput.x, 0f, _moveInput.y).normalized;

        // 2. Only rotate if there is actual input movement
        if (inputDirection.magnitude >= 0.1f)
        {
            // Determine the target rotation based on the input vector
            Quaternion targetRotation = Quaternion.LookRotation(inputDirection);

            // Smoothly rotate the character towards the target direction
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                targetRotation, 
                rotationSpeed * Time.deltaTime // Using rotationSpeed directly for smooth turns
            );
        }
    }
    // -------------------------------------------------------------------

    private void HandleAnimation()
    {
        // ... (Animation logic remains the same for 2D Blend Tree)
        float inputX = _moveInput.x;
        float inputY = _moveInput.y;
        
        _animator.SetFloat("InputX", inputX, 0f, Time.deltaTime);
        _animator.SetFloat("InputY", inputY, 0f, Time.deltaTime);

        float speedMagnitude = new Vector3(inputX, 0, inputY).magnitude; 
        _animator.SetFloat("Speed", Mathf.Clamp01(speedMagnitude), 0f, Time.deltaTime);
    }

    // --- (PerformAttack and AttackRoutine remain the same) ---
    private void PerformAttack()
    {
        if (_isAttacking) 
        {
            if (_comboWindowOpen)
            {
                _comboInputReceived = true; 
            }
            return; 
        }
        
        if (_isDashing) return; 

        _animator.SetTrigger("IsAttacking");
        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        _isAttacking = true;
        _comboInputReceived = false;
        
        yield return new WaitForSeconds(attack1Duration - comboInputWindow); 

        _comboWindowOpen = true;
        yield return new WaitForSeconds(comboInputWindow);
        _comboWindowOpen = false;

        if (_comboInputReceived)
        {
            _animator.SetBool("ComboAttack", true); 
        }
        else
        {
            _animator.SetBool("ComboAttack", false); 
        }

        yield return new WaitForSeconds(1.5f); 
        
        _isAttacking = false;
        _animator.SetBool("ComboAttack", false); 
    }

    // --- (PerformDash and DashRoutine remain the same) ---
    private void PerformDash()
    {
        if (_isAttacking || _isDashing || !_canDash) return; 

        StartCoroutine(DashRoutine());
    }

    private IEnumerator DashRoutine()
    {
        _isDashing = true;
        _canDash = false;
        _animator.SetTrigger("IsDashing"); 

        // This line remains crucial for backward dash relative to current facing direction
        Vector3 dashDirection = -transform.forward; 
        
        float timer = 0;
        while (timer < dashDuration)
        {
            _controller.Move(dashDirection * dashSpeed * Time.deltaTime);
            _controller.Move(Vector3.down * 5f * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null; 
        }

        _isDashing = false; 
        yield return new WaitForSeconds(dashCooldown);
        _canDash = true;
    }
}