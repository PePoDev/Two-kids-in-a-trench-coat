using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Main Game Manager for handling scene transitions and game state.
/// Implements singleton pattern for easy access from any script.
/// </summary>
public class GameManager : MonoBehaviour
{
    // Singleton instance
    public static GameManager Instance { get; private set; }

    [Header("Loading Transition")]
    [Tooltip("Canvas for loading screen (should have a CanvasGroup)")]
    public Canvas loadingCanvas;

    [Tooltip("Image for loading screen background")]
    public Image loadingBackground;

    [Tooltip("Optional loading progress bar")]
    public Slider loadingProgressBar;

    [Tooltip("Optional loading text")]
    public TMPro.TextMeshProUGUI loadingText;

    [Tooltip("Fade duration (seconds)")]
    public float fadeDuration = 1f;

    [Tooltip("Minimum loading screen display time (seconds)")]
    public float minLoadingTime = 1f;

    [Header("Game Over")]
    [Tooltip("Canvas for game over screen (should have a CanvasGroup)")]
    public Canvas gameOverCanvas;

    [Tooltip("Audio clip to play on game over")]
    public AudioClip gameOverSound;

    [Tooltip("Key to press to retry (default: R)")]
    public UnityEngine.InputSystem.Key retryKey = UnityEngine.InputSystem.Key.R;

    [Tooltip("Optional game over text to display")]
    public TMPro.TextMeshProUGUI gameOverText;

    [Tooltip("Fade duration for game over screen (seconds)")]
    public float gameOverFadeDuration = 1f;

    private CanvasGroup loadingCanvasGroup;
    private CanvasGroup gameOverCanvasGroup;
    private AudioSource audioSource;
    private bool isLoading = false;
    private bool isGameOver = false;

    void Awake()
    {
        // Implement singleton pattern
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Setup audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && gameOverSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Setup loading canvas
        if (loadingCanvas != null)
        {
            loadingCanvasGroup = loadingCanvas.GetComponent<CanvasGroup>();
            if (loadingCanvasGroup == null)
            {
                loadingCanvasGroup = loadingCanvas.gameObject.AddComponent<CanvasGroup>();
            }

            // Start with loading screen fully visible (black screen)
            loadingCanvas.gameObject.SetActive(true);
            loadingCanvasGroup.alpha = 1f;
        }

        // Setup game over canvas
        if (gameOverCanvas != null)
        {
            gameOverCanvasGroup = gameOverCanvas.GetComponent<CanvasGroup>();
            if (gameOverCanvasGroup == null)
            {
                gameOverCanvasGroup = gameOverCanvas.gameObject.AddComponent<CanvasGroup>();
            }

            // Hide game over screen initially
            gameOverCanvas.gameObject.SetActive(false);
            gameOverCanvasGroup.alpha = 0f;
        }
    }

    void Start()
    {
        // Fade out the loading screen when the scene starts
        StartCoroutine(FadeLoadingScreen(false));
    }

