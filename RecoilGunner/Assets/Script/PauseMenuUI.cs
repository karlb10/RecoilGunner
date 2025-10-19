using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseMenuController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject pausePanel; // Parent panel with CanvasGroup
    private CanvasGroup canvasGroup;
    private bool isPaused = false;

    [Header("Fade Settings")]
    public float fadeDuration = 0.5f; // Fade time (seconds)
    public CanvasGroup fadeScreen; // Optional full-screen fade overlay

    [Header("HUD Elements")]
    public GameObject scoreUI;
    public GameObject waveUI;
    public GameObject healthUI;
    public GameObject pauseButton; // Add pause button reference
    public GameObject gameOverPanel; // Add game over panel reference

    private RecoilPlayerController playerController;

    void Start()
    {
        // Setup pause panel fade
        canvasGroup = pausePanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = pausePanel.AddComponent<CanvasGroup>();
        }
        pausePanel.SetActive(false);
        canvasGroup.alpha = 0;

        // Setup fade screen
        if (fadeScreen != null)
        {
            fadeScreen.alpha = 0;
            fadeScreen.gameObject.SetActive(false);
        }

        // Auto-find HUD elements if not assigned
        if (scoreUI == null)
            scoreUI = GameObject.Find("ScoreText") ?? GameObject.Find("ScoreUI");
        if (waveUI == null)
            waveUI = GameObject.Find("WaveText") ?? GameObject.Find("WaveUI");
        if (healthUI == null)
            healthUI = GameObject.Find("HealthText") ?? GameObject.Find("HealthUI");
        if (pauseButton == null)
            pauseButton = GameObject.Find("PauseButton") ?? GameObject.Find("Pause Button");
        if (gameOverPanel == null)
            gameOverPanel = GameObject.Find("GameOverPanel") ?? GameObject.Find("GameOver Panel");

        // Find player controller
        playerController = FindObjectOfType<RecoilPlayerController>();

        Debug.Log($"✅ PauseMenuController initialized");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pausePanel.SetActive(true);
        StartCoroutine(FadeCanvasGroup(canvasGroup, 0f, 1f));

        // Stop charging sound immediately
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopChargeSound();
        }

        // Disable player controller
        if (playerController != null)
            playerController.enabled = false;

        // Play button sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSound();
        }
    }

    public void ResumeGame()
    {
        // Play button sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSound();
        }

        StartCoroutine(FadeOutAndUnpause());
    }

    private IEnumerator FadeOutAndUnpause()
    {
        yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 1f, 0f));
        pausePanel.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;

        // Re-enable player controller
        if (playerController != null)
            playerController.enabled = true;
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float start, float end)
    {
        float elapsed = 0f;
        cg.interactable = false;
        cg.blocksRaycasts = false;

        while (elapsed < fadeDuration)
        {
            cg.alpha = Mathf.Lerp(start, end, elapsed / fadeDuration);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        cg.alpha = end;
        cg.interactable = end == 1;
        cg.blocksRaycasts = end == 1;
    }

    public void ResetGame()
    {
        // Play button sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSound();
        }

        // Hide game over panel, pause button and pause panel immediately
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
        if (pauseButton != null)
            pauseButton.SetActive(false);
        if (pausePanel != null)
            pausePanel.SetActive(false);

        StartCoroutine(FadeAndLoadScene(SceneManager.GetActiveScene().name));
    }

    public void LoadMainMenu()
    {
        // Play button sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSound();
        }

        // Hide pause button immediately
        if (pauseButton != null)
            pauseButton.SetActive(false);

        // Stop the charging sound if playing
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopChargeSound();
        }

        StartCoroutine(FadeAndLoadScene("MainMenu", hidePanel: true));
    }

    public void LoadLevelSelect()
    {
        // Play button sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSound();
        }

        StartCoroutine(FadeAndLoadScene("LevelSelection", hidePanel: true));
    }

    private IEnumerator FadeAndLoadScene(string sceneName, bool hidePanel = false)
    {
        // Hide HUD elements
        HideAllHUD();

        // Immediately hide pause panel
        if (hidePanel && pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        // Unpause time
        Time.timeScale = 1f;
        isPaused = false;

        // Stop all audio before transitioning
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAllAudio();
        }

        // Fade to black
        if (fadeScreen != null)
        {
            fadeScreen.gameObject.SetActive(true);
            yield return StartCoroutine(FadeCanvasGroup(fadeScreen, 0f, 1f));
        }

        yield return new WaitForSecondsRealtime(0.2f);
        SceneManager.LoadScene(sceneName);

        // Play background music for main menu
        if (sceneName == "MainMenu" && AudioManager.Instance != null)
        {
            yield return new WaitForSecondsRealtime(0.5f);
            AudioManager.Instance.PlayBackgroundMusic();
        }
    }

    private void HideAllHUD()
    {
        if (scoreUI != null)
            scoreUI.SetActive(false);
        if (waveUI != null)
            waveUI.SetActive(false);
        if (healthUI != null)
            healthUI.SetActive(false);
    }

    private void ShowAllHUD()
    {
        if (scoreUI != null)
            scoreUI.SetActive(true);
        if (waveUI != null)
            waveUI.SetActive(true);
        if (healthUI != null)
            healthUI.SetActive(true);
    }
}