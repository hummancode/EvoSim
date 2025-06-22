using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

/// <summary>
/// High-performance agent manager using multi-threading and data-oriented design
/// Can handle 1000+ agents at high speeds by offloading computation to background threads
/// </summary>
public class MultiThreadedAgentManager : MonoBehaviour
{
    [Header("Threading Settings")]
    [SerializeField] private bool enableMultiThreading = true;
    [SerializeField] private int maxThreads = 4; // Usually CPU cores
    [SerializeField] private int agentsPerBatch = 50;

    [Header("Performance Settings")]
    [SerializeField] private float baseUpdateInterval = 0.2f;
    [SerializeField] private bool enableDataOrientedProcessing = true;
    [SerializeField] private bool enableAggressiveOptimizations = true;

    // Agent data structures (Data-Oriented Design)
    private struct AgentData
    {
        public int agentID;
        public Vector3 position;
        public Vector3 target;
        public float energy;
        public float age;
        public bool isHungry;
        public bool canMate;
        public bool isMating;
        public float lastUpdateTime;
        public AgentController controller; // Keep reference for Unity updates
    }

    // Collections
    private List<AgentData> agentDataList = new List<AgentData>();
    private Dictionary<int, int> agentIDToIndex = new Dictionary<int, int>();
    private ConcurrentQueue<AgentDecision> pendingDecisions = new ConcurrentQueue<AgentDecision>();

    // Threading
    private Task[] workerTasks;
    private volatile bool isProcessing = false;

    // Performance tracking
    private int totalAgentsProcessed = 0;
    private float lastBenchmarkTime = 0f;

    private struct AgentDecision
    {
        public int agentID;
        public DecisionType decision;
        public Vector3 targetPosition;
        public int targetAgentID;
    }

    private enum DecisionType
    {
        Wander,
        SeekFood,
        SeekMate,
        Mate,
        Stay
    }

    private static MultiThreadedAgentManager instance;
    public static MultiThreadedAgentManager Instance => instance;

    #region Initialization

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeThreading();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void InitializeThreading()
    {
        if (enableMultiThreading)
        {
            // Limit threads to available CPU cores
            maxThreads = Mathf.Min(maxThreads, System.Environment.ProcessorCount);
            workerTasks = new Task[maxThreads];
            Debug.Log($"Initialized {maxThreads} worker threads for agent processing");
        }
    }

    #endregion

    #region Main Update Loop

    void Update()
    {
        if (enableDataOrientedProcessing)
        {
            ProcessAgentsDataOriented();
        }
        else
        {
            ProcessAgentsTraditional();
        }

        // Apply decisions from background threads
        ApplyPendingDecisions();

        // Performance monitoring
        UpdatePerformanceMetrics();
    }

    #endregion

    #region Data-Oriented Processing

    private void ProcessAgentsDataOriented()
    {
        if (agentDataList.Count == 0) return;

        float currentTime = Time.time;
        float requiredInterval = baseUpdateInterval / Time.timeScale;
        requiredInterval = Mathf.Max(requiredInterval, 0.016f); // Min 60fps equivalent

        if (enableMultiThreading && !isProcessing)
        {
            StartBackgroundProcessing(currentTime, requiredInterval);
        }
        else
        {
            // Single-threaded fallback
            ProcessAgentBatchSingleThreaded(0, agentDataList.Count, currentTime, requiredInterval);
        }
    }

    private void StartBackgroundProcessing(float currentTime, float requiredInterval)
    {
        isProcessing = true;

        int agentsPerThread = Mathf.CeilToInt((float)agentDataList.Count / maxThreads);

        for (int i = 0; i < maxThreads && i * agentsPerThread < agentDataList.Count; i++)
        {
            int threadIndex = i;
            int startIndex = threadIndex * agentsPerThread;
            int endIndex = Mathf.Min(startIndex + agentsPerThread, agentDataList.Count);

            workerTasks[threadIndex] = Task.Run(() =>
                ProcessAgentBatchMultiThreaded(startIndex, endIndex, currentTime, requiredInterval));
        }

        // Wait for all threads to complete (non-blocking check)
        Task.Run(async () =>
        {
            await Task.WhenAll(workerTasks.Where(t => t != null));
            isProcessing = false;
        });
    }

    private void ProcessAgentBatchSingleThreaded(int startIndex, int endIndex, float currentTime, float requiredInterval)
    {
        for (int i = startIndex; i < endIndex; i++)
        {
            ProcessSingleAgent(i, currentTime, requiredInterval);
        }
    }

    private void ProcessAgentBatchMultiThreaded(int startIndex, int endIndex, float currentTime, float requiredInterval)
    {
        // This runs on background thread - no Unity API calls allowed!
        for (int i = startIndex; i < endIndex; i++)
        {
            if (i >= agentDataList.Count) break;

            var agentData = agentDataList[i];

            // Skip if not ready for update
            if (currentTime - agentData.lastUpdateTime < requiredInterval)
                continue;

            // Make decisions based on agent state (pure computation)
            var decision = MakeAgentDecision(agentData, currentTime);

            // Queue decision for main thread
            pendingDecisions.Enqueue(decision);

            // Update agent data
            agentData.lastUpdateTime = currentTime;
            agentDataList[i] = agentData;

            totalAgentsProcessed++;
        }
    }

