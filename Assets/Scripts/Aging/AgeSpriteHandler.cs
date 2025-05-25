// <summary>
using UnityEngine;
/// Handles sprite appearance changes based on age
/// </summary>
public class AgeSpriteHandler : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private float maturityAge = 20f;
    [SerializeField] private float minScale = 0.5f;  // Baby scale
    [SerializeField] private float maxScale = 1f;    // Adult scale
    [SerializeField] private Color babyColor = new Color(0.2f, 0.4f, 0.8f, 1f); // Darker blue
    [SerializeField] private Color adultColor = Color.white;

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private AgentAge agentAge;

    void Awake()
    {
        // Get components if not assigned
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (agentAge == null)
            agentAge = GetComponent<AgentAge>();

        // Subscribe to age changes
        if (agentAge != null)
        {
            agentAge.OnAgeChanged += UpdateSpriteAppearance;
        }
    }

    void Start()
    {
        // Set initial appearance
        UpdateSpriteAppearance(agentAge?.CurrentAge ?? 0f);
    }

    void OnDestroy()
    {
        // Unsubscribe
        if (agentAge != null)
        {
            agentAge.OnAgeChanged -= UpdateSpriteAppearance;
        }
    }

    private void UpdateSpriteAppearance(float currentAge)
    {
        if (spriteRenderer == null) return;

        // Calculate scale (0 to maturityAge maps to minScale to maxScale)
        float maturityProgress = Mathf.Clamp01(currentAge / maturityAge);
        float targetScale = Mathf.Lerp(minScale, maxScale, maturityProgress);
        transform.localScale = Vector3.one * targetScale;

        // Calculate color (baby blue to adult white)
        Color targetColor;
        if (currentAge <= maturityAge)
        {
            // Baby to adult: dark blue to white
            targetColor = Color.Lerp(babyColor, adultColor, maturityProgress);
        }
        else
        {
            // Adult to elderly: white to slightly gray
            float elderlyProgress = (currentAge - maturityAge) / (agentAge.MaxAge - maturityAge);
            targetColor = Color.Lerp(adultColor, Color.gray, elderlyProgress * 0.3f);
        }

        spriteRenderer.color = targetColor;
    }

    // Helper methods for external use
    public bool IsBaby() => agentAge != null && agentAge.CurrentAge < maturityAge * 0.3f;
    public bool IsChild() => agentAge != null && agentAge.CurrentAge < maturityAge;
    public bool IsAdult() => agentAge != null && agentAge.CurrentAge >= maturityAge && agentAge.CurrentAge < agentAge.MaxAge * 0.8f;
    public bool IsElderly() => agentAge != null && agentAge.CurrentAge >= agentAge.MaxAge * 0.8f;

    // Configuration methods
    public void SetMaturityAge(float age) => maturityAge = age;
    public void SetScaleRange(float min, float max) { minScale = min; maxScale = max; }
    public void SetBabyColor(Color color) => babyColor = color;
    public void SetAdultColor(Color color) => adultColor = color;
}

// ============================================================================
// INTEGRATION HELPERS - Minimal code to add to existing classes
// ============================================================================

/// <summary>
/// Simple extensions for your existing AgentController
/// </summary>
public static class SimpleAgeExtensions
{
    /// <summary>
    /// Add age system to agent - call this in AgentController.Awake()
    /// </summary>
    public static void AddAgeSystem(this AgentController agent)
    {
        // Add age tracking
        if (agent.GetComponent<AgentAge>() == null)
            agent.gameObject.AddComponent<AgentAge>();

        // Add sprite handler
        if (agent.GetComponent<AgeSpriteHandler>() == null)
            agent.gameObject.AddComponent<AgeSpriteHandler>();

        // Connect to death system
        var agentAge = agent.GetComponent<AgentAge>();
        var deathSystem = agent.GetComponent<DeathSystem>();

        if (agentAge != null && deathSystem != null)
        {
            agentAge.OnDiedFromAge += () => deathSystem.Die("old age");
        }
    }

    /// <summary>
    /// Setup offspring age - call this in AgentSpawner.SpawnOffspring()
    /// </summary>
    public static void SetupOffspringAge(this GameObject offspring, GameObject parent1, GameObject parent2 = null)
    {
        var offspringAge = offspring.GetComponent<AgentAge>();
        var parent1Age = parent1?.GetComponent<AgentAge>();
        var parent2Age = parent2?.GetComponent<AgentAge>();

        if (offspringAge != null && parent1Age != null)
        {
            // Start as baby
            offspringAge.SetAge(0f);

            // Inherit lifespan
            offspringAge.InheritMaxAgeFromParents(parent1Age, parent2Age);
        }
    }

    /// <summary>
    /// Get age info - use this in your behavior logic
    /// </summary>
    public static bool IsChildAge(this AgentController agent)
    {
        var spriteHandler = agent.GetComponent<AgeSpriteHandler>();
        return spriteHandler != null && spriteHandler.IsChild();
    }

    public static bool IsAdultAge(this AgentController agent)
    {
        var spriteHandler = agent.GetComponent<AgeSpriteHandler>();
        return spriteHandler != null && spriteHandler.IsAdult();
    }

    public static float GetCurrentAge(this AgentController agent)
    {
        var agentAge = agent.GetComponent<AgentAge>();
        return agentAge?.CurrentAge ?? 0f;
    }
}
