// ============================================================================
// FILE: SheepLikeGeneticFluff.cs
// PURPOSE: Sheep-style fluff with separate head and body clouds
// ============================================================================

using UnityEngine;

/// <summary>
/// Sheep-like fluff system with separate head and body fluff clouds
/// Fluff level gene controls the size and density of both clouds
/// </summary>
public class SheepLikeGeneticFluff : MonoBehaviour
{
    [Header("?? SHEEP FLUFF SETTINGS")]
    [SerializeField] private bool enableFluff = true;
    [SerializeField] private float baseFluffLevel = 0.5f;
    [SerializeField] private Color fluffColor = new Color(0.95f, 0.92f, 0.85f, 1f); // Cream white, NO ALPHA

    [Header("?? GENETICS")]
    [SerializeField] private string fluffGeneTraitName = "FluffLevel";
    [SerializeField] private float geneticInfluence = 0.8f;
    [SerializeField] private bool autoAddGeneticTrait = true;

    [Header("?? AGE EFFECTS")]
    [SerializeField] private bool useAgeEffects = true;
    [SerializeField] private float babyFluffMultiplier = 0.3f; // Babies have less fluff
    [SerializeField] private float childFluffMultiplier = 0.6f;
    [SerializeField] private float adultFluffMultiplier = 1.0f; // Full fluff
    [SerializeField] private float elderlyFluffMultiplier = 0.8f;

    [Header("?? HEAD FLUFF SETTINGS")]
    [SerializeField] private Vector2 headFluffOffset = new Vector2(0f, 0.3f); // Offset from agent center
    [SerializeField] private float headFluffBaseSize = 0.8f; // Base size multiplier
    [SerializeField] private float headFluffMaxSize = 1.5f; // Max size at full genetics
    [SerializeField] private int headFluffDensity = 6; // Cloud density

    [Header("?? BODY FLUFF SETTINGS")]
    [SerializeField] private Vector2 bodyFluffOffset = new Vector2(0f, -0.1f); // Slightly below center
    [SerializeField] private Vector2 bodyFluffBaseSize = new Vector2(1.0f, 0.7f); // Base ellipse size (width, height)
    [SerializeField] private Vector2 bodyFluffMaxSize = new Vector2(1.8f, 1.2f); // Max ellipse size
    [SerializeField] private int bodyFluffDensity = 8; // Cloud density

    [Header("?? CLOUD TEXTURE SETTINGS")]
    [SerializeField, Range(0.05f, 0.3f)] private float cloudNoiseScale = 0.12f;
    [SerializeField, Range(0.3f, 1.0f)] private float cloudNoiseStrength = 0.7f;
    [SerializeField, Range(2, 6)] private int cloudLayers = 4;
    [SerializeField, Range(1f, 3f)] private float cloudSoftness = 2f;

    [Header("?? DEBUG INFO")]
    [SerializeField] private float currentFluffLevel;
    [SerializeField] private float headFluffSize;
    [SerializeField] private Vector2 bodyFluffSize;
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
        CreateSheepFluffSprites();
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

        // Create or find head fluff
        Transform headFluffChild = transform.Find("HeadFluff");
        GameObject headFluffObject;

        if (headFluffChild != null)
        {
            headFluffObject = headFluffChild.gameObject;
            headFluffRenderer = headFluffObject.GetComponent<SpriteRenderer>();
        }
        else
        {
            headFluffObject = new GameObject("HeadFluff");
            headFluffObject.transform.SetParent(transform);
            headFluffRenderer = headFluffObject.AddComponent<SpriteRenderer>();
        }

        // Position and setup head fluff
        headFluffObject.transform.localPosition = new Vector3(headFluffOffset.x, headFluffOffset.y, 0);
        headFluffRenderer.sortingLayerName = agentSpriteRenderer.sortingLayerName;
        headFluffRenderer.sortingOrder = agentSpriteRenderer.sortingOrder + 1; // ON TOP of agent

        // Create or find body fluff
        Transform bodyFluffChild = transform.Find("BodyFluff");
        GameObject bodyFluffObject;

        if (bodyFluffChild != null)
        {
            bodyFluffObject = bodyFluffChild.gameObject;
            bodyFluffRenderer = bodyFluffObject.GetComponent<SpriteRenderer>();
        }
        else
        {
            bodyFluffObject = new GameObject("BodyFluff");
            bodyFluffObject.transform.SetParent(transform);
            bodyFluffRenderer = bodyFluffObject.AddComponent<SpriteRenderer>();
        }

