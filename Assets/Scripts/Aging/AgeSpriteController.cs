// FILE: AgeSpriteController.cs (Enhanced with aging and starvation effects)
// PURPOSE: Enhanced version with gradual whitening for old age and increasing starvation flash
// ============================================================================

using UnityEngine;
using System.Collections;

/// <summary>
/// Enhanced AgeSpriteController with aging whitening and progressive starvation effects
/// </summary>
public class AgeSpriteController : MonoBehaviour
{
    [Header("Visual Settings (Your Existing Settings)")]
    [SerializeField] private float maturityAge = 20f;
    [SerializeField] private float minScale = 0.5f;  // Baby size
    [SerializeField] private float maxScale = 1f;    // Adult size
    [SerializeField] private Color babyColor = new Color(0.2f, 0.4f, 0.8f, 1f); // Darker blue
    [SerializeField] private Color adultColor = Color.cyan;

    [Header("NEW: Aging Effects")]
    [SerializeField] private float oldAgeThreshold = 0.8f; // 80% of death age
    [SerializeField] private Color oldAgeColor = Color.white; // Gradually becomes white
    [SerializeField] private float maxWhitening = 0.7f; // How white they get (0-1)
    [SerializeField] private bool enableAgingWhitening = true;

    [Header("NEW: Starvation Effects")]
    [SerializeField] private float starvationThreshold = 0.3f; // 30% energy or below
    [SerializeField] private float criticalStarvationThreshold = 0.1f; // 10% energy - critical
    [SerializeField] private Color starvationColor = new Color(1f, 0.6f, 0.2f, 1f); // Orange-red
    [SerializeField] private float baseStarvationFlashSpeed = 1f;
    [SerializeField] private float maxStarvationFlashSpeed = 8f; // Max flash frequency
    [SerializeField] private bool enableStarvationEffects = true;

    [Header("Enhanced Visual Effects")]
    [SerializeField] private Color matingColor = new Color(1f, 0.5f, 0.8f, 1f); // Pink
    [SerializeField] private Color hungryColor = new Color(1f, 0.8f, 0.3f, 1f); // Orange
    [SerializeField] private Color lowEnergyColor = Color.red;
    [SerializeField] private float matingPulseSpeed = 2f;
    [SerializeField] private float hungryFlashSpeed = 1.5f;
    [SerializeField] private bool enableMatingEffects = true;
    [SerializeField] private bool enableHungerEffects = true;
    [SerializeField] private bool enableDeathEffects = true;

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private AgeSystem ageSystem;

    // Component references
    private EnergySystem energySystem;
    private ReproductionSystem reproductionSystem;
    private GeneticsSystem geneticsSystem;

    // Visual state tracking
    private bool isMating = false;
    private bool isHungry = false;
    private bool isStarving = false;
    private bool isDead = false;
    private Color originalColor;
    private Vector3 originalScale;
    private Coroutine currentEffectCoroutine;

    void Awake()
    {
        // Your existing code
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        if (ageSystem == null)
            ageSystem = GetComponent<AgeSystem>();

        // Get additional components
        energySystem = GetComponent<EnergySystem>();
        reproductionSystem = GetComponent<ReproductionSystem>();
        geneticsSystem = GetComponent<GeneticsSystem>();

        // Cache original values
        originalColor = spriteRenderer ? spriteRenderer.color : Color.white;
        originalScale = transform.localScale;
    }

    void Start()
    {
        // Subscribe to events
        SubscribeToEvents();
    }

    void Update()
    {
        if (!isDead)
        {
            // Your existing age-based appearance update
            UpdateAgeBasedAppearance();

            // Update state-based effects
            UpdateStateBasedEffects();
        }
    }

