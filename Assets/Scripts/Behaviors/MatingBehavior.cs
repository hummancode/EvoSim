using UnityEngine;

public class MatingBehavior : IBehaviorStrategy
{
    private Transform partnerTransform;
    private IMovementStrategy stationaryStrategy;
    private float matingStartTime;

    public MatingBehavior(Transform partner)
    {
        partnerTransform = partner;
        stationaryStrategy = new StationaryMovement();
        matingStartTime = Time.time;
    }

    public void Execute(AgentContext context)
    {
        // Stay still during mating
        context.Movement.SetMovementStrategy(stationaryStrategy);
    }

    public bool ShouldTransition(AgentContext context, out IBehaviorStrategy nextStrategy)
    {
        nextStrategy = null;

        // Check if mating has completed
        if (!context.Reproduction.IsMating)
        {
            // Transition to wandering when mating is complete
            nextStrategy = new WanderingBehavior();
            return true;
        }

        // Double-check timing (fallback in case IsMating doesn't update correctly)
        // This ensures we don't get stuck in the mating behavior
        float elapsedTime = Time.time - matingStartTime;
        if (elapsedTime > 15f) // Slightly longer than mating duration for safety
        {
            Debug.LogWarning("Mating behavior timeout - forcing transition to wandering");
            nextStrategy = new WanderingBehavior();
            return true;
        }

        return false;
    }
}