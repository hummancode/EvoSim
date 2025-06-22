using UnityEngine;

public class AgentController : MonoBehaviour
{
    [Header("Generation")]
    [SerializeField] private int generation = 1;

    [Header("Behavior")]
    [SerializeField] private float behaviorUpdateInterval = 1f;
    [SerializeField] private string currentBehaviorName;

    // Components
    private MovementSystem movementSystem;
    private SensorSystem sensorSystem;
    private EnergySystem energySystem;
    private ConsumptionSystem consumptionSystem;
    private DeathSystem deathSystem;
    private ReproductionSystem reproductionSystem;

    // Managers - these will be injected
    private IAgentComponentProvider componentProvider;
    private IAgentEventManager eventManager;
    private IBehaviorManager behaviorManager;
    private IAgentContextBuilder contextBuilder;

    // Components cache - fast access to common components
    private AgentContext context;

    // Event delegates
    public delegate void AgentEventHandler(GameObject agent);
    public event AgentEventHandler OnAgentDeath;
    private AgentIdentity identity;
    private GeneticsSystem geneticsSystem;
    private AgeSystem ageSystem;
    private AgeLifeStageTracker ageLifeStageTracker;
    [Header("Central Management")]
    [SerializeField] private bool isCentrallyManaged = false;
    [SerializeField] private bool enableCentralManagement = true; // Toggle for testing
    // Properties
    public int Generation
    {
        get => identity != null ? identity.Generation : 1;
        set
        {
            if (identity != null)
                identity.SetGeneration(value);
        }
    }

    public int AgentId => identity != null ? identity.AgentId : 0;

    void Awake()
    {   
        // Set the agent to the Agent layer
        gameObject.layer = LayerMask.NameToLayer("Agent");
        VisualSystemIntegration.AddAgentVisualSystem(gameObject);


        //SimpleFluffGeneHelper.SetupSimpleFluff(gameObject);
        // Get identity component
        identity = GetComponent<AgentIdentity>();
        if (identity == null)
        {
            identity = gameObject.AddComponent<AgentIdentity>();
        }

        // Get genetics system
        geneticsSystem = GetComponent<GeneticsSystem>();
        if (geneticsSystem == null)
        {
            geneticsSystem = gameObject.AddComponent<GeneticsSystem>();
        }

        // Get age system
        ageSystem = GetComponent<AgeSystem>();
        if (ageSystem == null)
        {
            ageSystem = gameObject.AddComponent<AgeSystem>();
            ageLifeStageTracker = gameObject.AddComponent<AgeLifeStageTracker>();
        }

        // Create managers (existing code)
        componentProvider = new AgentComponentProvider(this);
        contextBuilder = new AgentContextBuilder(this, componentProvider);
        behaviorManager = new SmartBehaviorManager(this, behaviorUpdateInterval);
        eventManager = new AgentEventManager(this, componentProvider);

        movementSystem = componentProvider.GetMovementSystem();
        sensorSystem = componentProvider.GetSensorSystem();
        energySystem = componentProvider.GetEnergySystem();
        consumptionSystem = componentProvider.GetConsumptionSystem();
        deathSystem = componentProvider.GetDeathSystem();
        reproductionSystem = componentProvider.GetReproductionSystem();
        ageSystem.Initialize(deathSystem, geneticsSystem);
        // Get additional systems
    
        // Initialize age system with dependencies
        if (ageSystem != null)
        {
            ageSystem.Initialize(deathSystem, geneticsSystem);
        }

        // Build initial context
        context = contextBuilder.BuildContext();

        // Initialize event subscriptions
        eventManager.SubscribeToEvents();

        // Start with wandering behavior
        behaviorManager.SetInitialBehavior(context);
        //SimpleAgeIntegration.AddAgeSpriteSystem(gameObject);
        if (enableCentralManagement && CentralAgentManager.Instance != null)
        {
            CentralAgentManager.Instance.RegisterAgent(this);
            isCentrallyManaged = true;
        }
    }

    // Existing methods...

    // Add method to access genetics system
    public GeneticsSystem GetGeneticsSystem()
    {
        return geneticsSystem;
    }

    // Add method to access age system
    public AgeSystem GetAgeSystem()
    {
        return ageSystem;
    }

    void OnDestroy()
    {
        // Clean up subscriptions
        eventManager.UnsubscribeFromEvents();
        if (CentralAgentManager.Instance != null)
        {
            CentralAgentManager.Instance.UnregisterAgent(this);
        }
    }

    void Update()
    {
        // Update behavior through behavior manager
        if (!isCentrallyManaged)
        {
            // Your existing update logic
            behaviorManager.UpdateBehavior(context);
        }

    }

    // Public interface
    public AgentContext GetContext()
    {
        return context;
    }

    // For display in Inspector
    public void UpdateBehaviorName(string name)
    {
        currentBehaviorName = name;
    }

    // Event relay - forward event to subscribers
    public void OnDeath(string cause)
    {
        Debug.Log($"Agent {gameObject.name} (Gen {Generation}) died from {cause}");
        OnAgentDeath?.Invoke(gameObject);
    }

    // Optional behavior forcing methods
    public void ForceWandering()
    {
        behaviorManager.ForceBehavior<WanderingBehavior>(context);
    }

    public void ForceForaging()
    {
        behaviorManager.ForceBehavior<ForagingBehavior>(context);
    }

    public void ForceMateSeek()
    {
        behaviorManager.ForceBehavior<MateSeekingBehavior>(context);
    }
    public IBehaviorManager GetBehaviorManager()
    {
        return behaviorManager;
    }

    public void SetCentrallyManaged(bool managed)
    {
        isCentrallyManaged = managed;
    }

    public bool IsCentrallyManaged()
    {
        return isCentrallyManaged;
    }

    // Force a behavior update (called by central manager)
    public void ForceUpdate()
    {
        behaviorManager?.UpdateBehavior(context);
    }
}