using UnityEngine;

public class AgentContextBuilder : IAgentContextBuilder
{
    private readonly MonoBehaviour agent;
    private readonly IAgentComponentProvider componentProvider;

    public AgentContextBuilder(MonoBehaviour agent, IAgentComponentProvider componentProvider)
    {
        this.agent = agent;
        this.componentProvider = componentProvider;
    }

    // ========================================================================
    // INTERFACE IMPLEMENTATION - No changes to method signatures!
    // ========================================================================

    public AgentContext BuildContext()
    {
        // MUCH SIMPLER - Just create context with GameObject
        var context = new AgentContext(agent.gameObject);

        // Initialize any complex services that need setup
        InitializeComplexServices(context);

        Debug.Log($"Context built for {agent.name} - All services will be resolved automatically");

        return context;
    }

    public void UpdateContext(AgentContext context)
    {
        if (context == null)
        {
            Debug.LogError("Cannot update null context");
            return;
        }

        // MUCH SIMPLER - Just refresh the service cache
        context.RefreshServices();

        // Re-initialize complex services if needed
        InitializeComplexServices(context);

        Debug.Log($"Context updated for {agent.name}");
    }

    // ========================================================================
    // PRIVATE HELPER METHODS
    // ========================================================================

    private void InitializeComplexServices(AgentContext context)
    {
        // Initialize services that need special setup
        var reproductionSystem = context.Reproduction;
        if (reproductionSystem != null)
        {
            // Load reproduction config and configure mate detection range
            ConfigureMateDetectionRange(context, reproductionSystem);

            // Initialize reproduction system with dependencies
            reproductionSystem.Initialize(
                context.Agent,
                context.MateFinder,
                context.Energy
            );

            Debug.Log($"Initialized ReproductionSystem for {agent.name}");
        }

        // Add any other complex initialization here
        // Most services will be automatically resolved when accessed
    }

    /// <summary>
    /// Load reproduction config and set mate detection range
    /// </summary>
    private void ConfigureMateDetectionRange(AgentContext context, ReproductionSystem reproductionSystem)
    {
        try
        {
            // Get the reproduction config (this will auto-load it)
            ReproductionConfig config = reproductionSystem.GetConfig();

            if (config != null && config.mateDetectionRange > 0)
            {
                // Get the mate finder and update its detection range
                var mateFinder = context.MateFinder;
                if (mateFinder is SensorMateFinder sensorMateFinder)
                {
                    sensorMateFinder.SetMateDetectionRange(config.mateDetectionRange);
                    Debug.Log($"Configured mate detection range: {config.mateDetectionRange} for {agent.name}");
                }
                else
                {
                    Debug.LogWarning($"MateFinder is not SensorMateFinder type for {agent.name}");
                }
            }
            else
            {
                Debug.LogWarning($"No valid reproduction config found for {agent.name}, using default mate detection range");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error configuring mate detection range for {agent.name}: {e.Message}");
        }
    }
}
