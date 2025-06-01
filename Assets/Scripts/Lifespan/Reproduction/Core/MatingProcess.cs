using UnityEngine;

/// <summary>
/// Represents a mating process between two agents
/// </summary>
public class MatingProcess
{
    // Properties
    public IAgent Initiator { get; private set; }
    public IAgent Partner { get; private set; }
    public bool IsCompleted { get; private set; }
    public float StartTime { get; private set; }

    /// <summary>
    /// Creates a new mating process between two agents
    /// </summary>
    public MatingProcess(IAgent initiator, IAgent partner)
    {
        Initiator = initiator;
        Partner = partner;
        StartTime = Time.time;
        IsCompleted = false;
    }

    /// <summary>
    /// Checks if offspring creation conditions are met
    /// </summary>
    public bool CanProduceOffspring()
    {
        try
        {
            // CRITICAL FIX: Check if agents still exist
            if (Initiator == null || Partner == null)
                return false;

            // Check if agent adapters are still valid
            if (Initiator is AgentAdapter initiatorAdapter && !IsAgentValid(initiatorAdapter))
                return false;

            if (Partner is AgentAdapter partnerAdapter && !IsAgentValid(partnerAdapter))
                return false;

            // Check energy systems exist and have enough energy
            var initiatorEnergy = Initiator.EnergySystem;
            var partnerEnergy = Partner.EnergySystem;

            if (initiatorEnergy == null || partnerEnergy == null)
                return false;

            return initiatorEnergy.HasEnoughEnergyForMating &&
                   partnerEnergy.HasEnoughEnergyForMating;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Error in CanProduceOffspring: {e.Message}");
            return false;
        }
    }

    private bool IsAgentValid(AgentAdapter adapter)
    {
        try
        {
            return adapter != null && adapter.GameObject != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Calculates the position for a new offspring
    /// </summary>
    public Vector3 CalculateOffspringPosition(Vector2 variance)
    {
        if (Initiator == null || Partner == null)
            return Vector3.zero;

        // Calculate midpoint between agents
        Vector3 midpoint = (Initiator.Position + Partner.Position) / 2f;

        // Add random offset
        midpoint += new Vector3(
            Random.Range(-variance.x, variance.x),
            Random.Range(-variance.y, variance.y),
            0f
        );

        return midpoint;
    }

    /// <summary>
    /// Marks the process as completed
    /// </summary>
    public void Complete()
    {
        IsCompleted = true;
    }
}