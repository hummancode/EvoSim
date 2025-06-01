using UnityEngine;

public class BehaviorManager : IBehaviorManager
{
    private readonly AgentController agent;
    private readonly float updateInterval;

    private IBehaviorStrategy currentBehavior;
    private float lastUpdateTime;

    public BehaviorManager(AgentController agent, float updateInterval)
    {
        this.agent = agent;
        this.updateInterval = updateInterval;
    }

    public void SetInitialBehavior(AgentContext context)
    {
        SetBehavior(new WanderingBehavior(), context);
    }

    public void UpdateBehavior(AgentContext context)
    {
        // Check update interval
        if (Time.time - lastUpdateTime < updateInterval)
            return;

        // Execute current behavior
        currentBehavior?.Execute(context);

        // Determine if behavior should change
        DetermineBehavior(context);

        lastUpdateTime = Time.time;
    }

    public void ForceBehavior<T>(AgentContext context) where T : IBehaviorStrategy, new()
    {
        SetBehavior(new T(), context);
    }

    private void DetermineBehavior(AgentContext context)
    {
        // Get references
        IReproductionCapable reproduction = context.Reproduction;
        IEnergyProvider energy = context.Energy;
        SensorSystem sensor = context.Sensor;

        // ADD THIS - Get age system to check maturity
        AgeSystem ageSystem = agent.GetComponent<AgeSystem>();

        // If currently mating, stay in mating behavior
        if (reproduction.IsMating)
        {
            if (!(currentBehavior is MatingBehavior))
                SetBehavior(new MatingBehavior(), context);
            return;
        }

        // If hungry and food is nearby, switch to foraging
        if (energy.IsHungry)
        {
            IEdible nearestFood = sensor.GetNearestEdible();
            if (nearestFood != null)
            {
                if (!(currentBehavior is ForagingBehavior))
                    SetBehavior(new ForagingBehavior(), context);
                return;
            }
        }

        // MODIFIED - If ready to mate AND mature, seek mates
        if (context.Energy.HasEnoughEnergyForMating &&
                 context.Reproduction.CanMate &&
                 context.Maturity.CanReproduce)
        {
            IAgent potentialMate = context.MateFinder.FindNearestPotentialMate();

            if (potentialMate != null)
            {
                if (!(currentBehavior is MateSeekingBehavior))
                    SetBehavior(new MateSeekingBehavior(), context);
                return;
            }
        }

        // Default to wandering
        if (!(currentBehavior is WanderingBehavior))
            SetBehavior(new WanderingBehavior(), context);
    }
    private void SetBehavior(IBehaviorStrategy behavior, AgentContext context)
    {
        // Avoid redundant behavior changes
        if (currentBehavior != null && behavior.GetType() == currentBehavior.GetType())
            return;

        currentBehavior = behavior;

        // Update the display name in the agent
        string behaviorName = behavior != null ? behavior.GetType().Name : "None";
        agent.UpdateBehaviorName(behaviorName);

        Debug.Log($"Agent {agent.gameObject.name} (Gen {agent.Generation}) switched to {behaviorName}");
    }
}