// ============================================================================
// FILE: TexturedGeneticFluff.cs
// PURPOSE: Highly textured genetic fluff - Add directly to prefab!
// ============================================================================

using UnityEngine;

/// <summary>
/// Textured genetic fluff system - Add this directly to your agent prefab!
/// Play with all the settings in the inspector for different fluff styles
/// </summary>
public class TexturedGeneticFluff : MonoBehaviour
{
    [Header("🐹 BASIC FLUFF SETTINGS")]
    [SerializeField] private bool enableFluff = true;
    [SerializeField] private float baseFluffiness = 0.5f;
    [SerializeField] private Color fluffColor = new Color(0.9f, 0.7f, 0.5f, 0.8f);
    [SerializeField] private Vector3 fluffScale = Vector3.one;

    [Header("🧬 GENETICS")]
    [SerializeField] private string fluffGeneTraitName = "FluffAmount";
    [SerializeField] private float geneticInfluence = 0.8f;
    [SerializeField] private bool autoAddGeneticTrait = true; // Auto-setup genetics

    [Header("👶 AGE EFFECTS")]
    [SerializeField] private bool useAgeEffects = true;
    [SerializeField] private float babyFluffMultiplier = 1.8f; // Extra fluffy babies!
    [SerializeField] private float childFluffMultiplier = 1.3f;
    [SerializeField] private float adultFluffMultiplier = 1.0f;
    [SerializeField] private float elderlyFluffMultiplier = 0.6f;

    [Header("✨ TEXTURE SETTINGS - PLAY WITH THESE!")]
    [SerializeField] private FluffTextureType textureType = FluffTextureType.Wispy;
    [SerializeField, Range(0.4f, 1.0f)] private float minAlpha = 0.6f;
    [SerializeField, Range(0.7f, 1.0f)] private float maxAlpha = 0.9f;
    [SerializeField, Range(1.0f, 2.0f)] private float contrast = 1.3f;
    [SerializeField, Range(3, 12)] private int textureRings = 8;
    [SerializeField, Range(0.5f, 2.0f)] private float fluffSize = 1.2f;

    [Header("🎨 TEXTURE DETAILS")]
    [SerializeField, Range(0.02f, 0.2f)] private float noiseScale = 0.08f;
    [SerializeField, Range(0.3f, 1.0f)] private float noiseStrength = 0.6f;
    [SerializeField, Range(2f, 8f)] private float edgeSoftness = 4f;
    [SerializeField, Range(0.1f, 0.4f)] private float ringFade = 0.12f;
    [SerializeField] private bool usePerlinNoise = true;
    [SerializeField] private bool useFractalNoise = false;

    [Header("🌪️ WISPY EFFECTS (for Wispy texture)")]
    [SerializeField, Range(0.0f, 1.0f)] private float wispiness = 0.7f;
    [SerializeField, Range(0.1f, 0.5f)] private float wispScale = 0.2f;
    [SerializeField, Range(0.5f, 2.0f)] private float wispStretch = 1.2f;

    [Header("💨 CLOUD EFFECTS (for Cloudy texture)")]
    [SerializeField, Range(0.0f, 1.0f)] private float cloudiness = 0.8f;
    [SerializeField, Range(0.05f, 0.3f)] private float cloudScale = 0.15f;
    [SerializeField, Range(2, 6)] private int cloudLayers = 3;

    [Header("🌟 SPARKLY EFFECTS (for Sparkly texture)")]
    [SerializeField, Range(0.0f, 1.0f)] private float sparkle = 0.5f;
    [SerializeField, Range(0.8f, 1.2f)] private float sparkleContrast = 1.1f;
    [SerializeField, Range(0.3f, 0.8f)] private float sparkleThreshold = 0.6f;

    [Header("📊 DEBUG INFO")]
    [SerializeField] private float currentFluffiness;
    [SerializeField] private float finalAlpha;
    [SerializeField] private bool hasGenetics;
    [SerializeField] private float geneticValue;
    [SerializeField] private string currentLifeStage;

    public enum FluffTextureType
    {
        Wispy,     // Light, airy fluff with directional wisps
        Cloudy,    // Dense, cloud-like fluff
        Sparkly,   // Glittery, magical fluff
        Fuzzy,     // Dense, fur-like texture
        Cottony    // Soft, cotton ball texture
    }

