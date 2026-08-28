using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public interface IDamageable
{
    void TakeDamage(int damage);
}

public class PlayerStats : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Deadpool Healing Factor")]
    public bool enableHealingFactor = true;
    public float healthRegenPerSecond = 5f; // How much HP he gets back every second
    private float regenTimer = 0f;

    [Header("Invulnerability")]
    public float iFramesDuration = 1.0f;
    private float iFrameTimer;

    [Header("UI")]
    public Image healthFillImage;
    public TextMeshProUGUI healthText;
    public GameObject deathScreenUI;

    [Header("Fading Settings")]
    public Image fadeImage;
    public float fadeSpeed = 1.0f;

    void Start()
    {
        currentHealth = maxHealth;

        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true);
            Color c = fadeImage.color;
            c.a = 0;
            fadeImage.color = c;
        }
    }

    void Update()
    {
        if (iFrameTimer > 0) iFrameTimer -= Time.deltaTime;

        // --- HEALING FACTOR LOGIC ---
        if (enableHealingFactor && currentHealth > 0 && currentHealth < maxHealth)
        {
            regenTimer += Time.deltaTime;
            if (regenTimer >= 1f) // Tick every 1 second
            {
                Heal(healthRegenPerSecond);
                regenTimer = 0f; // Reset timer
            }
        }

        UpdateUI();
    }

    public void TakeDamage(int damage)
    {
        TakeDamage((float)damage);
    }

    public void TakeDamage(float damage)
    {
        if (iFrameTimer > 0) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();
        Debug.Log("Player HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            iFrameTimer = iFramesDuration;
        }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();
    }

    public void Die()
    {
        this.enabled = false;
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        if (fadeImage != null)
        {
            float alpha = 0;
            while (alpha < 1)
            {
                alpha += Time.deltaTime * fadeSpeed;
                Color c = fadeImage.color;
                c.a = alpha;
                fadeImage.color = c;
                yield return null;
            }
        }

        yield return new WaitForSeconds(1.0f);

        if (deathScreenUI != null) deathScreenUI.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void UpdateUI()
    {
        float healthPercent = currentHealth / maxHealth;

        if (healthFillImage != null)
        {
            healthFillImage.fillAmount = healthPercent;
        }

        if (healthText != null)
        {
            healthText.text = $"{currentHealth:0}";
        }
    }
}