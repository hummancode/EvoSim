// ============================================================================
// FILE: SimpleGeneticFluffOverlay.cs - FIXED ALPHA VERSION
// PURPOSE: High visibility genetic fluff with proper alpha values
// ============================================================================

using UnityEngine;

/// <summary>
/// Simple genetic fluff system with VISIBLE fluff (fixed alpha issues)
/// </summary>
public class SimpleGeneticFluffOverlay : MonoBehaviour
{
    [Header("Fluff Settings")]
    [SerializeField] private bool enableFluff = true;
    [SerializeField] private float baseFluffiness = 0.4f;
    [SerializeField] private Color fluffColor = new Color(0.9f, 0.7f, 0.5f, 0.8f); // MUCH MORE OPAQUE

    [Header("Genetics")]
    [SerializeField] private string fluffGeneTraitName = "FluffAmount";
    [SerializeField] private float geneticInfluence = 0.8f;

    [Header("Age Effects")]
    [SerializeField] private bool useAgeEffects = true;
    [SerializeField] private float babyFluffMultiplier = 1.5f;
    [SerializeField] private float adultFluffMultiplier = 1.0f;
    [SerializeField] private float elderlyFluffMultiplier = 0.7f;

    [Header("Visibility Settings - FIXED")]
    [SerializeField] private float minFluffAlpha = 0.4f; // MINIMUM alpha so fluff is always visible
    [SerializeField] private float maxFluffAlpha = 0.9f; // MAXIMUM alpha for very fluffy agents
    [SerializeField] private float fluffContrast = 1.2f; // How much fluff stands out

    [Header("Debug")]
    [SerializeField] private float currentFluffiness;
    [SerializeField] private bool hasGenetics;
    [SerializeField] private float geneticValue;
    [SerializeField] private float finalAlpha; // Show the actual alpha being used

    // Components
    private SpriteRenderer agentSpriteRenderer;
    private SpriteRenderer fluffSpriteRenderer;
    private GeneticsSystem geneticsSystem;
    private AgeLifeStageTracker lifeStageTracker;

    // Fluff sprites (pre-made for performance)
    private Sprite[] fluffSprites;

    void Awake()
    {
        SetupComponents();
        CreateFluffSprites();
    }

    void Start()
    {
        Invoke(nameof(UpdateFluffiness), 0.1f);
    }

    void Update()
    {
        if (Time.frameCount % 60 == 0) // Every 60 frames
        {
            UpdateFluffiness();
        }
    }

    private void SetupComponents()
    {
        agentSpriteRenderer = GetComponent<SpriteRenderer>();
        if (agentSpriteRenderer == null)
        {
            Debug.LogWarning($"No SpriteRenderer found on {gameObject.name}");
            return;
        }

        // Create fluff sprite renderer as child
        GameObject fluffObject = new GameObject("FluffOverlay");
        fluffObject.transform.SetParent(transform);
        fluffObject.transform.localPosition = Vector3.zero;
        fluffObject.transform.localScale = Vector3.one;

        fluffSpriteRenderer = fluffObject.AddComponent<SpriteRenderer>();
        fluffSpriteRenderer.sortingLayerName = agentSpriteRenderer.sortingLayerName;
        fluffSpriteRenderer.sortingOrder = agentSpriteRenderer.sortingOrder + 1;

        geneticsSystem = GetComponent<GeneticsSystem>();
        lifeStageTracker = GetComponent<AgeLifeStageTracker>();

        Debug.Log($"SimpleGeneticFluff setup complete for {gameObject.name}");
    }

    /// <summary>
    /// Create fluff sprites with MUCH better visibility
    /// </summary>
    private void CreateFluffSprites()
    {
        int fluffLevels = 5;
        fluffSprites = new Sprite[fluffLevels];

        for (int i = 0; i < fluffLevels; i++)
        {
            float fluffAmount = i / (float)(fluffLevels - 1);
            fluffSprites[i] = CreateVisibleFluffSprite(fluffAmount);
        }

        Debug.Log($"Created {fluffLevels} visible fluff sprites");
    }

    /// <summary>
    /// Create fluff sprite with PROPER visibility
    /// </summary>
    private Sprite CreateVisibleFluffSprite(float fluffAmount)
    {
        if (fluffAmount <= 0f) return null;

        int size = 64;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        // Clear background
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }

        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float maxRadius = size * 0.45f; // Slightly bigger

        // Draw multiple fluff layers with GOOD visibility
        int fluffRings = Mathf.RoundToInt(8 * fluffAmount); // More rings

        for (int ring = 0; ring < fluffRings; ring++)
        {
            float ringRadius = maxRadius * (0.7f + 0.5f * ring / fluffRings);

            // MUCH MORE VISIBLE alpha calculation
            float baseAlpha = Mathf.Lerp(minFluffAlpha, maxFluffAlpha, fluffAmount);
            float ringAlpha = baseAlpha * (1f - ring * 0.1f); // Less fade per ring

            Color ringColor = fluffColor;
            ringColor.a = ringAlpha;

            DrawVisibleFluffyCircle(pixels, center, ringRadius, ringColor, size);
        }

