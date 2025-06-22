// ============================================================================
// FILE: SheepLikeGeneticFluff.cs
// PURPOSE: Simplified sheep-style fluff with single fluffiness parameter
// ============================================================================

using UnityEngine;

/// <summary>
/// Simplified sheep-like fluff system with single fluffiness parameter
/// Controls shape from elliptical (0) to circular (1) with fluffy sine curves
/// </summary>
public class SheepLikeGeneticFluff : MonoBehaviour
{
    [Header("?? SHEEP FLUFF SETTINGS")]
    [SerializeField] private bool enableFluff = true;
    [SerializeField] private float baseFluffLevel = 0.5f;
    [SerializeField] private Color fluffColor = new Color(0.95f, 0.92f, 0.85f, 1f); // Cream white

    [Header("?? GENETICS")]
    [SerializeField] private string fluffGeneTraitName = "FluffLevel";
    [SerializeField] private float geneticInfluence = 0.8f;
    [SerializeField] private bool autoAddGeneticTrait = true;

    [Header("?? AGE EFFECTS")]
    [SerializeField] private bool useAgeEffects = true;
    [SerializeField] private float babyFluffMultiplier = 0.3f;
    [SerializeField] private float childFluffMultiplier = 0.6f;
    [SerializeField] private float adultFluffMultiplier = 1.0f;
    [SerializeField] private float elderlyFluffMultiplier = 0.8f;

    [Header("?? SHAPE SETTINGS")]
    [SerializeField] private Vector2 headFluffOffset = new Vector2(0f, 0.3f);
    [SerializeField] private Vector2 bodyFluffOffset = new Vector2(0f, -0.1f);
    [SerializeField] private float baseSize = 0.8f; // Base radius/size
    [SerializeField] private float maxSize = 1.5f; // Max size at full fluffiness

    [Header("?? FLUFFINESS CURVE SETTINGS")]
    [SerializeField, Range(4, 32)] private int curvaturePoints = 16; // Points around the edge
    [SerializeField, Range(0.2f, 1.0f)] private float maxCurvatureDepth = 0.6f; // Max sine curve depth (BIGGER!)
    [SerializeField, Range(4f, 12f)] private float curveFrequency = 8f; // Sine frequency multiplier (MORE WAVES!)
    [SerializeField, Range(0f, 0.3f)] private float randomness = 0.1f; // Random variation (LESS CHAOS!)

    [Header("?? DEBUG INFO")]
    [SerializeField] private float fluffiness; // THE main parameter (0-1)
    [SerializeField] private float currentFluffLevel; // For compatibility
    [SerializeField] private float currentSize;
    [SerializeField] private bool hasGenetics;
    [SerializeField] private float geneticValue;
    [SerializeField] private string currentLifeStage;

    // Components
    private SpriteRenderer agentSpriteRenderer;
    private SpriteRenderer headFluffRenderer;
    private SpriteRenderer bodyFluffRenderer;
    private GeneticsSystem geneticsSystem;
    private AgeLifeStageTracker lifeStageTracker;

    // Fluff sprites
    private Sprite[] headFluffSprites;
    private Sprite[] bodyFluffSprites;

    void Awake()
    {
        SetupComponents();
        SetupGenetics();
    }

    void Start()
    {
        CreateFluffSprites();
        Invoke(nameof(UpdateFluffiness), 0.1f);
    }

    void Update()
    {
        // Update fluff occasionally
        if (Time.frameCount % 60 == 0)
        {
            UpdateFluffiness();
        }
    }

    /// <summary>
    /// Setup head and body fluff renderers
    /// </summary>
    private void SetupComponents()
    {
        agentSpriteRenderer = GetComponent<SpriteRenderer>();
        if (agentSpriteRenderer == null)
        {
            Debug.LogWarning($"No SpriteRenderer on {gameObject.name}");
            return;
        }

        // Setup head fluff
        SetupFluffRenderer("HeadFluff", headFluffOffset, out headFluffRenderer, 1);
        // Setup body fluff  
        SetupFluffRenderer("BodyFluff", bodyFluffOffset, out bodyFluffRenderer, 2);

        // Get other components
        geneticsSystem = GetComponent<GeneticsSystem>();
        lifeStageTracker = GetComponent<AgeLifeStageTracker>();

        Debug.Log($"SheepLikeGeneticFluff setup for {gameObject.name}");
    }

