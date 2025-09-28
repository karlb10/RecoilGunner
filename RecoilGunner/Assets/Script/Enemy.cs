using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int health = 3;
    public int maxHealth = 3;
    public float moveSpeed = 2f;
    public int scoreValue = 10;

    [Header("AI Behavior")]
    public float attackRange = 1.5f;
    public float attackCooldown = 1f;
    public int attackDamage = 1;

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;
    public Color hitColor = Color.red;
    public float hitFlashDuration = 0.1f;
    [Header("Health Bar")]
    public bool showHealthBar = true;

    [Header("Death Effect")]
    public ParticleSystem deathEffect;
    public GameObject deathEffectPrefab; // Alternative if using prefab instead of component

    [Header("Debug")]
    public bool showDebugInfo = true;

    private Transform player;
    private Rigidbody2D rb;
    private float nextAttackTime;
    private Color originalColor;
    private bool isFlashing = false;
    private bool isDead = false; // Prevent multiple death calls
    private EnemyHealthBar healthBar;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Find player
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("❌ No GameObject with 'Player' tag found! Make sure your player has the 'Player' tag.");
        }

        // Get sprite renderer
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        else
        {
            Debug.LogWarning("⚠️ No SpriteRenderer found on enemy. Visual feedback won't work.");
        }

        // Initialize health
        health = maxHealth;

        // Set up health bar
        if (showHealthBar)
        {
            healthBar = GetComponent<EnemyHealthBar>();
            if (healthBar == null)
            {
                healthBar = gameObject.AddComponent<EnemyHealthBar>();
            }
        }

        // Validate collider setup
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError("❌ Enemy has no Collider2D! Adding BoxCollider2D...");
            gameObject.AddComponent<BoxCollider2D>();
        }
        else
        {
            // Make sure it's set as trigger for bullet detection
            col.isTrigger = true;
        }

        // Make sure enemy has the correct tag
        if (!gameObject.CompareTag("Enemy"))
        {
            Debug.LogWarning("⚠️ Enemy doesn't have 'Enemy' tag. Setting it now...");
            gameObject.tag = "Enemy";
        }

        if (showDebugInfo)
        {
            Debug.Log($"✅ Enemy spawned with {health} health at {transform.position}");
        }
    }

    void Update()
    {
        // STOP ALL AI BEHAVIOR IF GAME IS OVER
        if (GameManager.Instance != null && GameManager.Instance.gameOver)
        {
            // Stop movement immediately
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
            return; // Exit early - no AI processing
        }

        if (isDead || player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // ALWAYS FOLLOW THE PLAYER (no distance limit)
        if (distance > attackRange)
        {
            // Always move toward player unless in attack range
            Vector2 direction = (player.position - transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;
        }
        else
        {
            // Stop when in attack range
            rb.linearVelocity = Vector2.zero;
        }

        // Attack player when close enough
        if (distance <= attackRange && Time.time >= nextAttackTime)
        {
            PlayerHealth ph = player.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(attackDamage);
                if (showDebugInfo)
                {
                    Debug.Log($"⚔️ Enemy attacked player for {attackDamage} damage!");
                }
            }
            nextAttackTime = Time.time + attackCooldown;
        }

        FacePlayer();
    }

    void FacePlayer()
    {
        if (player == null || spriteRenderer == null) return;

        Vector2 dir = player.position - transform.position;
        if (dir.x != 0)
        {
            spriteRenderer.flipX = dir.x < 0;
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return; // Prevent damage to dead enemies

        health -= damage;

        if (showDebugInfo)
        {
            Debug.Log($"💥 Enemy took {damage} damage! Health: {health}/{maxHealth}");
        }

        // Notify health bar of damage
        if (healthBar != null)
        {
            healthBar.OnEnemyDamaged();
        }

        // Flash red when taking damage
        if (!isFlashing && spriteRenderer != null)
            StartCoroutine(FlashRed());

        // Check if enemy should die
        if (health <= 0)
        {
            Die();
        }
    }

    System.Collections.IEnumerator FlashRed()
    {
        if (spriteRenderer == null) yield break;

        isFlashing = true;
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(hitFlashDuration);

        if (spriteRenderer != null && !isDead) // Check if still alive
        {
            spriteRenderer.color = originalColor;
        }

        isFlashing = false;
    }

    void Die()
    {
        if (isDead) return; // Prevent multiple death calls

        isDead = true;

        if (showDebugInfo)
        {
            Debug.Log($"💀 Enemy died! Giving {scoreValue} points.");
        }

        // Camera shake when enemy dies
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.ShakeEnemyDeath();
        }

        // Stop movement
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Spawn death effect
        CreateDeathEffect();

        // Give score to player
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddScore(scoreValue);
        }
        else
        {
            Debug.LogWarning("⚠️ GameManager.Instance is null! Score not added.");
        }

        // Destroy the enemy
        Destroy(gameObject);
    }

    void CreateDeathEffect()
    {
        // Try to use ParticleSystem component first
        if (deathEffect != null)
        {
            ParticleSystem fx = Instantiate(deathEffect, transform.position, Quaternion.identity);
            fx.Play();
            Destroy(fx.gameObject, fx.main.duration + 1f);
        }
        // Try to use prefab if component isn't available
        else if (deathEffectPrefab != null)
        {
            GameObject effect = Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 2f); // Clean up after 2 seconds
        }
        else
        {
            if (showDebugInfo)
            {
                Debug.Log("💨 No death effect assigned, skipping visual effect.");
            }
        }
    }

    // Handle collision with bullets
    void OnTriggerEnter2D(Collider2D other)
    {
        if (isDead) return;

        if (other.CompareTag("Bullet"))
        {
            Bullet bullet = other.GetComponent<Bullet>();
            if (bullet != null)
            {
                TakeDamage(bullet.damage);

                if (showDebugInfo)
                {
                    Debug.Log($"🎯 Enemy hit by bullet for {bullet.damage} damage!");
                }
            }
            else
            {
                // Fallback if bullet doesn't have Bullet script
                TakeDamage(1);

                if (showDebugInfo)
                {
                    Debug.Log("🎯 Enemy hit by bullet (no script, using default 1 damage)!");
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        // Visualize attack range in editor
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Since enemies always follow the player, we don't need to show follow range
        // The follow range is now infinite!
    }

    // Debug method to test damage
    [ContextMenu("Test Take Damage")]
    public void TestTakeDamage()
    {
        TakeDamage(1);
    }

    // Debug method to test death
    [ContextMenu("Test Death")]
    public void TestDeath()
    {
        Die();
    }
}