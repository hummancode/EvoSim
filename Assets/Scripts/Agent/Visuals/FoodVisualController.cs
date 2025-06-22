// ============================================================================
// FILE: FoodVisualController.cs
// PURPOSE: Visual management for food items with sprites and effects
// ============================================================================

using UnityEngine;
using System.Collections;

/// <summary>
/// Visual controller for food items with aging, consumption, and spawning effects
/// </summary>
public class FoodVisualController : MonoBehaviour
{
    [Header("Sprite References")]
    [SerializeField] private SpriteRenderer mainSpriteRenderer;
    [SerializeField] private SpriteRenderer effectSpriteRenderer;

    [Header("Food Sprites")]
    [SerializeField] private Sprite freshFoodSprite;
    [SerializeField] private Sprite agingFoodSprite;
    [SerializeField] private Sprite rottingFoodSprite;
    [SerializeField] private Sprite consumedSprite; // Flash sprite when consumed

    [Header("Color Progression")]
    [SerializeField] private Color freshColor = new Color(0.2f, 0.8f, 0.3f, 1f); // Bright green
    [SerializeField] private Color agingColor = new Color(0.8f, 0.6f, 0.2f, 1f); // Orange
    [SerializeField] private Color rottingColor = new Color(0.5f, 0.3f, 0.2f, 1f); // Brown
    [SerializeField] private Color consumedColor = Color.yellow;

    [Header("Visual Settings")]
    [SerializeField] private float pulseSpeed = 1f;
    [SerializeField] private float maxPulseScale = 1.2f;
    [SerializeField] private bool enableFreshPulse = true;
    [SerializeField] private bool enableAgingEffects = true;

    [Header("Consumption Effect")]
    [SerializeField] private float consumptionFlashDuration = 0.3f;
    [SerializeField] private float consumptionScaleBoost = 1.5f;

    [Header("Spawn Effect")]
    [SerializeField] private float spawnAnimationDuration = 0.5f;
    [SerializeField] private AnimationCurve spawnScaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Decay Effect")]
    [SerializeField] private float decayAnimationDuration = 1f;
    [SerializeField] private int decayFlashCount = 2;

    // Component references
    private Food foodComponent;
    private Collider2D foodCollider;

    // Visual state
    private bool isConsumed = false;
    private bool isDecaying = false;
    private float spawnTime;
    private Coroutine currentEffectCoroutine;

    // Original values
    private Vector3 originalScale;
    private Sprite originalSprite;

    void Awake()
    {
        SetupComponents();
        SetupSpriteRenderers();
        CacheOriginalValues();
    }

    void Start()
    {
        spawnTime = Time.time;

        // Play spawn animation
        PlaySpawnEffect();

        // Subscribe to food events if available
        SubscribeToEvents();
    }

    void Update()
    {
        if (!isConsumed && !isDecaying)
        {
            UpdateFoodVisuals();
        }
    }

    private void SetupComponents()
    {
        foodComponent = GetComponent<Food>();
        foodCollider = GetComponent<Collider2D>();
    }