    private void SetupFluffRenderer(string name, Vector2 offset, out SpriteRenderer renderer, int sortingOrder)
    {
        Transform fluffChild = transform.Find(name);
        GameObject fluffObject;

        if (fluffChild != null)
        {
            fluffObject = fluffChild.gameObject;
            renderer = fluffObject.GetComponent<SpriteRenderer>();
        }
        else
        {
            fluffObject = new GameObject(name);
            fluffObject.transform.SetParent(transform);
            renderer = fluffObject.AddComponent<SpriteRenderer>();
        }

        fluffObject.transform.localPosition = new Vector3(offset.x, offset.y, 0);
        renderer.sortingLayerName = agentSpriteRenderer.sortingLayerName;
        renderer.sortingOrder = agentSpriteRenderer.sortingOrder + sortingOrder;
    }

    /// <summary>
    /// Auto-setup genetics
    /// </summary>
    private void SetupGenetics()
    {
        if (!autoAddGeneticTrait) return;

        if (geneticsSystem != null && !geneticsSystem.Genome.HasTrait(fluffGeneTraitName))
        {
            GeneticTrait fluffTrait = new GeneticTrait(
                fluffGeneTraitName,
                Random.Range(0.2f, 0.9f),
                0f, 1f,
                0.15f, 0.25f
            );

            geneticsSystem.Genome.AddTrait(fluffTrait);
            Debug.Log($"Auto-added FluffLevel trait: {fluffTrait.value:F2}");
        }
    }

    /// <summary>
    /// Create fluff sprites with varying shapes and curvature
    /// </summary>
    private void CreateFluffSprites()
    {
        int levels = 6;
        headFluffSprites = new Sprite[levels];
        bodyFluffSprites = new Sprite[levels];

        for (int i = 0; i < levels; i++)
        {
            float levelFluffiness = i / (float)(levels - 1);
            headFluffSprites[i] = CreateFluffSprite(levelFluffiness, true); // Head is more circular
            bodyFluffSprites[i] = CreateFluffSprite(levelFluffiness, false); // Body is more elliptical
        }

        Debug.Log($"Created {levels} fluff sprites with varying shapes");
    }

    /// <summary>
    /// Create a single fluff sprite with shape morphing and sine curvature
    /// </summary>
    private Sprite CreateFluffSprite(float fluffiness, bool isHead)
    {
        if (fluffiness <= 0f) return null;

        int size = isHead ? 64 : 80;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        // Clear background
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }

        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float currentSize = Mathf.Lerp(baseSize, maxSize, fluffiness);
        float baseRadius = size * 0.35f * currentSize;

        // Calculate shape ratios based on fluffiness
        // At 0: very elliptical, At 1: more circular
        float aspectRatio = isHead ?
            Mathf.Lerp(1.0f, 1.0f, fluffiness) : // Head stays circular
            Mathf.Lerp(0.6f, 0.9f, fluffiness);  // Body becomes less elliptical

        float radiusX = baseRadius;
        float radiusY = baseRadius * aspectRatio;

        // Draw fluffy shape with sine curvature
        DrawFluffyShape(pixels, center, radiusX, radiusY, fluffiness, size);

        texture.SetPixels(pixels);
        texture.filterMode = FilterMode.Bilinear;
        texture.Apply();

