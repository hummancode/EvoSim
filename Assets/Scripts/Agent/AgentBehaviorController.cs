using UnityEngine;

public class AgentBehaviorController : MonoBehaviour
{
    [SerializeField] private float behaviorUpdateInterval = 0.2f;

    private AgentContext context;
    private IBehaviorStrategy currentBehavior;
    private float lastBehaviorUpdate;

    void Awake()
    {
        // Initialize context
        context = GetComponent<AgentController>().GetContext();

        // Start with wandering
        currentBehavior = new WanderingBehavior();
    }

    void Update()
    {
        if (Time.time - lastBehaviorUpdate < behaviorUpdateInterval)
            return;

        // Execute current behavior
        currentBehavior.Execute(context);

        // Check for state transitions
        DetermineBehavior();

        lastBehaviorUpdate = Time.time;
    }

    private void DetermineBehavior()
    {
        // Priority-based behavior selection

        // If currently mating, stay in that state
        if (context.Reproduction.IsMating)
        {
            if (!(currentBehavior is MatingBehavior))
                SetBehavior(new MatingBehavior());
            return;
        }

        // Check for nearby food first if hungry
        if (context.Energy.IsHungry)
        {
            GameObject food = context.Sensor.GetNearestFood();
            if (food != null && !(currentBehavior is ForagingBehavior))
            {
                SetBehavior(new ForagingBehavior());
                return;
            }
        }

        // Check for mating opportunity
        if (context.Energy.HasEnoughEnergyForMating && context.Reproduction.CanMate)
        {
            IAgent potentialMate = context.MateFinder.FindNearestPotentialMate();
            if (potentialMate != null && !(currentBehavior is MateSeekingBehavior))
            {
                SetBehavior(new MateSeekingBehavior());
                return;
            }
        }

        // Default to wandering
        if (!(currentBehavior is WanderingBehavior))
            SetBehavior(new WanderingBehavior());
    }

    private void SetBehavior(IBehaviorStrategy behavior)
    {
        if (currentBehavior?.GetType() == behavior.GetType())
            return;

        currentBehavior = behavior;
        Debug.Log($"Agent {gameObject.name} switched to {behavior.GetType().Name}");
    }
}