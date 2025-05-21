using UnityEngine;

public class AgentEventManager : IAgentEventManager
{
    private readonly AgentController agent;
    private readonly IAgentComponentProvider componentProvider;

    private ReproductionSystem reproductionSystem;
    private DeathSystem deathSystem;

    public AgentEventManager(AgentController agent, IAgentComponentProvider componentProvider)
    {
        this.agent = agent;
        this.componentProvider = componentProvider;
    }

    public void SubscribeToEvents()
    {
        // Get systems
        reproductionSystem = componentProvider.GetReproductionSystem();
        deathSystem = componentProvider.GetDeathSystem();

        // Subscribe to reproduction events
        if (reproductionSystem != null)
        {
            // Make sure we're not double-subscribing
            reproductionSystem.OnMatingStarted -= HandleMatingStarted;
            reproductionSystem.OnMatingCompleted -= HandleMatingCompleted;

            // Subscribe to events
            reproductionSystem.OnMatingStarted += HandleMatingStarted;
            reproductionSystem.OnMatingCompleted += HandleMatingCompleted;

            Debug.Log("Successfully subscribed to reproduction events");
        }
        else
        {
            Debug.LogError("reproductionSystem is null in SubscribeToEvents");
        }

        // Subscribe to death events
        if (deathSystem != null)
        {
            deathSystem.OnDeath -= HandleDeath;
            deathSystem.OnDeath += HandleDeath;
        }
    }

    public void UnsubscribeFromEvents()
    {
        if (reproductionSystem != null)
        {
            reproductionSystem.OnMatingStarted -= HandleMatingStarted;
            reproductionSystem.OnMatingCompleted -= HandleMatingCompleted;
        }

        if (deathSystem != null)
        {
            deathSystem.OnDeath -= HandleDeath;
        }
    }

    // Event handlers
    private void HandleMatingStarted(IAgent partner)
    {
        Debug.Log($"{agent.gameObject.name}: HandleMatingStarted called");

        // Force behavior change
        agent.ForceWandering();
    }

    private void HandleMatingCompleted()
    {
        Debug.Log($"{agent.gameObject.name}: HandleMatingCompleted called");

        // Switch back to wandering
        agent.ForceWandering();
    }

    private void HandleDeath(string cause)
    {
        agent.OnDeath(cause);
    }
}