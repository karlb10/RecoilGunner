using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    public float lifetime = 3f;
    public int damage = 1;
    public bool destroyOnHit = true;

    [Header("Visual Effects")]
    public GameObject hitEffectPrefab;
    public TrailRenderer trail;

    [Header("Debug")]
    public bool showDebugInfo = false;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Destroy bullet after lifetime
        Destroy(gameObject, lifetime);

        // Ignore collision with player
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            Collider2D bulletCollider = GetComponent<Collider2D>();

            if (playerCollider != null && bulletCollider != null)
            {
                Physics2D.IgnoreCollision(bulletCollider, playerCollider);
            }
        }

        // Make sure bullet has proper setup
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError("❌ Bullet has no Collider2D! Adding CircleCollider2D...");
            CircleCollider2D circleCol = gameObject.AddComponent<CircleCollider2D>();
            circleCol.radius = 0.1f;
            circleCol.isTrigger = true;
        }
        else
        {
            col.isTrigger = true; // Make sure it's a trigger
        }

        // Make sure bullet has the correct tag
        if (!gameObject.CompareTag("Bullet"))
        {
            if (showDebugInfo)
            {
                Debug.Log("🔫 Setting bullet tag to 'Bullet'");
            }
            gameObject.tag = "Bullet";
        }

        if (showDebugInfo)
        {
            Debug.Log($"🔫 Bullet created with {damage} damage, lifetime: {lifetime}s");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (showDebugInfo)
        {
            Debug.Log($"🔫 Bullet collided with: {other.name} (Tag: {other.tag})");
        }

        // Check if hit an enemy
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);

                // Small camera shake on bullet hit
                if (CameraShake.Instance != null)
                {
                    CameraShake.Instance.ShakeBulletHit();
                }

                if (showDebugInfo)
                {
                    Debug.Log($"🎯 Bullet hit enemy {other.name} for {damage} damage!");
                }
            }
            else
            {
                if (showDebugInfo)
                {
                    Debug.LogWarning($"⚠️ Object {other.name} has Enemy tag but no Enemy script!");
                }
            }

            CreateHitEffect(other.transform.position);

            if (destroyOnHit)
            {
                DestroyBullet();
            }
        }
        // Check if hit a wall or obstacle
        else if (other.CompareTag("Wall") || other.CompareTag("Obstacle"))
        {
            if (showDebugInfo)
            {
                Debug.Log($"🧱 Bullet hit wall/obstacle: {other.name}");
            }

            CreateHitEffect(transform.position);
            DestroyBullet();
        }
        // Debug: Log other collisions
        else
        {
            if (showDebugInfo)
            {
                Debug.Log($"🔫 Bullet hit something else: {other.name} (Tag: {other.tag})");
            }
        }
    }

    void CreateHitEffect(Vector3 position)
    {
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 1f); // Clean up effect after 1 second
        }
    }

    void DestroyBullet()
    {
        // Disable trail before destroying to prevent visual glitches
        if (trail != null)
        {
            trail.enabled = false;
        }

        if (showDebugInfo)
        {
            Debug.Log("💥 Bullet destroyed");
        }

        Destroy(gameObject);
    }

    // Optional: Make bullet affected by boundaries
    void OnBecameInvisible()
    {
        // Destroy bullet when it goes off screen
        if (showDebugInfo)
        {
            Debug.Log("🔫 Bullet went off-screen, destroying");
        }
        Destroy(gameObject);
    }

    // Debug method to test collision
    [ContextMenu("Test Hit Enemy")]
    public void TestHitEnemy()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        if (enemies.Length > 0)
        {
            enemies[0].TakeDamage(damage);
            Debug.Log($"🎯 Test: Hit {enemies[0].name} for {damage} damage!");
        }
        else
        {
            Debug.Log("❌ No enemies found to test with!");
        }
    }
}