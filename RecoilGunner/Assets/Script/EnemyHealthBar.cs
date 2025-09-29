using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    [Header("Health Bar Settings")]
    public float healthBarOffset = 1f;
    public Vector2 healthBarSize = new Vector2(1f, 0.15f);
    public Color healthColor = Color.green;
    public Color lowHealthColor = Color.red;
    public Color backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
    public float lowHealthThreshold = 0.3f;

    [Header("Visibility")]
    public bool alwaysVisible = true;
    public float hideDelay = 2f;
    public bool hideAtFullHealth = false;

    private Canvas worldCanvas;
    private GameObject healthBarBG;
    private GameObject healthBarFill;
    private Image bgImage;
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
            Debug.LogError("❌ EnemyHealthBar requires Enemy script!");
            Destroy(this);
            return;
        }

        maxHealth = enemyScript.maxHealth;
        currentHealth = enemyScript.health;

        CreateHealthBar();
        UpdateHealthBar();
    }

    void CreateHealthBar()
    {
        // Create canvas
        GameObject canvasObj = new GameObject("EnemyHealthCanvas");
        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = Vector3.up * healthBarOffset;

        worldCanvas = canvasObj.AddComponent<Canvas>();
        worldCanvas.renderMode = RenderMode.WorldSpace;
        worldCanvas.sortingLayerName = "Default"; // Change this if you have custom sorting layers
        worldCanvas.sortingOrder = 3;

        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = healthBarSize * 100f;
        canvasRect.localScale = Vector3.one * 0.01f;

        // Create background
        healthBarBG = new GameObject("Background");
        healthBarBG.transform.SetParent(canvasObj.transform, false);

        RectTransform bgRect = healthBarBG.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition = Vector2.zero;

        bgImage = healthBarBG.AddComponent<Image>();
        bgImage.color = backgroundColor;

        // Create fill (the actual health bar)
        healthBarFill = new GameObject("Fill");
        healthBarFill.transform.SetParent(healthBarBG.transform, false);

        RectTransform fillRect = healthBarFill.AddComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        fillRect.anchoredPosition = Vector2.zero;

        fillImage = healthBarFill.AddComponent<Image>();
        fillImage.color = healthColor;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImage.fillAmount = 1f;

        // Set initial visibility
        SetHealthBarVisibility(alwaysVisible || !hideAtFullHealth);

        Debug.Log($"✅ Enemy health bar created for {gameObject.name}");
    }

    void Update()
    {
        if (enemyScript == null || worldCanvas == null) return;

        // Update health
        int newHealth = enemyScript.health;
        if (newHealth != currentHealth)
        {
            currentHealth = newHealth;
            UpdateHealthBar();

            if (!alwaysVisible)
            {
                ShowHealthBar();
                hideTimer = hideDelay;
            }
        }

        // Hide after delay
        if (!alwaysVisible && isVisible && hideTimer > 0)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0)
            {
                if (hideAtFullHealth && currentHealth >= maxHealth)
                {
                    HideHealthBar();
                }
            }
        }

        // Billboard effect (face camera)
        if (mainCamera != null)
        {
            worldCanvas.transform.rotation = Quaternion.LookRotation(
                worldCanvas.transform.position - mainCamera.transform.position
            );
        }
    }

    void UpdateHealthBar()
    {
        if (fillImage == null || maxHealth <= 0) return;

        float healthPercent = (float)currentHealth / maxHealth;
        fillImage.fillAmount = healthPercent;

        // Change color based on health
        if (healthPercent <= lowHealthThreshold)
        {
            fillImage.color = Color.Lerp(lowHealthColor, healthColor, healthPercent / lowHealthThreshold);
        }
        else
        {
            fillImage.color = healthColor;
        }
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
        if (worldCanvas != null)
        {
            worldCanvas.gameObject.SetActive(visible);
        }
    }

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
        if (worldCanvas != null)
        {
            Destroy(worldCanvas.gameObject);
        }
    }
}