        texture.SetPixels(pixels);
        texture.filterMode = FilterMode.Bilinear;
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size * 0.7f);
    }

    /// <summary>
    /// Draw fluffy circle with GOOD visibility
    /// </summary>
    private void DrawVisibleFluffyCircle(Color[] pixels, Vector2 center, float radius, Color color, int size)
    {
        int centerX = Mathf.RoundToInt(center.x);
        int centerY = Mathf.RoundToInt(center.y);
        int r = Mathf.RoundToInt(radius);

        for (int y = -r - 8; y <= r + 8; y++)
        {
            for (int x = -r - 8; x <= r + 8; x++)
            {
                int px = centerX + x;
                int py = centerY + y;

                if (px >= 0 && px < size && py >= 0 && py < size)
                {
                    float distance = Mathf.Sqrt(x * x + y * y);

                    // BETTER edge softness for visibility
                    float edgeSoftness = 4f;
                    float alpha = 1f - Mathf.Clamp01((distance - radius) / edgeSoftness);

                    if (alpha > 0.1f) // Only draw if reasonably visible
                    {
                        // LESS noise so fluff is more solid and visible
                        float noise = Mathf.PerlinNoise(px * 0.08f, py * 0.08f);
                        alpha *= (0.8f + 0.2f * noise); // Less variation

                        Color fluffPixel = color;
                        fluffPixel.a *= alpha;

                        // BOOST the alpha for better visibility
                        fluffPixel.a = Mathf.Min(fluffPixel.a * fluffContrast, 1f);

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

        // Update debug info
        hasGenetics = geneticsSystem != null;
        geneticValue = hasGenetics ? geneticsSystem.GetTraitValue(fluffGeneTraitName, 0.5f) : 0f;
    }

    private void CalculateFluffiness()
    {
        currentFluffiness = baseFluffiness;

        // Apply genetic influence
        if (geneticsSystem != null)
        {
            float geneticFluff = geneticsSystem.GetTraitValue(fluffGeneTraitName, 0.5f);
            currentFluffiness = Mathf.Lerp(baseFluffiness, 1f, geneticFluff * geneticInfluence);
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
                    ageMultiplier = 1.2f;
                    break;
                case AgeLifeStageTracker.LifeStage.Adult:
                    ageMultiplier = adultFluffMultiplier;
                    break;
                case AgeLifeStageTracker.LifeStage.Elderly:
                    ageMultiplier = elderlyFluffMultiplier;
                    break;
            }
            currentFluffiness *= ageMultiplier;
        }

        currentFluffiness = Mathf.Clamp01(currentFluffiness);
    }

    /// <summary>
    /// Apply fluff sprite with VISIBLE alpha
    /// </summary>
    private void ApplyFluffSprite()
    {
        if (fluffSpriteRenderer == null || fluffSprites == null) return;

        // Choose sprite based on fluffiness level
        int spriteIndex = Mathf.RoundToInt(currentFluffiness * (fluffSprites.Length - 1));
        spriteIndex = Mathf.Clamp(spriteIndex, 0, fluffSprites.Length - 1);

        fluffSpriteRenderer.sprite = fluffSprites[spriteIndex];

        // CALCULATE FINAL VISIBLE ALPHA
        finalAlpha = Mathf.Lerp(minFluffAlpha, maxFluffAlpha, currentFluffiness);

        // Apply color with genetic blending
        Color finalColor = fluffColor;
        if (geneticsSystem != null)
        {
            Color geneticColor = geneticsSystem.BaseColor;
            finalColor = Color.Lerp(fluffColor, geneticColor, 0.3f);
        }

        // ENSURE alpha is visible
        finalColor.a = finalAlpha;
        fluffSpriteRenderer.color = finalColor;

        Debug.Log($"Applied fluff: sprite={spriteIndex}, fluffiness={currentFluffiness:F2}, alpha={finalAlpha:F2}");
    }

    /// <summary>
    /// Set fluff color with automatic alpha boost
    /// </summary>
    public void SetFluffColor(Color color)
    {
        // ENSURE the color has good alpha
        if (color.a < 0.3f)
        {
            color.a = 0.6f; // Boost too-transparent colors
        }

        fluffColor = color;
        CreateFluffSprites(); // Recreate with new color
        UpdateFluffiness();

        Debug.Log($"Set fluff color: {color} (alpha boosted if needed)");
    }

    public float GetCurrentFluffiness()
    {
        return currentFluffiness;
    }

    [ContextMenu("Update Fluffiness")]
    public void ForceUpdate()
    {
        UpdateFluffiness();
    }

    /// <summary>
    /// Make fluff VERY visible for testing
    /// </summary>
    [ContextMenu("Make Fluff Super Visible")]
    public void MakeFluffSuperVisible()
    {
        minFluffAlpha = 0.7f;
        maxFluffAlpha = 1.0f;
        fluffContrast = 1.5f;
        SetFluffColor(new Color(1f, 0.5f, 0f, 0.9f)); // Bright orange

        Debug.Log("Made fluff super visible with bright orange color!");
    }

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
?? FIXED ALPHA VISIBILITY ISSUE!

THE PROBLEM: Fluff alpha was too low (0.6f base alpha)
THE SOLUTION: Much higher alpha values and better visibility

NEW FEATURES:
? minFluffAlpha = 0.4f (minimum visible alpha)
? maxFluffAlpha = 0.9f (maximum alpha for very fluffy)
? fluffContrast = 1.2f (boost alpha for better visibility) 
? Less transparency fade between fluff rings
? Bigger, more solid fluff circles
? Debug shows "finalAlpha" so you can see actual alpha used

QUICK FIXES:
1. Right-click component ? "Make Fluff Super Visible" 
2. In inspector, increase "Min Fluff Alpha" to 0.8
3. In inspector, increase "Fluff Contrast" to 1.5
4. Change "Fluff Color" to something bright like orange

NOW YOUR FLUFF SHOULD BE CLEARLY VISIBLE! ???

If you want even MORE visible fluff, set:
- minFluffAlpha = 0.8f
- maxFluffAlpha = 1.0f  
- fluffContrast = 1.5f
========================================================================
*/