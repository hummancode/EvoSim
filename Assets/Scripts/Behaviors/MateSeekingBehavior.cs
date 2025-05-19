using UnityEngine;

/// <summary>
/// Behavior for seeking and initiating mating with other agents
/// </summary>
public class MateSeekingBehavior : IBehaviorStrategy
{
    private IMovementStrategy movementStrategy;
    private IAgent currentTarget;

    public void Execute(AgentContext context)
    {
        // Check if already mating
        if (context.Reproduction.IsMating)
        {
            Debug.Log("Already mating, staying still");
            StopMoving(context);
            return;
        }

        // Find a potential mate using the adapter
        IAgent potentialMate = context.MateFinder.FindNearestPotentialMate();

        if (potentialMate != null)
        {
            Debug.Log("Found potential mate, checking if close enough to mate");

            // Check if close enough to mate
            if (context.Reproduction.CanMateWith(potentialMate))
            {
                Debug.Log("Close enough to mate, creating mating command");

                // Create and execute mating command
                ICommand matingCommand = new InitiateMatingCommand(
                    context.Agent,
                    potentialMate,
                    MatingCoordinator.Instance
                );

                CommandDispatcher.Instance.ExecuteCommand(matingCommand);
                StopMoving(context);
            }
            else
            {
                Debug.Log("Moving toward potential mate");
                MoveTowardMate(context, potentialMate);
            }
        }
        else
        {
            Debug.Log("No potential mates found, wandering");

            // No potential mates, just wander
            currentTarget = null;
            movementStrategy = MovementStrategyFactory.CreateRandomMovement();
            context.Movement.SetMovementStrategy(movementStrategy);
        }
    }

    /// <summary>
    /// Sets movement to approach the potential mate
    /// </summary>
    private void MoveTowardMate(AgentContext context, IAgent potentialMate)
    {
        if (potentialMate is AgentAdapter adapter && adapter.GameObject != null)
        {
            // Set as current target
            currentTarget = potentialMate;

            // Create targeted movement
            movementStrategy = new TargetedMovement(adapter.GameObject.transform);
            context.Movement.SetMovementStrategy(movementStrategy);
        }
    }

    /// <summary>
    /// Stops the agent's movement
    /// </summary>
    private void StopMoving(AgentContext context)
    {
        // Create a stationary strategy
        movementStrategy = new StationaryMovement();
        context.Movement.SetMovementStrategy(movementStrategy);
    }
}