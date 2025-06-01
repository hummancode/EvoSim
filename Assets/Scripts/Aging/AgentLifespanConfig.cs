// FILE: AgentLifespanConfig.cs - ScriptableObject for centralized configuration
// ============================================================================

using UnityEngine;

/// <summary>
/// Centralized configuration for agent lifespan settings
/// Create this as a ScriptableObject asset in your project
/// </summary>
[CreateAssetMenu(fileName = "AgentLifespanConfig", menuName = "Simulation/Agent Lifespan Config", order = 1)]
public class AgentLifespanConfig : ScriptableObject
{
    [Header("Base Lifespan Settings")]
    [SerializeField] private float baseMaxAge = 140f;
    [SerializeField] private float baseMaturityAge = 20f;
    [SerializeField] private float ageRate = 1f; // Age units per second

    [Header("Variation Settings")]
    [SerializeField] private float maxAgeVariation = 10f; // ±10 years variation
    [SerializeField] private float maturityVariation = 3f; // ±3 years variation

    [Header("Genetic Inheritance")]
    [SerializeField] private float parentalInfluence = 0.7f; // How much parents affect offspring lifespan
    [SerializeField] private bool enableMutations = true;
    [SerializeField] private float mutationRate = 0.1f;
    [SerializeField] private float mutationAmount = 0.15f;

    [Header("Generation Effects")]
    [SerializeField] private bool generationAffectsLifespan = false;
    [SerializeField] private float generationBonusPerGen = 1f; // +1 year per generation

    // Properties with validation
    public float BaseMaxAge => Mathf.Max(10f, baseMaxAge);
    public float BaseMaturityAge => Mathf.Max(1f, Mathf.Min(baseMaturityAge, BaseMaxAge * 0.8f));
    public float AgeRate => Mathf.Max(0.1f, ageRate);
    public float MaxAgeVariation => Mathf.Max(0f, maxAgeVariation);
    public float MaturityVariation => Mathf.Max(0f, maturityVariation);
    public float ParentalInfluence => Mathf.Clamp01(parentalInfluence);
    public bool EnableMutations => enableMutations;
    public float MutationRate => Mathf.Clamp01(mutationRate);
    public float MutationAmount => Mathf.Clamp01(mutationAmount);
    public bool GenerationAffectsLifespan => generationAffectsLifespan;
    public float GenerationBonusPerGen => generationBonusPerGen;

    /// <summary>
    /// Generate random max age for initial agents
    /// </summary>
    public float GetRandomMaxAge()
    {
        return BaseMaxAge + Random.Range(-MaxAgeVariation, MaxAgeVariation);
    }

    /// <summary>
    /// Generate random maturity age for initial agents
    /// </summary>
    public float GetRandomMaturityAge()
    {
        return BaseMaturityAge + Random.Range(-MaturityVariation, MaturityVariation);
    }

    /// <summary>
    /// Calculate inherited max age from parents
    /// </summary>
    public float CalculateInheritedMaxAge(float parent1MaxAge, float parent2MaxAge = -1f, int generation = 1)
    {
        float inheritedAge;

        if (parent2MaxAge > 0)
        {
            // Two parents - blend their lifespans
            inheritedAge = Mathf.Lerp(
                (parent1MaxAge + parent2MaxAge) / 2f,
                BaseMaxAge,
                1f - ParentalInfluence
            );
        }
        else
        {
            // Single parent
            inheritedAge = Mathf.Lerp(parent1MaxAge, BaseMaxAge, 1f - ParentalInfluence);
        }

        // Add generation bonus if enabled
        if (GenerationAffectsLifespan)
        {
            inheritedAge += generation * GenerationBonusPerGen;
        }

        // Apply mutations
        if (EnableMutations && Random.value < MutationRate)
        {
            float mutation = Random.Range(-MutationAmount, MutationAmount) * BaseMaxAge;
            inheritedAge += mutation;
            Debug.Log($"Age mutation: {mutation:F1} years");
        }

        // Add some natural variation
        inheritedAge += Random.Range(-MaxAgeVariation * 0.5f, MaxAgeVariation * 0.5f);

        // Ensure reasonable bounds
        return Mathf.Max(BaseMaxAge * 0.5f, inheritedAge);
    }

    /// <summary>
    /// Calculate inherited maturity age from parents
    /// </summary>
    public float CalculateInheritedMaturityAge(float parent1MaturityAge, float parent2MaturityAge = -1f)
    {
        float inheritedMaturity;

        if (parent2MaturityAge > 0)
        {
            inheritedMaturity = (parent1MaturityAge + parent2MaturityAge) / 2f;
        }
        else
        {
            inheritedMaturity = parent1MaturityAge;
        }

        // Add variation
        inheritedMaturity += Random.Range(-MaturityVariation, MaturityVariation);

        // Ensure reasonable bounds
        return Mathf.Max(BaseMaturityAge * 0.5f, inheritedMaturity);
    }
}