    /// <summary>
    /// Enhanced age-based appearance logic with aging whitening
    /// </summary>
    private void UpdateAgeBasedAppearance()
    {
        if (spriteRenderer == null || ageSystem == null) return;

        float currentAge = ageSystem.Age;
        float maxAge = ageSystem.MaxAge;

        // Only update age-based visuals if not in special states
        if (!isMating && !isStarving && !isDead)
        {
            // Your existing scale calculation
            float maturityProgress = Mathf.Clamp01(currentAge / maturityAge);
            float targetScale = Mathf.Lerp(minScale, maxScale, maturityProgress);
            transform.localScale = originalScale * targetScale;

            // Enhanced color calculation with aging whitening
            Color ageColor = CalculateEnhancedAgeColor(currentAge, maxAge);
            Color geneticColor = GetGeneticColor();
            Color finalColor = Color.Lerp(ageColor, geneticColor, 0.3f);

            spriteRenderer.color = finalColor;
        }
    }

    /// <summary>
    /// Enhanced age color calculation with gradual whitening for old age
    /// </summary>
    private Color CalculateEnhancedAgeColor(float currentAge, float maxAge)
    {
        Color baseColor;

        if (currentAge <= maturityAge)
        {
            // Baby to adult: dark blue to cyan (your existing logic)
            float progress = currentAge / maturityAge;
            baseColor = Color.Lerp(babyColor, adultColor, progress);
        }
        else
        {
            // Adult to elderly: cyan to gray (your existing logic)
            float elderlyProgress = (currentAge - maturityAge) / (maxAge - maturityAge);
            baseColor = Color.Lerp(adultColor, Color.gray, elderlyProgress * 0.3f);
        }

        // NEW: Apply aging whitening effect
        if (enableAgingWhitening)
        {
            float ageProgress = currentAge / maxAge;
            if (ageProgress >= oldAgeThreshold)
            {
                // Calculate how "old" they are beyond the threshold
                float oldnessProgress = (ageProgress - oldAgeThreshold) / (1f - oldAgeThreshold);
                oldnessProgress = Mathf.Clamp01(oldnessProgress);

                // Gradually whiten the sprite
                float whiteningAmount = oldnessProgress * maxWhitening;
                baseColor = Color.Lerp(baseColor, oldAgeColor, whiteningAmount);

                // Optional: Also make them slightly smaller as they get very old
                if (oldnessProgress > 0.5f)
                {
                    float shrinkage = (oldnessProgress - 0.5f) * 0.1f; // Max 10% shrinkage
                    transform.localScale = originalScale * (Mathf.Lerp(minScale, maxScale, Mathf.Clamp01(currentAge / maturityAge)) - shrinkage);
                }
            }
        }

        return baseColor;
    }

    /// <summary>
    /// Get genetic color influence
    /// </summary>
    private Color GetGeneticColor()
    {
        return geneticsSystem != null ? geneticsSystem.BaseColor : Color.white;
    }

    /// <summary>
    /// Enhanced state-based effects with progressive starvation
    /// </summary>
    private void UpdateStateBasedEffects()
    {
        if (energySystem == null) return;

        // Check starvation state
        bool wasStarving = isStarving;
        bool wasHungry = isHungry;

        float energyPercent = energySystem.EnergyPercent;
        isStarving = energyPercent <= starvationThreshold;
        isHungry = energySystem.IsHungry && !isStarving; // Regular hunger (not starvation)

        // Handle starvation effects (takes priority over hunger)
        if (enableStarvationEffects)
        {
            if (isStarving && !wasStarving && !isMating)
            {
                StartStarvationEffect();
            }
            else if (!isStarving && wasStarving)
            {
                StopStarvationEffect();
            }
        }

        // Handle regular hunger effects (only if not starving)
        if (enableHungerEffects && !isStarving)
        {
            if (isHungry && !wasHungry && !isMating)
            {
                StartHungerEffect();
            }
            else if (!isHungry && wasHungry)
            {
                StopHungerEffect();
            }
        }

        // Very low energy warning (additional overlay)
        if (energyPercent < 0.15f && !isMating && !isDead && !isStarving)
        {
            Color currentColor = spriteRenderer.color;
            Color warningColor = Color.Lerp(currentColor, lowEnergyColor, 0.2f);
            spriteRenderer.color = warningColor;
        }
    }

    /// <summary>
    /// Subscribe to component events
    /// </summary>
    private void SubscribeToEvents()
    {
        if (reproductionSystem != null && enableMatingEffects)
        {
            reproductionSystem.OnMatingStarted += OnMatingStarted;
            reproductionSystem.OnMatingCompleted += OnMatingCompleted;
        }

        if (energySystem != null && enableDeathEffects)
        {
            energySystem.OnDeath += OnDeath;
        }
    }

