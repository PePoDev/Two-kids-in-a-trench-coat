using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Creates a colored border around a camera view using UI.
/// Attach to a Canvas that renders to a specific camera.
/// </summary>
public class CameraBorder : MonoBehaviour
{
    [Header("Border Settings")]
    [SerializeField] private Color borderColor = Color.magenta;
    [SerializeField] private float borderWidth = 10f;
    
    [Header("References")]
    [SerializeField] private RectTransform canvasRect;
    
    // Border images
    private Image topBorder;
    private Image bottomBorder;
    private Image leftBorder;
    private Image rightBorder;

    void Start()
    {
        CreateBorders();
    }

    private void CreateBorders()
    {
        if (canvasRect == null)
        {
            canvasRect = GetComponent<RectTransform>();
        }
        
        // Create border container
        GameObject borderContainer = new GameObject("BorderContainer");
        borderContainer.transform.SetParent(transform, false);
        RectTransform containerRect = borderContainer.AddComponent<RectTransform>();
        containerRect.anchorMin = Vector2.zero;
        containerRect.anchorMax = Vector2.one;
        containerRect.sizeDelta = Vector2.zero;
        containerRect.anchoredPosition = Vector2.zero;
        
        // Create each border
        topBorder = CreateBorderImage("TopBorder", containerRect);
        bottomBorder = CreateBorderImage("BottomBorder", containerRect);
        leftBorder = CreateBorderImage("LeftBorder", containerRect);
        rightBorder = CreateBorderImage("RightBorder", containerRect);
        
        // Position borders
        SetupTopBorder();
        SetupBottomBorder();
        SetupLeftBorder();
        SetupRightBorder();
    }

    private Image CreateBorderImage(string name, RectTransform parent)
    {
        GameObject borderObj = new GameObject(name);
        borderObj.transform.SetParent(parent, false);
        
        RectTransform rect = borderObj.AddComponent<RectTransform>();
        Image image = borderObj.AddComponent<Image>();
        image.color = borderColor;
        
        return image;
    }

    private void SetupTopBorder()
    {
        RectTransform rect = topBorder.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(0.5f, 1);
        rect.sizeDelta = new Vector2(0, borderWidth);
        rect.anchoredPosition = Vector2.zero;
    }

    private void SetupBottomBorder()
    {
        RectTransform rect = bottomBorder.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(1, 0);
        rect.pivot = new Vector2(0.5f, 0);
        rect.sizeDelta = new Vector2(0, borderWidth);
        rect.anchoredPosition = Vector2.zero;
    }

    private void SetupLeftBorder()
    {
        RectTransform rect = leftBorder.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0, 0);
        rect.anchorMax = new Vector2(0, 1);
        rect.pivot = new Vector2(0, 0.5f);
        rect.sizeDelta = new Vector2(borderWidth, 0);
        rect.anchoredPosition = Vector2.zero;
    }

    private void SetupRightBorder()
    {
        RectTransform rect = rightBorder.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1, 0);
        rect.anchorMax = new Vector2(1, 1);
        rect.pivot = new Vector2(1, 0.5f);
        rect.sizeDelta = new Vector2(borderWidth, 0);
        rect.anchoredPosition = Vector2.zero;
    }

    // ===== PUBLIC API =====
    
    public void SetBorderColor(Color color)
    {
        borderColor = color;
        if (topBorder != null) topBorder.color = color;
        if (bottomBorder != null) bottomBorder.color = color;
        if (leftBorder != null) leftBorder.color = color;
        if (rightBorder != null) rightBorder.color = color;
    }

    public void SetBorderWidth(float width)
    {
        borderWidth = width;
        if (topBorder != null)
        {
            SetupTopBorder();
            SetupBottomBorder();
            SetupLeftBorder();
            SetupRightBorder();
        }
    }
}
