using UnityEngine;

public class MateSeekingBehavior : IBehaviorStrategy
{
    private IMovementStrategy movementStrategy;
    private AgentController currentTarget;

    public void Execute(AgentContext context)
    {
        // Check if already mating
        if (context.Reproduction.IsMating)
        {
            // Stay still during mating
            StopMoving(context);
            return;
        }

        // Try to find a mate if close enough
        bool foundMate = context.Reproduction.TryFindMate();

        // If didn't find a close mate, look for one to move toward
        if (!foundMate)
        {
            // Use sensor to find a potential mate
            AgentController potentialMate = context.Sensor.GetNearestEntity<AgentController>(
                filter: agent => {
                    ReproductionSystem reproduction = agent.GetComponent<ReproductionSystem>();
                    EnergySystem energy = agent.GetComponent<EnergySystem>();

                    return reproduction != null && reproduction.CanMateAgain &&
                           energy != null && energy.HasEnoughEnergyForMating;
                }
            );

            if (potentialMate != null)
            {
                // Move towards potential mate
                currentTarget = potentialMate;
                movementStrategy = new TargetedMovement(potentialMate.transform);
                context.Movement.SetMovementStrategy(movementStrategy);
            }
            else
            {
                // No potential mates, revert to wandering
                movementStrategy = MovementStrategyFactory.CreateRandomMovement();
                context.Movement.SetMovementStrategy(movementStrategy);
            }
        }
    }

    private void StopMoving(AgentContext context)
    {
        // Create a stationary strategy
        movementStrategy = new StationaryMovement();
        context.Movement.SetMovementStrategy(movementStrategy);
    }

    public bool ShouldTransition(AgentContext context, out IBehaviorStrategy nextStrategy)
    {
        nextStrategy = null;

        // If mating completed, return to wandering
        if (!context.Reproduction.IsMating && context.Reproduction.LastMatingTime > 0)
        {
            nextStrategy = new WanderingBehavior();
            return true;
        }

        // If no longer has energy to mate, check if hungry
        if (!context.Energy.HasEnoughEnergyForMating || !context.Reproduction.CanMateAgain)
        {
            if (context.Energy.IsHungry)
            {
                // Check if food is nearby
                GameObject food = context.Sensor.GetNearestFood();
                if (food != null)
                {
                    nextStrategy = new ForagingBehavior();
                }
                else
                {
                    // No food nearby, just wander
                    nextStrategy = new WanderingBehavior();
                }
                return true;
            }
            else
            {
                // Not hungry, return to wandering
                nextStrategy = new WanderingBehavior();
                return true;
            }
        }

        // If no potential mates nearby, check if hungry
        AgentController potentialMate = context.Sensor.GetNearestEntity<AgentController>(
            filter: agent => {
                ReproductionSystem reproduction = agent.GetComponent<ReproductionSystem>();
                EnergySystem energy = agent.GetComponent<EnergySystem>();

                return reproduction != null && reproduction.CanMateAgain &&
                       energy != null && energy.HasEnoughEnergyForMating;
            }
        );

        if (potentialMate == null)
        {
            if (context.Energy.IsHungry)
            {
                // Check if food is nearby
                GameObject food = context.Sensor.GetNearestFood();
                if (food != null)
                {
                    nextStrategy = new ForagingBehavior();
                    return true;
                }
            }

            // No mates and not hungry (or no food), return to wandering
            nextStrategy = new WanderingBehavior();
            return true;
        }

        // No transition needed - continue seeking mate
        return false;
    }
}