        // Position and setup body fluff
        bodyFluffObject.transform.localPosition = new Vector3(bodyFluffOffset.x, bodyFluffOffset.y, 0);
        bodyFluffRenderer.sortingLayerName = agentSpriteRenderer.sortingLayerName;
        bodyFluffRenderer.sortingOrder = agentSpriteRenderer.sortingOrder + 2; // ON TOP of head fluff

        // Get other components
        geneticsSystem = GetComponent<GeneticsSystem>();
        lifeStageTracker = GetComponent<AgeLifeStageTracker>();

        Debug.Log($"SheepLikeGeneticFluff setup for {gameObject.name}");
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
                Random.Range(0.2f, 0.9f), // Wide range for variety
                0f, 1f,
                0.15f, 0.25f // Higher mutation for more variety
            );

            geneticsSystem.Genome.AddTrait(fluffTrait);
            Debug.Log($"Auto-added FluffLevel trait: {fluffTrait.value:F2}");
        }
    }

    /// <summary>
    /// Create separate head and body fluff sprites
    /// </summary>
    private void CreateSheepFluffSprites()
    {
        int levels = 6;
        headFluffSprites = new Sprite[levels];
        bodyFluffSprites = new Sprite[levels];

        for (int i = 0; i < levels; i++)
        {
            float fluffLevel = i / (float)(levels - 1);
            headFluffSprites[i] = CreateHeadFluffSprite(fluffLevel);
            bodyFluffSprites[i] = CreateBodyFluffSprite(fluffLevel);
        }

        Debug.Log($"Created {levels} head and body fluff sprites");
    }

    /// <summary>
    /// Create circular head fluff sprite
    /// </summary>
    private Sprite CreateHeadFluffSprite(float fluffLevel)
    {
        if (fluffLevel <= 0f) return null;

        int size = 64;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        // Clear background
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }

        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float currentSize = Mathf.Lerp(headFluffBaseSize, headFluffMaxSize, fluffLevel);
        float radius = size * 0.4f * currentSize;

        // Draw cloudy circle for head
        DrawCloudyCircle(pixels, center, radius, fluffLevel, size, headFluffDensity);

        texture.SetPixels(pixels);
        texture.filterMode = FilterMode.Bilinear;
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size * 0.8f);
    }

    /// <summary>
    /// Create elliptical body fluff sprite
    /// </summary>
    private Sprite CreateBodyFluffSprite(float fluffLevel)
    {
        if (fluffLevel <= 0f) return null;

        int size = 80; // Slightly bigger for body
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        // Clear background
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }

        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);

        // Calculate current ellipse size
        Vector2 currentSize = Vector2.Lerp(bodyFluffBaseSize, bodyFluffMaxSize, fluffLevel);
        float radiusX = size * 0.35f * currentSize.x;
        float radiusY = size * 0.35f * currentSize.y;

        // Draw cloudy ellipse for body
        DrawCloudyEllipse(pixels, center, radiusX, radiusY, fluffLevel, size, bodyFluffDensity);

        texture.SetPixels(pixels);
        texture.filterMode = FilterMode.Bilinear;
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size * 0.7f);
    }

    /// <summary>
    /// Draw cloudy circular pattern
    /// </summary>
    private void DrawCloudyCircle(Color[] pixels, Vector2 center, float radius, float fluffLevel, int size, int density)
    {
        int centerX = Mathf.RoundToInt(center.x);
        int centerY = Mathf.RoundToInt(center.y);
        int r = Mathf.RoundToInt(radius);

        for (int y = -r - 10; y <= r + 10; y++)
        {
            for (int x = -r - 10; x <= r + 10; x++)
            {
                int px = centerX + x;
                int py = centerY + y;

                if (px >= 0 && px < size && py >= 0 && py < size)
                {
                    float distance = Mathf.Sqrt(x * x + y * y);

                    // Create soft circular boundary
                    float edgeAlpha = 1f - Mathf.Clamp01((distance - radius) / (cloudSoftness * fluffLevel + 1f));

                    if (edgeAlpha > 0.1f)
                    {
                        // Multi-layer cloud noise
                        float cloudValue = GetMultiLayerCloudNoise(px, py, density);

                        // FULL OPACITY - no alpha blending
                        float finalAlpha = edgeAlpha * cloudValue * fluffLevel;

                        if (finalAlpha > 0.3f) // Only draw substantial cloud parts
                        {
                            Color cloudPixel = fluffColor; // Already has alpha = 1
                            cloudPixel.a = 1f; // FORCE full opacity

                            int index = py * size + px;
                            if (index >= 0 && index < pixels.Length)
                            {
                                // Simple replacement - no alpha blending
                                pixels[index] = cloudPixel;
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Draw cloudy elliptical pattern
    /// </summary>
    private void DrawCloudyEllipse(Color[] pixels, Vector2 center, float radiusX, float radiusY, float fluffLevel, int size, int density)
    {
        int centerX = Mathf.RoundToInt(center.x);
        int centerY = Mathf.RoundToInt(center.y);
        int rX = Mathf.RoundToInt(radiusX);
        int rY = Mathf.RoundToInt(radiusY);

        for (int y = -rY - 10; y <= rY + 10; y++)
        {
            for (int x = -rX - 10; x <= rX + 10; x++)
            {
                int px = centerX + x;
                int py = centerY + y;

                if (px >= 0 && px < size && py >= 0 && py < size)
                {
                    // Ellipse distance calculation
                    float ellipseDistance = (x * x) / (float)(rX * rX) + (y * y) / (float)(rY * rY);

                    // Create soft elliptical boundary
                    float edgeAlpha = 1f - Mathf.Clamp01((ellipseDistance - 1f) / (cloudSoftness * fluffLevel * 0.5f + 0.5f));

                    if (edgeAlpha > 0.1f)
                    {
                        // Multi-layer cloud noise
                        float cloudValue = GetMultiLayerCloudNoise(px, py, density);

                        // FULL OPACITY
                        float finalAlpha = edgeAlpha * cloudValue * fluffLevel;

                        if (finalAlpha > 0.3f) // Only draw substantial cloud parts
                        {
                            Color cloudPixel = fluffColor; // Full opacity color
                            cloudPixel.a = 1f; // FORCE full opacity

                            int index = py * size + px;
                            if (index >= 0 && index < pixels.Length)
                            {
                                // Simple replacement - no alpha blending
                                pixels[index] = cloudPixel;
                            }
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Generate multi-layer cloud noise
    /// </summary>
    private float GetMultiLayerCloudNoise(float x, float y, int density)
    {
        float cloudNoise = 0f;
        float totalWeight = 0f;

        for (int layer = 0; layer < cloudLayers; layer++)
        {
            float scale = cloudNoiseScale * (1f + layer * 0.3f);
            float weight = 1f / (layer + 1);

            float layerNoise = Mathf.PerlinNoise(x * scale, y * scale);
            cloudNoise += layerNoise * weight;
            totalWeight += weight;
        }

        cloudNoise /= totalWeight;

        // Enhance contrast for more defined clouds
        cloudNoise = Mathf.Pow(cloudNoise, 1.2f);

        // Apply noise strength
        return Mathf.Lerp(0.4f, 1f, cloudNoise * cloudNoiseStrength);
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
            float ageMultiplier = 1f;
            switch (lifeStageTracker.CurrentStage)
            {
                case AgeLifeStageTracker.LifeStage.Baby:
                    ageMultiplier = babyFluffMultiplier;
                    break;
                case AgeLifeStageTracker.LifeStage.Child:
                    ageMultiplier = childFluffMultiplier;
                    break;
                case AgeLifeStageTracker.LifeStage.Adult:
                    ageMultiplier = adultFluffMultiplier;
                    break;
                case AgeLifeStageTracker.LifeStage.Elderly:
                    ageMultiplier = elderlyFluffMultiplier;
                    break;
            }
            currentFluffLevel *= ageMultiplier;
        }

        currentFluffLevel = Mathf.Clamp01(currentFluffLevel);

        // Calculate debug sizes
        headFluffSize = Mathf.Lerp(headFluffBaseSize, headFluffMaxSize, currentFluffLevel);
        bodyFluffSize = Vector2.Lerp(bodyFluffBaseSize, bodyFluffMaxSize, currentFluffLevel);
    }

private void ApplyFluffSprites()
{
    if (headFluffSprites == null || bodyFluffSprites == null) return;

    // Choose sprites based on fluff level
    int spriteIndex = Mathf.RoundToInt(currentFluffLevel * (headFluffSprites.Length - 1));
    spriteIndex = Mathf.Clamp(spriteIndex, 0, headFluffSprites.Length - 1);

    // Apply head fluff
    if (headFluffRenderer != null)
    {
        headFluffRenderer.sprite = headFluffSprites[spriteIndex];

        // CHECK: If AgentVisualController is managing colors, DON'T override them
        AgentVisualController visualController = GetComponent<AgentVisualController>();
        if (visualController == null || !visualController.controlFluffColors)
        {
            // Only set color if AgentVisualController isn't controlling it
            Color headColor = fluffColor;
            if (geneticsSystem != null)
            {
                headColor = Color.Lerp(fluffColor, geneticsSystem.BaseColor, 0.15f);
                headColor.a = 1f; // Keep full opacity
            }
            headFluffRenderer.color = headColor;
        }
    }

    // Apply body fluff
    if (bodyFluffRenderer != null)
    {
        bodyFluffRenderer.sprite = bodyFluffSprites[spriteIndex];

        // CHECK: If AgentVisualController is managing colors, DON'T override them
        AgentVisualController visualController = GetComponent<AgentVisualController>();
        if (visualController == null || !visualController.controlFluffColors)
        {
            // Only set color if AgentVisualController isn't controlling it
            Color bodyColor = fluffColor;
            if (geneticsSystem != null)
            {
                bodyColor = Color.Lerp(fluffColor, geneticsSystem.BaseColor, 0.1f);
                bodyColor.a = 1f; // Keep full opacity
            }
            bodyFluffRenderer.color = bodyColor;
        }
    }

    Debug.Log($"Applied sheep fluff: level={currentFluffLevel:F2}, head={headFluffSize:F2}, body={bodyFluffSize}");
}
    private void UpdateDebugInfo()
    {
        hasGenetics = geneticsSystem != null;
        geneticValue = hasGenetics ? geneticsSystem.GetTraitValue(fluffGeneTraitName, 0.5f) : 0f;
        currentLifeStage = lifeStageTracker?.CurrentStage.ToString() ?? "None";
    }

    // ========================================================================
    // PUBLIC METHODS
    // ========================================================================

    [ContextMenu("Regenerate Sheep Fluff")]
    public void RegenerateFluff()
    {
        CreateSheepFluffSprites();
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

    public float GetCurrentFluffLevel() => currentFluffLevel;

    void OnDestroy()
    {
        // Clean up textures
        if (headFluffSprites != null)
        {
            foreach (var sprite in headFluffSprites)
            {
                if (sprite != null && sprite.texture != null)
                    DestroyImmediate(sprite.texture);
            }
        }

        if (bodyFluffSprites != null)
        {
            foreach (var sprite in bodyFluffSprites)
            {
                if (sprite != null && sprite.texture != null)
                    DestroyImmediate(sprite.texture);
            }
        }
    }
}

/*
========================================================================
?? SHEEP-LIKE GENETIC FLUFF SYSTEM

STRUCTURE:
? HEAD FLUFF - Circular cloud above the agent (ON TOP sorting order)
? BODY FLUFF - Elliptical cloud around the agent body (ON TOP of head)
? FULL OPACITY - No alpha blending, solid cloud colors
? GENETICS - "FluffLevel" trait controls size of both clouds
? AGE EFFECTS - Babies have less fluff, adults have full fluff

LAYERING (front to back):
1. Body Fluff (topmost layer)
2. Head Fluff (middle layer)  
3. Agent Sprite (bottom layer)

HOW TO USE:
1. Add SheepLikeGeneticFluff component to your agent prefab
2. Adjust settings in inspector
3. Fluff will automatically appear behind your agent sprite

SETTINGS TO PLAY WITH:
- Base Fluff Level: Starting fluffiness
- Fluff Color: Color of the clouds (no alpha!)
- Head/Body Fluff Offsets: Position relative to agent
- Head/Body Fluff Sizes: Min and max sizes
- Cloud settings: Noise scale, layers, softness

GENETICS:
- "FluffLevel" trait (0.0 to 1.0)
- 0.0 = No fluff (just the agent sprite)
- 1.0 = Maximum fluffy sheep-like appearance
- Inherited from parents with mutations

VISUAL RESULT:
Your agents will look like fluffy sheep with varying amounts of 
head and body fluff based on their genetics! ???

No more alpha issues - solid, visible fluff clouds!
========================================================================
*/