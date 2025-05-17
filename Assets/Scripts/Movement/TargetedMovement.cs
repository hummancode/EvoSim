using UnityEngine;

public class TargetedMovement : IMovementStrategy
{
    private Transform target;
    private float arrivalDistance = 0.5f;

    public TargetedMovement(Transform targetTransform)
    {
        target = targetTransform;
    }

    public void Move(Transform transform, Vector3 currentPosition, ref Vector3 targetPosition, float speed)
    {
        // Update target position if target still exists
        if (target != null)
        {
            targetPosition = target.position;
        }

        // Move towards target
        Vector3 direction = targetPosition - currentPosition;

        // If we're not at the target yet
        if (direction.magnitude > arrivalDistance)
        {
            // Calculate movement this frame
            direction.Normalize();
            Vector3 movement = direction * speed * Time.deltaTime;

            // Update position
            transform.position += movement;

            // Update rotation to face movement direction
            if (movement != Vector3.zero)
            {
                // Calculate angle in degrees
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                // Rotate to face direction (adjust -90 as needed for your sprite orientation)
                transform.rotation = Quaternion.Euler(0, 0, angle - 90);
            }
        }
    }

    public Vector3 ChooseDestination(Vector3 currentPosition, Vector2 worldBounds)
    {
        // Return target position if target exists, otherwise return current position
        return target != null ? target.position : currentPosition;
    }
}