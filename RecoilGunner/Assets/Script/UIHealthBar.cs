using UnityEngine;
using TMPro;

public class UIHealthBar : MonoBehaviour
{
    [Header("UI References")]
    public RectTransform fillTransform;
    public TMP_Text healthText;
    public float smoothSpeed = 5f;

    private float targetWidth;
    private float currentWidth;
    private float fullWidth;

    void Start()
    {
        fullWidth = fillTransform.sizeDelta.x;
        currentWidth = fullWidth;

        fillTransform.pivot = new Vector2(0f, 0.5f);
        fillTransform.anchorMin = new Vector2(0f, 0.5f);
        fillTransform.anchorMax = new Vector2(0f, 0.5f);
    }

    void Update()
    {
        currentWidth = Mathf.Lerp(currentWidth, targetWidth, Time.deltaTime * smoothSpeed);
        Vector2 size = fillTransform.sizeDelta;
        size.x = currentWidth;
        fillTransform.sizeDelta = size;
    }

    public void UpdateHealth(int health, int max)
    {
        targetWidth = fullWidth * ((float)health / max);
        if (healthText != null)
            healthText.text = $"HP {health}/{max}";
    }
}
