using UnityEngine;
/// <summary>
/// Extension methods for FoodSpawner to integrate with visual system
/// Add this script to your project and call the integration method from FoodSpawner
/// </summary>
public static class FoodSpawnerVisualIntegration
{
    /// <summary>
    /// Enhanced food spawning with visual effects
    /// Call this from your FoodSpawner instead of regular Instantiate
    /// </summary>
    public static GameObject SpawnFoodWithVisuals(GameObject foodPrefab, Vector3 position, Quaternion rotation)
    {
        // Spawn the food
        GameObject newFood = UnityEngine.Object.Instantiate(foodPrefab, position, rotation);

        // Ensure it has a Food component
        Food foodComponent = newFood.GetComponent<Food>();
        if (foodComponent == null)
        {
            foodComponent = newFood.AddComponent<Food>();
        }

        // Add visual system if not already present
        if (newFood.GetComponent<FoodVisualController>() == null)
        {
            VisualSystemIntegration.AddFoodVisualSystem(newFood);
        }

        return newFood;
    }

    /// <summary>
    /// Create a food prefab with full visual integration
    /// Useful for setting up your food prefab in code
    /// </summary>
    public static GameObject CreateFoodPrefabWithVisuals(string name = "FoodPrefab")
    {
        GameObject foodPrefab = new GameObject(name);

        // Add core components
        Food foodComponent = foodPrefab.AddComponent<Food>();
        SpriteRenderer spriteRenderer = foodPrefab.AddComponent<SpriteRenderer>();

        // Add collider for interaction
        CircleCollider2D collider = foodPrefab.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.5f;

        // Add visual controller
        VisualSystemIntegration.AddFoodVisualSystem(foodPrefab);

        // Set layer and tag
        foodPrefab.layer = LayerMask.NameToLayer("Food");
        foodPrefab.tag = "Food";

        Debug.Log($"Created food prefab with visual integration: {name}");
        return foodPrefab;
    }
}

// ============================================================================
// FILE: AgentControllerVisualIntegration.cs  
// PURPOSE: Update AgentController to work with visual system
// ============================================================================

/// <summary>
/// Integration helper for AgentController visual system
/// </summary>
public static class AgentControllerVisualIntegration
{
    /// <summary>
    /// Add this line to your AgentController.Awake() method:
    /// AgentControllerVisualIntegration.IntegrateVisualSystem(gameObject);
    /// </summary>
    public static AgentVisualController IntegrateVisualSystem(GameObject agent)
    {
        return VisualSystemIntegration.AddAgentVisualSystem(agent);
    }

    /// <summary>
    /// Enhanced agent spawning with visual effects
    /// Use this in your AgentSpawner instead of regular Instantiate
    /// </summary>
    public static GameObject SpawnAgentWithVisuals(GameObject agentPrefab, Vector3 position, Quaternion rotation)
    {
        // Spawn the agent
        GameObject newAgent = UnityEngine.Object.Instantiate(agentPrefab, position, rotation);

        // Ensure visual system is added
        if (newAgent.GetComponent<AgentVisualController>() == null)
        {
            VisualSystemIntegration.AddAgentVisualSystem(newAgent);
        }

        return newAgent;
    }
}