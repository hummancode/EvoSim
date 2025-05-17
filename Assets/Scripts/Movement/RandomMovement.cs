using UnityEngine;

public class RandomMovement : IMovementStrategy
{
    // Movement parameters
    private float changeDirectionChance = 0.1f;  // Chance to change direction each update
    private float minMoveDistance = 5f;          // Minimum distance to move before possibly changing
    private float maxMoveDistance = 15f;         // Maximum distance to move in one direction
    private float distanceTraveled = 0f;         // Track distance moved in current direction
    private float nextDirectionChange = 0f;      // Distance threshold for next direction change

    // Optional: Add personality to movement
    private float directionalBias = 0f;          // Tendency to favor certain directions (-1 to 1)
    private float wanderFactor = 0.8f;           // How much the agent "wanders" during movement (0-1)

    public RandomMovement(float changeDirectionFrequency = 0.1f, float directionalBias = 0f, float wanderFactor = 0.8f)
    {
        this.changeDirectionChance = Mathf.Clamp01(changeDirectionFrequency);
        this.directionalBias = Mathf.Clamp(directionalBias, -1f, 1f);
        this.wanderFactor = Mathf.Clamp01(wanderFactor);
        SetNextDirectionChangeThreshold();
    }

    public void Move(Transform transform, Vector3 currentPosition, ref Vector3 target, float speed)
    {
        Vector3 direction = target - currentPosition;

        // If we're close to target or should change direction
        if (direction.magnitude < 0.1f || ShouldChangeDirection(direction.magnitude))
        {
            // Reset tracking and choose a new destination
            distanceTraveled = 0f;
            SetNextDirectionChangeThreshold();
            target = ChooseDestination(currentPosition, new Vector2(50f, 50f)); // Default bounds if none provided
        }

        // Apply some wandering behavior to make movement more natural
        if (wanderFactor > 0)
        {
            Vector3 wanderDirection = new Vector3(
                Mathf.Sin(Time.time * 2f) * wanderFactor,
                Mathf.Cos(Time.time * 3f) * wanderFactor, // Y instead of 0
                0  // Z is 0 in 2D
            );

            // Blend between direct path and wander
            direction = Vector3.Lerp(direction.normalized,
                                    (direction.normalized + wanderDirection).normalized,
                                    wanderFactor * 0.3f);
        }
        else
        {
            direction = direction.normalized;
        }

        // Calculate movement this frame
        Vector3 movement = direction * speed * Time.deltaTime;

        // Update position
        transform.position += movement;

        // Track distance traveled
        distanceTraveled += movement.magnitude;

        // Update rotation to face movement direction
        if (movement != Vector3.zero)
        {
            // Calculate angle in degrees from vector
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // Create a target rotation (may need to adjust the -90 offset 
            // depending on which way your sprite faces by default)
            Quaternion targetRotation = Quaternion.Euler(0, 0, angle - 90);

            // Smoothly rotate
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                0.15f  // Smooth rotation factor
            );
        }
    }

    public Vector3 ChooseDestination(Vector3 currentPosition, Vector2 worldBounds)
    {
        // Directional bias affects where we choose to go
        float biasedXRange = directionalBias * worldBounds.x * 0.5f;

        // Choose random destination with potential bias
        float randomX = Random.Range(-worldBounds.x + biasedXRange, worldBounds.x + biasedXRange);
        float randomY = Random.Range(-worldBounds.y, worldBounds.y); // Y instead of Z for 2D

        // Clamp to ensure within world bounds
        randomX = Mathf.Clamp(randomX, -worldBounds.x, worldBounds.x);
        randomY = Mathf.Clamp(randomY, -worldBounds.y, worldBounds.y); // Y instead of Z

        // Choose random distance to travel in this direction
        float distanceMultiplier = Random.Range(0.3f, 1.0f);
        Vector3 direction = new Vector3(randomX, randomY, 0) - currentPosition; // Y instead of 0, Z is 0

        // Return a point along that direction vector
        return currentPosition + direction.normalized *
               Mathf.Lerp(minMoveDistance, maxMoveDistance, distanceMultiplier);
    }

    private bool ShouldChangeDirection(float distanceToTarget)
    {
        // Change direction if we've traveled far enough or randomly
        bool distanceCheck = distanceTraveled >= nextDirectionChange;
        bool randomCheck = Random.value < changeDirectionChance * Time.deltaTime;

        return distanceCheck || randomCheck;
    }

    private void SetNextDirectionChangeThreshold()
    {
        // Set a random distance threshold for the next direction change
        nextDirectionChange = Random.Range(minMoveDistance, maxMoveDistance);
    }
}