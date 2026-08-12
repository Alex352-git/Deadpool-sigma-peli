using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class LizardMonsterAI : MonoBehaviour, IDamageable
{
    [Header("Audio & Volume")]
    public AudioSource audioSource;
    public AudioClip attack1_Sound;
    public AudioClip attack2_Sound1;
    public AudioClip attack2_Sound2;
    [Range(-0.5f, 0.5f)] public float attack2_SecondHitNudge = 0f;
    [Range(0f, 1f)] public float attackVolume = 1f; // Adjust attack volume here
    public AudioClip[] deathSounds;
    [Range(0f, 1f)] public float deathVolume = 1f; // Adjust death volume here

    [Header("Dimension Snapping")]
    public float zSnapRange = 25f; // How close they need to be to jump into your 2D lane

    [Header("Health & Stats")]
    public int maxHealth = 100;
    private int currentHealth;
    public bool isDead = false;

    [Header("Targeting")]
    public Transform player;
    public float detectionRadius = 15f;
    public float meleeRange = 3.0f;

    [Header("Combat")]
    public int attackDamage = 15;
    public float moveSpeed = 6f;

    private Rigidbody rb;
    private Animator anim;

    private bool isAttacking = false;
    public bool isStunned = false;
    public bool isParryable = false;

    private string currentAnim;
    private float cooldownTimer = 0f;

    private Renderer[] meshRenderers;
    private Color[] originalColors;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
        anim = GetComponentInChildren<Animator>();

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
        anim.applyRootMotion = false;

        meshRenderers = GetComponentsInChildren<Renderer>();
        originalColors = new Color[meshRenderers.Length];
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if (meshRenderers[i] != null && meshRenderers[i].material.HasProperty("_Color"))
                originalColors[i] = meshRenderers[i].material.color;
        }

        // --- FORCE 3D AUDIO ---
        if (audioSource != null)
        {
            audioSource.spatialBlend = 1f; // 100% 3D Audio
            audioSource.maxDistance = 25f; // Sound cuts off if you are 25 units away
            audioSource.rolloffMode = AudioRolloffMode.Linear;
        }

        // Subscribe to the Dimension Manager radio station
      
    }

 

    private void HandleDimensionSwitch(bool is2D)
    {
        if (isDead || player == null) return;

        // If we switched to 2D and the player is close enough, snap to their Z line!
        if (is2D && Vector3.Distance(transform.position, player.position) <= zSnapRange)
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, player.position.z);
        }
    }

    void FixedUpdate()
    {
        if (isDead || isStunned || player == null) return;

        if (cooldownTimer > 0) cooldownTimer -= Time.fixedDeltaTime;
        if (isAttacking) return;

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist <= detectionRadius)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            dir.y = 0;

            if (dir != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 12f * Time.deltaTime);

            if (dist <= meleeRange)
            {
                rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);

                if (cooldownTimer <= 0f)
                {
                    if (Random.value > 0.5f) StartCoroutine(Attack1Routine());
                    else StartCoroutine(Attack2Routine());
                }
                else
                {
                    ChangeAnimationState("battleidle");
                }
            }
            else
            {
                ChangeAnimationState("run");
                rb.linearVelocity = new Vector3(dir.x * moveSpeed, rb.linearVelocity.y, dir.z * moveSpeed);
            }
        }
        else
        {
            ChangeAnimationState("idle");
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    IEnumerator Attack1Routine()
    {
        isAttacking = true;
        rb.linearVelocity = Vector3.zero;
        ChangeAnimationState("attack1", 0.1f);

        isParryable = true;
        yield return new WaitForSeconds(0.5f); // WINDUP

        // --- PLAY ATTACK 1 SOUND ---
        if (audioSource != null && attack1_Sound != null) audioSource.PlayOneShot(attack1_Sound, attackVolume);

        isParryable = false;

        float activeDuration = 0.3f;
        float elapsed = 0f;
        bool hasHit = false;

        while (elapsed < activeDuration)
        {
            if (isStunned || isDead) yield break;

            if (!hasHit && Vector3.Distance(transform.position, player.position) <= meleeRange + 0.5f)
            {
                DealDamageToPlayer();
                hasHit = true;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(1.33f);
        cooldownTimer = 0.5f;
        isAttacking = false;
    }

    IEnumerator Attack2Routine()
    {
        isAttacking = true;
        rb.linearVelocity = Vector3.zero;
        ChangeAnimationState("attack2", 0.1f);

        isParryable = true;
        yield return new WaitForSeconds(0.5f); // WINDUP 1

        // --- PLAY ATTACK 2 SOUND 1 ---
        if (audioSource != null && attack2_Sound1 != null) audioSource.PlayOneShot(attack2_Sound1, attackVolume);

        isParryable = false;

        if (isStunned || isDead) yield break;

        float hit1Duration = 0.23f;
        float elapsed1 = 0f;
        bool hasHit1 = false;
        while (elapsed1 < hit1Duration)
        {
            if (isStunned || isDead) yield break;
            if (!hasHit1 && Vector3.Distance(transform.position, player.position) <= meleeRange + 0.5f)
            {
                DealDamageToPlayer();
                hasHit1 = true;
            }
            elapsed1 += Time.deltaTime;
            yield return null;
        }

        isParryable = true;
        yield return new WaitForSeconds(0.46f + attack2_SecondHitNudge); // WINDUP 2 + NUDGE

        // --- PLAY ATTACK 2 SOUND 2 ---
        if (audioSource != null && attack2_Sound2 != null) audioSource.PlayOneShot(attack2_Sound2, attackVolume);

        isParryable = false;

        if (isStunned || isDead) yield break;

        float hit2Duration = 0.36f;
        float elapsed2 = 0f;
        bool hasHit2 = false;
        while (elapsed2 < hit2Duration)
        {
            if (isStunned || isDead) yield break;
            if (!hasHit2 && Vector3.Distance(transform.position, player.position) <= meleeRange + 0.5f)
            {
                DealDamageToPlayer();
                hasHit2 = true;
            }
            elapsed2 += Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(2.3f);
        cooldownTimer = 0.5f;
        isAttacking = false;
    }

    void DealDamageToPlayer()
    {
        PlayerStats pStats = player.transform.root.GetComponentInChildren<PlayerStats>();

        if (pStats != null)
        {
            pStats.TakeDamage(attackDamage);
        }
    }

    void ChangeAnimationState(string newState, float blendTime = 0.2f)
    {
        if (currentAnim == newState || anim == null) return;
        anim.CrossFadeInFixedTime(newState, blendTime);
        currentAnim = newState;
    }

    public void OnParried()
    {
        if (isStunned || isDead) return;
        StopAllCoroutines();
        StartCoroutine(StunRoutine());
    }

    IEnumerator StunRoutine()
    {
        isStunned = true;
        isAttacking = false;
        isParryable = false;
        rb.linearVelocity = Vector3.zero;
        ChangeAnimationState("hit", 0.1f);
        yield return new WaitForSeconds(2.0f);
        cooldownTimer = 1.0f;
        isStunned = false;
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        StartCoroutine(FlashRedRoutine());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    IEnumerator FlashRedRoutine()
    {
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if (meshRenderers[i] != null && meshRenderers[i].material.HasProperty("_Color"))
                meshRenderers[i].material.color = Color.red;
        }

        yield return new WaitForSeconds(0.15f);

        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if (meshRenderers[i] != null && meshRenderers[i].material.HasProperty("_Color"))
                meshRenderers[i].material.color = originalColors[i];
        }
    }

    void Die()
    {
        if (isDead && currentHealth > 0) return;
        isDead = true;

        StopAllCoroutines();

        for (int i = 0; i < meshRenderers.Length; i++)
        {
            if (meshRenderers[i] != null && meshRenderers[i].material.HasProperty("_Color"))
                meshRenderers[i].material.color = originalColors[i];
        }

        rb.linearVelocity = Vector3.zero;
        GetComponent<Collider>().enabled = false;

        if (audioSource != null && deathSounds.Length > 0)
        {
            AudioClip randomClip = deathSounds[Random.Range(0, deathSounds.Length)];
            audioSource.PlayOneShot(randomClip, deathVolume);
        }

        ChangeAnimationState("die", 0.1f);

        Destroy(gameObject, 4f);
    }
}