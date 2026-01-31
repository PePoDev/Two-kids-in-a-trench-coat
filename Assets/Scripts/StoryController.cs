using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class StoryController : MonoBehaviour
{
    [Header("Story Images")]
    [Tooltip("Array of story images/panels to display in sequence")]
    public Sprite[] storyPanels;
    
    [Header("UI References")]
    [Tooltip("Image component to display the current story panel")]
    public Image storyImageDisplay;
    
    [Tooltip("Optional text display for captions/dialogue")]
    public TextMeshProUGUI captionText;
    
    [Tooltip("Next button to advance story")]
    public Button nextButton;
    
    [Tooltip("Previous button to go back")]
    public Button previousButton;
    
    [Tooltip("Skip button to skip the entire story")]
    public Button skipButton;
    
    [Header("Story Captions (Optional)")]
    [Tooltip("Text captions for each panel (must match panel count)")]
    [TextArea(2, 5)]
    public string[] panelCaptions;
    
    [Header("Navigation Settings")]
    [Tooltip("Allow going back to previous panels")]
    public bool allowPrevious = true;
    
    [Tooltip("Allow skipping the entire story")]
    public bool allowSkip = true;
    
    [Tooltip("Auto-advance to next panel after delay (0 = disabled)")]
    public float autoAdvanceDelay = 0f;
    
    [Header("Transition Settings")]
    [Tooltip("Fade transition duration between panels")]
    public float fadeDuration = 0.5f;
    
    [Tooltip("Use fade transition between panels")]
    public bool useFadeTransition = true;
    
    [Header("Input Settings")]
    [Tooltip("Key to advance to next panel")]
    public KeyCode nextKey = KeyCode.Space;
    
    [Tooltip("Key to go to previous panel")]
    public KeyCode previousKey = KeyCode.Backspace;
    
    [Tooltip("Key to skip story")]
    public KeyCode skipKey = KeyCode.Escape;
    
    [Header("Events")]
    [Tooltip("Called when story starts")]
    public UnityEvent onStoryStart;
    
    [Tooltip("Called when story ends")]
    public UnityEvent onStoryComplete;
    
    [Tooltip("Called when story is skipped")]
    public UnityEvent onStorySkipped;
    
    [Tooltip("Called when panel changes (passes current panel index)")]
    public UnityEvent<int> onPanelChanged;
    
    // Private variables
    private int currentPanelIndex = 0;
    private bool isTransitioning = false;
    private float autoAdvanceTimer = 0f;
    private CanvasGroup canvasGroup;
    
    void Start()
    {
        // Validate setup
        if (storyPanels == null || storyPanels.Length == 0)
        {
            Debug.LogError("StoryController: No story panels assigned!");
            enabled = false;
            return;
        }
        
        if (storyImageDisplay == null)
        {
            Debug.LogError("StoryController: No story image display assigned!");
            enabled = false;
            return;
        }
        
        // Setup canvas group for fading
        if (useFadeTransition)
        {
            canvasGroup = storyImageDisplay.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = storyImageDisplay.gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        // Setup button listeners
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(ShowNextPanel);
        }
        
        if (previousButton != null)
        {
            previousButton.onClick.AddListener(ShowPreviousPanel);
        }
        
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipStory);
            skipButton.gameObject.SetActive(allowSkip);
        }
        
        // Start the story
        StartStory();
    }
    
    void Update()
    {
        if (isTransitioning)
            return;
        
        // Keyboard input
        if (Input.GetKeyDown(nextKey))
        {
            ShowNextPanel();
        }
        
        if (Input.GetKeyDown(previousKey) && allowPrevious)
        {
            ShowPreviousPanel();
        }
        
        if (Input.GetKeyDown(skipKey) && allowSkip)
        {
            SkipStory();
        }
        
        // Auto-advance
        if (autoAdvanceDelay > 0f && currentPanelIndex < storyPanels.Length - 1)
        {
            autoAdvanceTimer += Time.deltaTime;
            if (autoAdvanceTimer >= autoAdvanceDelay)
            {
                ShowNextPanel();
                autoAdvanceTimer = 0f;
            }
        }
    }
    
    void StartStory()
    {
        currentPanelIndex = 0;
        DisplayPanel(0);
        
        // Invoke start event
        if (onStoryStart != null)
        {
            onStoryStart.Invoke();
        }
        
        Debug.Log("Story started!");
    }
    
    public void ShowNextPanel()
    {
        if (isTransitioning)
            return;
        
        if (currentPanelIndex < storyPanels.Length - 1)
        {
            currentPanelIndex++;
            DisplayPanel(currentPanelIndex);
            autoAdvanceTimer = 0f;
        }
        else
        {
            // Reached the end
            CompleteStory();
        }
    }
    
    public void ShowPreviousPanel()
    {
        if (isTransitioning || !allowPrevious)
            return;
        
        if (currentPanelIndex > 0)
        {
            currentPanelIndex--;
            DisplayPanel(currentPanelIndex);
            autoAdvanceTimer = 0f;
        }
    }
    
    void DisplayPanel(int panelIndex)
    {
        if (panelIndex < 0 || panelIndex >= storyPanels.Length)
        {
            Debug.LogWarning($"StoryController: Invalid panel index {panelIndex}");
            return;
        }
        
        if (useFadeTransition && fadeDuration > 0f)
        {
            StartCoroutine(FadeToPanel(panelIndex));
        }
        else
        {
            SetPanel(panelIndex);
        }
        
        // Update button states
        UpdateButtonStates();
        
        // Invoke panel changed event
        if (onPanelChanged != null)
        {
            onPanelChanged.Invoke(panelIndex);
        }
        
        Debug.Log($"Displaying panel {panelIndex + 1}/{storyPanels.Length}");
    }
    
    void SetPanel(int panelIndex)
    {
        // Set the image
        storyImageDisplay.sprite = storyPanels[panelIndex];
        
        // Set caption if available
        if (captionText != null && panelCaptions != null && panelIndex < panelCaptions.Length)
        {
            captionText.text = panelCaptions[panelIndex];
        }
    }
    
    System.Collections.IEnumerator FadeToPanel(int panelIndex)
    {
        isTransitioning = true;
        
        // Fade out
        float elapsed = 0f;
        while (elapsed < fadeDuration / 2f)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = 1f - (elapsed / (fadeDuration / 2f));
            yield return null;
        }
        
        canvasGroup.alpha = 0f;
        
        // Change panel
        SetPanel(panelIndex);
        
        // Fade in
        elapsed = 0f;
        while (elapsed < fadeDuration / 2f)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = elapsed / (fadeDuration / 2f);
            yield return null;
        }
        
        canvasGroup.alpha = 1f;
        isTransitioning = false;
    }
    
    void UpdateButtonStates()
    {
        // Update next button
        if (nextButton != null)
        {
            bool isLastPanel = currentPanelIndex >= storyPanels.Length - 1;
            nextButton.interactable = !isLastPanel;
            
            // Optionally change button text on last panel
            TextMeshProUGUI buttonText = nextButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = isLastPanel ? "Complete" : "Next";
            }
        }
        
        // Update previous button
        if (previousButton != null && allowPrevious)
        {
            previousButton.interactable = currentPanelIndex > 0;
            previousButton.gameObject.SetActive(allowPrevious);
        }
    }
    
    void CompleteStory()
    {
        Debug.Log("Story completed!");
        
        // Invoke complete event
        if (onStoryComplete != null)
        {
            onStoryComplete.Invoke();
        }
    }
    
    public void SkipStory()
    {
        if (!allowSkip)
            return;
        
        Debug.Log("Story skipped!");
        
        // Invoke skip event
        if (onStorySkipped != null)
        {
            onStorySkipped.Invoke();
        }
        
        // Also invoke complete event
        if (onStoryComplete != null)
        {
            onStoryComplete.Invoke();
        }
    }
    
    // Public utility methods
    public void RestartStory()
    {
        currentPanelIndex = 0;
        DisplayPanel(0);
        autoAdvanceTimer = 0f;
        Debug.Log("Story restarted!");
    }
    
    public void JumpToPanel(int panelIndex)
    {
        if (panelIndex >= 0 && panelIndex < storyPanels.Length)
        {
            currentPanelIndex = panelIndex;
            DisplayPanel(panelIndex);
        }
    }
    
    public int GetCurrentPanelIndex()
    {
        return currentPanelIndex;
    }
    
    public int GetTotalPanels()
    {
        return storyPanels != null ? storyPanels.Length : 0;
    }
    
    public bool IsLastPanel()
    {
        return currentPanelIndex >= storyPanels.Length - 1;
    }
    
    public bool IsFirstPanel()
    {
        return currentPanelIndex == 0;
    }
}
