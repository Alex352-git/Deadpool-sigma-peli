using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem; // Uses Unity's New Input System

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider), typeof(AudioSource))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 7f;
    public float rotationSpeed = 15f;
    public float jumpForce = 5f;
    public Transform cameraTransform;

    [Header("Combat Settings")]
    public Transform attackPoint;
    public float attackRange = 1.8f;
    public int attackDamage = 25;
    public LayerMask enemyLayers;
    public float attackCooldown = 0.5f;

    [Header("Audio")]
    public AudioClip attackSound;
    public AudioClip jumpSound;

    private Rigidbody rb;
    private Animator anim;
    private PlayerStats stats;
    private AudioSource audioSource;

    private bool isAttacking = false;
    private bool isGrounded = true;
    private float nextAttackTime = 0f;
    private string currentAnimState;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();

        // Grab PlayerStats from either this object or parent
        stats = GetComponent<PlayerStats>();
        if (stats == null) stats = GetComponentInParent<PlayerStats>();

        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        // Lock cursor to screen center
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (anim != null) anim.applyRootMotion = false;
    }

    void Update()
    {
        if (stats != null && stats.currentHealth <= 0) return;

        CheckGrounded();

        // Safety check for active input devices
        if (Keyboard.current == null || Mouse.current == null) return;

        // Left Click -> Attack
        if (Time.time >= nextAttackTime && Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartCoroutine(AttackRoutine());
        }

        // Spacebar -> Jump
        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded && !isAttacking)
        {
            Jump();
        }
    }

    void FixedUpdate()
    {
        if (stats != null && stats.currentHealth <= 0) return;

        if (isAttacking)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            return;
        }

        MoveAndRotate();
    }

    void MoveAndRotate()
    {
        if (Keyboard.current == null) return;

        // Read WASD input directly via New Input System
        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current.wKey.isPressed) vertical += 1f;
        if (Keyboard.current.sKey.isPressed) vertical -= 1f;
        if (Keyboard.current.aKey.isPressed) horizontal -= 1f;
        if (Keyboard.current.dKey.isPressed) horizontal += 1f;

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f && cameraTransform != null)
        {
            // Calculate movement angle relative to camera view
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

            rb.linearVelocity = new Vector3(moveDir.x * moveSpeed, rb.linearVelocity.y, moveDir.z * moveSpeed);

            if (isGrounded) ChangeAnimationState("run");
        }
        else
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            if (isGrounded) ChangeAnimationState("idle");
        }
    }

    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;

        ChangeAnimationState("attack", 0.1f);
        if (audioSource != null && attackSound != null) audioSource.PlayOneShot(attackSound);

        yield return new WaitForSeconds(0.2f);

        Vector3 hitCenter = attackPoint != null ? attackPoint.position : transform.position + transform.forward;
        Collider[] hitEnemies = Physics.OverlapSphere(hitCenter, attackRange, enemyLayers);

        foreach (Collider enemyCol in hitEnemies)
        {
            IDamageable damageable = enemyCol.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(attackDamage);
            }
        }

        yield return new WaitForSeconds(0.3f);
        isAttacking = false;
    }

    void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        ChangeAnimationState("jump", 0.1f);
        if (audioSource != null && jumpSound != null) audioSource.PlayOneShot(jumpSound);
    }

    void CheckGrounded()
    {
        isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, 0.4f);
    }

    void ChangeAnimationState(string newState, float blendTime = 0.2f)
    {
        if (currentAnimState == newState || anim == null) return;
        anim.CrossFadeInFixedTime(newState, blendTime);
        currentAnimState = newState;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}