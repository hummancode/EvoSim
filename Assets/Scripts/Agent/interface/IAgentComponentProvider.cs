

using UnityEngine;
/// <summary>
/// Interface for providing agent components
/// </summary>
public interface IAgentComponentProvider
{
    T GetOrAddComponent<T>() where T : Component;
    MovementSystem GetMovementSystem();
    SensorSystem GetSensorSystem();
    EnergySystem GetEnergySystem();
    ConsumptionSystem GetConsumptionSystem();
    DeathSystem GetDeathSystem();
    ReproductionSystem GetReproductionSystem();
}