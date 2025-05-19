using UnityEngine;

/// <summary>
/// Encapsulates the mating state of an agent without any Unity-specific logic
/// </summary>
public class MatingState
{
    // Properties
    public bool IsMating { get; private set; }
    public float LastMatingTime { get; private set; } = -999f; // Initial value to allow immediate mating
    public IAgent Partner { get; private set; }

    /// <summary>
    /// Sets the state to mating with the specified partner
    /// </summary>
    public void StartMating(IAgent partner)
    {
        IsMating = true;
        Partner = partner;
    }

    /// <summary>
    /// Resets the mating state and records the time
    /// </summary>
    public void EndMating()
    {
        IsMating = false;
        LastMatingTime = Time.time;
        Partner = null;
    }

    /// <summary>
    /// Checks if the agent can mate again based on cooldown time
    /// </summary>
    public bool CanMateAgain(float cooldownDuration)
    {
        return !IsMating && Time.time - LastMatingTime >= cooldownDuration;
    }
}