using UnityEngine;

public class ForagingBehavior : IBehaviorStrategy
{
    private IMovementStrategy movementStrategy;

    public ForagingBehavior()
    {
        // Movement strategy will be created in Execute
    }

    public void Execute(AgentContext context)
    {
        // Create food seeking strategy using the agent's sensor
        movementStrategy = MovementStrategyFactory.CreateFoodSeeking(context.Sensor);
        context.Movement.SetMovementStrategy(movementStrategy);
    }
    public bool ShouldTransition(AgentContext context, out IBehaviorStrategy nextStrategy)
    {
        // Check if we can no longer see food
        GameObject food = context.Sensor.GetNearestFood();

        if (food == null)
        {
            nextStrategy = new WanderingBehavior();
            return true;
        }

        // No transition needed
        nextStrategy = null;
        return false;
    }
}