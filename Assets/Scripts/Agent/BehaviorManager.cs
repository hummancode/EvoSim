using UnityEngine;
/// <summary>
/// Default implementation for managing behaviors
/// </summary>
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
        // If currently mating, stay in mating behavior
        if (reproductionSystem.IsMating)
        {
            if (!(currentBehavior is MatingBehavior))
                SetBehavior(new MatingBehavior());
            return;
        }

        // If hungry and food is nearby, switch to foraging
        if (energySystem.IsHungry)
        {
            IEdible nearestFood = sensorSystem.GetNearestEdible();
            if (nearestFood != null)
            {
                if (!(currentBehavior is ForagingBehavior))
                    SetBehavior(new ForagingBehavior());
                return;
            }
        }

        // If ready to mate and potential mates exist, seek mates
        if (energySystem.HasEnoughEnergyForMating && reproductionSystem.CanMate)
        {
            AgentController potentialMate = sensorSystem.GetNearestEntity<AgentController>(
                filter: agent => {
                    if (agent.gameObject == gameObject) return false; // Skip self

                    ReproductionSystem repro = agent.GetComponent<ReproductionSystem>();
                    EnergySystem energy = agent.GetComponent<EnergySystem>();

                    return repro != null && repro.CanMate &&
                           energy != null && energy.HasEnoughEnergyForMating;
                }
            );

            if (potentialMate != null)
            {
                if (!(currentBehavior is MateSeekingBehavior))
                    SetBehavior(new MateSeekingBehavior());
                return;
            }
        }

        // Default to wandering
        if (!(currentBehavior is WanderingBehavior))
            SetBehavior(new WanderingBehavior());
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