using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIHealthBar : MonoBehaviour
{
    [Header("UI References")]
    public Image fillImage; // Use Image instead of RectTransform
    public TMP_Text healthText;

    [Header("Colors")]
    public Color fullHealthColor = Color.green;
    public Color lowHealthColor = Color.red;
    public float lowHealthThreshold = 0.3f;

    [Header("Animation")]
    public float smoothSpeed = 5f;

    private float targetFillAmount;
    private float currentFillAmount;

    void Start()
    {
        if (fillImage == null)
        {
            Debug.LogError("❌ Fill Image is not assigned in UIHealthBar! Please assign it in the inspector.");
            return;
        }

        // Setup fill image
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImage.fillAmount = 1f;

        currentFillAmount = 1f;
        targetFillAmount = 1f;

        Debug.Log("✅ UIHealthBar initialized successfully!");
    }

    void Update()
    {
        if (fillImage == null) return;

        // Smoothly animate fill amount
        currentFillAmount = Mathf.Lerp(currentFillAmount, targetFillAmount, Time.unscaledDeltaTime * smoothSpeed);
        fillImage.fillAmount = currentFillAmount;

        // Update color based on health percentage
        float healthPercent = currentFillAmount;
        if (healthPercent <= lowHealthThreshold)
        {
            fillImage.color = Color.Lerp(lowHealthColor, fullHealthColor, healthPercent / lowHealthThreshold);
        }
        else
        {
            fillImage.color = fullHealthColor;
        }
    }

    public void UpdateHealth(int health, int max)
    {
        if (max <= 0)
        {
            Debug.LogWarning("⚠️ Max health is 0 or negative!");
            return;
        }

        targetFillAmount = Mathf.Clamp01((float)health / max);

        if (healthText != null)
        {
            healthText.text = $"{health}/{max}";
        }

        Debug.Log($"💚 Health updated: {health}/{max} ({targetFillAmount * 100f}%)");
    }
}