using UnityEngine;

public class SmartBehaviorManager : IBehaviorManager
{
    private readonly AgentController agent;
    private readonly float baseBehaviorInterval;

    private IBehaviorStrategy currentBehavior;
    private float lastUpdateTime;
    private float adaptiveInterval;

    public SmartBehaviorManager(AgentController agent, float baseBehaviorInterval)
    {
        this.agent = agent;
        this.baseBehaviorInterval = baseBehaviorInterval;
        CalculateAdaptiveInterval();
    }

    public void UpdateBehavior(AgentContext context)
    {
        // SMART STRATEGY: More frequent logical updates at higher speeds
        // This ensures agents make decisions at appropriate game-time intervals

        if (Time.time - lastUpdateTime < adaptiveInterval)
            return;

        // Execute current behavior
        currentBehavior?.Execute(context);

        // Make behavioral decisions
        DetermineBehavior(context);

        lastUpdateTime = Time.time;

        // Recalculate interval if time scale changed significantly
        if (Time.frameCount % 30 == 0) // Check every 30 frames
        {
            CalculateAdaptiveInterval();
        }
    }

    /// <summary>
    /// Calculate optimal behavior update interval based on time scale
    /// </summary>
    private void CalculateAdaptiveInterval()
    {
        // At higher time scales, we want the same logical update frequency
        // So if game runs 10x faster, we update 10x more often in real-time
        // This maintains consistent decision-making in game-time

        float timeScale = Mathf.Max(Time.timeScale, 0.1f);

        // Maintain consistent game-time intervals
        adaptiveInterval = baseBehaviorInterval / timeScale;

        // Clamp to reasonable real-time bounds for performance
        adaptiveInterval = Mathf.Clamp(adaptiveInterval, 0.016f, 1f); // 60fps to 1fps

        // At very high speeds (>60x), we cap update frequency to maintain performance
        if (timeScale > 60f)
        {
            adaptiveInterval = 0.016f; // Max 60 updates per second in real-time
        }
    }

    // Rest of your existing behavior logic...
    public void SetInitialBehavior(AgentContext context)
    {
        SetBehavior(new WanderingBehavior(), context);
    }

    public void ForceBehavior<T>(AgentContext context) where T : IBehaviorStrategy, new()
    {
        SetBehavior(new T(), context);
    }

    private void DetermineBehavior(AgentContext context)
    {
        // High-priority behaviors first (these are time-critical)
        if (context.Reproduction.IsMating)
        {
            if (!(currentBehavior is MatingBehavior))
                SetBehavior(new MatingBehavior(), context);
            return;
        }

        // Energy-critical behavior
        if (context.Energy.IsHungry)
        {
            IEdible nearestFood = context.Sensor.GetNearestEdible();
            if (nearestFood != null)
            {
                if (!(currentBehavior is ForagingBehavior))
                    SetBehavior(new ForagingBehavior(), context);
                return;
            }
        }

        // Reproduction behavior
        if (context.Energy.HasEnoughEnergyForMating && context.Reproduction.CanMate)
        {
            IAgent potentialMate = context.MateFinder.FindNearestPotentialMate();
            if (potentialMate != null)
            {
                if (!(currentBehavior is MateSeekingBehavior))
                    SetBehavior(new MateSeekingBehavior(), context);
                return;
            }
        }

        // Default behavior
        if (!(currentBehavior is WanderingBehavior))
            SetBehavior(new WanderingBehavior(), context);
    }

    private void SetBehavior(IBehaviorStrategy behavior, AgentContext context)
    {
        if (currentBehavior != null && behavior.GetType() == currentBehavior.GetType())
            return;

        currentBehavior = behavior;
        string behaviorName = behavior != null ? behavior.GetType().Name : "None";
        agent.UpdateBehaviorName(behaviorName);
    }

    /// <summary>
    /// Get current update frequency for monitoring
    /// </summary>
    public float GetCurrentUpdateFrequency()
    {
        return 1f / adaptiveInterval;
    }
}