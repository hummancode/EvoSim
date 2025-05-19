using UnityEngine;

public class MateSeekingBehavior : IBehaviorStrategy
{
    private IAgent currentTarget;
    private IMovementStrategy movementStrategy;

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
                Debug.Log("Close enough to mate, initiating mating");

                // Start mating
                context.Reproduction.InitiateMating(potentialMate);

                // Tell partner to accept mating
                potentialMate.ReproductionSystem.AcceptMating(context.Agent);

                // Stay still during mating
                StopMoving(context);
                Debug.Log("Mating initiated, staying still");
            }
            else
            {
                // Not close enough, move toward mate
                if (potentialMate is AgentAdapter adapter)
                {
                    Debug.Log("Moving toward potential mate");

                    // Set as current target
                    currentTarget = potentialMate;

                    // Create targeted movement
                    movementStrategy = new TargetedMovement(adapter.GameObject.transform);
                    context.Movement.SetMovementStrategy(movementStrategy);
                }
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

    private void StopMoving(AgentContext context)
    {
        // Create a stationary strategy
        movementStrategy = new StationaryMovement();
        context.Movement.SetMovementStrategy(movementStrategy);
    }
}