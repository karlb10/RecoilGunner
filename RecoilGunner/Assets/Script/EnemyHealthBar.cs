using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("Health Bar Settings")]
    public float healthBarOffset = 1f; // Distance above enemy
    public Vector2 healthBarSize = new Vector2(1f, 0.15f); // Width and height
    public Color healthColor = Color.green;
    public Color lowHealthColor = Color.red;
    public Color backgroundColor = Color.black;
    public float lowHealthThreshold = 0.3f; // When to turn red

    [Header("Visibility")]
    public bool alwaysVisible = false; // If false, only shows when damaged
    public float hideDelay = 2f; // Seconds to hide after taking damage
    public bool hideAtFullHealth = true; // Hide when at full health

    private Canvas worldCanvas;
    private GameObject healthBarObject;
    private Image backgroundImage;
    private Image fillImage;
    private Enemy enemyScript;
    private Camera mainCamera;

    private int currentHealth;
    private int maxHealth;
    private float hideTimer;
    private bool isVisible = false;

    void Start()
    {
        enemyScript = GetComponent<Enemy>();
        mainCamera = Camera.main;

        if (enemyScript == null)
        {
            Debug.LogError("❌ EnemyHealthBar requires Enemy script on the same GameObject!");
            Destroy(this);
            return;
        }

        // Initialize health values
        maxHealth = enemyScript.maxHealth;
        currentHealth = enemyScript.health;

        CreateHealthBar();
    }

    void CreateHealthBar()
    {
        // Create world space canvas
        GameObject canvasObject = new GameObject("EnemyHealthCanvas");
        canvasObject.transform.SetParent(transform);

        worldCanvas = canvasObject.AddComponent<Canvas>();
        worldCanvas.renderMode = RenderMode.WorldSpace;
        worldCanvas.sortingOrder = 10; // Above most game objects

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 100;

        // Position canvas above enemy
        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = healthBarSize;
        canvasRect.localPosition = Vector3.up * healthBarOffset;
        canvasRect.localScale = Vector3.one * 0.01f; // Scale down for world space

        // Create health bar background
        healthBarObject = new GameObject("HealthBar");
        healthBarObject.transform.SetParent(canvasObject.transform, false);

        RectTransform healthBarRect = healthBarObject.AddComponent<RectTransform>();
        healthBarRect.sizeDelta = Vector2.one * 100f; // Fill parent
        healthBarRect.anchorMin = Vector2.zero;
        healthBarRect.anchorMax = Vector2.one;
        healthBarRect.offsetMin = Vector2.zero;
        healthBarRect.offsetMax = Vector2.zero;

        // Background image
        backgroundImage = healthBarObject.AddComponent<Image>();
        backgroundImage.color = backgroundColor;

        // Create fill image (the actual health bar)
        GameObject fillObject = new GameObject("Fill");
        fillObject.transform.SetParent(healthBarObject.transform, false);

        RectTransform fillRect = fillObject.AddComponent<RectTransform>();
        fillRect.sizeDelta = Vector2.one * 100f;
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(1, 1);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        fillImage = fillObject.AddComponent<Image>();
        fillImage.color = healthColor;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;

        // Set initial visibility
        SetHealthBarVisibility(!hideAtFullHealth || !alwaysVisible);
    }

    void Update()
    {
        if (enemyScript == null || healthBarObject == null) return;

        // Update health values
        int newHealth = enemyScript.health;
        if (newHealth != currentHealth)
        {
            currentHealth = newHealth;
            UpdateHealthBar();

            // Show health bar when damaged
            if (!alwaysVisible)
            {
                ShowHealthBar();
                hideTimer = hideDelay;
            }
        }

        // Handle hiding after delay
        if (!alwaysVisible && isVisible && hideTimer > 0)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0)
            {
                if (hideAtFullHealth && currentHealth >= maxHealth)
                {
                    HideHealthBar();
                }
                else if (!hideAtFullHealth)
                {
                    HideHealthBar();
                }
            }
        }

        // Face camera (billboard effect)
        if (worldCanvas != null && mainCamera != null)
        {
            worldCanvas.transform.LookAt(worldCanvas.transform.position + mainCamera.transform.rotation * Vector3.forward,
                                        mainCamera.transform.rotation * Vector3.up);
        }
    }

    void UpdateHealthBar()
    {
        if (fillImage == null) return;

        // Calculate health percentage
        float healthPercentage = (float)currentHealth / maxHealth;
        fillImage.fillAmount = healthPercentage;

        // Change color based on health
        Color targetColor = Color.Lerp(lowHealthColor, healthColor, healthPercentage / lowHealthThreshold);
        fillImage.color = healthPercentage <= lowHealthThreshold ? targetColor : healthColor;
    }

    public void ShowHealthBar()
    {
        SetHealthBarVisibility(true);
    }

    public void HideHealthBar()
    {
        SetHealthBarVisibility(false);
    }

    void SetHealthBarVisibility(bool visible)
    {
        isVisible = visible;
        if (healthBarObject != null)
        {
            healthBarObject.SetActive(visible);
        }
    }

    // Public method for external scripts to trigger health bar display
    public void OnEnemyDamaged()
    {
        if (!alwaysVisible)
        {
            ShowHealthBar();
            hideTimer = hideDelay;
        }
    }

    void OnDestroy()
    {
        // Clean up when enemy is destroyed
        if (worldCanvas != null)
        {
            Destroy(worldCanvas.gameObject);
        }
    }
}