    // ========================================================================
    // Event Handlers
    // ========================================================================

    private void OnMatingStarted(IAgent partner)
    {
        if (enableMatingEffects)
        {
            StartMatingEffect();
        }
    }

    private void OnMatingCompleted()
    {
        if (enableMatingEffects)
        {
            StopMatingEffect();
        }
    }

    private void OnDeath()
    {
        if (enableDeathEffects)
        {
            StartDeathEffect();
        }
    }

    // ========================================================================
    // Visual Effects
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
        UpdateAgeBasedAppearance(); // Return to normal
    }

    private void StartHungerEffect()
    {
        if (!isMating && !isStarving)
        {
            StopCurrentEffect();
            currentEffectCoroutine = StartCoroutine(HungerEffectCoroutine());
        }
    }

    private void StopHungerEffect()
    {
        if (!isMating && !isStarving)
        {
            StopCurrentEffect();
            UpdateAgeBasedAppearance(); // Return to normal
        }
    }

    /// <summary>
    /// NEW: Start progressive starvation effect
    /// </summary>
    private void StartStarvationEffect()
    {
        if (!isMating)
        {
            StopCurrentEffect();
            currentEffectCoroutine = StartCoroutine(StarvationEffectCoroutine());
        }
    }

    /// <summary>
    /// NEW: Stop starvation effect
    /// </summary>
    private void StopStarvationEffect()
    {
        if (!isMating)
        {
            StopCurrentEffect();
            UpdateAgeBasedAppearance(); // Return to normal
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
    // Effect Coroutines
    // ========================================================================

    private IEnumerator MatingEffectCoroutine()
    {
        while (isMating)
        {
            float pulse = (Mathf.Sin(Time.time * matingPulseSpeed) + 1f) * 0.5f;
            Color baseColor = CalculateEnhancedAgeColor(ageSystem.Age, ageSystem.MaxAge);
            Color pulseColor = Color.Lerp(baseColor, matingColor, pulse * 0.7f);
            spriteRenderer.color = pulseColor;

            yield return null;
        }
    }

    private IEnumerator HungerEffectCoroutine()
    {
        while (isHungry && !isMating && !isDead && !isStarving)
        {
            float flash = (Mathf.Sin(Time.time * hungryFlashSpeed) + 1f) * 0.5f;
            Color baseColor = CalculateEnhancedAgeColor(ageSystem.Age, ageSystem.MaxAge);
            Color flashColor = Color.Lerp(baseColor, hungryColor, flash * 0.5f);
            spriteRenderer.color = flashColor;

            yield return null;
        }
    }

    /// <summary>
    /// NEW: Progressive starvation effect with increasing flash frequency
    /// </summary>
    private IEnumerator StarvationEffectCoroutine()
    {
        while (isStarving && !isMating && !isDead)
        {
            // Calculate starvation intensity (how close to death)
            float energyPercent = energySystem.EnergyPercent;
            float starvationIntensity = 1f - (energyPercent / starvationThreshold);
            starvationIntensity = Mathf.Clamp01(starvationIntensity);

            // Calculate flash speed based on starvation intensity
            float currentFlashSpeed = Mathf.Lerp(baseStarvationFlashSpeed, maxStarvationFlashSpeed, starvationIntensity);

            // Critical starvation - even faster and more intense
            if (energyPercent <= criticalStarvationThreshold)
            {
                float criticalIntensity = 1f - (energyPercent / criticalStarvationThreshold);
                currentFlashSpeed = Mathf.Lerp(maxStarvationFlashSpeed, maxStarvationFlashSpeed * 2f, criticalIntensity);
            }

            // Create the flash effect
            float flash = (Mathf.Sin(Time.time * currentFlashSpeed) + 1f) * 0.5f;
            Color baseColor = CalculateEnhancedAgeColor(ageSystem.Age, ageSystem.MaxAge);

            // Intensity affects how strong the starvation color is
            float colorIntensity = Mathf.Lerp(0.3f, 0.8f, starvationIntensity);
            Color flashColor = Color.Lerp(baseColor, starvationColor, flash * colorIntensity);

            spriteRenderer.color = flashColor;

            yield return null;
        }
    }

    private IEnumerator DeathEffectCoroutine()
    {
        // Flash effect
        for (int i = 0; i < 3; i++)
        {
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.1f);
        }

        // Fade out
        Color startColor = spriteRenderer.color;
        for (float t = 0; t < 0.5f; t += Time.deltaTime)
        {
            float alpha = Mathf.Lerp(1f, 0f, t / 0.5f);
            Color fadeColor = startColor;
            fadeColor.a = alpha;
            spriteRenderer.color = fadeColor;
            yield return null;
        }
    }

    // ========================================================================
    // Your existing public methods (unchanged)
    // ========================================================================

    public bool IsBaby() => ageSystem != null && ageSystem.Age < maturityAge * 0.3f;
    public bool IsChild() => ageSystem != null && ageSystem.Age < maturityAge;
    public bool IsAdult() => ageSystem != null && ageSystem.Age >= maturityAge;

    // ========================================================================
    // NEW: Additional public methods
    // ========================================================================

    public bool IsMating() => isMating;
    public bool IsHungry() => isHungry;
    public bool IsStarving() => isStarving;
    public bool IsOld() => ageSystem != null && (ageSystem.Age / ageSystem.MaxAge) >= oldAgeThreshold;

    public float GetAgeProgress() => ageSystem != null ? ageSystem.Age / ageSystem.MaxAge : 0f;
    public float GetStarvationIntensity() => energySystem != null ? 1f - (energySystem.EnergyPercent / starvationThreshold) : 0f;

    public void SetMatingEffectsEnabled(bool enabled) => enableMatingEffects = enabled;
    public void SetHungerEffectsEnabled(bool enabled) => enableHungerEffects = enabled;
    public void SetStarvationEffectsEnabled(bool enabled) => enableStarvationEffects = enabled;
    public void SetDeathEffectsEnabled(bool enabled) => enableDeathEffects = enabled;
    public void SetAgingWhiteningEnabled(bool enabled) => enableAgingWhitening = enabled;

    void OnDestroy()
    {
        // Unsubscribe from events
        if (reproductionSystem != null)
        {
            reproductionSystem.OnMatingStarted -= OnMatingStarted;
            reproductionSystem.OnMatingCompleted -= OnMatingCompleted;
        }

        if (energySystem != null)
        {
            energySystem.OnDeath -= OnDeath;
        }
    }
}

