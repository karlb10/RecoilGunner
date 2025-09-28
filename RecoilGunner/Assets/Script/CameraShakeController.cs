using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    public float defaultShakeDuration = 0.2f;
    public float defaultShakeIntensity = 0.3f;
    public AnimationCurve shakeCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);

    [Header("Different Shake Types")]
    public float bulletHitShake = 0.1f;
    public float enemyDeathShake = 0.2f;
    public float playerDamageShake = 0.4f;
    public float gameOverShake = 1.0f;

    private Vector3 originalPosition;
    private bool isShaking = false;

    public static CameraShake Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            originalPosition = transform.position;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        originalPosition = transform.position;
    }

    // Main shake method
    public void Shake(float duration = -1f, float intensity = -1f)
    {
        if (duration == -1f) duration = defaultShakeDuration;
        if (intensity == -1f) intensity = defaultShakeIntensity;

        if (!isShaking)
        {
            StartCoroutine(ShakeCoroutine(duration, intensity));
        }
    }

    // Specific shake methods for different events
    public void ShakeBulletHit()
    {
        Shake(0.1f, bulletHitShake);
    }

    public void ShakeEnemyDeath()
    {
        Shake(0.15f, enemyDeathShake);
    }

    public void ShakePlayerDamage()
    {
        Shake(0.3f, playerDamageShake);
    }

    public void ShakeGameOver()
    {
        Shake(0.8f, gameOverShake);
    }

    public void ShakeExplosion()
    {
        Shake(0.5f, 0.6f);
    }

    private IEnumerator ShakeCoroutine(float duration, float intensity)
    {
        isShaking = true;
        float elapsed = 0f;
        Vector3 originalPos = originalPosition;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaled time so it works even when game is paused

            // Calculate shake intensity using curve
            float strength = intensity * shakeCurve.Evaluate(elapsed / duration);

            // Generate random shake offset
            Vector3 randomOffset = Random.insideUnitSphere * strength;
            randomOffset.z = 0f; // Keep Z position unchanged for 2D

            // Apply shake
            transform.position = originalPos + randomOffset;

            yield return null;
        }

        // Return to original position
        transform.position = originalPos;
        isShaking = false;
    }

    // Update original position (useful if camera follows player)
    public void UpdateOriginalPosition()
    {
        if (!isShaking)
        {
            originalPosition = transform.position;
        }
    }

    // Stop shake immediately
    public void StopShake()
    {
        if (isShaking)
        {
            StopAllCoroutines();
            transform.position = originalPosition;
            isShaking = false;
        }
    }
}