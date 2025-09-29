using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 5; // Changed to 5 to match your 6 sprites (0-5)
    public float invulnerabilityDuration = 1f;

    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer;
    public Color hitColor = Color.red;
    public float flashSpeed = 10f;

    [Header("UI - Choose One")]
    public SpriteHealthBar spriteHealthBar; // New sprite-based health bar
    public UIHealthBar uiHealthBar; // Old fill-based health bar (optional)

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

        // Try to find health bar if not assigned
        if (spriteHealthBar == null)
            spriteHealthBar = FindObjectOfType<SpriteHealthBar>();

        if (uiHealthBar == null)
            uiHealthBar = FindObjectOfType<UIHealthBar>();

        UpdateHealthBar();
        OnHealthChanged?.Invoke(currentHealth);

        Debug.Log($"✅ PlayerHealth initialized: {currentHealth}/{maxHealth}");
    }

    public void TakeDamage(int damage)
    {
        if (isInvulnerable) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Camera shake when player takes damage
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.ShakePlayerDamage();
        }

        OnHealthChanged?.Invoke(currentHealth);
        UpdateHealthBar();

        Debug.Log($"💔 Player took {damage} damage! Health: {currentHealth}/{maxHealth}");

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

        Debug.Log($"💚 Player healed {amount}! Health: {currentHealth}/{maxHealth}");
    }

    void UpdateHealthBar()
    {
        // Update sprite-based health bar (new system)
        if (spriteHealthBar != null)
        {
            spriteHealthBar.UpdateHealth(currentHealth, maxHealth);
        }

        // Update old fill-based health bar (backward compatibility)
        if (uiHealthBar != null)
        {
            uiHealthBar.UpdateHealth(currentHealth, maxHealth);
        }
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
            flashTimer += Time.unscaledDeltaTime * flashSpeed;
            spriteRenderer.color = Color.Lerp(originalColor, hitColor, (Mathf.Sin(flashTimer) + 1f) / 2f);
            yield return null;
        }

        if (spriteRenderer != null)
            spriteRenderer.color = originalColor;
    }

    void Die()
    {
        // Big camera shake for death
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
            TakeDamage(1); // Changed to 1 damage since max health is now 5
        }
    }

    // Debug methods for testing
    [ContextMenu("Take 1 Damage")]
    void TestDamage()
    {
        TakeDamage(1);
    }

    [ContextMenu("Heal 1")]
    void TestHeal()
    {
        Heal(1);
    }

    [ContextMenu("Set Full Health")]
    void TestFullHealth()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }
}