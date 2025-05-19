using UnityEngine;

public class AgentContext
{
    public IAgent Agent { get; set; }
    public MovementSystem Movement { get; set; }
    public SensorSystem Sensor { get; set; }
    public IMateFinder MateFinder { get; set; }  // New interface
    public EnergySystem Energy { get; set; }
    public ReproductionSystem Reproduction { get; set; }

    // Helper methods using appropriate interfaces
    public bool HasFoodNearby()
    {
        return Sensor.GetNearestEdible() != null;
    }

    public bool HasPotentialMatesNearby()
    {
        // Check if MateFinder is initialized
        if (MateFinder == null)
        {
            //Debug.LogWarning("MateFinder is null in AgentContext.HasPotentialMatesNearby");
            return false;
        }

        return MateFinder.FindNearestPotentialMate() != null;
    }
}