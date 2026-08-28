using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider), typeof(AudioSource))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 7f;
    public float rotationSpeed = 12f;
    public float jumpForce = 5f;
    public Transform cameraTransform;

    [Header("Combat Settings")]
    public Transform attackPoint;
    public float attackRange = 1.8f;
    public int attackDamage = 25;
    public LayerMask enemyLayers;

    [Header("Individual Combo Timings")]
    public float attack1Duration = 0.5f;
    public float attack2Duration = 0.4f;
    public float attack3Duration = 0.6f;
    public float maxComboDelay = 1.2f;

    [Header("Audio")]
    public AudioClip attackSound;
    public AudioClip jumpSound;

    private Rigidbody rb;
    private Animator anim;
    private PlayerStats stats;
    private AudioSource audioSource;

    private bool isAttacking = false;
    private bool isGrounded = true;
    private string currentAnimState;

    private int clickCount = 0;
    private float lastClickTime = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();
        stats = GetComponent<PlayerStats>() ?? GetComponentInParent<PlayerStats>();
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        // Forces the script to use the actual lens, fixing backward running
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (stats != null && stats.currentHealth <= 0) return;

        CheckGrounded();
        if (Keyboard.current == null || Mouse.current == null) return;

        if (Time.time - lastClickTime > maxComboDelay)
        {
            clickCount = 0;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            lastClickTime = Time.time;
            clickCount++;

            if (clickCount > 3) clickCount = 1;

            if (!isAttacking)
            {
                StartCoroutine(ComboAttackRoutine());
            }
        }

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
        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current.wKey.isPressed) vertical += 1f;
        if (Keyboard.current.sKey.isPressed) vertical -= 1f;
        if (Keyboard.current.aKey.isPressed) horizontal -= 1f;
        if (Keyboard.current.dKey.isPressed) horizontal += 1f;

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f && cameraTransform != null)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);

            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

            Vector3 moveDir = targetRotation * Vector3.forward;
            rb.linearVelocity = new Vector3(moveDir.x * moveSpeed, rb.linearVelocity.y, moveDir.z * moveSpeed);

            if (isGrounded) ChangeAnimationState("Run", 0.1f);
        }
        else
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
            if (isGrounded) ChangeAnimationState("Idle", 0.1f);
        }
    }

    IEnumerator ComboAttackRoutine()
    {
        isAttacking = true;

        while (clickCount > 0)
        {
            int currentComboStep = clickCount;
            float stepDuration = attack1Duration;

            // Assign dynamic duration based on which swing is playing
            if (currentComboStep == 1)
            {
                ChangeAnimationState("Attack1", 0.05f);
                stepDuration = attack1Duration;
            }
            else if (currentComboStep == 2)
            {
                ChangeAnimationState("Attack2", 0.05f);
                stepDuration = attack2Duration;
            }
            else if (currentComboStep >= 3)
            {
                ChangeAnimationState("Attack3", 0.05f);
                stepDuration = attack3Duration;
                clickCount = 0;
            }

            if (audioSource != null && attackSound != null) audioSource.PlayOneShot(attackSound);

            yield return new WaitForSeconds(stepDuration * 0.4f);

            Vector3 hitCenter = attackPoint != null ? attackPoint.position : transform.position + transform.forward;
            Collider[] hitEnemies = Physics.OverlapSphere(hitCenter, attackRange, enemyLayers);
            foreach (Collider enemyCol in hitEnemies)
            {
                IDamageable damageable = enemyCol.GetComponentInParent<IDamageable>();
                if (damageable != null) damageable.TakeDamage(attackDamage);
            }

            yield return new WaitForSeconds(stepDuration * 0.6f);

            if (clickCount == currentComboStep)
            {
                clickCount = 0;
                break;
            }
        }

        isAttacking = false;
        ChangeAnimationState("Idle", 0.15f);
    }

    void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        ChangeAnimationState("Jump", 0.1f);
        if (audioSource != null && jumpSound != null) audioSource.PlayOneShot(jumpSound);
    }

    void CheckGrounded()
    {
        isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, 0.4f);
    }

    void ChangeAnimationState(string newState, float transitionTime)
    {
        // Removed the strict state lock so rapid combo chaining doesn't get blocked
        if (anim == null) return;
        anim.CrossFadeInFixedTime(newState, transitionTime);
        currentAnimState = newState;
    }

    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}