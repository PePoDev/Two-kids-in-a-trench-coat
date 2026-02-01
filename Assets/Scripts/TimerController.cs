using UnityEngine;
using TMPro;

/// <summary>
/// Countdown timer that displays remaining time and triggers game over when time runs out.
/// Plays warning sound during the last 5 seconds.
/// </summary>
public class TimerController : MonoBehaviour
{
    [Header("Timer Settings")]
    [Tooltip("Starting time in seconds")]
    public float startTime = 60f;

    [Tooltip("Text component to display the timer")]
    public TextMeshProUGUI timerText;

    [Header("Game Over")]
    [Tooltip("Trigger game over when timer reaches 0")]
    public bool triggerGameOver = true;

    [Header("Warning Settings")]
    [Tooltip("Time in seconds when warning sound starts playing")]
    public float warningThreshold = 5f;

    [Tooltip("Sound effect to play during warning period")]
    public AudioClip warningSound;

    [Tooltip("Warning sound volume")]
    [Range(0f, 1f)]
    public float warningSoundVolume = 0.5f;

    [Header("Display Format")]
    [Tooltip("Time display format: MM:SS or just seconds")]
    public bool useMinutesSecondsFormat = true;

    [Tooltip("Color for normal time")]
    public Color normalColor = Color.white;

    [Tooltip("Color for warning time")]
    public Color warningColor = Color.red;

    private float currentTime;
    private bool isRunning = false;
    private bool hasTriggeredGameOver = false;
    private bool isInWarningPeriod = false;
    private AudioSource audioSource;

    void Start()
    {
        // Initialize timer
        currentTime = startTime;

        // Setup audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && warningSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (audioSource != null)
        {
            audioSource.loop = true;
            audioSource.volume = warningSoundVolume;
            audioSource.playOnAwake = false;
        }

        // Update display immediately
        UpdateTimerDisplay();
    }

    void Update()
    {
        if (!isRunning || hasTriggeredGameOver)
            return;

        // Countdown
        currentTime -= Time.deltaTime;

        // Check if timer reached 0
        if (currentTime <= 0)
        {
            currentTime = 0;
            isRunning = false;

            // Stop warning sound
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            // Trigger game over
            if (triggerGameOver && !hasTriggeredGameOver)
            {
                hasTriggeredGameOver = true;
                OnTimerEnd();
            }
        }
        else
        {
            // Check if we entered warning period
            if (currentTime <= warningThreshold && !isInWarningPeriod)
            {
                isInWarningPeriod = true;
                StartWarningSound();
            }
        }

        // Update display
        UpdateTimerDisplay();
    }

    /// <summary>
    /// Start or resume the timer
    /// </summary>
    public void StartTimer()
    {
        isRunning = true;
    }

    /// <summary>
    /// Pause the timer
    /// </summary>
    public void PauseTimer()
    {
        isRunning = false;

        // Pause warning sound
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
        }
    }

    /// <summary>
    /// Reset the timer to starting time
    /// </summary>
    public void ResetTimer()
    {
        currentTime = startTime;
        hasTriggeredGameOver = false;
        isInWarningPeriod = false;

        // Stop warning sound
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        UpdateTimerDisplay();
    }

    /// <summary>
    /// Add time to the current timer
    /// </summary>
    public void AddTime(float seconds)
    {
        currentTime += seconds;

        // If we added enough time to exit warning period
        if (currentTime > warningThreshold && isInWarningPeriod)
        {
            isInWarningPeriod = false;
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }

        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        if (timerText == null)
            return;

        // Apply warning color if in warning period
        if (isInWarningPeriod)
        {
            timerText.color = warningColor;
        }
        else
        {
            timerText.color = normalColor;
        }

        // Format the time
        if (useMinutesSecondsFormat)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60f);
            int seconds = Mathf.FloorToInt(currentTime % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
        else
        {
            timerText.text = Mathf.CeilToInt(currentTime).ToString();
        }
    }

    private void StartWarningSound()
    {
        if (audioSource != null && warningSound != null && !audioSource.isPlaying)
        {
            audioSource.clip = warningSound;
            audioSource.Play();
        }
    }

    private void OnTimerEnd()
    {
        Debug.Log("Timer ended! Triggering game over.");

        // Call GameManager to trigger game over
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
        else
        {
            Debug.LogWarning("GameManager not found! Cannot trigger game over.");
        }
    }

    void OnDestroy()
    {
        // Clean up audio source
        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
    }
}
