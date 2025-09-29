using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpriteHealthBar : MonoBehaviour
{
    [Header("Health Bar Sprites")]
    [Tooltip("Assign 6 sprites: 0 health, 1 health, 2 health, 3 health, 4 health, 5 health (full)")]
    public Sprite[] healthBarSprites = new Sprite[6];

    [Header("UI References")]
    public Image healthBarImage;
    public TMP_Text healthText;

    [Header("Optional Settings")]
    public bool showHealthNumbers = true;
    public bool hideAtFullHealth = false;

    private int currentHealth;
    private int maxHealth;

    void Start()
    {
        if (healthBarImage == null)
        {
            healthBarImage = GetComponent<Image>();
        }

        if (healthBarImage == null)
        {
            Debug.LogError("❌ No Image component found for SpriteHealthBar!");
            return;
        }

        // Validate sprites
        if (healthBarSprites == null || healthBarSprites.Length == 0)
        {
            Debug.LogError("❌ No health bar sprites assigned! Please assign 6 sprites (0 to 5 health)");
            return;
        }

        Debug.Log($"✅ SpriteHealthBar initialized with {healthBarSprites.Length} sprites");
    }

    public void UpdateHealth(int health, int max)
    {
        currentHealth = Mathf.Clamp(health, 0, max);
        maxHealth = max;

        // Calculate which sprite to show based on health percentage
        int spriteIndex = CalculateSpriteIndex(currentHealth, maxHealth);

        // Update the sprite
        if (spriteIndex >= 0 && spriteIndex < healthBarSprites.Length)
        {
            if (healthBarSprites[spriteIndex] != null)
            {
                healthBarImage.sprite = healthBarSprites[spriteIndex];
            }
            else
            {
                Debug.LogWarning($"⚠️ Health bar sprite at index {spriteIndex} is null!");
            }
        }

        // Update health text if enabled
        if (showHealthNumbers && healthText != null)
        {
            healthText.text = $"{currentHealth}/{maxHealth}";
        }

        // Hide/show based on settings
        if (hideAtFullHealth)
        {
            healthBarImage.gameObject.SetActive(currentHealth < maxHealth);
        }

        Debug.Log($"💚 Health updated: {currentHealth}/{maxHealth} - Using sprite index: {spriteIndex}");
    }

    int CalculateSpriteIndex(int health, int max)
    {
        if (max <= 0) return 0;

        // Calculate health percentage
        float healthPercent = (float)health / max;

        // Map to sprite index (0 to healthBarSprites.Length - 1)
        int spriteCount = healthBarSprites.Length;
        int index = Mathf.RoundToInt(healthPercent * (spriteCount - 1));

        // Clamp to valid range
        return Mathf.Clamp(index, 0, spriteCount - 1);
    }

    // Public method to set health directly (useful for testing)
    public void SetHealth(int health)
    {
        UpdateHealth(health, maxHealth);
    }

    // Debug method to test different health levels
    [ContextMenu("Test Health Levels")]
    void TestHealthLevels()
    {
        StartCoroutine(TestHealthSequence());
    }

    System.Collections.IEnumerator TestHealthSequence()
    {
        int testMaxHealth = 5;

        for (int i = testMaxHealth; i >= 0; i--)
        {
            UpdateHealth(i, testMaxHealth);
            yield return new WaitForSeconds(0.5f);
        }

        // Reset to full
        UpdateHealth(testMaxHealth, testMaxHealth);
    }
}