    private void SetupSpriteRenderers()
    {
        // Setup main sprite renderer
        if (mainSpriteRenderer == null)
        {
            mainSpriteRenderer = GetComponent<SpriteRenderer>();
            if (mainSpriteRenderer == null)
            {
                mainSpriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
        }

        // Setup effect sprite renderer
        if (effectSpriteRenderer == null)
        {
            GameObject effectObj = new GameObject("FoodEffectRenderer");
            effectObj.transform.SetParent(transform);
            effectObj.transform.localPosition = Vector3.zero;
            effectObj.transform.localScale = Vector3.one;

            effectSpriteRenderer = effectObj.AddComponent<SpriteRenderer>();
            effectSpriteRenderer.sortingOrder = mainSpriteRenderer.sortingOrder + 1;
        }

        // Create default sprites if none assigned
        if (freshFoodSprite == null) CreateDefaultFoodSprites();
    }

    private void CacheOriginalValues()
    {
        originalScale = transform.localScale;
        originalSprite = mainSpriteRenderer.sprite;
    }

    private void SubscribeToEvents()
    {
        // If your Food class has events, subscribe here
        // For example: foodComponent.OnConsumed += OnFoodConsumed;
    }

    private void UpdateFoodVisuals()
    {
        if (foodComponent == null) return;

        // Calculate food age progression (0 = fresh, 1 = about to rot)
        float ageProgress = GetFoodAgeProgress();

        // Update sprite based on age
        UpdateSpriteForAge(ageProgress);

        // Update color based on age
        UpdateColorForAge(ageProgress);

        // Update scale and effects
        UpdateEffectsForAge(ageProgress);
    }

    private float GetFoodAgeProgress()
    {
        if (foodComponent == null) return 0f;

        // Assuming Food has a lifespan - adjust this based on your Food implementation
        float currentAge = Time.time - spawnTime;
        float maxAge = 230f; // From your food lifespan setting

        return Mathf.Clamp01(currentAge / maxAge);
    }

    private void UpdateSpriteForAge(float ageProgress)
    {
        Sprite targetSprite = freshFoodSprite;

        if (ageProgress < 0.4f)
        {
            targetSprite = freshFoodSprite;
        }
        else if (ageProgress < 0.8f)
        {
            targetSprite = agingFoodSprite ?? freshFoodSprite;
        }
        else
        {
            targetSprite = rottingFoodSprite ?? agingFoodSprite ?? freshFoodSprite;
        }

        if (targetSprite != null)
        {
            mainSpriteRenderer.sprite = targetSprite;
        }
    }

    private void UpdateColorForAge(float ageProgress)
    {
        Color targetColor;

        if (ageProgress < 0.4f)
        {
            targetColor = freshColor;
        }
        else if (ageProgress < 0.8f)
        {
            float lerpValue = (ageProgress - 0.4f) / 0.4f;
            targetColor = Color.Lerp(freshColor, agingColor, lerpValue);
        }
        else
        {
            float lerpValue = (ageProgress - 0.8f) / 0.2f;
            targetColor = Color.Lerp(agingColor, rottingColor, lerpValue);
        }

        mainSpriteRenderer.color = targetColor;
    }

    private void UpdateEffectsForAge(float ageProgress)
    {
        // Fresh food pulse effect
        if (enableFreshPulse && ageProgress < 0.3f)
        {
            float pulseValue = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
            float scaleMultiplier = Mathf.Lerp(1f, maxPulseScale, pulseValue * 0.2f);
            transform.localScale = originalScale * scaleMultiplier;
        }
        else if (enableAgingEffects && ageProgress > 0.9f)
        {
            // Slight wobble for very old food
            float wobble = Mathf.Sin(Time.time * pulseSpeed * 2f) * 0.05f;
            transform.localScale = originalScale * (1f + wobble);
        }
        else
        {
            transform.localScale = originalScale;
        }
    }

    // ========================================================================
    // EFFECT METHODS
    // ========================================================================

    public void PlaySpawnEffect()
    {
        if (currentEffectCoroutine != null)
            StopCoroutine(currentEffectCoroutine);

        currentEffectCoroutine = StartCoroutine(SpawnEffectCoroutine());
    }

    public void PlayConsumptionEffect()
    {
        isConsumed = true;

        if (currentEffectCoroutine != null)
            StopCoroutine(currentEffectCoroutine);

        currentEffectCoroutine = StartCoroutine(ConsumptionEffectCoroutine());
    }

    public void PlayDecayEffect()
    {
        isDecaying = true;

        if (currentEffectCoroutine != null)
            StopCoroutine(currentEffectCoroutine);

        currentEffectCoroutine = StartCoroutine(DecayEffectCoroutine());
    }

    // ========================================================================
    // EFFECT COROUTINES
    // ========================================================================

    private IEnumerator SpawnEffectCoroutine()
    {
        // Start invisible and scale up
        Color startColor = mainSpriteRenderer.color;
        startColor.a = 0f;
        mainSpriteRenderer.color = startColor;
        transform.localScale = Vector3.zero;

        float elapsedTime = 0f;

        while (elapsedTime < spawnAnimationDuration)
        {
            float progress = elapsedTime / spawnAnimationDuration;

            // Scale animation
            float scaleValue = spawnScaleCurve.Evaluate(progress);
            transform.localScale = originalScale * scaleValue;

            // Fade in
            Color currentColor = mainSpriteRenderer.color;
            currentColor.a = progress;
            mainSpriteRenderer.color = currentColor;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final values
        transform.localScale = originalScale;
        Color finalColor = mainSpriteRenderer.color;
        finalColor.a = 1f;
        mainSpriteRenderer.color = finalColor;

        currentEffectCoroutine = null;
    }

    private IEnumerator ConsumptionEffectCoroutine()
    {
        // Flash bright and scale up briefly
        Vector3 startScale = transform.localScale;
        Color startColor = mainSpriteRenderer.color;

        // Change to consumption sprite if available
        if (consumedSprite != null)
        {
            mainSpriteRenderer.sprite = consumedSprite;
        }

        float elapsedTime = 0f;

        while (elapsedTime < consumptionFlashDuration)
        {
            float progress = elapsedTime / consumptionFlashDuration;

            // Scale pulse
            float scaleMultiplier = Mathf.Lerp(consumptionScaleBoost, 1f, progress);
            transform.localScale = startScale * scaleMultiplier;

            // Color flash
            Color flashColor = Color.Lerp(consumedColor, startColor, progress);
            mainSpriteRenderer.color = flashColor;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Start fade out
        elapsedTime = 0f;
        float fadeOutDuration = consumptionFlashDuration * 0.5f;

        while (elapsedTime < fadeOutDuration)
        {
            float progress = elapsedTime / fadeOutDuration;

            Color fadeColor = mainSpriteRenderer.color;
            fadeColor.a = Mathf.Lerp(1f, 0f, progress);
            mainSpriteRenderer.color = fadeColor;

            // Shrink slightly
            float shrinkScale = Mathf.Lerp(1f, 0.8f, progress);
            transform.localScale = startScale * shrinkScale;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Object will be destroyed by Food component
        currentEffectCoroutine = null;
    }

    private IEnumerator DecayEffectCoroutine()
    {
        // Flash effect to indicate decay/despawn
        Color startColor = mainSpriteRenderer.color;

        for (int i = 0; i < decayFlashCount; i++)
        {
            // Flash to decay color
            mainSpriteRenderer.color = rottingColor;
            yield return new WaitForSeconds(decayAnimationDuration / (decayFlashCount * 4));

            // Flash back to original
            mainSpriteRenderer.color = startColor;
            yield return new WaitForSeconds(decayAnimationDuration / (decayFlashCount * 4));
        }

        // Final fade out
        float fadeTime = decayAnimationDuration * 0.3f;
        float elapsedTime = 0f;

        while (elapsedTime < fadeTime)
        {
            float progress = elapsedTime / fadeTime;

            // Fade out
            Color fadeColor = startColor;
            fadeColor.a = Mathf.Lerp(1f, 0f, progress);
            mainSpriteRenderer.color = fadeColor;

            // Shrink
            float shrinkScale = Mathf.Lerp(1f, 0.5f, progress);
            transform.localScale = originalScale * shrinkScale;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Object will be destroyed by Food component
        currentEffectCoroutine = null;
    }

    // ========================================================================
    // PROCEDURAL SPRITE CREATION
    // ========================================================================

    private void CreateDefaultFoodSprites()
    {
        // Create simple food sprites if none are assigned
        freshFoodSprite = CreateFoodSprite(20, freshColor, FoodShape.Circle);
        agingFoodSprite = CreateFoodSprite(18, agingColor, FoodShape.Square);
        rottingFoodSprite = CreateFoodSprite(16, rottingColor, FoodShape.Triangle);
        consumedSprite = CreateFoodSprite(24, consumedColor, FoodShape.Star);

        Debug.Log("Created default procedural food sprites");
    }

    private enum FoodShape { Circle, Square, Triangle, Star }

    private Sprite CreateFoodSprite(int size, Color color, FoodShape shape)
    {
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool isInShape = false;
                Vector2 pos = new Vector2(x, y);

                switch (shape)
                {
                    case FoodShape.Circle:
                        isInShape = Vector2.Distance(pos, center) <= size * 0.4f;
                        break;

                    case FoodShape.Square:
                        isInShape = Mathf.Abs(x - center.x) <= size * 0.3f &&
                                   Mathf.Abs(y - center.y) <= size * 0.3f;
                        break;

                    case FoodShape.Triangle:
                        float triangleHeight = size * 0.6f;
                        float triangleBase = size * 0.6f;
                        float relativeY = y - (center.y - triangleHeight * 0.3f);
                        float maxWidth = (triangleBase * (triangleHeight - relativeY)) / triangleHeight;
                        isInShape = relativeY >= 0 && relativeY <= triangleHeight &&
                                   Mathf.Abs(x - center.x) <= maxWidth * 0.5f;
                        break;

                    case FoodShape.Star:
                        isInShape = IsPointInStar(pos, center, size * 0.4f, 5);
                        break;
                }

                pixels[y * size + x] = isInShape ? color : Color.clear;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    private bool IsPointInStar(Vector2 point, Vector2 center, float radius, int points)
    {
        Vector2 offset = point - center;
        float angle = Mathf.Atan2(offset.y, offset.x);
        float distance = offset.magnitude;

        // Normalize angle to 0-2?
        if (angle < 0) angle += 2 * Mathf.PI;

        // Calculate the angle for each star point
        float segmentAngle = (2 * Mathf.PI) / points;
        float localAngle = angle % segmentAngle;

        // Calculate the radius at this angle (star shape)
        float innerRadius = radius * 0.5f;
        float t = Mathf.Abs(localAngle - segmentAngle * 0.5f) / (segmentAngle * 0.5f);
        float currentRadius = Mathf.Lerp(radius, innerRadius, t);

        return distance <= currentRadius;
    }

    // ========================================================================
    // PUBLIC INTERFACE
    // ========================================================================

    public void SetFoodColors(Color fresh, Color aging, Color rotting)
    {
        freshColor = fresh;
        agingColor = aging;
        rottingColor = rotting;
    }

    public void SetPulseSettings(float speed, float maxScale, bool enablePulse)
    {
        pulseSpeed = speed;
        maxPulseScale = maxScale;
        enableFreshPulse = enablePulse;
    }

    public float GetAgeProgress()
    {
        return GetFoodAgeProgress();
    }

    public bool IsPlayingEffect()
    {
        return currentEffectCoroutine != null;
    }

    // Called by Food component when consumed
    public void OnConsumed()
    {
        PlayConsumptionEffect();
    }

    // Called by Food component when decaying
    public void OnDecay()
    {
        PlayDecayEffect();
    }

    void OnDestroy()
    {
        if (currentEffectCoroutine != null)
        {
            StopCoroutine(currentEffectCoroutine);
        }
    }
}