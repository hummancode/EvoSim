using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Define movement strategy interface
public interface IMovementStrategy
{
    void Move(Transform transform, Vector3 currentPosition, ref Vector3 target, float speed);
    Vector3 ChooseDestination(Vector3 currentPosition, Vector2 worldBounds);
}

// Random movement implementation


// Food-seeking movement implementation


// In MovementSystem
public class MovementSystem : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private bool isMoving = true;

    private Vector3 currentDestination;
    private IMovementStrategy movementStrategy;

    void Update()
    {
        if (isMoving && movementStrategy != null)
        {
            movementStrategy.Move(transform, transform.position, ref currentDestination, moveSpeed);
        }
    }

    public void SetMovementStrategy(IMovementStrategy strategy)
    {
        movementStrategy = strategy;
    }

    public void SetNewDestination(Vector2 worldBounds)
    {
        if (movementStrategy != null)
        {
            currentDestination = movementStrategy.ChooseDestination(transform.position, worldBounds);
        }
    }

    public void SetSpeed(float speed)
    {
        moveSpeed = speed;
    }

    public void StartMoving()
    {
        isMoving = true;
    }

    public void StopMoving()
    {
        isMoving = false;
    }
}