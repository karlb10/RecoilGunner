using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class MainMenuManager : MonoBehaviour
{
    public CanvasGroup fadePanel;
    public float fadeDuration = 1f;

    public GameObject mainMenuPanel;
    public GameObject aboutPanel;

    public enum TransitionType { Fade, Slide, Scale, Bounce }

    public TransitionType playTransition = TransitionType.Fade;
    public TransitionType aboutTransition = TransitionType.Slide;
    public TransitionType backTransition = TransitionType.Fade;

    private void Start()
    {
        if (fadePanel != null)
            fadePanel.alpha = 0f;

        // Play background music when main menu starts
        if (AudioManager.Instance != null)
        {
            Debug.Log("🎵 Playing background music on MainMenu");
            AudioManager.Instance.PlayBackgroundMusic();
        }
        else
        {
            Debug.LogWarning("⚠️ AudioManager.Instance not found!");
        }
    }

    public void OnPlayButtonPressed()
    {
        mainMenuPanel.SetActive(false);
        StartCoroutine(FadeAndLoadScene("GameScene"));
    }

    public void OnAboutButtonPressed()
    {
        // Play button click sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSound();
        }

        StartCoroutine(TransitionToPanel(aboutPanel, aboutTransition));
        aboutPanel.transform.SetAsLastSibling();
    }

    public void OnBackButtonPressed()
    {
        // Play button click sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSound();
        }

        aboutPanel.SetActive(false);
    }

    public void OnQuitButtonPressed()
    {
        // Play button click sound
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayButtonClickSound();
        }

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    private IEnumerator TransitionToPanel(GameObject targetPanel, TransitionType transitionType)
    {
        // Only hide other panels, keep main menu visible
        if (targetPanel == aboutPanel)
        {
            // Hide nothing, just show about panel on top
        }
        else if (targetPanel == mainMenuPanel)
        {
            aboutPanel.SetActive(false);
        }

        targetPanel.SetActive(true);

        switch (transitionType)
        {
            case TransitionType.Fade:
                yield return StartCoroutine(FadeInPanel(targetPanel));
                break;
            case TransitionType.Slide:
                yield return StartCoroutine(SlideInPanel(targetPanel));
                break;
            case TransitionType.Scale:
                yield return StartCoroutine(ScaleInPanel(targetPanel));
                break;
            case TransitionType.Bounce:
                yield return StartCoroutine(BounceInPanel(targetPanel));
                break;
        }
    }

    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        yield return StartCoroutine(FadeOut());
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadePanel.alpha = Mathf.Clamp01(elapsed / fadeDuration);
            yield return null;
        }
        fadePanel.alpha = 1f;
    }

    private IEnumerator FadeInPanel(GameObject panel)
    {
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = panel.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = elapsed / fadeDuration;
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    private IEnumerator SlideInPanel(GameObject panel)
    {
        RectTransform rect = panel.GetComponent<RectTransform>();
        Vector2 startPos = new Vector2(Screen.width, 0);
        Vector2 endPos = Vector2.zero;

        rect.anchoredPosition = startPos;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeDuration;
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, progress);
            yield return null;
        }
        rect.anchoredPosition = endPos;
    }

    private IEnumerator ScaleInPanel(GameObject panel)
    {
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.localScale = Vector3.zero;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeDuration;
            rect.localScale = Vector3.one * progress;
            yield return null;
        }
        rect.localScale = Vector3.one;
    }

    private IEnumerator BounceInPanel(GameObject panel)
    {
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.localScale = Vector3.zero;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeDuration;
            float bounce = Mathf.Sin(progress * Mathf.PI * 1.5f);
            rect.localScale = Vector3.one * bounce;
            yield return null;
        }
        rect.localScale = Vector3.one;
    }
}