    // Components
    private SpriteRenderer agentSpriteRenderer;
    private SpriteRenderer fluffSpriteRenderer;
    private GeneticsSystem geneticsSystem;
    private AgeLifeStageTracker lifeStageTracker;

    // Texture cache
    private Sprite[] fluffSprites;
    private FluffTextureType lastTextureType;
    private float lastNoiseScale;

    void Awake()
    {
        SetupComponents();
        SetupGenetics();
    }

    void Start()
    {
        CreateTexturedFluffSprites();
        Invoke(nameof(UpdateFluffiness), 0.1f);
    }

    void Update()
    {
        // Check if we need to regenerate textures
        if (textureType != lastTextureType || Mathf.Abs(noiseScale - lastNoiseScale) > 0.01f)
        {
            CreateTexturedFluffSprites();
            UpdateFluffiness();
            lastTextureType = textureType;
            lastNoiseScale = noiseScale;
        }

        // Regular updates
        if (Time.frameCount % 60 == 0)
        {
            UpdateFluffiness();
        }
    }

    /// <summary>
    /// Setup components automatically
    /// </summary>
    private void SetupComponents()
    {
        agentSpriteRenderer = GetComponent<SpriteRenderer>();
        if (agentSpriteRenderer == null)
        {
            Debug.LogWarning($"No SpriteRenderer on {gameObject.name}");
            return;
        }

        // Create or find fluff renderer
        Transform fluffChild = transform.Find("TexturedFluffOverlay");
        GameObject fluffObject;

        if (fluffChild != null)
        {
            fluffObject = fluffChild.gameObject;
            fluffSpriteRenderer = fluffObject.GetComponent<SpriteRenderer>();
        }
        else
        {
            fluffObject = new GameObject("TexturedFluffOverlay");
            fluffObject.transform.SetParent(transform);
            fluffObject.transform.localPosition = Vector3.zero;
            fluffSpriteRenderer = fluffObject.AddComponent<SpriteRenderer>();
        }

        // Setup fluff renderer
        fluffObject.transform.localScale = fluffScale;
        fluffSpriteRenderer.sortingLayerName = agentSpriteRenderer.sortingLayerName;
        fluffSpriteRenderer.sortingOrder = agentSpriteRenderer.sortingOrder + 1;

        // Get other components
        geneticsSystem = GetComponent<GeneticsSystem>();
        lifeStageTracker = GetComponent<AgeLifeStageTracker>();

        Debug.Log($"TexturedGeneticFluff setup for {gameObject.name}");
    }

    /// <summary>
    /// Auto-setup genetics if needed
    /// </summary>
    private void SetupGenetics()
    {
        if (!autoAddGeneticTrait) return;

        if (geneticsSystem != null && !geneticsSystem.Genome.HasTrait(fluffGeneTraitName))
        {
            GeneticTrait fluffTrait = new GeneticTrait(
                fluffGeneTraitName,
                Random.Range(0.3f, 0.8f),
                0f, 1f,
                0.1f, 0.2f
            );

            geneticsSystem.Genome.AddTrait(fluffTrait);
            Debug.Log($"Auto-added FluffAmount trait: {fluffTrait.value:F2}");
        }
    }

    /// <summary>
    /// Create textured fluff sprites with different styles
    /// </summary>
    private void CreateTexturedFluffSprites()
    {
        int levels = 6; // More levels for smoother transitions
        fluffSprites = new Sprite[levels];

        for (int i = 0; i < levels; i++)
        {
            float fluffAmount = i / (float)(levels - 1);
            fluffSprites[i] = CreateTexturedSprite(fluffAmount);
        }

        Debug.Log($"Created {levels} textured fluff sprites ({textureType})");
    }

    /// <summary>
    /// Create a single textured fluff sprite
    /// </summary>
    private Sprite CreateTexturedSprite(float fluffAmount)
    {
        if (fluffAmount <= 0f) return null;

        int size = 80; // Bigger for more detail
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        // Clear background
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }

        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float baseRadius = size * 0.4f * fluffSize;

