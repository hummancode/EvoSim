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
        Debug.Log("Executing ForagingBehavior");

        // Check if there's food available
        IEdible food = context.Sensor.GetNearestEdible();
        if (food == null)
        {
            Debug.LogWarning("ForagingBehavior cannot find food - switching to wandering");
            context.Movement.SetMovementStrategy(MovementStrategyFactory.CreateRandomMovement());
            return;
        }

        // Create food seeking strategy
        movementStrategy = MovementStrategyFactory.CreateFoodSeeking(context.Sensor);

        // Set the strategy
        Debug.Log("Setting FoodSeekingMovement strategy");
        context.Movement.SetMovementStrategy(movementStrategy);
    }
    public bool ShouldTransition(AgentContext context, out IBehaviorStrategy nextStrategy)
    {
        // Check if we can no longer see food
        IEdible nearestFood = context.Sensor.GetNearestEdible();

        if (nearestFood == null)
        {
            nextStrategy = new WanderingBehavior();
            return true;
        }

        // No transition needed
        nextStrategy = null;
        return false;
    }
}