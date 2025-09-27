using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public int health = 3;
    public int maxHealth = 3;
    public float moveSpeed = 2f;
    public int scoreValue = 10;

    [Header("AI Behavior")]
    public float followRange = 8f;
    public float attackRange = 1.5f;
    public float attackCooldown = 1f;
    public int attackDamage = 1;

    [Header("Visual Feedback")]
    public SpriteRenderer spriteRenderer;
    public Color hitColor = Color.red;
    public float hitFlashDuration = 0.1f;

    private Transform player;
    private Rigidbody2D rb;
    private float nextAttackTime;
    private Color originalColor;
    private bool isFlashing = false;

    void Start()
    {
        // Wait a frame to ensure all components are added
        StartCoroutine(InitializeEnemy());
    }

    System.Collections.IEnumerator InitializeEnemy()
    {
        yield return null; // Wait one frame

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError($"No Rigidbody2D found on {gameObject.name}!");
            yield break;
        }

        player = GameObject.FindWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogWarning("No player found with 'Player' tag!");
        }

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        health = maxHealth;
    }

    void Update()
    {
        if (player == null || rb == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Follow player if in range
        if (distanceToPlayer <= followRange && distanceToPlayer > attackRange)
        {
            MoveTowardsPlayer();
        }

        // Attack if in range and cooldown is ready
        if (distanceToPlayer <= attackRange && Time.time >= nextAttackTime)
        {
            AttackPlayer();
            nextAttackTime = Time.time + attackCooldown;
        }

        // Face the player
        FacePlayer();
    }

    void MoveTowardsPlayer()
    {
        if (rb == null || player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }

    void FacePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        if (direction != Vector2.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
    }

    void AttackPlayer()
    {
        if (rb == null || player == null) return;

        // Stop movement during attack
        rb.linearVelocity = Vector2.zero;

        // Deal damage to player
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
        }

        // Visual feedback for attack (you can add animation here)
        Debug.Log("Enemy attacks player!");
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        // Flash red when hit
        if (!isFlashing)
        {
            StartCoroutine(FlashRed());
        }

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
        spriteRenderer.color = originalColor;
        isFlashing = false;
    }

    void Die()
    {
        // Add score
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.AddScore(scoreValue);
        }

        // Optional: Death effect
        // Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        // Draw follow range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, followRange);

        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}