using System;
using UnityEngine;

public class SensorMateFinder : IMateFinder
{
    private SensorSystem sensorSystem;
    private GameObject ownerObject;

    public SensorMateFinder(SensorSystem sensorSystem, GameObject owner)
    {
        this.sensorSystem = sensorSystem;
        this.ownerObject = owner;

        // Log creation
        Debug.Log($"SensorMateFinder created for {owner.name}");
    }

    public IAgent FindNearestPotentialMate(Func<IAgent, bool> additionalFilter = null)
    {
        // Safety check
        if (sensorSystem == null)
        {
            Debug.LogError("SensorSystem is null in SensorMateFinder.FindNearestPotentialMate");
            return null;
        }

        try
        {
            // Use the SensorSystem to find a potential mate
            AgentController potentialMate = sensorSystem.GetNearestEntity<AgentController>(
                filter: agent => {
                    // Skip self
                    if (agent == null || agent.gameObject == ownerObject)
                        return false;

                    // Check for reproduction system
                    ReproductionSystem repro = agent.GetComponent<ReproductionSystem>();
                    if (repro == null || !repro.CanMate)
                        return false;

                    // Check for energy
                    EnergySystem energy = agent.GetComponent<EnergySystem>();
                    if (energy == null || !energy.HasEnoughEnergyForMating)
                        return false;

                    // Apply any additional filter if provided
                    return additionalFilter == null || additionalFilter(new AgentAdapter(agent));
                }
            );

            // Convert to IAgent or return null
            return potentialMate != null ? new AgentAdapter(potentialMate) : null;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error in FindNearestPotentialMate: " + e.Message + "\n" + e.StackTrace);
            return null;
        }
    }

    public float GetDistanceTo(IAgent other)
    {
        if (other == null)
        {
            Debug.LogError("Other agent is null in SensorMateFinder.GetDistanceTo");
            return float.MaxValue;
        }

        if (other is AgentAdapter adapter)
        {
            // Calculate distance using the Unity Vector3.Distance
            return Vector3.Distance(ownerObject.transform.position, adapter.Position);
        }

        Debug.LogError("Cannot get distance - other is not an AgentAdapter");
        return float.MaxValue;
    }
}