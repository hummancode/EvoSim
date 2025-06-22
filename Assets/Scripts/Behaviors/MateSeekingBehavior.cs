using UnityEngine;

/// <summary>
/// Behavior for seeking and initiating mating with other agents
/// FIXED: High-speed reliability improvements
/// </summary>
public class MateSeekingBehavior : IBehaviorStrategy
{
    private IMovementStrategy movementStrategy;
    private IAgent currentTarget;

    // HIGH-SPEED FIX: Add timing controls
    private float lastTargetSearch = 0f;
    private float targetSearchInterval = 0.1f; // Search for mates more frequently

    // HIGH-SPEED FIX: Add adaptive mating distances
    private float GetAdaptiveMatingDistance()
    {
        float timeScale = Time.timeScale;
        if (timeScale > 75f)
            return 3f;    // Larger distance for very high speeds
        else if (timeScale > 30f)
            return 2f;    // Larger distance for high speeds
        else
            return 1f;    // Normal distance for normal speeds
    }

    public void Execute(AgentContext context)
    {
        // Check if already mating
        if (context.Reproduction.IsMating)
        {
            StopMoving(context);
            return;
        }

        // HIGH-SPEED FIX: More frequent target searching at high speeds
        bool shouldSearchForTarget = Time.time - lastTargetSearch > GetAdaptiveSearchInterval();

        if (shouldSearchForTarget || currentTarget == null)
        {
            SearchForMate(context);
            lastTargetSearch = Time.time;
        }

        // Execute mating logic
        if (currentTarget != null)
        {
            ExecuteMatingLogic(context);
        }
        else
        {
            // No potential mates, just wander
            WanderWithoutTarget(context);
        }
    }

    /// <summary>
    /// HIGH-SPEED FIX: Adaptive search interval based on time scale
    /// </summary>
    private float GetAdaptiveSearchInterval()
    {
        float timeScale = Time.timeScale;

        if (timeScale > 50f)
            return 0.05f;  // Search every 0.05 game seconds at very high speeds
        else if (timeScale > 20f)
            return 0.1f;   // Search every 0.1 game seconds at high speeds
        else
            return 0.2f;   // Search every 0.2 game seconds at normal speeds
    }

    /// <summary>
    /// HIGH-SPEED FIX: Improved mate searching with better validation
    /// </summary>
    private void SearchForMate(AgentContext context)
    {
        try
        {
            // Find a potential mate using the mate finder
            IAgent potentialMate = context.MateFinder.FindNearestPotentialMate();

            // HIGH-SPEED FIX: Additional validation for high-speed scenarios
            if (potentialMate != null && ValidatePotentialMate(potentialMate))
            {
                currentTarget = potentialMate;

                if (Debug.isDebugBuild && Time.timeScale > 30f)
                {
                    Debug.Log($"HIGH-SPEED MATE SEARCH: Found target at {Time.timeScale:F0}x speed");
                }
            }
            else
            {
                currentTarget = null;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Error searching for mate: {e.Message}");
            currentTarget = null;
        }
    }

    /// <summary>
    /// HIGH-SPEED FIX: Better validation for potential mates
    /// </summary>
    private bool ValidatePotentialMate(IAgent potentialMate)
    {
        try
        {
            if (potentialMate == null)
                return false;

            // Check if adapter is valid
            if (potentialMate is AgentAdapter adapter && !adapter.IsValid())
                return false;

            // Check if systems exist
            var reproduction = potentialMate.ReproductionSystem;
            var energy = potentialMate.EnergySystem;

            if (reproduction == null || energy == null)
                return false;

            // Check if mate is available
            if (!reproduction.CanMate || !energy.HasEnoughEnergyForMating)
                return false;

            // HIGH-SPEED FIX: Check if already mating (to avoid conflicts)
            if (MatingCoordinator.Instance != null &&
                MatingCoordinator.Instance.IsAgentMating(potentialMate))
                return false;

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Error validating potential mate: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// HIGH-SPEED FIX: Improved mating logic with adaptive distances
    /// </summary>
    private void ExecuteMatingLogic(AgentContext context)
    {
        // Validate target is still good
        if (!ValidatePotentialMate(currentTarget))
        {
            currentTarget = null;
            return;
        }

        // Get adaptive mating distance based on current speed
        float matingDistance = GetAdaptiveMatingDistance();

        // Check if close enough to mate
        if (context.Reproduction.CanMateWith(currentTarget))
        {
            AttemptMating(context);
        }
        else
        {
            // HIGH-SPEED FIX: Check distance manually for high-speed scenarios
            float distance = context.MateFinder.GetDistanceTo(currentTarget);

            if (distance <= matingDistance)
            {
                // Close enough for high-speed mating
                AttemptMating(context);
            }
            else
            {
                // Move toward the mate
                MoveTowardMate(context);
            }
        }
    }

    /// <summary>
    /// HIGH-SPEED FIX: Improved mating attempt with better error handling
    /// </summary>
    private void AttemptMating(AgentContext context)
    {
        try
        {
            // Create and execute mating command
            ICommand matingCommand = new InitiateMatingCommand(
                context.Agent,
                currentTarget,
                MatingCoordinator.Instance
            );

            CommandDispatcher.Instance.ExecuteCommand(matingCommand);

            // Stop moving immediately
            StopMoving(context);

            // Clear current target
            currentTarget = null;

            if (Debug.isDebugBuild && Time.timeScale > 30f)
            {
                Debug.Log($"HIGH-SPEED MATING: Attempted mating at {Time.timeScale:F0}x speed");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error attempting mating: {e.Message}");
            currentTarget = null;
            StopMoving(context);
        }
    }

    /// <summary>
    /// Sets movement to approach the potential mate
    /// HIGH-SPEED FIX: Improved movement with speed scaling
    /// </summary>
    private void MoveTowardMate(AgentContext context)
    {
        if (currentTarget is AgentAdapter adapter && adapter.IsValid())
        {
            // HIGH-SPEED FIX: Use faster movement strategy at high speeds
            movementStrategy = CreateAdaptiveTargetedMovement(adapter.GameObject.transform);
            context.Movement.SetMovementStrategy(movementStrategy);
        }
        else
        {
            // Target became invalid
            currentTarget = null;
            WanderWithoutTarget(context);
        }
    }

    /// <summary>
    /// HIGH-SPEED FIX: Create movement strategy adapted to current time scale
    /// </summary>
    private IMovementStrategy CreateAdaptiveTargetedMovement(Transform target)
    {
        // At very high speeds, we might want different movement behavior
        // For now, just use the standard TargetedMovement
        return new TargetedMovement(target);
    }

    /// <summary>
    /// Wander when no target is available
    /// HIGH-SPEED FIX: Adaptive wandering
    /// </summary>
    private void WanderWithoutTarget(AgentContext context)
    {
        // Clear current target
        currentTarget = null;

        // HIGH-SPEED FIX: Use faster wandering at high speeds
        float timeScale = Time.timeScale;
        float changeFreq = timeScale > 50f ? 0.3f : 0.1f; // More frequent direction changes at high speeds

        movementStrategy = MovementStrategyFactory.CreateRandomMovement(changeFreq);
        context.Movement.SetMovementStrategy(movementStrategy);
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