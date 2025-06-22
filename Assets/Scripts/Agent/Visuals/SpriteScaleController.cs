// ============================================================================
// FILE: SpriteScaleController.cs
// PURPOSE: Dedicated sprite scaling controller for age-based sizing
// ============================================================================

using UnityEngine;

/// <summary>
/// Dedicated controller for managing sprite scaling based on age
/// Called by AgentVisualController to handle all size-related transformations
/// </summary>
public class SpriteScaleController : MonoBehaviour
{
    [Header("?? SCALE SETTINGS")]
    [SerializeField] private bool enableScaling = true;
    [SerializeField] private bool smoothScaling = true;
    [SerializeField] private float scalingSpeed = 5f; // Speed for smooth transitions

    [Header("?? AGE-BASED SCALES")]
    [SerializeField] private float babyScale = 0.5f;     // 50% size
    [SerializeField] private float childScale = 0.75f;   // 75% size  
    [SerializeField] private float adultScale = 1.0f;    // 100% size
    [SerializeField] private float elderlyScale = 0.9f;  // 90% size

    [Header("?? ADVANCED SETTINGS")]
    [SerializeField] private bool scaleFluffToo = true; // Scale fluff renderers separately if needed
    [SerializeField] private float fluffScaleMultiplier = 1.0f; // Additional fluff scale adjustment
    [SerializeField] private bool debugMode = false;

    [Header("?? DEBUG INFO")]
    [SerializeField] private Vector3 baseScale = Vector3.one;
    [SerializeField] private Vector3 currentTargetScale = Vector3.one;
    [SerializeField] private Vector3 actualScale = Vector3.one;
    [SerializeField] private string currentLifeStage = "Unknown";
    [SerializeField] private bool isScaling = false;

    // Component references
    private AgeLifeStageTracker lifeStageTracker;
    private SheepLikeGeneticFluff fluffSystem;
    private Transform headFluffTransform;
    private Transform bodyFluffTransform;

    // Scale tracking
    private Vector3 targetScale = Vector3.one;
    private bool hasInitialized = false;

    void Awake()
    {
        // Store the original scale as base scale
        baseScale = transform.localScale;
        targetScale = baseScale;

        // Get components
        lifeStageTracker = GetComponent<AgeLifeStageTracker>();
        fluffSystem = GetComponent<SheepLikeGeneticFluff>();

        // Find fluff transforms
        FindFluffTransforms();

        if (debugMode)
        {
            Debug.Log($"[SpriteScaleController] {gameObject.name} initialized with base scale: {baseScale}");
        }
    }

    void Start()
    {
        // Subscribe to life stage changes
        if (lifeStageTracker != null)
        {
            lifeStageTracker.OnLifeStageChanged += OnLifeStageChanged;

            // Set initial scale based on current life stage
            SetScaleForLifeStage(lifeStageTracker.CurrentStage, !smoothScaling); // Force immediate if not smooth
        }

        hasInitialized = true;

        if (debugMode)
        {
            Debug.Log($"[SpriteScaleController] {gameObject.name} started. Current life stage: {(lifeStageTracker?.CurrentStage.ToString() ?? "None")}");
        }
    }

    void Update()
    {
        if (!enableScaling || !hasInitialized) return;

        // Update debug info
        actualScale = transform.localScale;
        currentLifeStage = lifeStageTracker?.CurrentStage.ToString() ?? "Unknown";
        isScaling = Vector3.Distance(actualScale, targetScale) > 0.01f;

        // Handle smooth scaling
        if (smoothScaling && isScaling)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scalingSpeed);

