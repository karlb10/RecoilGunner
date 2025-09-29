using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LogoIntroManager : MonoBehaviour
{
    [Header("Logo Settings")]
    [Tooltip("Drag your 5 logo images here in order")]
    public Sprite[] logoSprites = new Sprite[5];

    [Header("Timing Settings")]
    public float fadeInDuration = 0.8f;
    public float displayDuration = 1.5f;
    public float fadeOutDuration = 0.8f;

    [Header("Scene Management")]
    [Tooltip("Name of the scene to load after intro (e.g., 'MainMenu' or 'GameScene')")]
    public string nextSceneName = "MainMenu";

    [Header("Skip Settings")]
    public bool allowSkip = true;
    public KeyCode skipKey = KeyCode.Space;

    [Header("Audio (Optional)")]
    public AudioClip introMusic;
    [Range(0f, 1f)]
    public float musicVolume = 0.5f;

    private Image logoImage;
    private CanvasGroup canvasGroup;
    private AudioSource audioSource;
    private int currentLogoIndex = 0;
    private bool isTransitioning = false;

    void Start()
    {
        SetupUI();

        // Play intro music if assigned
        if (introMusic != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = introMusic;
            audioSource.volume = musicVolume;
            audioSource.loop = false;
            audioSource.Play();
        }

        // Validate logos
        if (logoSprites.Length == 0)
        {
            Debug.LogError("❌ No logo sprites assigned! Please assign 5 logos in the inspector.");
            LoadNextScene();
            return;
        }

        // Start the intro sequence
        StartCoroutine(PlayIntroSequence());
    }

    void Update()
    {
        // Allow skipping the intro
        if (allowSkip && Input.GetKeyDown(skipKey))
        {
            SkipIntro();
        }
    }

    void SetupUI()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("IntroCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000; // Ensure it's on top

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        // Create background panel (black)
        GameObject bgPanel = new GameObject("Background");
        bgPanel.transform.SetParent(canvasObj.transform, false);
        RectTransform bgRect = bgPanel.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        Image bgImage = bgPanel.AddComponent<Image>();
        bgImage.color = Color.black;

        // Create logo image
        GameObject logoObj = new GameObject("LogoImage");
        logoObj.transform.SetParent(canvasObj.transform, false);
        RectTransform logoRect = logoObj.AddComponent<RectTransform>();
        logoRect.anchorMin = new Vector2(0.5f, 0.5f);
        logoRect.anchorMax = new Vector2(0.5f, 0.5f);
        logoRect.sizeDelta = new Vector2(1920, 1080); // Larger size for 1920x1080
        logoRect.anchoredPosition = Vector2.zero;

        logoImage = logoObj.AddComponent<Image>();
        logoImage.preserveAspect = true;

        // Add CanvasGroup for smooth fading
        canvasGroup = logoObj.AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;

        Debug.Log("✅ UI Setup Complete - Canvas and Logo Image created");
    }

    IEnumerator PlayIntroSequence()
    {
        for (int i = 0; i < logoSprites.Length; i++)
        {
            if (logoSprites[i] == null)
            {
                Debug.LogWarning($"⚠️ Logo {i + 1} is not assigned, skipping...");
                continue;
            }

            currentLogoIndex = i;
            yield return StartCoroutine(ShowLogo(logoSprites[i]));
        }

        // All logos shown, load next scene
        LoadNextScene();
    }

    IEnumerator ShowLogo(Sprite logo)
    {
        isTransitioning = true;

        // Set the logo sprite
        logoImage.sprite = logo;
        logoImage.color = Color.white; // Ensure it's fully visible

        Debug.Log($"🖼️ Showing logo: {logo.name}");

        // Fade IN
        yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 0f, 1f, fadeInDuration));

        // Display
        yield return new WaitForSeconds(displayDuration);

        // Fade OUT
        yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 1f, 0f, fadeOutDuration));

        isTransitioning = false;
    }

    IEnumerator FadeCanvasGroup(CanvasGroup group, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Smooth fade using ease in/out
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            group.alpha = Mathf.Lerp(startAlpha, endAlpha, smoothT);

            yield return null;
        }

        group.alpha = endAlpha;
    }

    void SkipIntro()
    {
        Debug.Log("⏭️ Intro skipped by user");
        StopAllCoroutines();
        LoadNextScene();
    }

    void LoadNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogError("❌ Next scene name is not set! Cannot load next scene.");
            return;
        }

        Debug.Log($"✅ Loading scene: {nextSceneName}");
        SceneManager.LoadScene(nextSceneName);
    }

    // Optional: Add this to your build settings to ensure scenes are included
    void OnValidate()
    {
        if (logoSprites.Length != 5)
        {
            Debug.LogWarning($"⚠️ LogoIntroManager expects 5 logos, but {logoSprites.Length} are assigned.");
        }
    }
}