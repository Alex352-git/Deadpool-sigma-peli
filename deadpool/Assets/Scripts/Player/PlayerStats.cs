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

    [Header("Invulnerability")]
    public float iFramesDuration = 1.0f;
    private float iFrameTimer;

    [Header("UI")]
    public Image healthFillImage;
    public TextMeshProUGUI healthText;
    public GameObject deathScreenUI;

    [Header("Fading Settings")]
    public Image fadeImage; // Drag a black UI Image here
    public float fadeSpeed = 1.0f;

    void Start()
    {
        currentHealth = maxHealth;

        // Ensure the fade image is invisible at the start
        if (fadeImage != null)
        {
            fadeImage.gameObject.SetActive(true); // Keep it active but clear
            Color c = fadeImage.color;
            c.a = 0;
            fadeImage.color = c;
        }
    }

    void Update()
    {
        if (iFrameTimer > 0) iFrameTimer -= Time.deltaTime;
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

    public void Heal(float damage)
    {
        currentHealth += damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateUI();
    }

    public void Die()
    {
        // Prevent multiple death triggers
        this.enabled = false;
        StartCoroutine(DeathRoutine());
    }

    private IEnumerator DeathRoutine()
    {
        // 1. Start the Fade to Black
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

        // 2. Wait a moment in the darkness
        yield return new WaitForSeconds(1.0f);

        // 3. Show the buttons (Restart/Quit)
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