            // Also scale fluff if enabled
            if (scaleFluffToo)
            {
                UpdateFluffScaling();
            }
        }
    }

    private void FindFluffTransforms()
    {
        headFluffTransform = transform.Find("HeadFluff");
        bodyFluffTransform = transform.Find("BodyFluff");

        if (debugMode)
        {
            Debug.Log($"[SpriteScaleController] Found fluff transforms - Head: {headFluffTransform != null}, Body: {bodyFluffTransform != null}");
        }
    }

    private void OnLifeStageChanged(AgeLifeStageTracker.LifeStage newStage)
    {
        if (debugMode)
        {
            Debug.Log($"[SpriteScaleController] {gameObject.name} life stage changed to: {newStage}");
        }

        SetScaleForLifeStage(newStage, false); // Use smooth transition
    }

    /// <summary>
    /// Set scale based on life stage
    /// </summary>
    /// <param name="stage">Life stage</param>
    /// <param name="immediate">If true, apply immediately without smooth transition</param>
    public void SetScaleForLifeStage(AgeLifeStageTracker.LifeStage stage, bool immediate = false)
    {
        if (!enableScaling) return;

        float scaleMultiplier = GetScaleMultiplierForStage(stage);
        Vector3 newTargetScale = baseScale * scaleMultiplier;

        targetScale = newTargetScale;
        currentTargetScale = newTargetScale; // For debug display

        if (immediate || !smoothScaling)
        {
            // Apply immediately
            transform.localScale = targetScale;

            if (scaleFluffToo)
            {
                UpdateFluffScaling();
            }
        }

        if (debugMode)
        {
            Debug.Log($"[SpriteScaleController] {gameObject.name} - {stage} scale set to {scaleMultiplier:F2} (Target: {targetScale}, Immediate: {immediate})");
        }
    }

    /// <summary>
    /// Get scale multiplier for a specific life stage
    /// </summary>
    public float GetScaleMultiplierForStage(AgeLifeStageTracker.LifeStage stage)
    {
        switch (stage)
        {
            case AgeLifeStageTracker.LifeStage.Baby: return babyScale;
            case AgeLifeStageTracker.LifeStage.Child: return childScale;
            case AgeLifeStageTracker.LifeStage.Adult: return adultScale;
            case AgeLifeStageTracker.LifeStage.Elderly: return elderlyScale;
            default: return adultScale;
        }
    }

    /// <summary>
    /// Update fluff scaling if enabled
    /// </summary>
    private void UpdateFluffScaling()
    {
        float currentScaleRatio = transform.localScale.x / baseScale.x;
        float fluffScale = currentScaleRatio * fluffScaleMultiplier;
        Vector3 fluffScaleVector = Vector3.one * fluffScale;

        if (headFluffTransform != null)
        {
            headFluffTransform.localScale = fluffScaleVector;
        }

        if (bodyFluffTransform != null)
        {
            bodyFluffTransform.localScale = fluffScaleVector;
        }
    }

    // ========================================================================
    // PUBLIC INTERFACE - Called by AgentVisualController
    // ========================================================================

    /// <summary>
    /// Force immediate scale update - called by AgentVisualController
    /// </summary>
    public void ForceUpdateScale()
    {
        if (!enableScaling || lifeStageTracker == null) return;

        SetScaleForLifeStage(lifeStageTracker.CurrentStage, true); // Force immediate

        if (debugMode)
        {
            Debug.Log($"[SpriteScaleController] {gameObject.name} - Force scale update applied");
        }
    }

    /// <summary>
    /// Set custom scale multiplier - useful for special effects
    /// </summary>
    public void SetCustomScale(float multiplier, bool immediate = false)
    {
        if (!enableScaling) return;

        Vector3 customScale = baseScale * multiplier;
        targetScale = customScale;
        currentTargetScale = customScale;

        if (immediate || !smoothScaling)
        {
            transform.localScale = targetScale;

            if (scaleFluffToo)
            {
                UpdateFluffScaling();
            }
        }

        if (debugMode)
        {
            Debug.Log($"[SpriteScaleController] {gameObject.name} - Custom scale {multiplier:F2} applied");
        }
    }

    /// <summary>
    /// Reset to base scale
    /// </summary>
    public void ResetToBaseScale(bool immediate = false)
    {
        SetCustomScale(1.0f, immediate);
    }

    /// <summary>
    /// Get current scale multiplier
    /// </summary>
    public float GetCurrentScaleMultiplier()
    {
        return transform.localScale.x / baseScale.x;
    }

    /// <summary>
    /// Check if currently scaling (smooth transition in progress)
    /// </summary>
    public bool IsCurrentlyScaling()
    {
        return smoothScaling && Vector3.Distance(transform.localScale, targetScale) > 0.01f;
    }

    // ========================================================================
    // CONTEXT MENU DEBUG METHODS
    // ========================================================================

    [ContextMenu("?? Test Baby Scale")]
    public void TestBabyScale()
    {
        SetScaleForLifeStage(AgeLifeStageTracker.LifeStage.Baby, true);
        Debug.Log($"Testing Baby scale: {babyScale}");
    }

    [ContextMenu("?? Test Child Scale")]
    public void TestChildScale()
    {
        SetScaleForLifeStage(AgeLifeStageTracker.LifeStage.Child, true);
        Debug.Log($"Testing Child scale: {childScale}");
    }

    [ContextMenu("?? Test Adult Scale")]
    public void TestAdultScale()
    {
        SetScaleForLifeStage(AgeLifeStageTracker.LifeStage.Adult, true);
        Debug.Log($"Testing Adult scale: {adultScale}");
    }

    [ContextMenu("?? Test Elderly Scale")]
    public void TestElderlyScale()
    {
        SetScaleForLifeStage(AgeLifeStageTracker.LifeStage.Elderly, true);
        Debug.Log($"Testing Elderly scale: {elderlyScale}");
    }

    [ContextMenu("?? Test All Scales")]
    public void TestAllScales()
    {
        StartCoroutine(TestAllScalesCoroutine());
    }

    [ContextMenu("?? Force Update")]
    public void DebugForceUpdate()
    {
        ForceUpdateScale();
    }

    [ContextMenu("?? Log Current Status")]
    public void LogCurrentStatus()
    {
        Debug.Log($"[SpriteScaleController] {gameObject.name} Status:");
        Debug.Log($"  - Enabled: {enableScaling}");
        Debug.Log($"  - Base Scale: {baseScale}");
        Debug.Log($"  - Target Scale: {targetScale}");
        Debug.Log($"  - Actual Scale: {transform.localScale}");
        Debug.Log($"  - Life Stage: {currentLifeStage}");
        Debug.Log($"  - Is Scaling: {IsCurrentlyScaling()}");
        Debug.Log($"  - Scale Multiplier: {GetCurrentScaleMultiplier():F2}");
    }

    private System.Collections.IEnumerator TestAllScalesCoroutine()
    {
        var stages = new[] {
            AgeLifeStageTracker.LifeStage.Baby,
            AgeLifeStageTracker.LifeStage.Child,
            AgeLifeStageTracker.LifeStage.Adult,
            AgeLifeStageTracker.LifeStage.Elderly
        };

        foreach (var stage in stages)
        {
            Debug.Log($"Testing {stage}...");
            SetScaleForLifeStage(stage, true);
            yield return new WaitForSeconds(1f);
        }

        Debug.Log("Scale test complete!");
    }

    // ========================================================================
    // CLEANUP
    // ========================================================================

    void OnDestroy()
    {
        if (lifeStageTracker != null)
        {
            lifeStageTracker.OnLifeStageChanged -= OnLifeStageChanged;
        }
    }
}