    private void ProcessSingleAgent(int index, float currentTime, float requiredInterval)
    {
        var agentData = agentDataList[index];

        if (currentTime - agentData.lastUpdateTime < requiredInterval)
            return;

        // Update agent data from Unity component (main thread only)
        UpdateAgentDataFromComponent(ref agentData);

        // Make decision
        var decision = MakeAgentDecision(agentData, currentTime);

        // Apply immediately since we're on main thread
        ApplyDecisionImmediate(decision);

        agentData.lastUpdateTime = currentTime;
        agentDataList[index] = agentData;
        totalAgentsProcessed++;
    }

    #endregion

    #region Decision Making (Thread-Safe)

    private AgentDecision MakeAgentDecision(AgentData agent, float currentTime)
    {
        var decision = new AgentDecision
        {
            agentID = agent.agentID,
            decision = DecisionType.Wander,
            targetPosition = agent.position,
            targetAgentID = -1
        };

        // Priority-based decision making
        if (agent.isMating)
        {
            decision.decision = DecisionType.Stay;
            return decision;
        }

        if (agent.isHungry)
        {
            // Look for food (simplified - in real implementation, use spatial hashing)
            Vector3? foodPosition = FindNearestFood(agent.position);
            if (foodPosition.HasValue)
            {
                decision.decision = DecisionType.SeekFood;
                decision.targetPosition = foodPosition.Value;
                return decision;
            }
        }

        if (agent.canMate && agent.energy > 150f)
        {
            // Look for mate (simplified)
            int mateID = FindNearestMate(agent);
            if (mateID != -1)
            {
                decision.decision = DecisionType.SeekMate;
                decision.targetAgentID = mateID;
                return decision;
            }
        }

        // Default: wander
        decision.decision = DecisionType.Wander;
        decision.targetPosition = GenerateWanderTarget(agent.position);

        return decision;
    }

    // Simplified food finding (replace with spatial hashing for better performance)
    private Vector3? FindNearestFood(Vector3 position)
    {
        // This is a simplified version - in reality, use spatial partitioning
        var foods = FindObjectsOfType<Food>();
        float closestDistance = float.MaxValue;
        Vector3? closestFood = null;

        foreach (var food in foods)
        {
            float distance = Vector3.Distance(position, food.transform.position);
            if (distance < closestDistance && distance < 5f) // Detection range
            {
                closestDistance = distance;
                closestFood = food.transform.position;
            }
        }

        return closestFood;
    }

    private int FindNearestMate(AgentData agent)
    {
        float closestDistance = float.MaxValue;
        int closestMateID = -1;

        for (int i = 0; i < agentDataList.Count; i++)
        {
            var other = agentDataList[i];
            if (other.agentID == agent.agentID || !other.canMate || other.isMating)
                continue;

            float distance = Vector3.Distance(agent.position, other.position);
            if (distance < closestDistance && distance < 3f) // Mate detection range
            {
                closestDistance = distance;
                closestMateID = other.agentID;
            }
        }

        return closestMateID;
    }

    private Vector3 GenerateWanderTarget(Vector3 currentPosition)
    {
        // Simple random wander target
        Vector3 randomDirection = new Vector3(
            UnityEngine.Random.Range(-1f, 1f),
            UnityEngine.Random.Range(-1f, 1f),
            0f
        ).normalized;

        return currentPosition + randomDirection * UnityEngine.Random.Range(2f, 8f);
    }

    #endregion

    #region Decision Application (Main Thread Only)

    private void ApplyPendingDecisions()
    {
        int decisionsProcessed = 0;
        const int maxDecisionsPerFrame = 100; // Prevent frame spikes

        while (pendingDecisions.TryDequeue(out AgentDecision decision) && decisionsProcessed < maxDecisionsPerFrame)
        {
            ApplyDecisionImmediate(decision);
            decisionsProcessed++;
        }
    }

