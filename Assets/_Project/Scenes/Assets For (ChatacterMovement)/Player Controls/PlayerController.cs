using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Setup")]
    public Transform cameraTransform; // Drag your Main Camera here

    [Header("Settings")]
    public float moveSpeed = 6f;
    public float rotationSpeed = 15f;

    // Internal Variables
    private CharacterController _controller;
    private Animator _animator;
    private PlayerControls _input; // Relies on the C# class generated in Phase 1
    private Vector2 _rawInput;
    private Vector3 _targetDirection;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        
        // Initialize the Input System class
        _input = new PlayerControls();

        // Bind the "Move" action
        _input.Player.Move.performed += ctx => _rawInput = ctx.ReadValue<Vector2>();
        _input.Player.Move.canceled += ctx => _rawInput = Vector2.zero;
        
        // Bind the "Attack" action
        _input.Player.Attack.performed += ctx => PerformAttack();

        // Auto-find camera if you forgot to assign it
        if (cameraTransform == null) 
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void OnEnable() => _input.Enable();
    private void OnDisable() => _input.Disable();

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        // 1. If we are attacking, maybe we shouldn't move? 
        // (Optional: remove this check if you want to slide while attacking)
        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Attack")) return;

        // 2. If no input, stop moving
        if (_rawInput.sqrMagnitude < 0.1f)
        {
            _animator.SetFloat("Speed", 0f, 0.1f, Time.deltaTime);
            return;
        }

        // 3. Calculate Direction Relative to Camera
        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;

        // Flatten Y so we don't walk into the ground
        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        _targetDirection = (camForward * _rawInput.y + camRight * _rawInput.x).normalized;

        // 4. Rotate Character
        if (_targetDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_targetDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // 5. Move Character
        _controller.Move(_targetDirection * moveSpeed * Time.deltaTime);

        // 6. Update Animation
        _animator.SetFloat("Speed", 1f, 0.1f, Time.deltaTime);

        // Gravity
        _controller.Move(Physics.gravity * Time.deltaTime);
    }

    private void PerformAttack()
    {
        _animator.SetTrigger("IsAttacking");
    }
}