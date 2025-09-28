using UnityEngine;

public class HealthBar : MonoBehaviour
{
    [Header("Health Bar Settings")]
    public float barWidth = 1f;
    public float barHeight = 0.2f;
    public float offsetY = 1f;
    public bool isPlayerHealthBar = false;

    [Header("Colors")]
    public Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
    public Color borderColor = Color.white;
    public Color playerHealthColor = Color.green;
    public Color enemyHealthColor = Color.red;

    private Camera mainCamera;
    private Transform targetTransform;
    private float currentHealthPercentage = 1f;

    void Start()
    {
        mainCamera = Camera.main;
        targetTransform = transform.parent;
    }

    void LateUpdate()
    {
        if (targetTransform == null || mainCamera == null) return;

        // Position above target
        transform.position = targetTransform.position + Vector3.up * offsetY;

        // Face camera
        transform.LookAt(mainCamera.transform);
        transform.Rotate(0, 180, 0);
    }

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        currentHealthPercentage = Mathf.Clamp01(currentHealth / maxHealth);
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Vector3 pos = transform.position;

        // Draw background (full bar)
        Gizmos.color = backgroundColor;
        DrawBar(pos, barWidth, barHeight);

        // Draw health fill
        Gizmos.color = isPlayerHealthBar ? playerHealthColor : enemyHealthColor;
        DrawBar(pos, barWidth * currentHealthPercentage, barHeight * 0.8f);

        // Draw border
        Gizmos.color = borderColor;
        DrawBarOutline(pos, barWidth, barHeight);
    }

    void DrawBar(Vector3 center, float width, float height)
    {
        Vector3 size = new Vector3(width, height, 0.01f);
        Gizmos.DrawCube(center, size);
    }

    void DrawBarOutline(Vector3 center, float width, float height)
    {
        Vector3 size = new Vector3(width, height, 0.01f);
        Gizmos.DrawWireCube(center, size);
    }

    public void SetHealthBarVisibility(bool visible)
    {
        enabled = visible;
    }
}