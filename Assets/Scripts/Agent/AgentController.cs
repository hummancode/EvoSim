using UnityEngine;

public class AgentController : MonoBehaviour
{
    [Header("Components")]
    private MovementSystem movementSystem;
    private SensorSystem sensorSystem;
    private EnergySystem energySystem;
    private ConsumptionSystem consumptionSystem;
    private DeathSystem deathSystem;
    private ReproductionSystem reproductionSystem;
    private MatingCoordinator coordinator;
    // Agent Context (for behavior strategies)
    private AgentContext context;

    // Current behavior
    private IBehaviorStrategy currentBehavior;
    [SerializeField] private string currentBehaviorName;
    // Behavior update interval
    [SerializeField] private float behaviorUpdateInterval = 0.2f;
    private float lastBehaviorUpdate;

    // Generation tracking
    public int Generation { get; set; } = 1;

    // Event delegates
    public delegate void AgentEventHandler(GameObject agent);
    public event AgentEventHandler OnAgentDeath;

    void Awake()
    {
        // Set the agent to the Agent layer
        gameObject.layer = LayerMask.NameToLayer("Agent");

        // Initialize components
        InitializeComponents();

        // Create and update context
        context = new AgentContext();
        UpdateContext();

        // Subscribe to events
        SubscribeToEvents();

        // Initialize with wandering behavior
        SetBehavior(new WanderingBehavior());
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        UnsubscribeFromEvents();
    }

    private void InitializeComponents()
    {
        MatingCoordinator coordinator = MatingCoordinator.Instance;
        // Get or add each component
        movementSystem = GetOrAddComponent<MovementSystem>();
        sensorSystem = GetOrAddComponent<SensorSystem>();
        energySystem = GetOrAddComponent<EnergySystem>();
        consumptionSystem = GetOrAddComponent<ConsumptionSystem>();
        deathSystem = GetOrAddComponent<DeathSystem>();
        reproductionSystem = GetOrAddComponent<ReproductionSystem>();

        // Create and initialize context
        context = new AgentContext();

        // Create agent adapter for self-reference
        IAgent selfAgent = new AgentAdapter(this);

        // Create the mate finder adapter - MAKE SURE THIS HAPPENS BEFORE Initialize
        IMateFinder mateFinder = new SensorMateFinder(sensorSystem, gameObject);

        // Update the context with all necessary references
        context.Agent = selfAgent;
        context.Movement = movementSystem;
        context.Sensor = sensorSystem;
        context.MateFinder = mateFinder;  // Set the MateFinder here
        context.Energy = energySystem;
        context.Reproduction = reproductionSystem;

        // Initialize reproduction system with dependencies
        reproductionSystem.Initialize(selfAgent, mateFinder, energySystem);

        Debug.Log("Context initialized - MateFinder: " + (context.MateFinder != null ? "Valid" : "NULL"));
    }
    private void SubscribeToEvents()
    {
        if (reproductionSystem != null)
        {
            // Make sure we're not double-subscribing
            reproductionSystem.OnMatingStarted -= HandleMatingStarted;
            reproductionSystem.OnMatingCompleted -= HandleMatingCompleted;
           

            // Subscribe to events
            reproductionSystem.OnMatingStarted += HandleMatingStarted;
            reproductionSystem.OnMatingCompleted += HandleMatingCompleted;
     ;

            Debug.Log("Successfully subscribed to reproduction events");
        }
        else
        {
            Debug.LogError("reproductionSystem is null in SubscribeToEvents");
        }

        // Other subscriptions...
    }

    private void UnsubscribeFromEvents()
    {
        if (deathSystem != null)
        {
            deathSystem.OnDeath -= HandleDeath;
        }

        if (reproductionSystem != null)
        {
            reproductionSystem.OnMatingStarted -= HandleMatingStarted;
            reproductionSystem.OnMatingCompleted -= HandleMatingCompleted;
           
        }

        if (energySystem != null)
        {
            // Unsubscribe from energy-related events
        }
    }

    private T GetOrAddComponent<T>() where T : Component
    {
        T component = GetComponent<T>();
        if (component == null)
        {
            component = gameObject.AddComponent<T>();
        }
        return component;
    }

    private void UpdateContext()
    {
        if (context == null)
        {
            Debug.LogError("Context is null in UpdateContext - creating new context");
            context = new AgentContext();
        }

        // Create components if they haven't been created yet
        if (context.MateFinder == null && sensorSystem != null)
        {
            Debug.Log("Creating new MateFinder in UpdateContext");
            context.MateFinder = new SensorMateFinder(sensorSystem, gameObject);
        }

        // Create agent adapter for self-reference
        IAgent selfAgent = new AgentAdapter(this);

        // Update the context
        context.Agent = selfAgent;
        context.Movement = movementSystem;
        context.Sensor = sensorSystem;
        context.Energy = energySystem;
        context.Reproduction = reproductionSystem;

        // Verify MateFinder is still valid
        if (context.MateFinder == null)
        {
            Debug.LogWarning("MateFinder is still null after UpdateContext - creating new one");
            context.MateFinder = new SensorMateFinder(sensorSystem, gameObject);
        }
    }

    void Update()
    {
        UpdateBehavior();
    }

    private void UpdateBehavior()
    {
        if (Time.time - lastBehaviorUpdate < behaviorUpdateInterval)
            return;

        // Execute current behavior
        currentBehavior?.Execute(context);

        // Determine if behavior should change
        DetermineBehavior();

        lastBehaviorUpdate = Time.time;
    }

    private void DetermineBehavior()
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

    private void SetBehavior(IBehaviorStrategy behavior)
    {
        // Avoid redundant behavior changes
        if (currentBehavior != null && behavior.GetType() == currentBehavior.GetType())
            return;

        currentBehavior = behavior;

        // Update the display name
        currentBehaviorName = behavior != null ? behavior.GetType().Name : "None";

        Debug.Log($"Agent {gameObject.name} (Gen {Generation}) switched to {currentBehaviorName}");
    }

    // Event handlers
    private void HandleMatingStarted(IAgent partner)
    {
        Debug.Log($"{gameObject.name}: HandleMatingStarted called");

        // Switch to mating behavior
        SetBehavior(new MatingBehavior());
    }

    private void HandleMatingCompleted()
    {
        Debug.Log($"{gameObject.name}: HandleMatingCompleted called");

        // Switch back to wandering
        SetBehavior(new WanderingBehavior());
    }


    private void HandleDeath(string cause)
    {
        // Trigger death event
        OnAgentDeath?.Invoke(gameObject);

        // Log death
        Debug.Log($"Agent {gameObject.name} (Gen {Generation}) died from {cause}");
    }

    // Public interface for external systems
    public AgentContext GetContext()
    {
        return context;
    }

    // Optional methods to force behavior changes
    public void ForceWandering()
    {
        SetBehavior(new WanderingBehavior());
    }

    public void ForceForaging()
    {
        SetBehavior(new ForagingBehavior());
    }

    public void ForceMateSeek()
    {
        SetBehavior(new MateSeekingBehavior());
    }
}