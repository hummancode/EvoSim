// ============================================================================
// FILE: SimpleAgeIntegration.cs
// PURPOSE: Easy integration helper for existing AgentController
// ============================================================================

using UnityEngine;

/// <summary>
/// Simple helper to add age-based visuals to existing agents
/// </summary>
public static class SimpleAgeIntegration
{
    /// <summary>
    /// Add age-based sprite system to an agent
    /// Call this in AgentController.Awake() or Start()
    /// </summary>
    public static void AddAgeSpriteSystem(GameObject agent)
    {
        // Add sprite controller if it doesn't exist
        if (agent.GetComponent<AgeSpriteController>() == null)
            agent.AddComponent<AgeSpriteController>();

        // Add life stage tracker if it doesn't exist  
        if (agent.GetComponent<AgeLifeStageTracker>() == null)
            agent.AddComponent<AgeLifeStageTracker>();

        // Add behavior helper if it doesn't exist
        if (agent.GetComponent<AgeBasedBehaviorHelper>() == null)
            agent.AddComponent<AgeBasedBehaviorHelper>();
    }

    /// <summary>
    /// Setup offspring age inheritance
    /// Call this in AgentSpawner.SpawnOffspring()
    /// </summary>
    public static void SetupOffspringAge(GameObject offspring, GameObject parent1, GameObject parent2 = null)
    {
        var offspringAge = offspring.GetComponent<AgeSystem>();
        var parent1Age = parent1?.GetComponent<AgeSystem>();
        var parent2Age = parent2?.GetComponent<AgeSystem>();

        if (offspringAge != null && parent1Age != null)
        {
            // Start as baby (age 0)
            // Your existing AgeSystem should handle age inheritance through genetics
            Debug.Log($"Offspring {offspring.name} born to parents with ages {parent1Age.Age:F1} and {parent2Age?.Age:F1 ?? 0f}");
        }
    }
}
