// ============================================================================
// FILE: GeneticHairTrait.cs
// PURPOSE: Adds hair trait to genetics system
// ============================================================================

using UnityEngine;

/// <summary>
/// Helper to add hair genetics to existing genetics system
/// </summary>
public static class GeneticHairTraitHelper
{
    /// <summary>
    /// Add hair trait to genetics system
    /// Call this when setting up genetics for offspring
    /// </summary>
    public static void AddHairTraitToGenome(GeneticsSystem genetics)
    {
        if (genetics?.Genome == null) return;

        // Add hair amount trait if it doesn't exist
        if (!genetics.Genome.HasTrait("HairAmount"))
        {
            GeneticTrait hairTrait = new GeneticTrait(
                "HairAmount",
                Random.Range(0.2f, 0.8f), // Random initial value
                0f,  // Min hair
                1f,  // Max hair
                0.15f, // Mutation rate
                0.3f   // Mutation amount
            );

            genetics.Genome.AddTrait(hairTrait);
        }
    }

    /// <summary>
    /// Setup fluffy animal sprite for an agent
    /// Call this in AgentController or during agent creation
    /// </summary>
    public static FluffyAnimalSpriteController SetupFluffySprite(GameObject agent)
    {
        FluffyAnimalSpriteController spriteController = agent.GetComponent<FluffyAnimalSpriteController>();

        if (spriteController == null)
        {
            spriteController = agent.AddComponent<FluffyAnimalSpriteController>();
        }

        // Ensure genetics has hair trait
        GeneticsSystem genetics = agent.GetComponent<GeneticsSystem>();
        if (genetics != null)
        {
            AddHairTraitToGenome(genetics);
        }

        return spriteController;
    }
}

/*
========================================================================
?? FLUFFY ANIMAL SPRITE SYSTEM WITH GENETIC HAIR

FEATURES:
? 5 different animal types (Hamster, Rabbit, Mouse, Squirrel, Hedgehog)
? Procedurally generated sprites with bodies and hair
? Hair amount controlled by genetics ("HairAmount" trait)
? Age-based hair changes (babies less fluffy, adults most fluffy)
? Animated hair that wiggles slightly
? Customizable colors and settings

GENETICS INTEGRATION:
- Adds "HairAmount" trait to genome (0.0 to 1.0)
- Parents pass hair genes to offspring
- Mutations can create extra fluffy or bald variants
- Hair amount affects visual fluffiness

SETUP:
1. Add FluffyAnimalSpriteController to your agent prefab
2. Choose animal type in inspector
3. Adjust hair settings and colors
4. The system automatically uses genetics if available

INTEGRATION WITH YOUR SYSTEM:
Add this to your AgentController.Awake():
```csharp
GeneticHairTraitHelper.SetupFluffySprite(gameObject);
```

Or add to your AgentSpawner.SpawnOffspring():
```csharp
FluffyAnimalSpriteController fluffySprite = 
    GeneticHairTraitHelper.SetupFluffySprite(offspring);
```

CUSTOMIZATION:
- Animal Type: Choose from 5 cute animals
- Hair Amount: Base fluffiness + genetic variation
- Colors: Body and hair colors
- Animation: Wiggling hair effect
- Age Effects: Babies less fluffy, adults most fluffy

RESULT:
Your agents will be adorable fluffy animals where hair/fur 
amount varies based on their genetics! Some will be super 
fluffy, others more smooth, and it's all inherited!
========================================================================
*/