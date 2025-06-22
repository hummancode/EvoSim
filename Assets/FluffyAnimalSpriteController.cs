
// ============================================================================
// FILE: FluffyAnimalSpriteController.cs - FIXED VERSION WITH DEBUG
// PURPOSE: Creates fluffy animal sprites with genetic hair variation + debugging
// ============================================================================

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Creates fluffy animal sprites where hair/fur amount is controlled by genetics
/// </summary>
public class FluffyAnimalSpriteController : MonoBehaviour
{
    [Header("Sprite Settings")]
    [SerializeField] private SpriteRenderer mainSpriteRenderer;
    [SerializeField] private int spriteSize = 64; // Size of generated sprite
    [SerializeField] private bool autoSetupOnStart = true;

    [Header("Animal Body")]
    [SerializeField] private Color bodyColor = new Color(0.8f, 0.6f, 0.4f, 1f); // Light brown
    [SerializeField] private float bodySize = 0.7f; // Size of main body relative to sprite
    [SerializeField] private AnimalType animalType = AnimalType.Hamster;

    [Header("Genetic Hair/Fur System")]
    [SerializeField] private bool useGeneticHair = true;
    [SerializeField] private float baseHairAmount = 0.5f; // INCREASED from 0.3f
    [SerializeField] private float maxHairAmount = 1.0f; // INCREASED from 0.8f
    [SerializeField] private Color hairColor = new Color(0.4f, 0.2f, 0.1f, 1f); // DARKER and OPAQUE
    [SerializeField] private int hairDensity = 80; // INCREASED from 50

    [Header("Hair Genetics")]
    [SerializeField] private string hairGeneTraitName = "HairAmount";
    [SerializeField] private float hairGeneticInfluence = 0.7f;

    [Header("Age-Based Changes")]
    [SerializeField] private bool enableAgeBasedHair = true;
    [SerializeField] private float adultHairMultiplier = 1.2f;
    [SerializeField] private float elderlyHairMultiplier = 0.8f;

    [Header("Visual Effects")]
    [SerializeField] private bool enableHairAnimation = false; // DISABLED by default for testing
    [SerializeField] private float hairWiggleSpeed = 2f;
    [SerializeField] private float hairWiggleAmount = 0.5f;

    [Header("DEBUG INFO - READ ONLY")]
    [SerializeField] private float debugCurrentHairAmount;
    [SerializeField] private int debugHairStrandsGenerated;
    [SerializeField] private bool debugGeneticsFound;
    [SerializeField] private float debugGeneticHairValue;
    [SerializeField] private string debugCurrentLifeStage;

    public enum AnimalType
    {
        Hamster,
        Rabbit,
        Mouse,
        Squirrel,
        Hedgehog
    }

    // Components
    private GeneticsSystem geneticsSystem;
    private AgeSystem ageSystem;
    private AgeLifeStageTracker lifeStageTracker;

    // Sprite data
    private Texture2D animalTexture;
    private List<Vector2> hairPositions = new List<Vector2>();
    private List<float> hairLengths = new List<float>();
    private float currentHairAmount;

    // Animation
    private float animationTime;

    void Awake()
    {
        SetupComponents();
    }

    void Start()
    {
        // DELAY GENERATION to ensure all components are ready
        Invoke(nameof(DelayedGeneration), 0.1f);
    }

    void DelayedGeneration()
    {
        if (autoSetupOnStart)
        {
            GenerateAnimalSprite();
        }
    }

    void Update()
    {
        // Update debug info
        UpdateDebugInfo();

        if (enableHairAnimation)
        {
            AnimateHair();
        }
    }

    /// <summary>
    /// Update debug information
    /// </summary>
    private void UpdateDebugInfo()
    {
        debugCurrentHairAmount = currentHairAmount;
        debugHairStrandsGenerated = hairPositions.Count;
        debugGeneticsFound = geneticsSystem != null;

        if (geneticsSystem != null)
        {
            debugGeneticHairValue = geneticsSystem.GetTraitValue(hairGeneTraitName, 0.5f);
        }

        if (lifeStageTracker != null)
        {
            debugCurrentLifeStage = lifeStageTracker.CurrentStage.ToString();
        }
        else
        {
            debugCurrentLifeStage = "No Life Stage Tracker";
        }
    }

