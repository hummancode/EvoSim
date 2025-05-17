public static class MovementStrategyFactory
{
    // Create random movement strategy
    public static IMovementStrategy CreateRandomMovement(
        float changeFrequency = 0.1f,
        float directionalBias = 0f,
        float wanderFactor = 0.8f)
    {
        return new RandomMovement(changeFrequency, directionalBias, wanderFactor);
    }

    // Create food seeking strategy with default random fallback
    public static IMovementStrategy CreateFoodSeeking(SensorSystem sensor)
    {
        // Create default random movement as fallback
        IMovementStrategy fallback = CreateRandomMovement();
        return CreateFoodSeeking(sensor, fallback);
    }

    // Create food seeking strategy with custom fallback
    public static IMovementStrategy CreateFoodSeeking(SensorSystem sensor, IMovementStrategy fallbackStrategy)
    {
        return new FoodSeekingMovement(
            new FoodSensorAdapter(sensor),
            fallbackStrategy
        );
    }

    // Advanced factory method that creates different behaviors based on context
    //public static IMovementStrategy CreateBehaviorStrategy(
    //    SensorSystem sensor,
    //    AgentContext context) // AgentContext could include energy, age, etc.
    //{
    //    // You could create different behaviors based on agent state
    //    if (context.IsHungry)
    //    {
    //        return CreateFoodSeeking(sensor);
    //    }
    //    else if (context.IsMature && context.HasHighEnergy)
    //    {
    //        // Future: Mate seeking strategy
    //        return CreateMateSeeking(sensor);
    //    }
    //    else
    //    {
    //        return CreateRandomMovement();
    //    }
    //}
}

// Optional: Context object for more complex factory decisions
