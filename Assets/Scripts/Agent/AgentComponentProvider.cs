

using UnityEngine;
/// <summary>
/// Default implementation for getting/creating agent components
/// </summary>
public class AgentComponentProvider : IAgentComponentProvider
{
    private readonly MonoBehaviour agent;

    // Component cache
    private MovementSystem movementSystem;
    private SensorSystem sensorSystem;
    private EnergySystem energySystem;
    private ConsumptionSystem consumptionSystem;
    private DeathSystem deathSystem;
    private ReproductionSystem reproductionSystem;

    public AgentComponentProvider(MonoBehaviour agent)
    {
        this.agent = agent;
    }

    public T GetOrAddComponent<T>() where T : Component
    {
        T component = agent.GetComponent<T>();
        if (component == null)
        {
            component = agent.gameObject.AddComponent<T>();
        }
        return component;
    }

    public MovementSystem GetMovementSystem()
    {
        if (movementSystem == null)
            movementSystem = GetOrAddComponent<MovementSystem>();
        return movementSystem;
    }

    public SensorSystem GetSensorSystem()
    {
        if (sensorSystem == null)
            sensorSystem = GetOrAddComponent<SensorSystem>();
        return sensorSystem;
    }

    public EnergySystem GetEnergySystem()
    {
        if (energySystem == null)
            energySystem = GetOrAddComponent<EnergySystem>();
        return energySystem;
    }

    public ConsumptionSystem GetConsumptionSystem()
    {
        if (consumptionSystem == null)
            consumptionSystem = GetOrAddComponent<ConsumptionSystem>();
        return consumptionSystem;
    }

    public DeathSystem GetDeathSystem()
    {
        if (deathSystem == null)
            deathSystem = GetOrAddComponent<DeathSystem>();
        return deathSystem;
    }

    public ReproductionSystem GetReproductionSystem()
    {
        if (reproductionSystem == null)
            reproductionSystem = GetOrAddComponent<ReproductionSystem>();
        return reproductionSystem;
    }
}