/*
========================================================================
NEW FEATURES ADDED:

?? AGING WHITENING:
- Agents gradually turn white as they approach 80% of their death age
- Configurable oldAgeThreshold, oldAgeColor, and maxWhitening
- Optional slight shrinkage for very old agents
- Can be enabled/disabled with enableAgingWhitening

?? PROGRESSIVE STARVATION:
- Starvation starts at 30% energy (configurable starvationThreshold)
- Flash frequency increases as energy gets lower
- Critical starvation at 10% energy - super fast flashing
- Flash color intensity increases with starvation severity
- Separate from regular hunger effects
- Can be enabled/disabled with enableStarvationEffects

?? ENHANCED VISUAL HIERARCHY:
- Starvation takes priority over hunger
- Mating effects override everything except death
- Aging whitening applies to base color calculations
- Multiple effects can be enabled/disabled independently

?? NEW PUBLIC METHODS:
- IsOld() - check if agent is in old age whitening phase
- IsStarving() - check if agent is starving (vs just hungry)
- GetAgeProgress() - get 0-1 age progress toward death
- GetStarvationIntensity() - get 0-1 starvation intensity
- Enable/disable individual effect types

USAGE:
Replace your existing AgeSpriteController.cs with this enhanced version.
All your existing settings and functionality remain the same, plus:
- Adjust oldAgeThreshold to control when whitening starts (0.8 = 80% of death age)
- Adjust starvationThreshold to control when starvation effects begin (0.3 = 30% energy)
- Tweak flash speeds and colors to your preference
- Enable/disable individual effects as needed
========================================================================
*/