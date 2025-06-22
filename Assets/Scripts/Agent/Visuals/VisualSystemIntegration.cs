
using UnityEngine;

/// <summary>
/// Helper class to easily integrate visual systems into existing simulation
/// </summary>
public static class VisualSystemIntegration
{
    /// <summary>
    /// Add complete visual system to an agent
    /// Call this in AgentController.Awake() or Start()
    /// </summary>
    public static AgentVisualController AddAgentVisualSystem(GameObject agent)
    {
        // Check if already has visual controller
        AgentVisualController visualController = agent.GetComponent<AgentVisualController>();
        if (visualController != null)
        {
            Debug.Log($"Agent {agent.name} already has visual controller");
            return visualController;
        }

        // Add the visual controller
        visualController = agent.AddComponent<AgentVisualController>();

        // Ensure agent has required components for visuals
        EnsureAgentHasRequiredComponents(agent);

        Debug.Log($"Added visual system to agent: {agent.name}");
        return visualController;
    }

    /// <summary>
    /// Add visual system to a food item
    /// Call this in Food.Awake() or Start()
    /// </summary>
    public static FoodVisualController AddFoodVisualSystem(GameObject food)
    {
        // Check if already has visual controller
        FoodVisualController visualController = food.GetComponent<FoodVisualController>();
        if (visualController != null)
        {
            Debug.Log($"Food {food.name} already has visual controller");
            return visualController;
        }

        // Add the visual controller
        visualController = food.AddComponent<FoodVisualController>();

        // Ensure food has required components
        EnsureFoodHasRequiredComponents(food);

        Debug.Log($"Added visual system to food: {food.name}");
        return visualController;
    }

    /// <summary>
    /// Ensure agent has all required components for visual system
    /// </summary>
    private static void EnsureAgentHasRequiredComponents(GameObject agent)
    {
        // Ensure SpriteRenderer exists
        if (agent.GetComponent<SpriteRenderer>() == null)
        {
            agent.AddComponent<SpriteRenderer>();
        }

        // Ensure required systems exist (these should already be added by your existing code)
        if (agent.GetComponent<AgeSystem>() == null)
        {
            Debug.LogWarning($"Agent {agent.name} missing AgeSystem - visual age effects may not work");
        }

        if (agent.GetComponent<AgeLifeStageTracker>() == null)
        {
            Debug.LogWarning($"Agent {agent.name} missing AgeLifeStageTracker - adding it now");
            agent.AddComponent<AgeLifeStageTracker>();
        }

        if (agent.GetComponent<EnergySystem>() == null)
        {
            Debug.LogWarning($"Agent {agent.name} missing EnergySystem - energy visual effects may not work");
        }

        if (agent.GetComponent<ReproductionSystem>() == null)
        {
            Debug.LogWarning($"Agent {agent.name} missing ReproductionSystem - mating visual effects may not work");
        }

        if (agent.GetComponent<GeneticsSystem>() == null)
        {
            Debug.LogWarning($"Agent {agent.name} missing GeneticsSystem - genetic visual traits may not work");
        }
    }

    /// <summary>
    /// Ensure food has all required components for visual system
    /// </summary>
    private static void EnsureFoodHasRequiredComponents(GameObject food)
    {
        // Ensure SpriteRenderer exists
        if (food.GetComponent<SpriteRenderer>() == null)
        {
            food.AddComponent<SpriteRenderer>();
        }

        // Ensure Food component exists
        if (food.GetComponent<Food>() == null)
        {
            Debug.LogWarning($"Food {food.name} missing Food component - some visual effects may not work properly");
        }

        // Ensure collider exists for interaction
        if (food.GetComponent<Collider2D>() == null)
        {
            Debug.LogWarning($"Food {food.name} missing Collider2D - adding CircleCollider2D");
            var collider = food.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.5f;
        }
    }

    /// <summary>
    /// Update existing AgentController to automatically add visual system
    /// </summary>
    public static void IntegrateWithAgentController()
    {
        Debug.Log("Visual system integration: Add this line to your AgentController.Awake():");
        Debug.Log("VisualSystemIntegration.AddAgentVisualSystem(gameObject);");
    }

    /// <summary>
    /// Update existing Food class to automatically add visual system
    /// </summary>
    public static void IntegrateWithFoodClass()
    {
        Debug.Log("Visual system integration: Add this line to your Food.Awake():");
        Debug.Log("VisualSystemIntegration.AddFoodVisualSystem(gameObject);");
    }

    /// <summary>
    /// Bulk add visual systems to all existing agents in scene
    /// </summary>
    [System.Obsolete("Use AddVisualSystemsToAllAgents() instead")]
    public static void AddVisualSystemsToExistingAgents()
    {
        AddVisualSystemsToAllAgents();
    }

    /// <summary>
    /// Add visual systems to all agents currently in the scene
    /// Useful for retrofitting existing simulations
    /// </summary>
    public static void AddVisualSystemsToAllAgents()
    {
        AgentController[] agents = Object.FindObjectsOfType<AgentController>();
        int addedCount = 0;

        foreach (var agent in agents)
        {
            if (agent.GetComponent<AgentVisualController>() == null)
            {
                AddAgentVisualSystem(agent.gameObject);
                addedCount++;
            }
        }

        Debug.Log($"Added visual systems to {addedCount} existing agents (out of {agents.Length} total)");
    }

    /// <summary>
    /// Add visual systems to all food items currently in the scene
    /// </summary>
    public static void AddVisualSystemsToAllFood()
    {
        Food[] foodItems = Object.FindObjectsOfType<Food>();
        int addedCount = 0;

        foreach (var food in foodItems)
        {
            if (food.GetComponent<FoodVisualController>() == null)
            {
                AddFoodVisualSystem(food.gameObject);
                addedCount++;
            }
        }

        Debug.Log($"Added visual systems to {addedCount} existing food items (out of {foodItems.Length} total)");
    }

    /// <summary>
    /// Create a test agent with full visual system for preview
    /// </summary>
    public static GameObject CreateTestAgentWithVisuals(Vector3 position)
    {
        GameObject testAgent = new GameObject("TestAgent");
        testAgent.transform.position = position;

        // Add core components
        testAgent.AddComponent<AgentController>();
        testAgent.AddComponent<SpriteRenderer>();

        // Add visual system
        AddAgentVisualSystem(testAgent);

        Debug.Log($"Created test agent with visual system at {position}");
        return testAgent;
    }

    /// <summary>
    /// Create a test food item with full visual system for preview
    /// </summary>
    public static GameObject CreateTestFoodWithVisuals(Vector3 position)
    {
        GameObject testFood = new GameObject("TestFood");
        testFood.transform.position = position;

        // Add core components
        testFood.AddComponent<Food>();
        testFood.AddComponent<SpriteRenderer>();

        // Add collider
        var collider = testFood.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.5f;

        // Add visual system
        AddFoodVisualSystem(testFood);

        Debug.Log($"Created test food with visual system at {position}");
        return testFood;
    }
}
