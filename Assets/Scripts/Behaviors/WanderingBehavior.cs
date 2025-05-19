using UnityEngine;

public class WanderingBehavior : IBehaviorStrategy
{
    private IMovementStrategy movementStrategy;

    public WanderingBehavior()
    {
        // Create a movement strategy with wandering parameters
        movementStrategy = MovementStrategyFactory.CreateRandomMovement(
            changeFrequency: 0.1f,
            directionalBias: 0f,
            wanderFactor: 0.8f
        );
    }

    public void Execute(AgentContext context)
    {
        // Apply the random movement strategy
        context.Movement.SetMovementStrategy(movementStrategy);
    }
    public bool ShouldTransition(AgentContext context, out IBehaviorStrategy nextStrategy)
    {
        // Check if food is detected
        GameObject food = context.Sensor.GetNearestFood();

        if (food != null)
        {
            nextStrategy = new ForagingBehavior();
            return true;
        }
        else if (context.Energy.HasEnoughEnergyForMating)
        {
            // This could be extended to actually check if mates are nearby
            nextStrategy = new MateSeekingBehavior();
            return true;
        }
        // No transition needed
        nextStrategy = null;
        return false;
    }
}