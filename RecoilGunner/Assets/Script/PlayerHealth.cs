using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 100;
    public float invulnerabilityDuration = 1f;

    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer;
    public Color hitColor = Color.red;
    public float flashSpeed = 10f;

    [Header("UI")]
    public UIHealthBar uiHealthBar;

    [Header("Events")]
    public UnityEvent<int> OnHealthChanged;
    public UnityEvent OnPlayerDied;

    private int currentHealth;
    private bool isInvulnerable = false;
    private bool isFlashing = false;
    private Color originalColor;

    void Start()
    {
        currentHealth = maxHealth;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        if (uiHealthBar == null)
            uiHealthBar = FindObjectOfType<UIHealthBar>();

        UpdateHealthBar();
        OnHealthChanged?.Invoke(currentHealth);
    }

    public void TakeDamage(int damage)
    {
        if (isInvulnerable) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Trigger camera shake when player takes damage
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.ShakePlayerDamage();
        }

        OnHealthChanged?.Invoke(currentHealth);
        UpdateHealthBar();

        if (currentHealth > 0)
        {
            StartCoroutine(InvulnerabilityPeriod());
        }
        else
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
        UpdateHealthBar();
    }

    void UpdateHealthBar()
    {
        if (uiHealthBar != null)
            uiHealthBar.UpdateHealth(currentHealth, maxHealth);
    }

    System.Collections.IEnumerator InvulnerabilityPeriod()
    {
        isInvulnerable = true;

        if (spriteRenderer != null && !isFlashing)
            StartCoroutine(FlashEffect());

        yield return new WaitForSeconds(invulnerabilityDuration);

        isInvulnerable = false;
        isFlashing = false;

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    System.Collections.IEnumerator FlashEffect()
    {
        isFlashing = true;
        float flashTimer = 0f;

        while (isInvulnerable && isFlashing)
        {
            flashTimer += Time.unscaledDeltaTime * flashSpeed; // Use unscaled time
            spriteRenderer.color = Color.Lerp(originalColor, hitColor, (Mathf.Sin(flashTimer) + 1f) / 2f);
            yield return null;
        }

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    void Die()
    {
        // Trigger big camera shake for death
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.ShakeGameOver();
        }

        OnPlayerDied?.Invoke();
        Debug.Log("💀 Player Died!");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && !isInvulnerable)
        {
            TakeDamage(10);
        }
    }
}