        // Generate texture based on type
        switch (textureType)
        {
            case FluffTextureType.Wispy:
                DrawWispyTexture(pixels, center, baseRadius, fluffAmount, size);
                break;
            case FluffTextureType.Cloudy:
                DrawCloudyTexture(pixels, center, baseRadius, fluffAmount, size);
                break;
            case FluffTextureType.Sparkly:
                DrawSparklyTexture(pixels, center, baseRadius, fluffAmount, size);
                break;
            case FluffTextureType.Fuzzy:
                DrawFuzzyTexture(pixels, center, baseRadius, fluffAmount, size);
                break;
            case FluffTextureType.Cottony:
                DrawCottonyTexture(pixels, center, baseRadius, fluffAmount, size);
                break;
        }

        texture.SetPixels(pixels);
        texture.filterMode = FilterMode.Bilinear;
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size * 0.8f);
    }

    /// <summary>
    /// Draw wispy, directional fluff texture
    /// </summary>
    private void DrawWispyTexture(Color[] pixels, Vector2 center, float radius, float fluffAmount, int size)
    {
        int rings = Mathf.RoundToInt(textureRings * fluffAmount);

        for (int ring = 0; ring < rings; ring++)
        {
            float ringRadius = radius * (0.6f + 0.6f * ring / rings);
            float alpha = CalculateRingAlpha(ring, rings, fluffAmount);

            DrawTexturedRing(pixels, center, ringRadius, alpha, size, (px, py) => {
                // Wispy noise with directional bias
                float noise1 = GetNoise(px * noiseScale, py * noiseScale * wispStretch);
                float noise2 = GetNoise(px * noiseScale * 2f, py * noiseScale * 0.5f);

                float wispNoise = Mathf.Lerp(noise1, noise2, wispiness);
                return Mathf.Lerp(0.7f, 1f, wispNoise * noiseStrength);
            });
        }
    }

    /// <summary>
    /// Draw dense, cloud-like texture
    /// </summary>
    private void DrawCloudyTexture(Color[] pixels, Vector2 center, float radius, float fluffAmount, int size)
    {
        int rings = Mathf.RoundToInt(textureRings * fluffAmount);

        for (int ring = 0; ring < rings; ring++)
        {
            float ringRadius = radius * (0.7f + 0.5f * ring / rings);
            float alpha = CalculateRingAlpha(ring, rings, fluffAmount);

            DrawTexturedRing(pixels, center, ringRadius, alpha, size, (px, py) => {
                float cloudNoise = 0f;

                // Multiple cloud layers
                for (int layer = 0; layer < cloudLayers; layer++)
                {
                    float scale = cloudScale * (1f + layer * 0.5f);
                    float weight = 1f / (layer + 1);
                    cloudNoise += GetNoise(px * scale, py * scale) * weight;
                }

                cloudNoise = Mathf.Lerp(0.6f, 1f, cloudNoise * cloudiness);
                return cloudNoise;
            });
        }
    }

    /// <summary>
    /// Draw sparkly, glittery texture
    /// </summary>
    private void DrawSparklyTexture(Color[] pixels, Vector2 center, float radius, float fluffAmount, int size)
    {
        int rings = Mathf.RoundToInt(textureRings * fluffAmount);

        for (int ring = 0; ring < rings; ring++)
        {
            float ringRadius = radius * (0.65f + 0.6f * ring / rings);
            float alpha = CalculateRingAlpha(ring, rings, fluffAmount);

            DrawTexturedRing(pixels, center, ringRadius, alpha, size, (px, py) => {
                float noise = GetNoise(px * noiseScale * 3f, py * noiseScale * 3f);

                // Create sparkle effect
                if (noise > sparkleThreshold)
                {
                    return Mathf.Lerp(1f, sparkleContrast, sparkle);
                }
                else
                {
                    return Mathf.Lerp(0.5f, 0.8f, noise * noiseStrength);
                }
            });
        }
    }

    /// <summary>
    /// Draw dense, fur-like texture
    /// </summary>
    private void DrawFuzzyTexture(Color[] pixels, Vector2 center, float radius, float fluffAmount, int size)
    {
        int rings = Mathf.RoundToInt(textureRings * fluffAmount * 1.2f); // More rings for density

        for (int ring = 0; ring < rings; ring++)
        {
            float ringRadius = radius * (0.8f + 0.4f * ring / rings);
            float alpha = CalculateRingAlpha(ring, rings, fluffAmount);

            DrawTexturedRing(pixels, center, ringRadius, alpha, size, (px, py) => {
                // Dense, fine-grained noise
                float noise1 = GetNoise(px * noiseScale * 4f, py * noiseScale * 4f);
                float noise2 = GetNoise(px * noiseScale * 8f, py * noiseScale * 8f);

                float fuzzyNoise = (noise1 * 0.7f + noise2 * 0.3f);
                return Mathf.Lerp(0.8f, 1f, fuzzyNoise * noiseStrength);
            });
        }
    }

    /// <summary>
    /// Draw soft, cotton-like texture
    /// </summary>
    private void DrawCottonyTexture(Color[] pixels, Vector2 center, float radius, float fluffAmount, int size)
    {
        int rings = Mathf.RoundToInt(textureRings * fluffAmount);

        for (int ring = 0; ring < rings; ring++)
        {
            float ringRadius = radius * (0.75f + 0.4f * ring / rings);
            float alpha = CalculateRingAlpha(ring, rings, fluffAmount);

            DrawTexturedRing(pixels, center, ringRadius, alpha, size, (px, py) => {
                // Soft, billowy noise
                float noise = GetNoise(px * noiseScale * 0.7f, py * noiseScale * 0.7f);
                float softNoise = Mathf.Pow(noise, 1.5f); // Softer curves

                return Mathf.Lerp(0.9f, 1f, softNoise * noiseStrength * 0.5f);
            });
        }
    }

    /// <summary>
    /// Draw a textured ring with custom noise function
    /// </summary>
    private void DrawTexturedRing(Color[] pixels, Vector2 center, float radius, float alpha, int size, System.Func<float, float, float> noiseFunc)
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
                    float edgeAlpha = 1f - Mathf.Clamp01((distance - radius) / edgeSoftness);

                    if (edgeAlpha > 0.05f)
                    {
                        float textureValue = noiseFunc(px, py);
                        float finalAlpha = alpha * edgeAlpha * textureValue * contrast;

                        Color fluffPixel = fluffColor;
                        fluffPixel.a = Mathf.Min(finalAlpha, 1f);

                        int index = py * size + px;
                        if (index >= 0 && index < pixels.Length)
                        {
                            pixels[index] = Color.Lerp(pixels[index], fluffPixel, fluffPixel.a);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Calculate alpha for ring based on position and settings
    /// </summary>
    private float CalculateRingAlpha(int ring, int totalRings, float fluffAmount)
    {
        float baseAlpha = Mathf.Lerp(minAlpha, maxAlpha, fluffAmount);
        float ringFadeAmount = 1f - (ring * ringFade);
        return baseAlpha * ringFadeAmount;
    }

    /// <summary>
    /// Get noise value with different types
    /// </summary>
    private float GetNoise(float x, float y)
    {
        if (useFractalNoise)
        {
            // Fractal noise (multiple octaves)
            float noise = 0f;
            float amplitude = 1f;
            float frequency = 1f;

            for (int i = 0; i < 3; i++)
            {
                noise += Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude;
                amplitude *= 0.5f;
                frequency *= 2f;
            }

            return Mathf.Clamp01(noise);
        }
        else if (usePerlinNoise)
        {
            return Mathf.PerlinNoise(x, y);
        }
        else
        {
            // Simple sine-based noise
            return (Mathf.Sin(x * 10f) * Mathf.Sin(y * 10f) + 1f) * 0.5f;
        }
    }

    /// <summary>
    /// Update fluffiness and apply sprite
    /// </summary>
    public void UpdateFluffiness()
    {
        if (!enableFluff)
        {
            if (fluffSpriteRenderer != null)
                fluffSpriteRenderer.sprite = null;
            return;
        }

        CalculateFluffiness();
        ApplyFluffSprite();
        UpdateDebugInfo();
    }

    private void CalculateFluffiness()
    {
        currentFluffiness = baseFluffiness;

        // Genetics
        if (geneticsSystem != null)
        {
            float geneticFluff = geneticsSystem.GetTraitValue(fluffGeneTraitName, 0.5f);
            currentFluffiness = Mathf.Lerp(baseFluffiness, 1f, geneticFluff * geneticInfluence);
        }

        // Age effects
        if (useAgeEffects && lifeStageTracker != null)
        {
            float multiplier = 1f;
            switch (lifeStageTracker.CurrentStage)
            {
                case AgeLifeStageTracker.LifeStage.Baby:
                    multiplier = babyFluffMultiplier;
                    break;
                case AgeLifeStageTracker.LifeStage.Child:
                    multiplier = childFluffMultiplier;
                    break;
                case AgeLifeStageTracker.LifeStage.Adult:
                    multiplier = adultFluffMultiplier;
                    break;
                case AgeLifeStageTracker.LifeStage.Elderly:
                    multiplier = elderlyFluffMultiplier;
                    break;
            }
            currentFluffiness *= multiplier;
        }

        currentFluffiness = Mathf.Clamp01(currentFluffiness);
    }

    private void ApplyFluffSprite()
    {
        if (fluffSpriteRenderer == null || fluffSprites == null) return;

        int spriteIndex = Mathf.RoundToInt(currentFluffiness * (fluffSprites.Length - 1));
        spriteIndex = Mathf.Clamp(spriteIndex, 0, fluffSprites.Length - 1);

        fluffSpriteRenderer.sprite = fluffSprites[spriteIndex];

        // Color blending with genetics
        Color finalColor = fluffColor;
        if (geneticsSystem != null)
        {
            finalColor = Color.Lerp(fluffColor, geneticsSystem.BaseColor, 0.2f);
        }

        fluffSpriteRenderer.color = finalColor;
    }

    private void UpdateDebugInfo()
    {
        hasGenetics = geneticsSystem != null;
        geneticValue = hasGenetics ? geneticsSystem.GetTraitValue(fluffGeneTraitName, 0.5f) : 0f;
        finalAlpha = Mathf.Lerp(minAlpha, maxAlpha, currentFluffiness);
        currentLifeStage = lifeStageTracker?.CurrentStage.ToString() ?? "None";
    }

    // ========================================================================
    // PUBLIC METHODS FOR RUNTIME TWEAKING
    // ========================================================================

    [ContextMenu("Regenerate Fluff")]
    public void RegenerateFluff()
    {
        CreateTexturedFluffSprites();
        UpdateFluffiness();
    }

    [ContextMenu("Set Wispy Style")]
    public void SetWispyStyle()
    {
        textureType = FluffTextureType.Wispy;
        RegenerateFluff();
    }

    [ContextMenu("Set Cloudy Style")]
    public void SetCloudyStyle()
    {
        textureType = FluffTextureType.Cloudy;
        RegenerateFluff();
    }

    [ContextMenu("Set Sparkly Style")]
    public void SetSparklyStyle()
    {
        textureType = FluffTextureType.Sparkly;
        RegenerateFluff();
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

    public float GetCurrentFluffiness() => currentFluffiness;

    void OnDestroy()
    {
        if (fluffSprites != null)
        {
            foreach (var sprite in fluffSprites)
            {
                if (sprite != null && sprite.texture != null)
                {
                    DestroyImmediate(sprite.texture);
                }
            }
        }
    }
}

/*
========================================================================
🐹 TEXTURED GENETIC FLUFF - PREFAB READY!

HOW TO USE:
1. Drag this script directly onto your agent prefab
2. Play with all the settings in the inspector!
3. Try different texture types and watch them change in real-time

TEXTURE TYPES:
🌪️ Wispy - Light, airy fluff with directional wisps
☁️ Cloudy - Dense, cloud-like fluff  
✨ Sparkly - Glittery, magical fluff
🧸 Fuzzy - Dense, fur-like texture
🪶 Cottony - Soft, cotton ball texture

PLAY WITH THESE SETTINGS:
- Texture Type: Switch between different fluff styles
- Min/Max Alpha: Control visibility range
- Contrast: Make fluff pop more
- Texture Rings: More rings = denser fluff
- Fluff Size: Overall size multiplier
- Noise Scale/Strength: Control texture detail
- Edge Softness: How soft the fluff edges are

SPECIAL EFFECTS (per texture type):
- Wispy: Wispiness, Wisp Scale, Wisp Stretch
- Cloudy: Cloudiness, Cloud Scale, Cloud Layers  
- Sparkly: Sparkle amount, Sparkle Contrast

CONTEXT MENU OPTIONS:
- Regenerate Fluff
- Set [Style] Style
- Test Maximum Fluff

Your agents will have beautiful, textured genetic fluff! 🐹✨
========================================================================
*/