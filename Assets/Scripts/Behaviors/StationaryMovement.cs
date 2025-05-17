using UnityEngine;

public class StationaryMovement : IMovementStrategy
{
    public void Move(Transform transform, Vector3 currentPosition, ref Vector3 target, float speed)
    {
        // Don't move - just stay still
        target = currentPosition;
    }

    public Vector3 ChooseDestination(Vector3 currentPosition, Vector2 worldBounds)
    {
        // Stay at current position
        return currentPosition;
    }
}