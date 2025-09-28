using UnityEngine;
using UnityEngine.Events;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public int maxHealth = 5;
    public float invulnerabilityDuration = 1f;

    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer;
    public Color hitColor = Color.red;
    public float flashSpeed = 10f;

    [Header("UI")]
    public HealthBar healthBar;

    [Header("Events")]
    public UnityEvent<int> OnHealthChanged;
    public UnityEvent OnPlayerDied;

    private int currentHealth;
    private bool isInvulnerable = false;
    private bool isFlashing = false;
    private Color originalColor;

    public int CurrentHealth => currentHealth;
    public bool IsInvulnerable => isInvulnerable;

    void Start()
    {
        currentHealth = maxHealth;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        // Create health bar if not assigned
        SetupHealthBar();

        // Notify UI of initial health
        OnHealthChanged?.Invoke(currentHealth);
        UpdateHealthBar();
    }

    public void TakeDamage(int damage)
    {
        if (isInvulnerable) return;

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Notify systems of health change
        OnHealthChanged?.Invoke(currentHealth);
        UpdateHealthBar();

        if (currentHealth > 0)
        {
            // Start invulnerability period
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

    public void SetHealth(int newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth);
        UpdateHealthBar();
    }

    void SetupHealthBar()
    {
        if (healthBar != null) return; // Already have one

        // Destroy any existing health bar objects first
        HealthBar[] existingHealthBars = GetComponentsInChildren<HealthBar>();
        for (int i = 0; i < existingHealthBars.Length; i++)
        {
            if (existingHealthBars[i] != null)
            {
                DestroyImmediate(existingHealthBars[i].gameObject);
            }
        }

        // Create new health bar
        GameObject healthBarObj = new GameObject("PlayerHealthBar");
        healthBarObj.transform.SetParent(transform);
        healthBar = healthBarObj.AddComponent<HealthBar>();
        healthBar.isPlayerHealthBar = true;
        healthBar.offsetY = 1.2f;
    }

    void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            healthBar.UpdateHealthBar(currentHealth, maxHealth);
        }
    }

    System.Collections.IEnumerator InvulnerabilityPeriod()
    {
        isInvulnerable = true;

        // Start flashing effect
        if (spriteRenderer != null && !isFlashing)
        {
            StartCoroutine(FlashEffect());
        }

        yield return new WaitForSeconds(invulnerabilityDuration);

        isInvulnerable = false;
        isFlashing = false;

        // Restore original color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    System.Collections.IEnumerator FlashEffect()
    {
        if (spriteRenderer == null) yield break;

        isFlashing = true;
        float flashTimer = 0f;

        while (isInvulnerable && isFlashing)
        {
            flashTimer += Time.deltaTime * flashSpeed;

            // Alternate between original color and hit color
            Color currentColor = Color.Lerp(originalColor, hitColor, (Mathf.Sin(flashTimer) + 1f) / 2f);
            spriteRenderer.color = currentColor;

            yield return null;
        }

        // Ensure we end with original color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    void Die()
    {
        // Notify game manager or other systems
        OnPlayerDied?.Invoke();

        // You can add death effects here
        Debug.Log("Player died!");

        // Disable player controls
        RecoilPlayerController playerController = GetComponent<RecoilPlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }

        // Or restart the game, show game over screen, etc.
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.GameOver();
        }
    }

    // Optional: Collision-based damage
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && !isInvulnerable)
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                TakeDamage(1); // Take 1 damage from touching enemies
            }
        }
    }
}