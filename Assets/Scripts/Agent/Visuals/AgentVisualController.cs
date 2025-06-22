// ============================================================================
// FILE: IntegratedAgentVisualController.cs
// PURPOSE: Complete visual management including main sprite AND fluff colors
// ============================================================================

using UnityEngine;
using System.Collections;

/// <summary>
/// Enhanced visual controller that manages all agent visuals including fluff
/// </summary>
public class AgentVisualController : MonoBehaviour
{
    [Header("Sprite References")]
    [SerializeField] private SpriteRenderer mainSpriteRenderer;
    [SerializeField] private SpriteRenderer effectSpriteRenderer;

    [Header("Agent Sprites")]
    [SerializeField] private Sprite babySprite;
    [SerializeField] private Sprite childSprite;
    [SerializeField] private Sprite adultSprite;
    [SerializeField] private Sprite elderlySprite;

    [Header("State Sprites")]
    [SerializeField] private Sprite matingSprite;
    [SerializeField] private Sprite hungrySprite;
    [SerializeField] private Sprite deathSprite;

    [Header("Main Sprite Colors")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color babyColor = new Color(0.7f, 0.9f, 1f, 1f);
    [SerializeField] private Color childColor = new Color(0.8f, 1f, 0.8f, 1f);
    [SerializeField] private Color adultColor = Color.white;
    [SerializeField] private Color elderlyColor = new Color(0.9f, 0.9f, 0.7f, 1f);
    [SerializeField] private Color matingColor = new Color(1f, 0.5f, 0.8f, 1f);
    [SerializeField] private Color hungryColor = new Color(1f, 0.8f, 0.3f, 1f);
    [SerializeField] private Color lowEnergyColor = Color.red;

    [Header("🐑 FLUFF COLOR CONTROL")]
    [SerializeField] public bool controlFluffColors = true;
    [SerializeField] private Color normalFluffColor = Color.white;                        // Same as normalColor
    [SerializeField] private Color babyFluffColor = new Color(0.7f, 0.9f, 1f, 1f);      // Same as babyColor
    [SerializeField] private Color childFluffColor = new Color(0.8f, 1f, 0.8f, 1f);     // Same as childColor
    [SerializeField] private Color adultFluffColor = Color.white;                        // Same as adultColor
    [SerializeField] private Color elderlyFluffColor = new Color(0.9f, 0.9f, 0.7f, 1f); // Same as elderlyColor
    [SerializeField] private Color matingFluffColor = new Color(1f, 0.5f, 0.8f, 1f);    // Same as matingColor
    [SerializeField] private Color hungryFluffColor = new Color(1f, 0.8f, 0.3f, 1f);    // Same as hungryColor
    [SerializeField] private Color lowEnergyFluffColor = Color.red;

    [Header("Effect Settings")]
    [SerializeField] private float matingPulseSpeed = 2f;
    [SerializeField] private float hungryFlashSpeed = 1.5f;
    [SerializeField] private float deathFlashDuration = 1f;
    [SerializeField] private int deathFlashCount = 3;
    [SerializeField] private float geneticColorInfluence = 0.2f;
    [SerializeField] private float fluffGeneticInfluence = 0.15f; // Fluff gets less genetic influence

    // Component references
    private AgeSystem ageSystem;
    private AgeLifeStageTracker lifeStageTracker;
    private EnergySystem energySystem;
    private ReproductionSystem reproductionSystem;
    private GeneticsSystem geneticsSystem;
    private SheepLikeGeneticFluff fluffSystem; // Reference to fluff system

    // Fluff renderer references (found automatically)
    private SpriteRenderer headFluffRenderer;
    private SpriteRenderer bodyFluffRenderer;

    // Visual state
    private bool isDead = false;
    private bool isMating = false;
    private bool isHungry = false;
    private Coroutine currentEffectCoroutine;

    // Original values for restoration
    private Sprite originalSprite;
    private Color originalColor;
    private Color originalFluffColor;


    void Awake()
    {
        // ENSURE fluff system is initialized FIRST
        SheepLikeGeneticFluff fluffSystem = GetComponent<SheepLikeGeneticFluff>();
        if (fluffSystem != null)
        {
            // Force the fluff system to setup components if it hasn't already
            fluffSystem.SendMessage("SetupComponents", SendMessageOptions.DontRequireReceiver);
        }

        SetupComponents();
        SetupSpriteRenderers();
        FindFluffRenderers(); // This should now find the fluff renderers
        CacheOriginalValues();
    }

    void Start()
    {
        SubscribeToEvents();
        UpdateVisuals();
    }

    void Update()
    {
        if (!isDead)
        {
            UpdateVisuals();
        }
    }

    private void SetupComponents()
    {
        ageSystem = GetComponent<AgeSystem>();
        lifeStageTracker = GetComponent<AgeLifeStageTracker>();
        energySystem = GetComponent<EnergySystem>();
        reproductionSystem = GetComponent<ReproductionSystem>();
        geneticsSystem = GetComponent<GeneticsSystem>();
        fluffSystem = GetComponent<SheepLikeGeneticFluff>();
    }

    private void SetupSpriteRenderers()
    {
        if (mainSpriteRenderer == null)
        {
            mainSpriteRenderer = GetComponent<SpriteRenderer>();
            if (mainSpriteRenderer == null)
            {
                mainSpriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
        }

        if (effectSpriteRenderer == null)
        {
            GameObject effectObj = new GameObject("EffectRenderer");
            effectObj.transform.SetParent(transform);
            effectObj.transform.localPosition = Vector3.zero;
            effectObj.transform.localScale = Vector3.one;

            effectSpriteRenderer = effectObj.AddComponent<SpriteRenderer>();
            effectSpriteRenderer.sortingOrder = mainSpriteRenderer.sortingOrder + 10; // Above everything
        }

        if (babySprite == null) CreateDefaultSprites();
    }

    /// <summary>
    /// Find fluff renderers automatically
    /// </summary>


    private void CacheOriginalValues()
    {
        originalSprite = mainSpriteRenderer.sprite;
        originalColor = mainSpriteRenderer.color;
        originalFluffColor = normalFluffColor;
    }

    private void SubscribeToEvents()
    {
        if (lifeStageTracker != null)
        {
            lifeStageTracker.OnLifeStageChanged += OnLifeStageChanged;
        }

        if (reproductionSystem != null)
        {
            reproductionSystem.OnMatingStarted += OnMatingStarted;
            reproductionSystem.OnMatingCompleted += OnMatingCompleted;
        }

        if (energySystem != null)
        {
            energySystem.OnDeath += OnDeath;
        }
    }

    /// <summary>
    /// Main visual update method - now includes fluff colors
    /// </summary>
    private void UpdateVisuals()
    {
        UpdateAgeVisuals();
        UpdateEnergyVisuals();
        UpdateGeneticVisuals();

        // NEW: Update fluff colors
        if (controlFluffColors)
        {
            UpdateFluffColors();
        }
    }

    private void UpdateAgeVisuals()
    {
        if (lifeStageTracker == null || ageSystem == null) return;

        // Update sprite based on life stage
        Sprite targetSprite = GetSpriteForLifeStage(lifeStageTracker.CurrentStage);
        if (targetSprite != null && !isMating && !isDead)
        {
            mainSpriteRenderer.sprite = targetSprite;
        }

        // Update color based on age (if not overridden by other states)
        if (!isMating && !isHungry && !isDead)
        {
            Color targetColor = GetColorForLifeStage(lifeStageTracker.CurrentStage);
            mainSpriteRenderer.color = targetColor;
        }

        // Update scale based on age
        float scaleMultiplier = GetScaleForLifeStage(lifeStageTracker.CurrentStage);
        transform.localScale = Vector3.one * scaleMultiplier;
    }

    private void UpdateEnergyVisuals()
    {
        if (energySystem == null) return;

        bool wasHungry = isHungry;
        isHungry = energySystem.IsHungry;

        // Start/stop hungry effect
        if (isHungry && !wasHungry && !isMating && !isDead)
        {
            StartHungryEffect();
        }
        else if (!isHungry && wasHungry)
        {
            StopHungryEffect();
        }

        // Very low energy warning
        if (energySystem.EnergyPercent < 0.2f && !isMating && !isDead)
        {
            Color lowEnergyTint = Color.Lerp(mainSpriteRenderer.color, lowEnergyColor, 0.3f);
            mainSpriteRenderer.color = lowEnergyTint;
        }
    }

    private void UpdateGeneticVisuals()
    {
        if (geneticsSystem == null || isMating || isDead) return;

        // Blend genetic color traits with current color
        Color geneticColor = geneticsSystem.BaseColor;
        Color currentColor = mainSpriteRenderer.color;

        // Genetic influence on main sprite
        Color blendedColor = Color.Lerp(currentColor, geneticColor, geneticColorInfluence);
        mainSpriteRenderer.color = blendedColor;
    }

    /// <summary>
    /// NEW: Update fluff colors based on current state
    /// </summary>
    /// <summary>
    /// Find fluff renderers automatically - with NULL safety
    /// </summary>
    private void FindFluffRenderers()
    {
        // DEBUG: List all child objects
        Debug.Log($"Agent {gameObject.name} has {transform.childCount} children:");
        for (int i = 0; i < transform.childCount; i++)
        {
            Debug.Log($"  Child {i}: {transform.GetChild(i).name}");
        }

        // Find head fluff renderer
        Transform headFluff = transform.Find("HeadFluff");
        if (headFluff != null)
        {
            headFluffRenderer = headFluff.GetComponent<SpriteRenderer>();
        }

        // Find body fluff renderer
        Transform bodyFluff = transform.Find("BodyFluff");
        if (bodyFluff != null)
        {
            bodyFluffRenderer = bodyFluff.GetComponent<SpriteRenderer>();
        }

        Debug.Log($"Visual Controller: Found head fluff: {headFluffRenderer != null}, body fluff: {bodyFluffRenderer != null}");
    }

    /// <summary>
    /// NEW: Ensure fluff renderers are found before using them
    /// </summary>
    private void EnsureFluffRenderersFound()
    {
        // If renderers are null, try to find them again
        if (headFluffRenderer == null || bodyFluffRenderer == null)
        {
            Debug.Log("Fluff renderers are NULL - re-finding them...");
            FindFluffRenderers();
        }
    }

    /// <summary>
    /// NEW: Update fluff colors based on current state - with NULL safety
    /// </summary>
    private void UpdateFluffColors()
    {
        Debug.Log($"🔹 UpdateFluffColors START for {gameObject.name}");

        if (!controlFluffColors)
        {
            Debug.Log("❌ Fluff color control is DISABLED");
            return;
        }
        Debug.Log("✅ Fluff color control is ENABLED");

        // ENSURE renderers are found
        EnsureFluffRenderersFound();

        Debug.Log($"🔹 After EnsureFluffRenderersFound: head={headFluffRenderer != null}, body={bodyFluffRenderer != null}");

        if (headFluffRenderer == null && bodyFluffRenderer == null)
        {
            Debug.LogError("❌ BOTH FLUFF RENDERERS ARE NULL - CANNOT SET COLORS!");
            return;
        }

        Color targetFluffColor = GetFluffColorForCurrentState();
        Debug.Log($"🎨 Target fluff color: {targetFluffColor} (isDead:{isDead}, isMating:{isMating}, isHungry:{isHungry})");

        // Apply to head fluff
        if (headFluffRenderer != null)
        {
            Color oldColor = headFluffRenderer.color;
            headFluffRenderer.color = targetFluffColor;
            Debug.Log($"🔴 HEAD FLUFF: {oldColor} -> {headFluffRenderer.color}");
        }
        else
        {
            Debug.LogError("❌ headFluffRenderer is NULL!");
        }

        // Apply to body fluff
        if (bodyFluffRenderer != null)
        {
            Color bodyColor = targetFluffColor;
            bodyColor.r *= 0.95f;
            bodyColor.g *= 0.95f;

            Color oldBodyColor = bodyFluffRenderer.color;
            bodyFluffRenderer.color = bodyColor;
            Debug.Log($"🔵 BODY FLUFF: {oldBodyColor} -> {bodyFluffRenderer.color}");
        }
        else
        {
            Debug.LogError("❌ bodyFluffRenderer is NULL!");
        }

        Debug.Log($"🔹 UpdateFluffColors END for {gameObject.name}");
    }
    /// <summary>
    /// Get fluff color based on current agent state
    /// </summary>
    private Color GetFluffColorForCurrentState()
    {
        // Priority: death > mating > hungry > low energy > age-based > normal

        if (isDead)
        {
            return Color.gray; // Dead fluff is gray
        }

        if (isMating)
        {
            return matingFluffColor;
        }

        if (isHungry)
        {
            return hungryFluffColor;
        }

        if (energySystem != null && energySystem.EnergyPercent < 0.2f)
        {
            return lowEnergyFluffColor;
        }

        // Age-based fluff color
        if (lifeStageTracker != null)
        {
            return GetFluffColorForLifeStage(lifeStageTracker.CurrentStage);
        }

        return normalFluffColor;
    }

    /// <summary>
    /// Get fluff color for specific life stage
    /// </summary>
    private Color GetFluffColorForLifeStage(AgeLifeStageTracker.LifeStage stage)
    {
        switch (stage)
        {
            case AgeLifeStageTracker.LifeStage.Baby:
                return babyFluffColor;
            case AgeLifeStageTracker.LifeStage.Child:
                return childFluffColor;
            case AgeLifeStageTracker.LifeStage.Adult:
                return adultFluffColor;
            case AgeLifeStageTracker.LifeStage.Elderly:
                return elderlyFluffColor;
            default:
                return normalFluffColor;
        }
    }

    // ========================================================================
    // EVENT HANDLERS
    // ========================================================================

    private void OnLifeStageChanged(AgeLifeStageTracker.LifeStage newStage)
    {
        Debug.Log($"{gameObject.name} entered {newStage} stage - updating visuals and fluff");
        UpdateVisuals();
    }

    private void OnMatingStarted(IAgent partner)
    {
        StartMatingEffect();
    }

    private void OnMatingCompleted()
    {
        StopMatingEffect();
    }

    private void OnDeath()
    {
        StartDeathEffect();
    }

    // ========================================================================
    // VISUAL EFFECTS (Enhanced with fluff support)
    // ========================================================================

    private void StartMatingEffect()
    {
        isMating = true;
        StopCurrentEffect();
        currentEffectCoroutine = StartCoroutine(MatingEffectCoroutine());
    }

    private void StopMatingEffect()
    {
        isMating = false;
        StopCurrentEffect();
        UpdateVisuals();
    }

    private void StartHungryEffect()
    {
        if (!isMating && !isDead)
        {
            StopCurrentEffect();
            currentEffectCoroutine = StartCoroutine(HungryEffectCoroutine());
        }
    }

    private void StopHungryEffect()
    {
        if (currentEffectCoroutine != null && !isMating && !isDead)
        {
            StopCurrentEffect();
            UpdateVisuals();
        }
    }

    private void StartDeathEffect()
    {
        isDead = true;
        StopCurrentEffect();
        currentEffectCoroutine = StartCoroutine(DeathEffectCoroutine());
    }

    private void StopCurrentEffect()
    {
        if (currentEffectCoroutine != null)
        {
            StopCoroutine(currentEffectCoroutine);
            currentEffectCoroutine = null;
        }
    }

    // ========================================================================
    // ENHANCED EFFECT COROUTINES (with fluff color changes)
    // ========================================================================

    private IEnumerator MatingEffectCoroutine()
    {
        if (matingSprite != null)
        {
            mainSpriteRenderer.sprite = matingSprite;
        }

        // Pulsing effect on both sprite and fluff
        while (isMating)
        {
            float pulse = (Mathf.Sin(Time.time * matingPulseSpeed) + 1f) * 0.5f;

            // Pulse main sprite
            Color pulseColor = Color.Lerp(normalColor, matingColor, pulse * 0.7f);
            mainSpriteRenderer.color = pulseColor;

            // Pulse fluff colors
            if (controlFluffColors)
            {
                Color fluffPulseColor = Color.Lerp(normalFluffColor, matingFluffColor, pulse * 0.5f);

                if (headFluffRenderer != null)
                    headFluffRenderer.color = fluffPulseColor;

                if (bodyFluffRenderer != null)
                    bodyFluffRenderer.color = fluffPulseColor;
            }

            yield return null;
        }
    }

    private IEnumerator HungryEffectCoroutine()
    {
        Sprite previousSprite = mainSpriteRenderer.sprite;
        if (hungrySprite != null)
        {
            mainSpriteRenderer.sprite = hungrySprite;
        }

        // Flashing effect on both sprite and fluff
        while (isHungry && !isMating && !isDead)
        {
            float flash = (Mathf.Sin(Time.time * hungryFlashSpeed) + 1f) * 0.5f;

            // Flash main sprite
            Color flashColor = Color.Lerp(normalColor, hungryColor, flash * 0.5f);
            mainSpriteRenderer.color = flashColor;

            // Flash fluff colors
            if (controlFluffColors)
            {
                Color fluffFlashColor = Color.Lerp(normalFluffColor, hungryFluffColor, flash * 0.3f);

                if (headFluffRenderer != null)
                    headFluffRenderer.color = fluffFlashColor;

                if (bodyFluffRenderer != null)
                    bodyFluffRenderer.color = fluffFlashColor;
            }

            yield return null;
        }

        if (hungrySprite != null)
        {
            mainSpriteRenderer.sprite = previousSprite;
        }
    }

    private IEnumerator DeathEffectCoroutine()
    {
        if (deathSprite != null)
        {
            mainSpriteRenderer.sprite = deathSprite;
        }

        // Flash effect
        for (int i = 0; i < deathFlashCount; i++)
        {
            // Flash white
            mainSpriteRenderer.color = Color.white;
            if (controlFluffColors)
            {
                if (headFluffRenderer != null) headFluffRenderer.color = Color.white;
                if (bodyFluffRenderer != null) bodyFluffRenderer.color = Color.white;
            }
            yield return new WaitForSeconds(deathFlashDuration / (deathFlashCount * 2));

            // Flash red
            mainSpriteRenderer.color = Color.red;
            if (controlFluffColors)
            {
                if (headFluffRenderer != null) headFluffRenderer.color = Color.red;
                if (bodyFluffRenderer != null) bodyFluffRenderer.color = Color.red;
            }
            yield return new WaitForSeconds(deathFlashDuration / (deathFlashCount * 2));
        }

        // Fade out
        float fadeTime = 0.5f;
        Color startColor = mainSpriteRenderer.color;
        Color startFluffColor = controlFluffColors && headFluffRenderer != null ? headFluffRenderer.color : Color.white;

        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            float alpha = Mathf.Lerp(1f, 0f, t / fadeTime);

            // Fade main sprite
            Color fadeColor = startColor;
            fadeColor.a = alpha;
            mainSpriteRenderer.color = fadeColor;

            // Fade fluff
            if (controlFluffColors)
            {
                Color fluffFadeColor = startFluffColor;
                fluffFadeColor.a = alpha;

                if (headFluffRenderer != null) headFluffRenderer.color = fluffFadeColor;
                if (bodyFluffRenderer != null) bodyFluffRenderer.color = fluffFadeColor;
            }

            yield return null;
        }
    }

    // ========================================================================
    // EXISTING HELPER METHODS (unchanged)
    // ========================================================================

    private Sprite GetSpriteForLifeStage(AgeLifeStageTracker.LifeStage stage)
    {
        switch (stage)
        {
            case AgeLifeStageTracker.LifeStage.Baby:
                return babySprite ?? originalSprite;
            case AgeLifeStageTracker.LifeStage.Child:
                return childSprite ?? originalSprite;
            case AgeLifeStageTracker.LifeStage.Adult:
                return adultSprite ?? originalSprite;
            case AgeLifeStageTracker.LifeStage.Elderly:
                return elderlySprite ?? originalSprite;
            default:
                return originalSprite;
        }
    }

    private Color GetColorForLifeStage(AgeLifeStageTracker.LifeStage stage)
    {
        switch (stage)
        {
            case AgeLifeStageTracker.LifeStage.Baby:
                return babyColor;
            case AgeLifeStageTracker.LifeStage.Child:
                return childColor;
            case AgeLifeStageTracker.LifeStage.Adult:
                return adultColor;
            case AgeLifeStageTracker.LifeStage.Elderly:
                return elderlyColor;
            default:
                return normalColor;
        }
    }

    private float GetScaleForLifeStage(AgeLifeStageTracker.LifeStage stage)
    {
        switch (stage)
        {
            case AgeLifeStageTracker.LifeStage.Baby:
                return 0.5f;
            case AgeLifeStageTracker.LifeStage.Child:
                return 0.75f;
            case AgeLifeStageTracker.LifeStage.Adult:
                return 1f;
            case AgeLifeStageTracker.LifeStage.Elderly:
                return 0.9f;
            default:
                return 1f;
        }
    }

    private void CreateDefaultSprites()
    {
        babySprite = CreateCircleSprite(16, Color.white);
        childSprite = CreateCircleSprite(20, Color.white);
        adultSprite = CreateCircleSprite(24, Color.white);
        elderlySprite = CreateCircleSprite(22, Color.white);
        matingSprite = CreateHeartSprite(24);
        hungrySprite = CreateCircleSprite(24, Color.white);
        deathSprite = CreateXSprite(24);
    }

    // ========================================================================
    // PROCEDURAL SPRITE CREATION (unchanged)
    // ========================================================================

    private Sprite CreateCircleSprite(int size, Color color)
    {
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
        float radius = size * 0.4f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                pixels[y * size + x] = distance <= radius ? color : Color.clear;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    private Sprite CreateHeartSprite(int size)
    {
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - size * 0.5f) / (size * 0.3f);
                float ny = (y - size * 0.6f) / (size * 0.3f);
                float heart = Mathf.Pow(nx * nx + ny * ny - 1, 3) - nx * nx * ny * ny * ny;
                pixels[y * size + x] = heart <= 0 ? Color.magenta : Color.clear;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    private Sprite CreateXSprite(int size)
    {
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool isOnDiagonal1 = Mathf.Abs(x - y) <= 2;
                bool isOnDiagonal2 = Mathf.Abs(x - (size - 1 - y)) <= 2;
                pixels[y * size + x] = (isOnDiagonal1 || isOnDiagonal2) ? Color.red : Color.clear;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    // ========================================================================
    // PUBLIC INTERFACE (enhanced)
    // ========================================================================

    public void SetCustomMatingColor(Color spriteColor, Color fluffColor)
    {
        matingColor = spriteColor;
        matingFluffColor = fluffColor;
    }

    public void SetCustomHungryColor(Color spriteColor, Color fluffColor)
    {
        hungryColor = spriteColor;
        hungryFluffColor = fluffColor;
    }

    public void ForceUpdateVisuals()
    {
        UpdateVisuals();
    }

    public bool IsPlayingEffect()
    {
        return currentEffectCoroutine != null;
    }

    /// <summary>
    /// Toggle fluff color control on/off
    /// </summary>
    [ContextMenu("Toggle Fluff Color Control")]
    public void ToggleFluffColorControl()
    {
        controlFluffColors = !controlFluffColors;
        Debug.Log($"Fluff color control: {controlFluffColors}");
        if (controlFluffColors) UpdateVisuals();
    }

    /// <summary>
    /// Test all visual states
    /// </summary>
    [ContextMenu("Test All Visual States")]
    public void TestAllVisualStates()
    {
        StartCoroutine(TestStatesCoroutine());
    }

    private IEnumerator TestStatesCoroutine()
    {
        Debug.Log("Testing normal state");
        yield return new WaitForSeconds(1f);

        Debug.Log("Testing mating state");
        StartMatingEffect();
        yield return new WaitForSeconds(2f);
        StopMatingEffect();

        Debug.Log("Testing hungry state");
        isHungry = true;
        StartHungryEffect();
        yield return new WaitForSeconds(2f);
        isHungry = false;
        StopHungryEffect();

        Debug.Log("Visual state test complete");
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (lifeStageTracker != null)
            lifeStageTracker.OnLifeStageChanged -= OnLifeStageChanged;

        if (reproductionSystem != null)
        {
            reproductionSystem.OnMatingStarted -= OnMatingStarted;
            reproductionSystem.OnMatingCompleted -= OnMatingCompleted;
        }

        if (energySystem != null)
            energySystem.OnDeath -= OnDeath;
    }

}

/*
========================================================================
🎨 INTEGRATED VISUAL CONTROLLER WITH FLUFF SUPPORT

NEW FEATURES:
✅ Controls BOTH main sprite AND fluff colors
✅ Separate color palettes for sprite vs fluff
✅ Automatic fluff renderer detection
✅ Age-based fluff color changes
✅ State-based fluff color changes (mating, hungry, etc.)
✅ Genetic influence on fluff colors
✅ Enhanced visual effects that affect fluff too

HOW TO USE:
1. Replace AgentVisualController with IntegratedAgentVisualController
2. It automatically finds HeadFluff and BodyFluff renderers
3. Configure colors in the inspector for both sprite and fluff
4. Enable "Control Fluff Colors" checkbox

COLOR COORDINATION:
🐑 Baby: Light cream fluff
🐑 Adult: Normal cream fluff  
🐑 Elderly: Greyish cream fluff
💗 Mating: Pink-tinted fluff + pulsing effect
🍎 Hungry: Yellow-tinted fluff + flashing effect
❤️ Low Energy: Reddish fluff
🧬 Genetics: Subtle color influence on both sprite and fluff

VISUAL EFFECTS:
- Mating: Both sprite and fluff pulse pink
- Hungry: Both sprite and fluff flash yellow/orange
- Death: Both sprite and fluff flash and fade together
- Age transitions: Smooth color changes for both

RESULT: Your sheep agents have coordinated sprite and fluff colors 
that change together based on their state! 🐑✨
========================================================================
*/