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

    public AgentContext BuildContext()
    {
        // Get components through provider
        SensorSystem sensorSystem = componentProvider.GetSensorSystem();
        MovementSystem movementSystem = componentProvider.GetMovementSystem();
        EnergySystem energySystem = componentProvider.GetEnergySystem();
        ReproductionSystem reproductionSystem = componentProvider.GetReproductionSystem();

        // Create adapter for self-reference
        IAgent selfAgent = new AgentAdapter(agent.GetComponent<AgentController>());

        // Create mate finder
        IMateFinder mateFinder = new SensorMateFinder(sensorSystem, agent.gameObject);

        // Initialize reproduction system
        reproductionSystem.Initialize(selfAgent, mateFinder, energySystem);

        // Create context
        AgentContext context = new AgentContext
        {
            Agent = selfAgent,
            Movement = movementSystem,
            Sensor = sensorSystem,
            Energy = energySystem,
            Reproduction = reproductionSystem,
            MateFinder = mateFinder
        };

        Debug.Log("Context built - MateFinder: " + (context.MateFinder != null ? "Valid" : "NULL"));

        return context;
    }

    public void UpdateContext(AgentContext context)
    {
        if (context == null)
        {
            Debug.LogError("Cannot update null context");
            return;
        }

        // Get components
        SensorSystem sensorSystem = componentProvider.GetSensorSystem();

        // Ensure MateFinder is valid
        if (context.MateFinder == null && sensorSystem != null)
        {
            Debug.Log("Creating new MateFinder in UpdateContext");
            context.MateFinder = new SensorMateFinder(sensorSystem, agent.gameObject);
        }

        // Update context references (only if necessary)
        if (context.Agent == null)
        {
            context.Agent = new AgentAdapter(agent.GetComponent<AgentController>());
        }

        context.Movement = componentProvider.GetMovementSystem();
        context.Sensor = sensorSystem;
        context.Energy = componentProvider.GetEnergySystem();
        context.Reproduction = componentProvider.GetReproductionSystem();
    }
}