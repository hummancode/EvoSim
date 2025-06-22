// ============================================================================
// FILE: AgentVisualController.cs - INTEGRATED WITH SPRITE SCALE CONTROLLER
// PURPOSE: Visual management that delegates scaling to SpriteScaleController
// ============================================================================

using UnityEngine;
using System.Collections;

/// <summary>
/// Enhanced visual controller that delegates scaling to SpriteScaleController
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
    [SerializeField] private Color normalFluffColor = Color.white;
    [SerializeField] private Color babyFluffColor = new Color(0.7f, 0.9f, 1f, 1f);
    [SerializeField] private Color childFluffColor = new Color(0.8f, 1f, 0.8f, 1f);
    [SerializeField] private Color adultFluffColor = Color.white;
    [SerializeField] private Color elderlyFluffColor = new Color(0.9f, 0.9f, 0.7f, 1f);
    [SerializeField] private Color matingFluffColor = new Color(1f, 0.5f, 0.8f, 1f);
    [SerializeField] private Color hungryFluffColor = new Color(1f, 0.8f, 0.3f, 1f);
    [SerializeField] private Color lowEnergyFluffColor = Color.red;

    [Header("📏 SCALING CONTROL")]
    [SerializeField] private bool useScaleController = true; // Enable/disable scale controller integration
    [SerializeField] private bool debugScaling = false;

    [Header("Effect Settings")]
    [SerializeField] private float matingPulseSpeed = 2f;
    [SerializeField] private float hungryFlashSpeed = 1.5f;
    [SerializeField] private float deathFlashDuration = 1f;
    [SerializeField] private int deathFlashCount = 3;
    [SerializeField] private float geneticColorInfluence = 0.2f;
    [SerializeField] private float fluffGeneticInfluence = 0.15f;

    // Component references
    private AgeSystem ageSystem;
    private AgeLifeStageTracker lifeStageTracker;
    private EnergySystem energySystem;
    private ReproductionSystem reproductionSystem;
    private GeneticsSystem geneticsSystem;
    private SheepLikeGeneticFluff fluffSystem;
    private SpriteScaleController scaleController; // NEW: Scale controller reference

    // Fluff renderer references
    private SpriteRenderer headFluffRenderer;
    private SpriteRenderer bodyFluffRenderer;

    // Visual state
    private bool isDead = false;
    private bool isMating = false;
    private bool isHungry = false;
    private Coroutine currentEffectCoroutine;

    // Original values
    private Sprite originalSprite;
    private Color originalColor;
    private Color originalFluffColor;

    void Awake()
    {
        SetupComponents();
        SetupSpriteRenderers();

        // NEW: Setup scale controller
        SetupScaleController();

        // Force fluff system setup first
        if (fluffSystem != null)
        {
            fluffSystem.SendMessage("SetupComponents", SendMessageOptions.DontRequireReceiver);
        }

        FindFluffRenderers();
        CacheOriginalValues();
    }

    void Start()
    {
        SubscribeToEvents();

        // NEW: Force initial scale update via scale controller
        if (useScaleController && scaleController != null)
        {
            scaleController.ForceUpdateScale();

            if (debugScaling)
            {
                Debug.Log($"[AgentVisualController] {gameObject.name} - Scale controller force update triggered");
            }
        }

        UpdateVisuals();
    }

    void Update()
    {
        if (!isDead)
        {
            UpdateVisuals();
        }
    }

    /// <summary>
    /// NEW: Setup scale controller
    /// </summary>
    private void SetupScaleController()
    {
        scaleController = GetComponent<SpriteScaleController>();

        if (useScaleController && scaleController == null)
        {
            // Auto-add scale controller if it doesn't exist
            scaleController = gameObject.AddComponent<SpriteScaleController>();

            if (debugScaling)
            {
                Debug.Log($"[AgentVisualController] {gameObject.name} - Auto-added SpriteScaleController");
            }
        }

        if (debugScaling)
        {
            Debug.Log($"[AgentVisualController] {gameObject.name} - Scale controller setup: {scaleController != null}");
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
            effectSpriteRenderer.sortingOrder = mainSpriteRenderer.sortingOrder + 10;
        }

        if (babySprite == null) CreateDefaultSprites();
    }

    private void FindFluffRenderers()
    {
        Transform headFluff = transform.Find("HeadFluff");
        if (headFluff != null)
        {
            headFluffRenderer = headFluff.GetComponent<SpriteRenderer>();
        }

        Transform bodyFluff = transform.Find("BodyFluff");
        if (bodyFluff != null)
        {
            bodyFluffRenderer = bodyFluff.GetComponent<SpriteRenderer>();
        }

        if (debugScaling)
        {
            Debug.Log($"[AgentVisualController] Found head fluff: {headFluffRenderer != null}, body fluff: {bodyFluffRenderer != null}");
        }
    }

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
    /// Main visual update method - NO MORE SCALING HERE!
    /// </summary>
    private void UpdateVisuals()
    {
        UpdateAgeVisuals();
        UpdateEnergyVisuals();
        UpdateGeneticVisuals();

        if (controlFluffColors)
        {
            UpdateFluffColors();
        }
    }

    /// <summary>
    /// SIMPLIFIED: Age visual updates WITHOUT scaling (scaling is handled by SpriteScaleController)
    /// </summary>
    private void UpdateAgeVisuals()
    {
        if (lifeStageTracker == null) return;

        // Update sprite based on life stage
        if (!isMating && !isDead)
        {
            Sprite targetSprite = GetSpriteForLifeStage(lifeStageTracker.CurrentStage);
            if (targetSprite != null)
            {
                mainSpriteRenderer.sprite = targetSprite;
            }
        }

        // Update color based on age (if not overridden by other states)
        if (!isMating && !isHungry && !isDead)
        {
            Color targetColor = GetColorForLifeStage(lifeStageTracker.CurrentStage);
            mainSpriteRenderer.color = targetColor;
        }

        // REMOVED: No more scaling code here! SpriteScaleController handles it automatically
    }

    private void UpdateEnergyVisuals()
    {
        if (energySystem == null) return;

        bool wasHungry = isHungry;
        isHungry = energySystem.IsHungry;

        if (isHungry && !wasHungry && !isMating && !isDead)
        {
            StartHungryEffect();
        }
        else if (!isHungry && wasHungry)
        {
            StopHungryEffect();
        }

        if (energySystem.EnergyPercent < 0.2f && !isMating && !isDead)
        {
            Color lowEnergyTint = Color.Lerp(mainSpriteRenderer.color, lowEnergyColor, 0.3f);
            mainSpriteRenderer.color = lowEnergyTint;
        }
    }

    private void UpdateGeneticVisuals()
    {
        if (geneticsSystem == null || isMating || isDead) return;

        Color geneticColor = geneticsSystem.BaseColor;
        Color currentColor = mainSpriteRenderer.color;
        Color blendedColor = Color.Lerp(currentColor, geneticColor, geneticColorInfluence);
        mainSpriteRenderer.color = blendedColor;
    }

    private void UpdateFluffColors()
    {
        if (!controlFluffColors) return;

        Color targetFluffColor = GetFluffColorForCurrentState();

        if (geneticsSystem != null && !isMating && !isDead)
        {
            Color geneticColor = geneticsSystem.BaseColor;
            targetFluffColor = Color.Lerp(targetFluffColor, geneticColor, fluffGeneticInfluence);
        }

        if (headFluffRenderer != null)
        {
            headFluffRenderer.color = targetFluffColor;
        }

        if (bodyFluffRenderer != null)
        {
            Color bodyColor = targetFluffColor;
            bodyColor.r *= 0.95f;
            bodyColor.g *= 0.95f;
            bodyFluffRenderer.color = bodyColor;
        }
    }

    private Color GetFluffColorForCurrentState()
    {
        if (isDead) return Color.gray;
        if (isMating) return matingFluffColor;
        if (isHungry) return hungryFluffColor;
        if (energySystem != null && energySystem.EnergyPercent < 0.2f) return lowEnergyFluffColor;

        if (lifeStageTracker != null)
        {
            return GetFluffColorForLifeStage(lifeStageTracker.CurrentStage);
        }

        return normalFluffColor;
    }

    private Color GetFluffColorForLifeStage(AgeLifeStageTracker.LifeStage stage)
    {
        switch (stage)
        {
            case AgeLifeStageTracker.LifeStage.Baby: return babyFluffColor;
            case AgeLifeStageTracker.LifeStage.Child: return childFluffColor;
            case AgeLifeStageTracker.LifeStage.Adult: return adultFluffColor;
            case AgeLifeStageTracker.LifeStage.Elderly: return elderlyFluffColor;
            default: return normalFluffColor;
        }
    }

    // ========================================================================
    // EVENT HANDLERS - UPDATED TO USE SCALE CONTROLLER
    // ========================================================================

    private void OnLifeStageChanged(AgeLifeStageTracker.LifeStage newStage)
    {
        if (debugScaling)
        {
            Debug.Log($"[AgentVisualController] {gameObject.name} LifeStage changed to: {newStage}");
        }

        // NEW: Delegate scaling to SpriteScaleController (it will handle this automatically)
        // No need to call anything - SpriteScaleController subscribes to the same event!

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
    // VISUAL EFFECTS (unchanged)
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

    private IEnumerator MatingEffectCoroutine()
    {
        if (matingSprite != null)
        {
            mainSpriteRenderer.sprite = matingSprite;
        }

        while (isMating)
        {
            float pulse = (Mathf.Sin(Time.time * matingPulseSpeed) + 1f) * 0.5f;

            Color pulseColor = Color.Lerp(normalColor, matingColor, pulse * 0.7f);
            mainSpriteRenderer.color = pulseColor;

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

        while (isHungry && !isMating && !isDead)
        {
            float flash = (Mathf.Sin(Time.time * hungryFlashSpeed) + 1f) * 0.5f;

            Color flashColor = Color.Lerp(normalColor, hungryColor, flash * 0.5f);
            mainSpriteRenderer.color = flashColor;

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

        for (int i = 0; i < deathFlashCount; i++)
        {
            mainSpriteRenderer.color = Color.white;
            if (controlFluffColors)
            {
                if (headFluffRenderer != null) headFluffRenderer.color = Color.white;
                if (bodyFluffRenderer != null) bodyFluffRenderer.color = Color.white;
            }
            yield return new WaitForSeconds(deathFlashDuration / (deathFlashCount * 2));

            mainSpriteRenderer.color = Color.red;
            if (controlFluffColors)
            {
                if (headFluffRenderer != null) headFluffRenderer.color = Color.red;
                if (bodyFluffRenderer != null) bodyFluffRenderer.color = Color.red;
            }
            yield return new WaitForSeconds(deathFlashDuration / (deathFlashCount * 2));
        }

        float fadeTime = 0.5f;
        Color startColor = mainSpriteRenderer.color;
        Color startFluffColor = controlFluffColors && headFluffRenderer != null ? headFluffRenderer.color : Color.white;

        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            float alpha = Mathf.Lerp(1f, 0f, t / fadeTime);

            Color fadeColor = startColor;
            fadeColor.a = alpha;
            mainSpriteRenderer.color = fadeColor;

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
    // HELPER METHODS (unchanged)
    // ========================================================================

    private Sprite GetSpriteForLifeStage(AgeLifeStageTracker.LifeStage stage)
    {
        switch (stage)
        {
            case AgeLifeStageTracker.LifeStage.Baby: return babySprite ?? originalSprite;
            case AgeLifeStageTracker.LifeStage.Child: return childSprite ?? originalSprite;
            case AgeLifeStageTracker.LifeStage.Adult: return adultSprite ?? originalSprite;
            case AgeLifeStageTracker.LifeStage.Elderly: return elderlySprite ?? originalSprite;
            default: return originalSprite;
        }
    }

    private Color GetColorForLifeStage(AgeLifeStageTracker.LifeStage stage)
    {
        switch (stage)
        {
            case AgeLifeStageTracker.LifeStage.Baby: return babyColor;
            case AgeLifeStageTracker.LifeStage.Child: return childColor;
            case AgeLifeStageTracker.LifeStage.Adult: return adultColor;
            case AgeLifeStageTracker.LifeStage.Elderly: return elderlyColor;
            default: return normalColor;
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
    // PUBLIC INTERFACE & SCALE CONTROLLER INTEGRATION
    // ========================================================================

    /// <summary>
    /// NEW: Force scale update via scale controller
    /// </summary>
    [ContextMenu("🔧 Force Scale Update")]
    public void ForceScaleUpdate()
    {
        if (useScaleController && scaleController != null)
        {
            scaleController.ForceUpdateScale();
            Debug.Log($"[AgentVisualController] {gameObject.name} - Forced scale update via SpriteScaleController");
        }
        else
        {
            Debug.LogWarning($"[AgentVisualController] {gameObject.name} - Scale controller not available or disabled");
        }
    }

    /// <summary>
    /// NEW: Test scale controller integration
    /// </summary>
    [ContextMenu("🧪 Test Scale Controller")]
    public void TestScaleController()
    {
        if (scaleController != null)
        {
            Debug.Log($"[AgentVisualController] Testing scale controller on {gameObject.name}");
            scaleController.TestAllScales();
        }
        else
        {
            Debug.LogError($"[AgentVisualController] {gameObject.name} - No SpriteScaleController found!");
        }
    }

    /// <summary>
    /// NEW: Toggle scale controller usage
    /// </summary>
    [ContextMenu("🔄 Toggle Scale Controller")]
    public void ToggleScaleController()
    {
        useScaleController = !useScaleController;
        Debug.Log($"[AgentVisualController] {gameObject.name} - Scale controller usage: {useScaleController}");

        if (useScaleController && scaleController != null)
        {
            scaleController.ForceUpdateScale();
        }
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
    /// NEW: Get scale controller reference (for external access)
    /// </summary>
    public SpriteScaleController GetScaleController()
    {
        return scaleController;
    }

    void OnDestroy()
    {
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
🎨 AGENT VISUAL CONTROLLER WITH SPRITE SCALE CONTROLLER INTEGRATION

✅ KEY CHANGES:
📏 DELEGATED SCALING: All scaling logic moved to SpriteScaleController
🔧 AUTO-SETUP: Automatically adds SpriteScaleController if missing
🎯 CLEAN SEPARATION: Visual effects separate from scaling logic
🔄 PROPER INTEGRATION: Both controllers subscribe to same life stage events

🆕 NEW FEATURES:
• useScaleController toggle to enable/disable integration
• Auto-adds SpriteScaleController component if needed
• Debug methods to test scale controller
• GetScaleController() for external access

🔧 HOW IT WORKS:
1. AgentVisualController handles colors, sprites, effects
2. SpriteScaleController handles ALL scaling automatically
3. Both subscribe to OnLifeStageChanged independently
4. No conflicts, clean separation of concerns

📊 SETUP STEPS:
1. Replace your AgentVisualController with this version
2. It will auto-add SpriteScaleController component
3. Configure scale values in SpriteScaleController inspector
4. Enable debugScaling if you want logs

🧪 TESTING:
• "Force Scale Update" - Test immediate scaling
• "Test Scale Controller" - Cycle through all life stages
• "Toggle Scale Controller" - Enable/disable scaling

RESULT: Clean, working age-based scaling with proper separation! 📏✨
The fluff system stays unchanged, scaling is handled separately! 🐑
========================================================================
*/