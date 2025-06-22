// ============================================================================
// FILE: CleanBackgroundSetup.cs
// PURPOSE: Simple, clean background setup for simulation focus
// ============================================================================

using UnityEngine;
using System.Reflection;

/// <summary>
/// Simple setup for clean, non-distracting backgrounds
/// </summary>
public class BackgroundSetupHelper : MonoBehaviour
{
    [Header("Quick Setup")]
    [SerializeField] private bool setupOnStart = true;

    [Header("Background Presets")]
    [SerializeField] private CleanBackgroundPreset selectedPreset = CleanBackgroundPreset.MinimalGrass;

    [Header("Custom Settings (Fixed)")]
    [SerializeField] private Vector2 customWorldBounds = new Vector2(30f, 20f);
    [SerializeField] private bool customEnableGrass = true;
    [SerializeField] private bool customEnableBushes = false;
    [SerializeField] private bool customEnableTrees = false;
    [SerializeField] private bool customEnableFlowers = false;
    [SerializeField] private int customGrassDensity = 30;
    [SerializeField] private float customOpacity = 0.6f;

    [SerializeField] private BackgroundGenerator backgroundGenerator;

    public enum CleanBackgroundPreset
    {
        MinimalGrass,    // Just subtle grass dots
        PlainGround,     // Ground texture only
        VerySubtle,      // Minimal everything
        Custom           // Use your custom settings
    }

    void Start()
    {
        if (setupOnStart)
        {
            SetupCleanBackground();
        }
    }

    /// <summary>
    /// Main setup method
    /// </summary>
    [ContextMenu("Setup Clean Background")]
    public void SetupCleanBackground()
    {
        // Create the background generator if it doesn't exist
        if (backgroundGenerator == null)
        {
            GameObject bgObject = new GameObject("Clean Pixel Background Generator");
            backgroundGenerator = bgObject.AddComponent<BackgroundGenerator>();
        }

        // Apply the selected preset
        ApplyCleanPreset(selectedPreset);

        // Generate the background
        backgroundGenerator.GenerateBackground();

        Debug.Log($"? Clean background setup complete with {selectedPreset} preset!");
    }

    /// <summary>
    /// Apply clean preset configurations
    /// </summary>
    private void ApplyCleanPreset(CleanBackgroundPreset preset)
    {
        switch (preset)
        {
            case CleanBackgroundPreset.MinimalGrass:
                ApplyMinimalGrassPreset();
                break;
            case CleanBackgroundPreset.PlainGround:
                ApplyPlainGroundPreset();
                break;
            case CleanBackgroundPreset.VerySubtle:
                ApplyVerySubtlePreset();
                break;
            case CleanBackgroundPreset.Custom:
                ApplyCustomPreset();
                break;
        }
    }

    private void ApplyMinimalGrassPreset()
    {
        // Just ground + sparse grass dots
        SetBackgroundField("worldBounds", new Vector2(30f, 20f));
        SetBackgroundField("enableGrass", true);
        SetBackgroundField("enableSmallBushes", false);
        SetBackgroundField("enableTrees", false);
        SetBackgroundField("enableFlowers", false);
        SetBackgroundField("grassDensity", 25);
        SetBackgroundField("overallOpacity", 0.7f);
        SetBackgroundField("enableGroundVariation", true);
    }

    private void ApplyPlainGroundPreset()
    {
        // Ground texture only - no vegetation
        SetBackgroundField("worldBounds", new Vector2(30f, 20f));
        SetBackgroundField("enableGrass", false);
        SetBackgroundField("enableSmallBushes", false);
        SetBackgroundField("enableTrees", false);
        SetBackgroundField("enableFlowers", false);
        SetBackgroundField("overallOpacity", 0.8f);
        SetBackgroundField("enableGroundVariation", true);
    }

    private void ApplyVerySubtlePreset()
    {
        // Barely visible everything
        SetBackgroundField("worldBounds", new Vector2(30f, 20f));
        SetBackgroundField("enableGrass", true);
        SetBackgroundField("enableSmallBushes", false);
        SetBackgroundField("enableTrees", false);
        SetBackgroundField("enableFlowers", false);
        SetBackgroundField("grassDensity", 15);
        SetBackgroundField("overallOpacity", 0.5f);
        SetBackgroundField("enableGroundVariation", false);
    }

    private void ApplyCustomPreset()
    {
        // FIXED: Use the custom settings from inspector
        SetBackgroundField("worldBounds", customWorldBounds);
        SetBackgroundField("enableGrass", customEnableGrass);
        SetBackgroundField("enableSmallBushes", customEnableBushes);
        SetBackgroundField("enableTrees", customEnableTrees);
        SetBackgroundField("enableFlowers", customEnableFlowers);
        SetBackgroundField("grassDensity", customGrassDensity);
        SetBackgroundField("overallOpacity", customOpacity);
        SetBackgroundField("enableGroundVariation", true);

        Debug.Log($"Applied custom settings: Grass={customEnableGrass}, Bushes={customEnableBushes}, Trees={customEnableTrees}, Flowers={customEnableFlowers}");
    }

    /// <summary>
    /// FIXED: Properly set fields on the background generator
    /// </summary>
    private void SetBackgroundField(string fieldName, object value)
    {
        if (backgroundGenerator == null) return;

        try
        {
            FieldInfo field = backgroundGenerator.GetType().GetField(fieldName,
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

            if (field != null)
            {
                field.SetValue(backgroundGenerator, value);
                //Debug.Log($"Set {fieldName} = {value}");
            }
            else
            {
                Debug.LogWarning($"Could not find field: {fieldName}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error setting {fieldName}: {e.Message}");
        }
    }

    // ========================================================================
    // PUBLIC INTERFACE
    // ========================================================================

    [ContextMenu("Clear Background")]
    public void ClearBackground()
    {
        if (backgroundGenerator != null)
        {
            backgroundGenerator.ClearBackground();
        }
    }

    [ContextMenu("Regenerate Background")]
    public void RegenerateBackground()
    {
        if (backgroundGenerator != null)
        {
            backgroundGenerator.RegenerateBackground();
        }
        else
        {
            SetupCleanBackground();
        }
    }

    /// <summary>
    /// Change preset at runtime
    /// </summary>
    public void ChangePreset(CleanBackgroundPreset newPreset)
    {
        selectedPreset = newPreset;
        ApplyCleanPreset(newPreset);

        if (backgroundGenerator != null)
        {
            backgroundGenerator.RegenerateBackground();
        }
    }

    /// <summary>
    /// Update settings and regenerate
    /// </summary>
    public void UpdateCustomSettings(bool grass, bool bushes, bool trees, bool flowers, float opacity = 0.7f)
    {
        customEnableGrass = grass;
        customEnableBushes = bushes;
        customEnableTrees = trees;
        customEnableFlowers = flowers;
        customOpacity = opacity;

        if (selectedPreset == CleanBackgroundPreset.Custom)
        {
            ApplyCustomPreset();
            if (backgroundGenerator != null)
                backgroundGenerator.RegenerateBackground();
        }
    }

    /// <summary>
    /// Get reference to the background generator
    /// </summary>
    public BackgroundGenerator GetBackgroundGenerator()
    {
        return backgroundGenerator;
    }
}