    /// <summary>
    /// Setup required components
    /// </summary>
    private void SetupComponents()
    {
        if (mainSpriteRenderer == null)
        {
            mainSpriteRenderer = GetComponent<SpriteRenderer>();
            if (mainSpriteRenderer == null)
            {
                mainSpriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                Debug.Log($"Added SpriteRenderer to {gameObject.name}");
            }
        }

        geneticsSystem = GetComponent<GeneticsSystem>();
        ageSystem = GetComponent<AgeSystem>();
        lifeStageTracker = GetComponent<AgeLifeStageTracker>();

        Debug.Log($"FluffySprite Setup: Genetics={geneticsSystem != null}, Age={ageSystem != null}, LifeStage={lifeStageTracker != null}");
    }

    /// <summary>
    /// Main method to generate the fluffy animal sprite
    /// </summary>
    [ContextMenu("Generate Animal Sprite")]
    public void GenerateAnimalSprite()
    {
        Debug.Log($"=== GENERATING FLUFFY SPRITE FOR {gameObject.name} ===");

        // Calculate hair amount based on genetics and age
        CalculateHairAmount();

        // Create the animal texture
        CreateAnimalTexture();

        // Apply to sprite renderer
        ApplyTextureToSprite();

        Debug.Log($"Generated {animalType} sprite with {currentHairAmount:P} hair amount ({debugHairStrandsGenerated} strands)");
    }

    /// <summary>
    /// Calculate how much hair this agent should have based on genetics and age
    /// </summary>
    private void CalculateHairAmount()
    {
        currentHairAmount = baseHairAmount;
        Debug.Log($"Starting hair calculation with base: {baseHairAmount}");

        // Apply genetic influence
        if (useGeneticHair && geneticsSystem != null)
        {
            float geneticHairValue = geneticsSystem.GetTraitValue(hairGeneTraitName, 0.5f);
            float geneticInfluence = Mathf.Lerp(0f, maxHairAmount - baseHairAmount, geneticHairValue);
            currentHairAmount = baseHairAmount + (geneticInfluence * hairGeneticInfluence);

            Debug.Log($"Applied genetics: trait value={geneticHairValue:F2}, influence={geneticInfluence:F2}, result={currentHairAmount:F2}");
        }
        else
        {
            Debug.Log("No genetics system found or genetic hair disabled");
        }

        // Apply age-based changes
        if (enableAgeBasedHair && lifeStageTracker != null)
        {
            float ageMultiplier = 1f;
            switch (lifeStageTracker.CurrentStage)
            {
                case AgeLifeStageTracker.LifeStage.Baby:
                    ageMultiplier = 0.6f;
                    break;
                case AgeLifeStageTracker.LifeStage.Child:
                    ageMultiplier = 0.8f;
                    break;
                case AgeLifeStageTracker.LifeStage.Adult:
                    ageMultiplier = adultHairMultiplier;
                    break;
                case AgeLifeStageTracker.LifeStage.Elderly:
                    ageMultiplier = elderlyHairMultiplier;
                    break;
            }
            currentHairAmount *= ageMultiplier;
            Debug.Log($"Applied age multiplier: stage={lifeStageTracker.CurrentStage}, multiplier={ageMultiplier:F2}, result={currentHairAmount:F2}");
        }
        else
        {
            Debug.Log("Age-based hair disabled or no life stage tracker");
        }

        // Clamp to valid range
        currentHairAmount = Mathf.Clamp(currentHairAmount, 0f, maxHairAmount);
        Debug.Log($"Final hair amount: {currentHairAmount:F2} (clamped to 0-{maxHairAmount})");
    }

    /// <summary>
    /// Create the animal texture with body and hair
    /// </summary>
    private void CreateAnimalTexture()
    {
        // Destroy old texture if exists
        if (animalTexture != null)
        {
            DestroyImmediate(animalTexture);
        }

        // Create texture
        animalTexture = new Texture2D(spriteSize, spriteSize);
        Color[] pixels = new Color[spriteSize * spriteSize];

        // Clear background
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }

