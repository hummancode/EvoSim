// ============================================================================
// FILE: SimpleFluffGeneHelper.cs
// PURPOSE: Easy integration with your existing system
// ============================================================================

using UnityEngine;
/// <summary>
/// Helper to easily add genetic fluff to agents
/// </summary>
public static class SimpleFluffGeneHelper
{
    /// <summary>
    /// Add fluff genetics to genome
    /// </summary>
    public static void AddFluffTraitToGenome(GeneticsSystem genetics)
    {
        if (genetics?.Genome == null) return;

        if (!genetics.Genome.HasTrait("FluffAmount"))
        {
            GeneticTrait fluffTrait = new GeneticTrait(
                "FluffAmount",
                Random.Range(0.2f, 0.8f), // Random fluffiness
                0f,    // Minimum (no fluff)
                1f,    // Maximum (maximum fluff)
                0.1f,  // Mutation rate
                0.2f   // Mutation amount
            );

            genetics.Genome.AddTrait(fluffTrait);
            Debug.Log($"Added FluffAmount trait: {fluffTrait.value:F2}");
        }
    }

    /// <summary>
    /// Setup simple fluff overlay for an agent
    /// MUCH simpler than the previous system!
    /// </summary>
    public static SimpleGeneticFluffOverlay SetupSimpleFluff(GameObject agent)
    {
        // Add the simple fluff component
        SimpleGeneticFluffOverlay fluffOverlay = agent.GetComponent<SimpleGeneticFluffOverlay>();
        if (fluffOverlay == null)
        {
            fluffOverlay = agent.AddComponent<SimpleGeneticFluffOverlay>();
            Debug.Log("added fluff to the agent");
        }

        // Add genetics trait
        GeneticsSystem genetics = agent.GetComponent<GeneticsSystem>();
        if (genetics != null)
        {
            AddFluffTraitToGenome(genetics);
        }

        return fluffOverlay;
    }
}
