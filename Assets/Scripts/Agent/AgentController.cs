using UnityEngine;

/// <summary>
/// Main controller for agents - now acts as a coordinator between specialized components
/// </summary>
public class AgentController : MonoBehaviour
{
    // External configuration
    [Header("Generation")]
    [SerializeField] private int generation = 1;

    [Header("Behavior")]
    [SerializeField] private float behaviorUpdateInterval = 0.2f;
    [SerializeField] private string currentBehaviorName;

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

    // Properties
    public int Generation
    {
        get => generation;
        set => generation = value;
    }

    void Awake()
    {
        // Set the agent to the Agent layer
        gameObject.layer = LayerMask.NameToLayer("Agent");

        // Create managers
        componentProvider = new AgentComponentProvider(this);
        contextBuilder = new AgentContextBuilder(this, componentProvider);
        behaviorManager = new BehaviorManager(this, behaviorUpdateInterval);
        eventManager = new AgentEventManager(this, componentProvider);

        // Build initial context
        context = contextBuilder.BuildContext();

        // Initialize event subscriptions
        eventManager.SubscribeToEvents();

        // Start with wandering behavior
        behaviorManager.SetInitialBehavior(context);
    }

    void OnDestroy()
    {
        // Clean up subscriptions
        eventManager.UnsubscribeFromEvents();
    }

    void Update()
    {
        // Update behavior through behavior manager
        behaviorManager.UpdateBehavior(context);
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
    public void ForceWandering() => behaviorManager.ForceBehavior<WanderingBehavior>(context);
    public void ForceForaging() => behaviorManager.ForceBehavior<ForagingBehavior>(context);
    public void ForceMateSeek() => behaviorManager.ForceBehavior<MateSeekingBehavior>(context);
}