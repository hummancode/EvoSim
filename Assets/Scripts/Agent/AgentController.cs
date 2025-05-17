using UnityEngine;

public class AgentController : MonoBehaviour
{ // Current behavior
    [Header("behavior")]
    [SerializeField] private IBehaviorStrategy currentBehavior;

    // Core components
    public MovementSystem MovementSystem { get; private set; }
    public SensorSystem SensorSystem { get; private set; }
    public EnergySystem EnergySystem { get; private set; }
    public ConsumptionSystem ConsumptionSystem { get; private set; }
    public DeathSystem DeathSystem { get; private set; }
    public ReproductionSystem ReproductionSystem { get; private set; }

   
  
    // Behavior update interval
    private float behaviorUpdateInterval = 0.2f;
    private float lastBehaviorUpdate;

    // Context for strategies
    private AgentContext context = new AgentContext();

    // Generation tracking
    public int Generation { get; set; } = 1;

    // Properties for external access
    public AgentContext GetContext()
    {
        return context;
    }

    void Awake()
    {
        // Set the agent to the Agent layer
        gameObject.layer = LayerMask.NameToLayer("Agent");

        // Initialize components
        InitializeComponents();

        // Initialize context
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
        // Get or add each component
        MovementSystem = GetOrAddComponent<MovementSystem>();
        SensorSystem = GetOrAddComponent<SensorSystem>();
        EnergySystem = GetOrAddComponent<EnergySystem>();
        ConsumptionSystem = GetOrAddComponent<ConsumptionSystem>();
        DeathSystem = GetOrAddComponent<DeathSystem>();
        ReproductionSystem = GetOrAddComponent<ReproductionSystem>();
    }

    private void SubscribeToEvents()
    {
        if (DeathSystem != null)
        {
            DeathSystem.OnDeath += HandleDeath;
        }

        if (ReproductionSystem != null)
        {
            ReproductionSystem.OnMatingStarted += HandleMatingStarted;
            ReproductionSystem.OnMatingCompleted += HandleMatingCompleted;
            ReproductionSystem.OnOffspringRequested += HandleOffspringRequested;
        }

        if (EnergySystem != null)
        {
            // Subscribe to energy-related events if needed
            // Example: EnergySystem.OnHungerStateChanged += HandleHungerStateChanged;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (DeathSystem != null)
        {
            DeathSystem.OnDeath -= HandleDeath;
        }

        if (ReproductionSystem != null)
        {
            ReproductionSystem.OnMatingStarted -= HandleMatingStarted;
            ReproductionSystem.OnMatingCompleted -= HandleMatingCompleted;
            ReproductionSystem.OnOffspringRequested -= HandleOffspringRequested;
        }

        if (EnergySystem != null)
        {
            // Unsubscribe from energy-related events
            // Example: EnergySystem.OnHungerStateChanged -= HandleHungerStateChanged;
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
        // Update the context with all necessary references
        context.Agent = gameObject;
        context.Movement = MovementSystem;
        context.Sensor = SensorSystem;
        context.Energy = EnergySystem;
        context.Reproduction = ReproductionSystem;
        // Add any other context references here
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

        // Check for behavior transitions
        if (currentBehavior != null &&
            currentBehavior.ShouldTransition(context, out IBehaviorStrategy nextBehavior))
        {
            SetBehavior(nextBehavior);
        }

        lastBehaviorUpdate = Time.time;
    }

    private void SetBehavior(IBehaviorStrategy behavior)
    {
        // Avoid redundant behavior changes
        if (currentBehavior != null && behavior.GetType() == currentBehavior.GetType())
            return;

        currentBehavior = behavior;
        Debug.Log($"Agent {gameObject.name} (Gen {Generation}) switched to {behavior.GetType().Name}");
    }

    // Event handlers
    private void HandleMatingStarted(GameObject partner)
    {
        // Switch to stationary mating behavior
        SetBehavior(new MatingBehavior(partner.transform));
    }

    private void HandleMatingCompleted()
    {
        // Return to wandering after mating
        SetBehavior(new WanderingBehavior());
    }

    private void HandleOffspringRequested(Vector3 position)
    {
        // Find the spawner and request offspring creation
        AgentSpawner spawner = FindObjectOfType<AgentSpawner>();
        if (spawner != null)
        {
            // Get the mating partner - use the property you added
            GameObject partner = ReproductionSystem.matingPartner;

            // Spawn the offspring
            spawner.SpawnOffspring(gameObject, partner, position);
        }
        else
        {
            Debug.LogWarning("No AgentSpawner found in scene. Cannot create offspring.");
        }
    }

    private void HandleDeath(string cause)
    {
        // Find spawner or simulation manager to report death
        AgentSpawner spawner = FindObjectOfType<AgentSpawner>();
        if (spawner != null)
        {
            spawner.HandleAgentDeath(gameObject, cause);
        }

        Debug.Log($"Agent {gameObject.name} (Gen {Generation}) died from {cause}");
    }

    // Optional: Add methods for external code to force behavior changes
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