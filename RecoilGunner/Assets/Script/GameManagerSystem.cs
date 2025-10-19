using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

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
    public float waveMultiplier = 1.3f;

    [Header("Enemy Spawning")]
    public Enemy enemyPrefab;
    public Transform[] spawnPoints;
    public float enemySpawnDelay = 0.6f;
    [Header("Spawn Settings")]
    public float spawnBorderBuffer = 2f; // Distance above screen to spawn enemies

    [Header("UI References")]
    public GameObject gameOverUI;
    public GameObject pauseMenuUI;
    public TMP_Text scoreText;
    public TMP_Text waveText;
    public TMP_Text healthText;

    private int enemiesRemaining;
    private float waveTimer;
    private bool spawningWave = false;
    private PlayerHealth playerHealth;

    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);

        playerHealth = FindObjectOfType<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged.AddListener(UpdateHealthUI);
            playerHealth.OnPlayerDied.AddListener(GameOver);
        }

        // Validate enemy prefab
        if (enemyPrefab == null)
        {
            Debug.LogError("❌ Enemy Prefab is not assigned in GameManager! Please assign it in the inspector.");
        }

        UpdateScoreUI();
        UpdateWaveUI();
        StartGame();
    }

    void Update()
    {
        if (gameOver) return;

        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();

        enemiesRemaining = FindObjectsOfType<Enemy>().Length;

        if (gameStarted && !spawningWave && enemiesRemaining <= 0)
        {
            waveTimer -= Time.deltaTime;
            if (waveTimer <= 0)
                NextWave();
        }
    }

    public void StartGame()
    {
        gameStarted = true;
        gameOver = false;
        currentScore = 0;
        currentWave = 1;
        UpdateScoreUI();
        UpdateWaveUI();
        if (gameOverUI != null) gameOverUI.SetActive(false);
        StartWave();
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
        Debug.Log($"🌊 Starting Wave {currentWave} with {enemiesToSpawn} enemies");
        StartCoroutine(SpawnWaveCoroutine(enemiesToSpawn));
    }

    System.Collections.IEnumerator SpawnWaveCoroutine(int enemyCount)
    {
        for (int i = 0; i < enemyCount; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(enemySpawnDelay);
        }

        spawningWave = false;
        waveTimer = timeBetweenWaves;
        Debug.Log($"✅ Wave {currentWave} spawning complete! Next wave in {timeBetweenWaves} seconds");
    }

    void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("❌ No Enemy Prefab assigned in GameManager!");
            return;
        }

        Vector3 spawnPos = GetValidSpawnPosition();

        if (spawnPos != Vector3.zero)
        {
            Enemy newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            newEnemy.name = $"Enemy_Wave{currentWave}_{Random.Range(1000, 9999)}";
            Debug.Log($"👹 Spawned enemy at {spawnPos}");
        }
        else
        {
            Debug.LogWarning("⚠️ Could not find valid spawn position for enemy!");
        }
    }

    Vector3 GetValidSpawnPosition()
    {
        // Method 1: Use assigned spawn points if available
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            // Filter out null spawn points
            var validSpawnPoints = System.Array.FindAll(spawnPoints, point => point != null);
            if (validSpawnPoints.Length > 0)
            {
                Transform chosenPoint = validSpawnPoints[Random.Range(0, validSpawnPoints.Length)];
                return chosenPoint.position;
            }
        }

        // Method 2: Always spawn at the top of the scene
        return GetTopSpawnPosition();
    }

    Vector3 GetTopSpawnPosition()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("❌ No main camera found!");
            return Vector3.zero;
        }

        float camHeight = cam.orthographicSize * 2f;
        float camWidth = camHeight * cam.aspect;

        // Spawn at the top of the screen, with random X position
        Vector3 spawnPos = new Vector3(
            Random.Range(-camWidth / 2f + 1f, camWidth / 2f - 1f), // Random X within screen bounds (with small buffer)
            cam.transform.position.y + camHeight / 2f + spawnBorderBuffer, // Always at the top
            0
        );

        Debug.Log($"👹 Spawning enemy at top: {spawnPos}");
        return spawnPos;
    }

    void NextWave()
    {
        currentWave++;
        UpdateWaveUI();
        StartWave();
    }

    public void GameOver()
    {
        if (gameOver) return; // Prevent multiple game over calls

        gameOver = true;
        gameStarted = false;

        // Stop all audio immediately
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAllAudio();
        }

        // STOP THE GAME - but keep UI functional
        Time.timeScale = 0f; // This will stop physics and most game logic

        // Camera shake for dramatic effect (works with unscaled time)
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.ShakeGameOver();
        }

        // Update high score
        if (currentScore > highScore)
        {
            highScore = currentScore;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save(); // Save immediately
        }

        // Show game over UI
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
        }

        // Disable all enemies completely
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.enabled = false; // Disable the enemy script completely
                Rigidbody2D enemyRb = enemy.GetComponent<Rigidbody2D>();
                if (enemyRb != null)
                {
                    enemyRb.linearVelocity = Vector2.zero;
                    enemyRb.angularVelocity = 0f;
                    enemyRb.isKinematic = true; // Stop all physics
                }
            }
        }

        // Disable player controls completely
        RecoilPlayerController playerController = FindObjectOfType<RecoilPlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false; // Disable player script completely
            Rigidbody2D playerRb = playerController.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector2.zero;
                playerRb.angularVelocity = 0f;
                playerRb.isKinematic = true; // Stop all physics
            }
        }

        // Stop all bullets
        Bullet[] bullets = FindObjectsOfType<Bullet>();
        foreach (Bullet bullet in bullets)
        {
            if (bullet != null)
            {
                Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
                if (bulletRb != null)
                {
                    bulletRb.linearVelocity = Vector2.zero;
                    bulletRb.isKinematic = true;
                }
            }
        }

        Debug.Log($"💀 GAME OVER! Everything stopped. Final Score: {currentScore}, High Score: {highScore}");
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

    // Helper method to restart the game
    public void RestartGame()
    {
        Time.timeScale = 1f; // Resume normal time
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Helper method to quit game
    public void QuitGame()
    {
        Time.timeScale = 1f; // Resume normal time before quitting
        Application.Quit();
    }

    // Debug method to manually spawn an enemy
    [ContextMenu("Spawn Test Enemy")]
    public void SpawnTestEnemy()
    {
        SpawnEnemy();
    }
}