    private void ApplyDecisionImmediate(AgentDecision decision)
    {
        if (!agentIDToIndex.TryGetValue(decision.agentID, out int index))
            return;

        if (index >= agentDataList.Count)
            return;

        var agentData = agentDataList[index];
        var controller = agentData.controller;

        if (controller == null)
            return;

        try
        {
            var context = controller.GetContext();
            if (context?.Movement == null)
                return;

            switch (decision.decision)
            {
                case DecisionType.Wander:
                    var wanderStrategy = MovementStrategyFactory.CreateRandomMovement();
                    context.Movement.SetMovementStrategy(wanderStrategy);
                    break;

                case DecisionType.SeekFood:
                    var foodStrategy = MovementStrategyFactory.CreateFoodSeeking(context.Sensor);
                    context.Movement.SetMovementStrategy(foodStrategy);
                    break;

                case DecisionType.SeekMate:
                    // Find actual mate and create targeted movement
                    if (agentIDToIndex.TryGetValue(decision.targetAgentID, out int mateIndex) &&
                        mateIndex < agentDataList.Count)
                    {
                        var mateController = agentDataList[mateIndex].controller;
                        if (mateController != null)
                        {
                            var targetedStrategy = new TargetedMovement(mateController.transform);
                            context.Movement.SetMovementStrategy(targetedStrategy);
                        }
                    }
                    break;

                case DecisionType.Stay:
                    var stationaryStrategy = new StationaryMovement();
                    context.Movement.SetMovementStrategy(stationaryStrategy);
                    break;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Error applying decision for agent {decision.agentID}: {e.Message}");
        }
    }

    #endregion

    #region Agent Management

    public void RegisterAgent(AgentController agent)
    {
        if (agent == null) return;

        int agentID = agent.GetInstanceID();

        if (agentIDToIndex.ContainsKey(agentID))
            return; // Already registered

        var agentData = new AgentData
        {
            agentID = agentID,
            position = agent.transform.position,
            target = agent.transform.position,
            energy = 100f,
            age = 0f,
            isHungry = false,
            canMate = false,
            isMating = false,
            lastUpdateTime = 0f,
            controller = agent
        };

        int index = agentDataList.Count;
        agentDataList.Add(agentData);
        agentIDToIndex[agentID] = index;

        Debug.Log($"Registered agent {agentID} at index {index}. Total agents: {agentDataList.Count}");
    }

    public void UnregisterAgent(AgentController agent)
    {
        if (agent == null) return;

        int agentID = agent.GetInstanceID();

        if (!agentIDToIndex.TryGetValue(agentID, out int index))
            return;

        // Remove from collections
        agentDataList.RemoveAt(index);
        agentIDToIndex.Remove(agentID);

        // Update indices for remaining agents
        for (int i = index; i < agentDataList.Count; i++)
        {
            var data = agentDataList[i];
            agentIDToIndex[data.agentID] = i;
        }

        Debug.Log($"Unregistered agent {agentID}. Remaining agents: {agentDataList.Count}");
    }

    private void UpdateAgentDataFromComponent(ref AgentData agentData)
    {
        if (agentData.controller == null)
            return;

        try
        {
            agentData.position = agentData.controller.transform.position;

            var context = agentData.controller.GetContext();
            if (context != null)
            {
                agentData.energy = context.Energy?.cEnergy ?? 100f;
                agentData.isHungry = context.Energy?.IsHungry ?? false;
                agentData.canMate = context.Reproduction?.CanMate ?? false;
                agentData.isMating = context.Reproduction?.IsMating ?? false;

                if (context.Maturity != null)
                    agentData.age = context.Maturity is AgeSystem ageSystem ? ageSystem.Age : 0f;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Error updating agent data: {e.Message}");
        }
    }

    #endregion

    #region Traditional Processing (Fallback)

    private void ProcessAgentsTraditional()
    {
        // Traditional single-threaded processing as fallback
        float currentTime = Time.time;
        float requiredInterval = baseUpdateInterval / Time.timeScale;

        int agentsToProcess = Mathf.Min(agentsPerBatch, agentDataList.Count);

        for (int i = 0; i < agentsToProcess; i++)
        {
            if (i >= agentDataList.Count) break;

            ProcessSingleAgent(i, currentTime, requiredInterval);
        }
    }

    #endregion

    #region Performance Monitoring

    private void UpdatePerformanceMetrics()
    {
        if (Time.time - lastBenchmarkTime >= 1f)
        {
            float agentsPerSecond = totalAgentsProcessed;
            float currentFPS = 1f / Time.unscaledDeltaTime;

            Debug.Log($"MULTITHREADED_AGENT_MANAGER: " +
                     $"Agents={agentDataList.Count}, " +
                     $"FPS={currentFPS:F1}, " +
                     $"Speed={Time.timeScale:F0}x, " +
                     $"Processed/sec={agentsPerSecond:F0}, " +
                     $"Threading={enableMultiThreading}, " +
                     $"QueuedDecisions={pendingDecisions.Count}");

            totalAgentsProcessed = 0;
            lastBenchmarkTime = Time.time;
        }
    }

    #endregion

    #region Public Interface

    public int GetAgentCount() => agentDataList.Count;
    public bool IsMultiThreaded() => enableMultiThreading;
    public int GetPendingDecisions() => pendingDecisions.Count;

    [ContextMenu("Toggle Multi-Threading")]
    public void ToggleMultiThreading()
    {
        enableMultiThreading = !enableMultiThreading;
        Debug.Log($"Multi-threading: {enableMultiThreading}");
    }

    [ContextMenu("Force Single-Threaded Mode")]
    public void ForceSingleThreaded()
    {
        enableMultiThreading = false;
        enableDataOrientedProcessing = false;
        Debug.Log("Forced single-threaded mode");
    }

    #endregion

    void OnDestroy()
    {
        // Clean up threads
        if (workerTasks != null)
        {
            foreach (var task in workerTasks)
            {
                task?.Wait(100); // Wait briefly for cleanup
            }
        }
    }
}