    void Update()
    {
        // Check for retry input when game over
        if (isGameOver)
        {
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard != null && keyboard[retryKey].wasPressedThisFrame)
            {
                RetryLevel();
            }
        }
    }

    /// <summary>
    /// Load a scene by name with transition effect
    /// </summary>
    public void LoadScene(string sceneName)
    {
        if (!isLoading)
        {
            StartCoroutine(LoadSceneWithTransition(sceneName));
        }
    }

    /// <summary>
    /// Load a scene by build index with transition effect
    /// </summary>
    public void LoadScene(int sceneIndex)
    {
        if (!isLoading)
        {
            StartCoroutine(LoadSceneWithTransition(sceneIndex));
        }
    }

    /// <summary>
    /// Reload the current scene
    /// </summary>
    public void ReloadCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        LoadScene(currentScene);
    }

    /// <summary>
    /// Show game over screen with sound and allow retry
    /// </summary>
    public void GameOver()
    {
        if (isGameOver)
            return;

        StartCoroutine(ShowGameOver());
    }

    /// <summary>
    /// Retry the current level
    /// </summary>
    public void RetryLevel()
    {
        if (!isGameOver)
            return;

        isGameOver = false;

        // Unpause the game
        Time.timeScale = 1f;

        // Hide game over screen
        if (gameOverCanvas != null)
        {
            gameOverCanvas.gameObject.SetActive(false);
        }

        ReloadCurrentScene();
    }

    /// <summary>
    /// Quit the game
    /// </summary>
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private IEnumerator ShowGameOver()
    {
        isGameOver = true;

        // Play game over sound
        if (audioSource != null && gameOverSound != null)
        {
            audioSource.PlayOneShot(gameOverSound);
        }

        // Pause the game
        Time.timeScale = 0f;

        // Show and fade in game over screen
        if (gameOverCanvas != null && gameOverCanvasGroup != null)
        {
            gameOverCanvas.gameObject.SetActive(true);

            // Update game over text if available
            if (gameOverText != null)
            {
                gameOverText.text = $"Game Over!\nPress {retryKey} to Retry";
            }

            float elapsed = 0f;
            while (elapsed < gameOverFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                gameOverCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / gameOverFadeDuration);
                yield return null;
            }

            gameOverCanvasGroup.alpha = 1f;
        }
    }

    private IEnumerator LoadSceneWithTransition(string sceneName)
    {
        isLoading = true;

        // Show and fade in loading screen
        yield return StartCoroutine(FadeLoadingScreen(true));

        // Start loading the scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        float startTime = Time.time;

        // Wait for scene to load and minimum display time
        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            // Update progress bar if available
            if (loadingProgressBar != null)
            {
                loadingProgressBar.value = progress;
            }

            // Update loading text if available
            if (loadingText != null)
            {
                loadingText.text = $"Loading... {Mathf.RoundToInt(progress * 100)}%";
            }

            // Check if loading is complete and minimum time has passed
            if (asyncLoad.progress >= 0.9f)
            {
                float elapsedTime = Time.time - startTime;
                if (elapsedTime >= minLoadingTime)
                {
                    asyncLoad.allowSceneActivation = true;
                }
            }

            yield return null;
        }

        // Fade out loading screen
        yield return StartCoroutine(FadeLoadingScreen(false));

        isLoading = false;
    }

    private IEnumerator LoadSceneWithTransition(int sceneIndex)
    {
        isLoading = true;

        // Show and fade in loading screen
        yield return StartCoroutine(FadeLoadingScreen(true));

        // Start loading the scene asynchronously
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex);
        asyncLoad.allowSceneActivation = false;

        float startTime = Time.time;

        // Wait for scene to load and minimum display time
        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            // Update progress bar if available
            if (loadingProgressBar != null)
            {
                loadingProgressBar.value = progress;
            }

            // Update loading text if available
            if (loadingText != null)
            {
                loadingText.text = $"Loading... {Mathf.RoundToInt(progress * 100)}%";
            }

            // Check if loading is complete and minimum time has passed
            if (asyncLoad.progress >= 0.9f)
            {
                float elapsedTime = Time.time - startTime;
                if (elapsedTime >= minLoadingTime)
                {
                    asyncLoad.allowSceneActivation = true;
                }
            }

            yield return null;
        }

        // Fade out loading screen
        yield return StartCoroutine(FadeLoadingScreen(false));

        isLoading = false;
    }

    private IEnumerator FadeLoadingScreen(bool fadeIn)
    {
        if (loadingCanvas == null || loadingCanvasGroup == null)
        {
            Debug.LogWarning("Loading canvas not set up!");
            yield break;
        }

        if (fadeIn)
        {
            loadingCanvas.gameObject.SetActive(true);

            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                loadingCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                yield return null;
            }

            loadingCanvasGroup.alpha = 1f;
        }
        else
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                loadingCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                yield return null;
            }

            loadingCanvasGroup.alpha = 0f;
        }
    }
}
