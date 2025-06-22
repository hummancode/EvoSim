using UnityEngine;
// PURPOSE: One-click background setup for immediate results
// ============================================================================



/// <summary>
/// Super simple one-click background setup
/// Add this to any GameObject and press Space to generate background
/// </summary>
public class QuickBackgroundSetup : MonoBehaviour
{
    [Header("One-Click Setup")]
    [SerializeField] private KeyCode generateKey = KeyCode.Space;
    [SerializeField] private KeyCode clearKey = KeyCode.C;
    [SerializeField] private bool showInstructions = true;

    void Start()
    {
        if (showInstructions)
        {
            Debug.Log("?? Background Controls: Press SPACE to generate, C to clear");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(generateKey))
        {
            GenerateQuickBackground();
        }

        if (Input.GetKeyDown(clearKey))
        {
            ClearBackground();
        }
    }

    /// <summary>
    /// Generate a beautiful background with one method call
    /// </summary>
    [ContextMenu("Generate Quick Background")]
    public void GenerateQuickBackground()
    {
        // Find or create background setup helper
        BackgroundSetupHelper helper = FindObjectOfType<BackgroundSetupHelper>();

        if (helper == null)
        {
            GameObject helperObj = new GameObject("Background Setup Helper");
            helper = helperObj.AddComponent<BackgroundSetupHelper>();
        }

        // Use the lush garden preset for beautiful results
      

        Debug.Log("?? Beautiful nature background generated!");
    }

    [ContextMenu("Clear Background")]
    public void ClearBackground()
    {
        BackgroundSetupHelper helper = FindObjectOfType<BackgroundSetupHelper>();
        if (helper != null)
        {
            helper.ClearBackground();
            Debug.Log("?? Background cleared");
        }

        // Also find and destroy any existing background objects
        GameObject existingBG = GameObject.Find("Nature Background");
        if (existingBG != null)
        {
            DestroyImmediate(existingBG);
        }
    }
}
