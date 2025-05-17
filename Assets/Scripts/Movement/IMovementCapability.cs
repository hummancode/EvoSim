using UnityEngine;
public interface IMovementCapability
{
    void SetTarget(Vector3 target);
    void SetTargetEntity(Transform targetTransform);
    void SetRandomMovement();
}