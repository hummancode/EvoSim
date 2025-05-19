using UnityEngine;
public interface IAgent
{
    GameObject GameObject { get; }
    Vector3 Position { get; }
    IReproductionCapable ReproductionSystem { get; }
    IEnergyProvider EnergySystem { get; }
}