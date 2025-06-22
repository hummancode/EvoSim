using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple backdrop approach - just adds a semi-transparent background
/// Much lighter than the blur approach above
/// </summary>
public class SimpleChartBackdrop : MonoBehaviour
{
    [Header("Backdrop Settings")]
    [SerializeField] private Color backdropColor = new Color(0.2f, 0.2f, 0.2f, 0.6f); // Darker backdrop
    [SerializeField] private bool enableBackdrop = true;
    [SerializeField] private Vector2 backdropPadding = new Vector2(10f, 10f);

    [Header("Setup")]
    [SerializeField] private bool setupOnStart = true;

    private GameObject backdropObject;
    private Image backdropImage;

    void Start()
    {
        if (setupOnStart)
        {
            SetupBackdrop();
        }
    }

    /// <summary>
    /// Setup simple backdrop
    /// </summary>
    [ContextMenu("Setup Backdrop")]
    public void SetupBackdrop()
    {
        ClearBackdrop();

        // Create backdrop object
        backdropObject = new GameObject("ChartBackdrop");
        backdropObject.transform.SetParent(transform, false);
        backdropObject.transform.SetSiblingIndex(0); // Behind chart

        // Add RectTransform
        RectTransform backdropRect = backdropObject.AddComponent<RectTransform>();
        backdropRect.anchorMin = Vector2.zero;
        backdropRect.anchorMax = Vector2.one;
        backdropRect.offsetMin = -backdropPadding;
        backdropRect.offsetMax = backdropPadding;

        // Add Image
        backdropImage = backdropObject.AddComponent<Image>();
        backdropImage.color = backdropColor;

        // Create simple sprite
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        backdropImage.sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));

        // Set proper sorting
        Canvas backdropCanvas = backdropObject.AddComponent<Canvas>();
        backdropCanvas.overrideSorting = true;
        backdropCanvas.sortingOrder = -10; // Behind everything

        Debug.Log($"Simple backdrop setup for {gameObject.name}");
    }

    /// <summary>
    /// Clear backdrop
    /// </summary>
    [ContextMenu("Clear Backdrop")]
    public void ClearBackdrop()
    {
        if (backdropObject != null)
        {
            DestroyImmediate(backdropObject);
            backdropObject = null;
            backdropImage = null;
        }
    }

    /// <summary>
    /// Set backdrop color
    /// </summary>
    public void SetBackdropColor(Color color)
    {
        backdropColor = color;
        if (backdropImage != null)
        {
            backdropImage.color = color;
        }
    }

    /// <summary>
    /// Enable/disable backdrop
    /// </summary>
    public void SetBackdropEnabled(bool enabled)
    {
        enableBackdrop = enabled;
        if (backdropObject != null)
        {
            backdropObject.SetActive(enabled);
        }
    }

    void OnValidate()
    {
        if (Application.isPlaying && backdropImage != null)
        {
            backdropImage.color = backdropColor;
        }
    }

    void OnDestroy()
    {
        ClearBackdrop();
    }
}