        float pixelsPerUnit = size * (isHead ? 0.8f : 0.7f);
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }

    /// <summary>
    /// Draw fluffy shape with sine-based edge curvature
    /// </summary>
    private void DrawFluffyShape(Color[] pixels, Vector2 center, float radiusX, float radiusY, float fluffiness, int size)
    {
        // Curvature increases with fluffiness
        float curvatureDepth = maxCurvatureDepth * fluffiness;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 point = new Vector2(x - center.x, y - center.y);

                // Check if point is inside the fluffy shape
                if (IsInsideFluffyShape(point, radiusX, radiusY, curvatureDepth, fluffiness))
                {
                    Color fluffPixel = fluffColor;
                    fluffPixel.a = 1f;

                    int index = y * size + x;
                    if (index >= 0 && index < pixels.Length)
                    {
                        pixels[index] = fluffPixel;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Check if a point is inside the fluffy shape with sine curvature
    /// </summary>
    private bool IsInsideFluffyShape(Vector2 point, float radiusX, float radiusY, float curvatureDepth, float fluffiness)
    {
        // Basic ellipse test
        float ellipseDistance = (point.x * point.x) / (radiusX * radiusX) + (point.y * point.y) / (radiusY * radiusY);

        if (ellipseDistance > 1.5f) return false; // Far outside

        // Calculate angle from center
        float angle = Mathf.Atan2(point.y, point.x);

        // Generate PRONOUNCED sine-based curvature
        float sineValue = Mathf.Sin(angle * curveFrequency);

        // Add secondary wave for more complexity
        float secondaryWave = Mathf.Sin(angle * curveFrequency * 1.7f) * 0.3f;

        // Minimal randomness, seed based on angle for consistency
        float randomSeed = Mathf.Sin(angle * 13.7f) * 0.5f + 0.5f;
        float randomOffset = (randomSeed - 0.5f) * randomness;

        // Combine waves with strong curvature effect
        float totalCurvature = (sineValue + secondaryWave) * curvatureDepth + randomOffset * curvatureDepth * 0.2f;

        // Apply curvature more aggressively
        float curvatureMultiplier = 1.0f + fluffiness * 1.5f; // Stronger effect at high fluffiness
        totalCurvature *= curvatureMultiplier;

        // Adjust the boundary based on curvature
        float adjustedRadiusX = radiusX + totalCurvature;
        float adjustedRadiusY = radiusY + totalCurvature * 0.8f; // Slightly less variation in Y

        // Prevent negative radii
        adjustedRadiusX = Mathf.Max(adjustedRadiusX, radiusX * 0.3f);
        adjustedRadiusY = Mathf.Max(adjustedRadiusY, radiusY * 0.3f);

        // Recalculate distance with adjusted radii
        float adjustedDistance = (point.x * point.x) / (adjustedRadiusX * adjustedRadiusX) +
                                (point.y * point.y) / (adjustedRadiusY * adjustedRadiusY);

        // Less soft edge so waves are more visible
        float softEdge = 1.0f + (1f - fluffiness) * 0.1f; // Much less softening
        return adjustedDistance <= softEdge;
    }

    /// <summary>
    /// Update fluff based on genetics and age
    /// </summary>
    public void UpdateFluffiness()
    {
        if (!enableFluff)
        {
            if (headFluffRenderer != null) headFluffRenderer.sprite = null;
            if (bodyFluffRenderer != null) bodyFluffRenderer.sprite = null;
            return;
        }

        CalculateFluffLevel();
        ApplyFluffSprites();
        UpdateDebugInfo();
    }

    private void CalculateFluffLevel()
    {
        currentFluffLevel = baseFluffLevel;

        // Apply genetics
        if (geneticsSystem != null)
        {
            float geneticFluff = geneticsSystem.GetTraitValue(fluffGeneTraitName, 0.5f);
            currentFluffLevel = Mathf.Lerp(baseFluffLevel, 1f, geneticFluff * geneticInfluence);
        }

        // Apply age effects
        if (useAgeEffects && lifeStageTracker != null)
        {
            float ageMultiplier = GetAgeMultiplier();
            currentFluffLevel *= ageMultiplier;
        }

        currentFluffLevel = Mathf.Clamp01(currentFluffLevel);

        // THIS IS THE MAIN FLUFFINESS PARAMETER!
        fluffiness = currentFluffLevel;

        currentSize = Mathf.Lerp(baseSize, maxSize, fluffiness);
    }

    private float GetAgeMultiplier()
    {
        switch (lifeStageTracker.CurrentStage)
        {
            case AgeLifeStageTracker.LifeStage.Baby: return babyFluffMultiplier;
            case AgeLifeStageTracker.LifeStage.Child: return childFluffMultiplier;
            case AgeLifeStageTracker.LifeStage.Adult: return adultFluffMultiplier;
            case AgeLifeStageTracker.LifeStage.Elderly: return elderlyFluffMultiplier;
            default: return 1f;
        }
    }

    private void ApplyFluffSprites()
    {
        if (headFluffSprites == null || bodyFluffSprites == null) return;

        int spriteIndex = Mathf.RoundToInt(fluffiness * (headFluffSprites.Length - 1));
        spriteIndex = Mathf.Clamp(spriteIndex, 0, headFluffSprites.Length - 1);

        // Apply head fluff
        if (headFluffRenderer != null)
        {
            headFluffRenderer.sprite = headFluffSprites[spriteIndex];
            SetFluffColor(headFluffRenderer);
        }

        // Apply body fluff
        if (bodyFluffRenderer != null)
        {
            bodyFluffRenderer.sprite = bodyFluffSprites[spriteIndex];
            SetFluffColor(bodyFluffRenderer);
        }

        Debug.Log($"Applied fluff: fluffiness={fluffiness:F2}, size={currentSize:F2}");
    }

    private void SetFluffColor(SpriteRenderer renderer)
    {
        // Check if AgentVisualController is managing colors
        AgentVisualController visualController = GetComponent<AgentVisualController>();
        if (visualController != null && visualController.controlFluffColors)
        {
            return; // Let AgentVisualController handle colors
        }

        // Set our own color
        Color color = fluffColor;
        if (geneticsSystem != null)
        {
            color = Color.Lerp(fluffColor, geneticsSystem.BaseColor, 0.1f);
        }
        color.a = 1f;
        renderer.color = color;
    }

    private void UpdateDebugInfo()
    {
        hasGenetics = geneticsSystem != null;
        geneticValue = hasGenetics ? geneticsSystem.GetTraitValue(fluffGeneTraitName, 0.5f) : 0f;
        currentLifeStage = lifeStageTracker?.CurrentStage.ToString() ?? "None";
    }

    // ========================================================================
    // PUBLIC METHODS (Keep same interface for compatibility)
    // ========================================================================

    [ContextMenu("Regenerate Sheep Fluff")]
    public void RegenerateFluff()
    {
        CreateFluffSprites();
        UpdateFluffiness();
    }

    [ContextMenu("Test Maximum Fluff")]
    public void TestMaximumFluff()
    {
        if (geneticsSystem != null)
        {
            geneticsSystem.SetTraitValue(fluffGeneTraitName, 1.0f);
        }
        UpdateFluffiness();
    }

    [ContextMenu("Test Minimum Fluff")]
    public void TestMinimumFluff()
    {
        if (geneticsSystem != null)
        {
            geneticsSystem.SetTraitValue(fluffGeneTraitName, 0.0f);
        }
        UpdateFluffiness();
    }

    public float GetCurrentFluffLevel() => fluffiness; // Return the main fluffiness parameter

    void OnDestroy()
    {
        // Clean up textures
        CleanupSprites(headFluffSprites);
        CleanupSprites(bodyFluffSprites);
    }

    private void CleanupSprites(Sprite[] sprites)
    {
        if (sprites == null) return;

        foreach (var sprite in sprites)
        {
            if (sprite != null && sprite.texture != null)
                DestroyImmediate(sprite.texture);
        }
    }
}

/*
========================================================================
?? SIMPLIFIED SHEEP-LIKE GENETIC FLUFF SYSTEM

SINGLE FLUFFINESS PARAMETER (0.0 to 1.0):
?? SHAPE MORPHING:
   • 0.0: Slim, very elliptical shapes
   • 1.0: More circular/rounded shapes (especially body)

?? FLUFFY EDGES:
   • Sine-based curvature around the periphery
   • Curvature depth increases with fluffiness
   • Random variation for natural fluffy look
   • More curves = more fluffy appearance

?? GENETICS: "FluffLevel" trait controls everything
?? AGE EFFECTS: Babies less fluffy, adults full fluffy
?? COLORS: Coordinates with AgentVisualController

VISUAL PROGRESSION:
0.0 ? Slim elliptical shapes with smooth edges
0.5 ? Medium rounded shapes with some fluffiness  
1.0 ? Rounder shapes with deep fluffy sine curves

RESULT: Sheep that look progressively fluffier and rounder! ???
========================================================================
*/