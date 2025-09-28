using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Game State")]
    public bool gameStarted = false;
    public bool gameOver = false;
    public bool gamePaused = false;

    [Header("Score System")]
    public int currentScore = 0;
    public int highScore = 0;

    [Header("Wave System")]
    public int currentWave = 1;
    public float timeBetweenWaves = 5f;
    public int baseEnemiesPerWave = 3;
    public float waveMultiplier = 1.2f;

    [Header("Enemy Spawning")]
    public Transform[] spawnPoints;
    public float spawnRadius = 10f;
    public float enemySpawnDelay = 1f;

    [Header("UI References")]
    public GameObject gameOverUI;
    public GameObject pauseMenuUI;
    public TMPro.TextMeshProUGUI scoreText;
    public TMPro.TextMeshProUGUI waveText;
    public TMPro.TextMeshProUGUI healthText;

    private int enemiesRemaining;
    private float waveTimer;
    private bool spawningWave = false;
    private PlayerHealth playerHealth;

    public static GameManager Instance { get; private set; }

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // Load high score
        highScore = PlayerPrefs.GetInt("HighScore", 0);

        // Find player health component
        playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged.AddListener(UpdateHealthUI);
            playerHealth.OnPlayerDied.AddListener(GameOver);
        }

        // Initialize UI
        UpdateScoreUI();
        UpdateWaveUI();

        // Start the game and first wave
        StartGame();
    }

    void Update()
    {
        if (gameOver) return;

        // Pause game
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }

        // Check if wave is complete
        if (gameStarted && !spawningWave && enemiesRemaining <= 0)
        {
            waveTimer -= Time.deltaTime;
            if (waveTimer <= 0)
            {
                NextWave();
            }
        }

        // Update enemy count
        UpdateEnemyCount();
    }

    public void StartGame()
    {
        gameStarted = true;
        gameOver = false;
        currentScore = 0;
        currentWave = 1;

        UpdateScoreUI();
        UpdateWaveUI();

        if (gameOverUI != null)
            gameOverUI.SetActive(false);
    }

    public void GameOver()
    {
        gameOver = true;
        gameStarted = false;

        // Check for high score
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }

        if (gameOverUI != null)
            gameOverUI.SetActive(true);

        Debug.Log($"Game Over! Final Score: {currentScore}");
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void AddScore(int points)
    {
        if (gameOver) return;

        currentScore += points;
        UpdateScoreUI();
    }

    void StartWave()
    {
        spawningWave = true;
        int enemiesToSpawn = Mathf.RoundToInt(baseEnemiesPerWave * Mathf.Pow(waveMultiplier, currentWave - 1));

        StartCoroutine(SpawnWaveCoroutine(enemiesToSpawn));

        Debug.Log($"Starting Wave {currentWave} with {enemiesToSpawn} enemies");
    }

    System.Collections.IEnumerator SpawnWaveCoroutine(int enemyCount)
    {
        for (int i = 0; i < enemyCount; i++)
        {
            SpawnRandomEnemy();
            yield return new WaitForSeconds(enemySpawnDelay);
        }

        spawningWave = false;
        waveTimer = timeBetweenWaves;
    }

    void SpawnRandomEnemy()
    {
        try
        {
            // Create a basic cube enemy if no prefab exists
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            if (enemy == null)
            {
                Debug.LogError("Failed to create enemy cube!");
                return;
            }

            enemy.name = "Enemy_" + Random.Range(1000, 9999);
            enemy.tag = "Enemy";

            // Remove the default BoxCollider (3D) that comes with primitives
            BoxCollider boxCollider3D = enemy.GetComponent<BoxCollider>();
            if (boxCollider3D != null)
            {
                DestroyImmediate(boxCollider3D);
            }

            // Add 2D components in the correct order
            Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearDamping = 2f;
            rb.angularDamping = 5f;

            // Add 2D collider
            BoxCollider2D collider2D = enemy.AddComponent<BoxCollider2D>();
            collider2D.isTrigger = true;

            // Set position first, then add Enemy script
            Vector3 spawnPos = GetRandomSpawnPosition();
            enemy.transform.position = spawnPos;
            enemy.transform.localScale = Vector3.one * 0.5f;

            // Add Enemy script last
            Enemy enemyScript = enemy.AddComponent<Enemy>();

            // Make it red
            Renderer renderer = enemy.GetComponent<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                renderer.material.color = Color.red;
            }

            Debug.Log($"Enemy '{enemy.name}' spawned at {spawnPos} with Rigidbody2D: {rb != null}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error spawning enemy: {e.Message}");
        }
    }

    Vector3 GetRandomSpawnPosition()
    {
        // If spawn points are defined, use them
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            if (spawnPoint != null)
            {
                return spawnPoint.position;
            }
        }

        // Get camera bounds for scene boundaries
        Camera cam = Camera.main;
        if (cam != null)
        {
            // Get screen boundaries in world space
            float camHeight = cam.orthographicSize * 2f;
            float camWidth = camHeight * cam.aspect;

            // Spawn within camera bounds but not too close to center
            float spawnMargin = 2f; // Distance from screen edge
            float minDistanceFromCenter = 4f; // Minimum distance from center

            Vector3 spawnPos;
            int attempts = 0;

            do
            {
                // Random position within camera bounds
                float x = Random.Range(-camWidth / 2f + spawnMargin, camWidth / 2f - spawnMargin);
                float y = Random.Range(-camHeight / 2f + spawnMargin, camHeight / 2f - spawnMargin);
                spawnPos = cam.transform.position + new Vector3(x, y, 0);
                spawnPos.z = 0; // Ensure 2D positioning

                attempts++;
            }
            while (Vector3.Distance(spawnPos, cam.transform.position) < minDistanceFromCenter && attempts < 10);

            return spawnPos;
        }

        // Fallback: spawn in a small area around origin
        Vector2 fallbackDirection = Random.insideUnitCircle.normalized;
        return (Vector3)(fallbackDirection * 5f);
    }

    void NextWave()
    {
        currentWave++;
        UpdateWaveUI();
        StartWave();
    }

    void UpdateEnemyCount()
    {
        enemiesRemaining = FindObjectsOfType<Enemy>().Length;
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {currentScore} | High: {highScore}";
    }

    void UpdateWaveUI()
    {
        if (waveText != null)
            waveText.text = $"Wave: {currentWave}";
    }

    void UpdateHealthUI(int health)
    {
        if (healthText != null)
            healthText.text = $"Health: {health}";
    }

    public void TogglePause()
    {
        gamePaused = !gamePaused;
        Time.timeScale = gamePaused ? 0f : 1f;

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(gamePaused);
    }
}