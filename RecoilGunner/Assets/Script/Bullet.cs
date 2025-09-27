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
            Physics2D.IgnoreCollision(GetComponent<Collider2D>(), player.GetComponent<Collider2D>());
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if hit an enemy
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
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
            CreateHitEffect(transform.position);
            DestroyBullet();
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

        Destroy(gameObject);
    }

    // Optional: Make bullet affected by boundaries
    void OnBecameInvisible()
    {
        // Destroy bullet when it goes off screen
        Destroy(gameObject);
    }
}