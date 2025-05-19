using UnityEngine;

/// <summary>
/// Behavior for agents that are currently mating
/// </summary>
public class MatingBehavior : IBehaviorStrategy
{
    private IMovementStrategy stationaryStrategy;

    public MatingBehavior()
    {
        stationaryStrategy = new StationaryMovement();
    }

    public void Execute(AgentContext context)
    {
        // Stay still during mating
        context.Movement.SetMovementStrategy(stationaryStrategy);

        // Check if mating has completed
        if (!context.Reproduction.IsMating)
        {
            Debug.Log("Mating completed, should transition to another behavior");
        }
    }
}