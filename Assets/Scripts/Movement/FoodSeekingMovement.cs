using UnityEngine;

public class FoodSeekingMovement : IMovementStrategy
{
    private readonly ISensorCapability sensorCapability;
    private readonly IMovementStrategy fallbackStrategy;
    private IEdible currentTarget;

    public FoodSeekingMovement(ISensorCapability sensorCapability, IMovementStrategy fallbackStrategy = null)
    {
        this.sensorCapability = sensorCapability ?? throw new System.ArgumentNullException(nameof(sensorCapability));
        this.fallbackStrategy = fallbackStrategy ?? new RandomMovement();
    }

    public void Move(Transform transform, Vector3 currentPosition, ref Vector3 target, float speed)
    {
        // Try to find food
        Vector3? foodPosition = sensorCapability.GetTargetPosition();

        if (foodPosition.HasValue)
        {
            currentTarget = sensorCapability.GetTargetObject();
            target = foodPosition.Value;

            // Move directly toward food
            Vector3 direction = target - currentPosition;

            if (direction.magnitude > 0.1f)
            {
                direction.Normalize();
                transform.position += direction * speed * Time.deltaTime;

                // Face the food
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0, 0, angle - 90);
            }
        }
        else
        {
            // No food found, use fallback strategy
            currentTarget = null;
            fallbackStrategy.Move(transform, currentPosition, ref target, speed);
        }
    }

    public Vector3 ChooseDestination(Vector3 currentPosition, Vector2 worldBounds)
    {
        // Try to find food
        Vector3? foodPosition = sensorCapability.GetTargetPosition();

        if (foodPosition.HasValue)
        {
            return foodPosition.Value;
        }
        else
        {
            // No food found, use fallback strategy
            return fallbackStrategy.ChooseDestination(currentPosition, worldBounds);
        }
    }
}

// Sensor adapter interface to decouple from SensorSystem


// Adapter to convert SensorSystem to ISensorCapability
