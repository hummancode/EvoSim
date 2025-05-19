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

    // Create mate seeking strategy
    public static IMovementStrategy CreateMateSeekingMovement(SensorSystem sensor)
    {
        // This would require implementing a MateSensorAdapter similar to FoodSensorAdapter
        IMovementStrategy fallback = CreateRandomMovement();

        // For now, we'll just create a direct targeted movement in MateSeekingBehavior
        // but this factory could be expanded with a proper adapter
        return fallback;
    }
}