        // Draw animal body
        DrawAnimalBody(pixels);

        // Draw hair/fur
        DrawHairFur(pixels);

        // Apply pixels
        animalTexture.SetPixels(pixels);
        animalTexture.filterMode = FilterMode.Point; // Pixel art style
        animalTexture.Apply();

        Debug.Log($"Created texture: {spriteSize}x{spriteSize}, hair strands: {hairPositions.Count}");
    }

    /// <summary>
    /// Draw the main animal body based on type
    /// </summary>
    private void DrawAnimalBody(Color[] pixels)
    {
        Vector2 center = new Vector2(spriteSize * 0.5f, spriteSize * 0.4f);
        float bodyRadius = spriteSize * bodySize * 0.3f;

        Debug.Log($"Drawing {animalType} body at center={center}, radius={bodyRadius}");

        switch (animalType)
        {
            case AnimalType.Hamster:
                DrawHamsterBody(pixels, center, bodyRadius);
                break;
            case AnimalType.Rabbit:
                DrawRabbitBody(pixels, center, bodyRadius);
                break;
            case AnimalType.Mouse:
                DrawMouseBody(pixels, center, bodyRadius);
                break;
            case AnimalType.Squirrel:
                DrawSquirrelBody(pixels, center, bodyRadius);
                break;
            case AnimalType.Hedgehog:
                DrawHedgehogBody(pixels, center, bodyRadius);
                break;
        }
    }

    /// <summary>
    /// Draw hamster body (round and chubby)
    /// </summary>
    private void DrawHamsterBody(Color[] pixels, Vector2 center, float radius)
    {
        // Main body (oval)
        DrawOval(pixels, center, radius, radius * 1.2f, bodyColor);

        // Head (smaller circle on top)
        Vector2 headCenter = center + new Vector2(0, radius * 0.8f);
        DrawCircle(pixels, headCenter, radius * 0.6f, bodyColor);

        // Eyes (BIGGER for visibility)
        DrawCircle(pixels, headCenter + new Vector2(-radius * 0.3f, radius * 0.2f), 3f, Color.black);
        DrawCircle(pixels, headCenter + new Vector2(radius * 0.3f, radius * 0.2f), 3f, Color.black);

        // Small ears
        DrawCircle(pixels, headCenter + new Vector2(-radius * 0.4f, radius * 0.5f), 4f, bodyColor);
        DrawCircle(pixels, headCenter + new Vector2(radius * 0.4f, radius * 0.5f), 4f, bodyColor);

        // Nose (BIGGER)
        DrawCircle(pixels, headCenter + new Vector2(0, -radius * 0.1f), 2f, Color.black);
    }

    /// <summary>
    /// Draw rabbit body (elongated with long ears)
    /// </summary>
    private void DrawRabbitBody(Color[] pixels, Vector2 center, float radius)
    {
        // Main body (oval)
        DrawOval(pixels, center, radius * 0.8f, radius * 1.4f, bodyColor);

        // Head
        Vector2 headCenter = center + new Vector2(0, radius * 1f);
        DrawOval(pixels, headCenter, radius * 0.5f, radius * 0.7f, bodyColor);

        // Long ears
        DrawOval(pixels, headCenter + new Vector2(-radius * 0.3f, radius * 0.8f), radius * 0.15f, radius * 0.6f, bodyColor);
        DrawOval(pixels, headCenter + new Vector2(radius * 0.3f, radius * 0.8f), radius * 0.15f, radius * 0.6f, bodyColor);

        // Eyes
        DrawCircle(pixels, headCenter + new Vector2(-radius * 0.2f, radius * 0.1f), 3f, Color.black);
        DrawCircle(pixels, headCenter + new Vector2(radius * 0.2f, radius * 0.1f), 3f, Color.black);

        // Nose
        DrawCircle(pixels, headCenter + new Vector2(0, -radius * 0.2f), 2f, Color.black);
    }

    /// <summary>
    /// Draw mouse body (small and nimble)
    /// </summary>
    private void DrawMouseBody(Color[] pixels, Vector2 center, float radius)
    {
        // Main body (small oval)
        DrawOval(pixels, center, radius * 0.6f, radius * 1f, bodyColor);

        // Head (pointed)
        Vector2 headCenter = center + new Vector2(0, radius * 0.7f);
        DrawOval(pixels, headCenter, radius * 0.4f, radius * 0.5f, bodyColor);

        // Large round ears
        DrawCircle(pixels, headCenter + new Vector2(-radius * 0.4f, radius * 0.3f), radius * 0.25f, bodyColor);
        DrawCircle(pixels, headCenter + new Vector2(radius * 0.4f, radius * 0.3f), radius * 0.25f, bodyColor);

        // Eyes
        DrawCircle(pixels, headCenter + new Vector2(-radius * 0.15f, radius * 0.1f), 2f, Color.black);
        DrawCircle(pixels, headCenter + new Vector2(radius * 0.15f, radius * 0.1f), 2f, Color.black);

        // Pointed nose
        DrawCircle(pixels, headCenter + new Vector2(0, -radius * 0.3f), 2f, Color.black);

        // Long tail
        DrawTail(pixels, center - new Vector2(0, radius * 0.8f), radius * 0.05f, radius * 1.5f);
    }

    /// <summary>
    /// Draw squirrel body (fluffy with big tail)
    /// </summary>
    private void DrawSquirrelBody(Color[] pixels, Vector2 center, float radius)
    {
        // Main body
        DrawOval(pixels, center, radius * 0.7f, radius * 1.1f, bodyColor);

        // Head
        Vector2 headCenter = center + new Vector2(0, radius * 0.8f);
        DrawCircle(pixels, headCenter, radius * 0.5f, bodyColor);

        // Pointed ears
        DrawTriangle(pixels, headCenter + new Vector2(-radius * 0.3f, radius * 0.4f), radius * 0.2f, bodyColor);
        DrawTriangle(pixels, headCenter + new Vector2(radius * 0.3f, radius * 0.4f), radius * 0.2f, bodyColor);

        // Eyes
        DrawCircle(pixels, headCenter + new Vector2(-radius * 0.2f, radius * 0.1f), 3f, Color.black);
        DrawCircle(pixels, headCenter + new Vector2(radius * 0.2f, radius * 0.1f), 3f, Color.black);

        // Nose
        DrawCircle(pixels, headCenter + new Vector2(0, -radius * 0.1f), 2f, Color.black);

        // Big fluffy tail
        DrawFluffyTail(pixels, center - new Vector2(radius * 0.3f, radius * 0.5f), radius * 0.8f);
    }

    /// <summary>
    /// Draw hedgehog body (round with spiky potential)
    /// </summary>
    private void DrawHedgehogBody(Color[] pixels, Vector2 center, float radius)
    {
        // Main body (very round)
        DrawCircle(pixels, center, radius, bodyColor);

        // Small head
        Vector2 headCenter = center + new Vector2(0, radius * 0.5f);
        DrawOval(pixels, headCenter, radius * 0.3f, radius * 0.4f, bodyColor);

        // Small ears
        DrawCircle(pixels, headCenter + new Vector2(-radius * 0.2f, radius * 0.3f), radius * 0.1f, bodyColor);
        DrawCircle(pixels, headCenter + new Vector2(radius * 0.2f, radius * 0.3f), radius * 0.1f, bodyColor);

        // Eyes
        DrawCircle(pixels, headCenter + new Vector2(-radius * 0.1f, radius * 0.1f), 2f, Color.black);
        DrawCircle(pixels, headCenter + new Vector2(radius * 0.1f, radius * 0.1f), 2f, Color.black);

        // Pointed nose
        DrawCircle(pixels, headCenter + new Vector2(0, -radius * 0.2f), 2f, Color.black);
    }

    /// <summary>
    /// Draw hair/fur based on current hair amount - IMPROVED VERSION
    /// </summary>
    private void DrawHairFur(Color[] pixels)
    {
        // Clear previous hair positions
        hairPositions.Clear();
        hairLengths.Clear();

        // Calculate number of hair strands based on hair amount
        int totalHairStrands = Mathf.RoundToInt(hairDensity * currentHairAmount);
        Debug.Log($"Drawing {totalHairStrands} hair strands (density={hairDensity}, amount={currentHairAmount:F2})");

        if (totalHairStrands <= 0)
        {
            Debug.LogWarning("No hair strands to draw!");
            return;
        }

        Vector2 center = new Vector2(spriteSize * 0.5f, spriteSize * 0.4f);
        float bodyRadius = spriteSize * bodySize * 0.3f;

        for (int i = 0; i < totalHairStrands; i++)
        {
            // Random position around the body
            float angle = Random.Range(0f, 2f * Mathf.PI);
            float distance = Random.Range(bodyRadius * 0.9f, bodyRadius * 1.3f); // EXTENDED range

            Vector2 hairBase = center + new Vector2(
                Mathf.Cos(angle) * distance,
                Mathf.Sin(angle) * distance
            );

            // Hair length varies with overall hair amount - LONGER hair
            float hairLength = Random.Range(3f, 12f) * currentHairAmount;

            // Store for animation
            hairPositions.Add(hairBase);
            hairLengths.Add(hairLength);

            // Draw hair strand
            DrawHairStrand(pixels, hairBase, angle, hairLength);
        }

        Debug.Log($"Drew {hairPositions.Count} hair strands");
    }

    /// <summary>
    /// Draw individual hair strand - IMPROVED VERSION
    /// </summary>
    private void DrawHairStrand(Color[] pixels, Vector2 basePos, float angle, float length)
    {
        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

        // DENSER hair drawing
        for (float t = 0; t < length; t += 0.3f) // Smaller steps = denser hair
        {
            Vector2 hairPos = basePos + direction * t;

            // Add some curve to the hair
            float curve = Mathf.Sin(t * 0.2f) * 0.8f;
            hairPos += new Vector2(-direction.y, direction.x) * curve;

            // Draw hair pixel with BETTER thickness
            DrawHairPixel(pixels, hairPos, t, length);
        }
    }

    /// <summary>
    /// Draw a hair pixel with better visibility
    /// </summary>
    private void DrawHairPixel(Color[] pixels, Vector2 hairPos, float distance, float totalLength)
    {
        int x = Mathf.RoundToInt(hairPos.x);
        int y = Mathf.RoundToInt(hairPos.y);

        // Draw multiple pixels for thicker hair
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                int px = x + dx;
                int py = y + dy;

                if (px >= 0 && px < spriteSize && py >= 0 && py < spriteSize)
                {
                    int index = py * spriteSize + px;
                    if (index >= 0 && index < pixels.Length)
                    {
                        // Hair gets slightly more transparent toward the tip, but still visible
                        Color hairPixel = hairColor;
                        hairPixel.a *= Mathf.Lerp(1f, 0.7f, distance / totalLength); // Min 70% opacity

                        // Blend with existing pixel
                        Color existing = pixels[index];
                        pixels[index] = Color.Lerp(existing, hairPixel, hairPixel.a);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Animate hair with subtle movement
    /// </summary>
    private void AnimateHair()
    {
        animationTime += Time.deltaTime * hairWiggleSpeed;

        // Regenerate sprite periodically for hair animation
        if (Time.frameCount % 60 == 0) // Update every 60 frames for performance
        {
            GenerateAnimalSprite();
        }
    }

    // ========================================================================
    // DRAWING HELPER METHODS
    // ========================================================================

    private void DrawCircle(Color[] pixels, Vector2 center, float radius, Color color)
    {
        int x0 = Mathf.RoundToInt(center.x);
        int y0 = Mathf.RoundToInt(center.y);
        int r = Mathf.RoundToInt(radius);

        for (int y = -r; y <= r; y++)
        {
            for (int x = -r; x <= r; x++)
            {
                if (x * x + y * y <= r * r)
                {
                    int px = x0 + x;
                    int py = y0 + y;
                    if (px >= 0 && px < spriteSize && py >= 0 && py < spriteSize)
                    {
                        pixels[py * spriteSize + px] = color;
                    }
                }
            }
        }
    }

    private void DrawOval(Color[] pixels, Vector2 center, float radiusX, float radiusY, Color color)
    {
        int x0 = Mathf.RoundToInt(center.x);
        int y0 = Mathf.RoundToInt(center.y);
        int rx = Mathf.RoundToInt(radiusX);
        int ry = Mathf.RoundToInt(radiusY);

        for (int y = -ry; y <= ry; y++)
        {
            for (int x = -rx; x <= rx; x++)
            {
                float distance = (x * x) / (float)(rx * rx) + (y * y) / (float)(ry * ry);
                if (distance <= 1f)
                {
                    int px = x0 + x;
                    int py = y0 + y;
                    if (px >= 0 && px < spriteSize && py >= 0 && py < spriteSize)
                    {
                        pixels[py * spriteSize + px] = color;
                    }
                }
            }
        }
    }

    private void DrawTriangle(Color[] pixels, Vector2 center, float size, Color color)
    {
        // Simple upward pointing triangle
        for (int y = 0; y < size; y++)
        {
            int width = Mathf.RoundToInt(size - y);
            for (int x = -width/2; x <= width/2; x++)
            {
                int px = Mathf.RoundToInt(center.x + x);
                int py = Mathf.RoundToInt(center.y + y);
                if (px >= 0 && px < spriteSize && py >= 0 && py < spriteSize)
                {
                    pixels[py * spriteSize + px] = color;
                }
            }
        }
    }

    private void DrawTail(Color[] pixels, Vector2 start, float thickness, float length)
    {
        Vector2 direction = new Vector2(0, -1); // Downward tail
        for (float t = 0; t < length; t += 0.5f)
        {
            Vector2 pos = start + direction * t;
            DrawCircle(pixels, pos, thickness, bodyColor);
        }
    }

    private void DrawFluffyTail(Color[] pixels, Vector2 start, float size)
    {
        // Big fluffy tail for squirrel
        DrawOval(pixels, start, size * 0.6f, size, hairColor);
        DrawOval(pixels, start, size * 0.4f, size * 0.8f, bodyColor);
    }

    /// <summary>
    /// Apply the generated texture to the sprite renderer
    /// </summary>
    private void ApplyTextureToSprite()
    {
        if (animalTexture != null && mainSpriteRenderer != null)
        {
            Sprite animalSprite = Sprite.Create(
                animalTexture,
                new Rect(0, 0, spriteSize, spriteSize),
                new Vector2(0.5f, 0.5f),
                spriteSize / 2f // FIXED: Better pixels per unit for visibility
            );

            mainSpriteRenderer.sprite = animalSprite;
            Debug.Log($"Applied sprite to renderer: {spriteSize}x{spriteSize}");
        }
        else
        {
            Debug.LogError($"Cannot apply sprite: texture={animalTexture != null}, renderer={mainSpriteRenderer != null}");
        }
    }

    // ========================================================================
    // PUBLIC INTERFACE
    // ========================================================================

    /// <summary>
    /// Set animal type and regenerate
    /// </summary>
    public void SetAnimalType(AnimalType type)
    {
        animalType = type;
        GenerateAnimalSprite();
    }

    /// <summary>
    /// Get current hair amount (for debugging)
    /// </summary>
    public float GetCurrentHairAmount()
    {
        return currentHairAmount;
    }

    /// <summary>
    /// Force regenerate sprite (useful when genetics change)
    /// </summary>
    [ContextMenu("Regenerate Sprite")]
    public void RegenerateSprite()
    {
        GenerateAnimalSprite();
    }

    /// <summary>
    /// Set hair colors
    /// </summary>
    public void SetHairColor(Color color)
    {
        hairColor = color;
        GenerateAnimalSprite();
    }

    /// <summary>
    /// Set body color
    /// </summary>
    public void SetBodyColor(Color color)
    {
        bodyColor = color;
        GenerateAnimalSprite();
    }

    void OnDestroy()
    {
        if (animalTexture != null)
        {
            DestroyImmediate(animalTexture);
        }
    }
}