/*
========================================================================
?? DEDICATED SPRITE SCALE CONTROLLER

? FEATURES:
?? Dedicated scaling logic separate from visual effects
?? Smooth or immediate scaling transitions  
?? Configurable scales for each life stage
?? Optional fluff scaling support
?? Public interface for AgentVisualController to call
?? Comprehensive debug tools and logging

?? USAGE:
1. Add SpriteScaleController to your agent GameObject
2. Configure scale values in inspector
3. AgentVisualController calls ForceUpdateScale() when needed
4. Automatically handles life stage changes

?? PUBLIC METHODS (for AgentVisualController):
- ForceUpdateScale() - Force immediate scale update
- SetCustomScale(float) - Set custom scale multiplier
- ResetToBaseScale() - Reset to original scale
- GetCurrentScaleMultiplier() - Get current scale ratio
- IsCurrentlyScaling() - Check if transition in progress

?? SCALE PROGRESSION:
Baby: 50% ? Child: 75% ? Adult: 100% ? Elderly: 90%

?? DEBUG TOOLS:
- Context menu test methods for each life stage
- "Test All Scales" cycles through all stages
- "Log Current Status" shows detailed info
- Toggle debug mode for console logging

RESULT: Clean, dedicated scaling system that AgentVisualController can easily use! ???
========================================================================
*/