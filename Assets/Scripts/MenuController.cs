using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class MenuController : MonoBehaviour
{
    [Header("Fade Settings")]
    [SerializeField] private Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;
    
    private void Start()
    {
        // Ensure fade image starts transparent
        if (fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = 0f;
            fadeImage.color = color;
        }
    }

    /// <summary>
    /// Loads Level 1 with a fade-to-black transition
    /// </summary>
    public void LoadLevelOne()
    {
        StartCoroutine(LoadSceneWithFadeCoroutine("Level1"));
    }

    /// <summary>
    /// Generic method to load any scene with fade effect
    /// </summary>
    /// <param name="sceneName">Name of the scene to load</param>
    public void LoadSceneWithFade(string sceneName)
    {
        StartCoroutine(LoadSceneWithFadeCoroutine(sceneName));
    }

    private IEnumerator LoadSceneWithFadeCoroutine(string sceneName)
    {
        // Fade to black
        yield return StartCoroutine(FadeToBlack());
        
        // Load the scene
        SceneManager.LoadScene(sceneName);
        
        // Note: Fade from black will happen in the new scene's MenuController Start method
        // or you can add another controller in the new scene to fade in
    }

    private IEnumerator FadeToBlack()
    {
        if (fadeImage == null)
        {
            Debug.LogWarning("Fade Image is not assigned in MenuController!");
            yield break;
        }

        float elapsedTime = 0f;
        Color color = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        // Ensure fully opaque
        color.a = 1f;
        fadeImage.color = color;
    }

    /// <summary>
    /// Fades from black to transparent (useful for scene start)
    /// </summary>
    public void FadeFromBlack()
    {
        StartCoroutine(FadeFromBlackCoroutine());
    }

    private IEnumerator FadeFromBlackCoroutine()
    {
        if (fadeImage == null)
        {
            Debug.LogWarning("Fade Image is not assigned in MenuController!");
            yield break;
        }

        float elapsedTime = 0f;
        Color color = fadeImage.color;
        color.a = 1f;
        fadeImage.color = color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = 1f - Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        // Ensure fully transparent
        color.a = 0f;
        fadeImage.color = color;
    }

    /// <summary>
    /// Quits the application
    /// </summary>
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
