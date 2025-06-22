using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Central manager for all agents - handles batched updates and performance scaling
/// Solves extinction issues at high game speeds by ensuring consistent update frequencies
/// </summary>
public class CentralAgentManager : MonoBehaviour
{
    [Header("Performance Settings")]
    [SerializeField] private float targetFPS = 60f;
    [SerializeField] private float baseAgentsPerFrame = 10f;
    [SerializeField] private float maxAgentsPerFrame = 100f;
    [SerializeField] private bool enableAdaptiveProcessing = true;
    [SerializeField] private bool enablePerformanceLogging = false;

    [Header("Update Settings")]
    [SerializeField] private float baseBehaviorInterval = 1f; // 5 updates per second at 1x speed
    [SerializeField] private float minUpdateInterval = 0.016f; // Max 60 updates per second
    [SerializeField] private float maxUpdateInterval = 1f;     // Min 1 update per second

    // Agent management
    private List<AgentController> allAgents = new List<AgentController>();
    private int currentAgentIndex = 0;

    // Performance tracking
    private float lastFPSCheck = 0f;
    private float currentFPS = 60f;
    private float averageFPS = 60f;
    private Queue<float> fpsHistory = new Queue<float>();

    // Update tracking per agent
    private Dictionary<AgentController, float> lastUpdateTimes = new Dictionary<AgentController, float>();

    // Statistics
    private int agentsProcessedThisFrame = 0;
    private int totalAgentsProcessedThisSecond = 0;
    private float lastStatsTime = 0f;

    // Singleton for easy access
    private static CentralAgentManager instance;
    public static CentralAgentManager Instance => instance;

    #region Initialization

    void Awake()
    {
        // Singleton setup
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Find all existing agents
        RefreshAgentList();
    }

    void Start()
    {
        lastFPSCheck = Time.unscaledTime;
        lastStatsTime = Time.unscaledTime;

        Debug.Log($"CentralAgentManager initialized with {allAgents.Count} agents");
    }

    #endregion

    #region Main Update Loop

    void Update()
    {
        // Update performance metrics
        UpdatePerformanceMetrics();

        // Calculate how many agents to process this frame
        int agentsToProcess = CalculateAgentsToProcess();

        // Reset frame counter
        agentsProcessedThisFrame = 0;

        // Process batch of agents
        ProcessAgentBatch(agentsToProcess);

        // Update statistics
        UpdateStatistics();

        // Log performance if enabled
        if (enablePerformanceLogging && Time.unscaledTime - lastStatsTime >= 1f)
        {
            LogPerformanceStats();
            lastStatsTime = Time.unscaledTime;
        }
    }

    #endregion

    #region Agent Processing

    private int CalculateAgentsToProcess()
    {
        if (!enableAdaptiveProcessing || allAgents.Count == 0)
            return Mathf.Min((int)baseAgentsPerFrame, allAgents.Count);

        // Base calculation on FPS performance
        float fpsRatio = averageFPS / targetFPS;

        // Scale based on time scale - higher time scale needs more updates per frame
        float timeScaleMultiplier = Mathf.Sqrt(Time.timeScale); // Square root for smoother scaling

        // Calculate target agents per frame
        float targetAgents = baseAgentsPerFrame * timeScaleMultiplier;

        // Adjust based on FPS performance
        if (fpsRatio < 0.8f) // FPS is low, reduce load
        {
            targetAgents *= fpsRatio;
        }
        else if (fpsRatio > 1.2f) // FPS is high, can handle more
        {
            targetAgents *= Mathf.Min(fpsRatio, 2f); // Cap the boost
        }

        // Ensure we process all agents regularly
        float minAgentsForCompleteCycle = allAgents.Count / 60f; // Complete cycle in 1 second at 60fps
        targetAgents = Mathf.Max(targetAgents, minAgentsForCompleteCycle);

        // Apply limits
        targetAgents = Mathf.Clamp(targetAgents, 1, maxAgentsPerFrame);
        targetAgents = Mathf.Min(targetAgents, allAgents.Count);

        return Mathf.RoundToInt(targetAgents);
    }

    private void ProcessAgentBatch(int agentsToProcess)
    {
        if (allAgents.Count == 0) return;

        for (int i = 0; i < agentsToProcess; i++)
        {
            // Wrap around if we've processed all agents
            if (currentAgentIndex >= allAgents.Count)
            {
                currentAgentIndex = 0;
                CleanupDeadAgents(); // Clean up any dead agents when we complete a cycle
            }

            if (currentAgentIndex < allAgents.Count)
            {
                AgentController agent = allAgents[currentAgentIndex];

                if (agent != null && ShouldUpdateAgent(agent))
                {
                    UpdateAgent(agent);
                    agentsProcessedThisFrame++;
                    totalAgentsProcessedThisSecond++;
                }

                currentAgentIndex++;
            }
        }
    }

    private bool ShouldUpdateAgent(AgentController agent)
    {
        if (agent == null) return false;

        // Check if enough time has passed since last update
        if (!lastUpdateTimes.TryGetValue(agent, out float lastUpdateTime))
        {
            lastUpdateTime = 0f;
        }

        // Calculate required update interval based on time scale
        float requiredInterval = CalculateRequiredUpdateInterval();

        return Time.time - lastUpdateTime >= requiredInterval;
    }

    private float CalculateRequiredUpdateInterval()
    {
        // At 1x speed: baseBehaviorInterval (e.g., 0.2s)
        // At 100x speed: baseBehaviorInterval / 100 (e.g., 0.002s)
        float interval = baseBehaviorInterval / Time.timeScale;

        // Clamp to reasonable bounds
        return Mathf.Clamp(interval, minUpdateInterval, maxUpdateInterval);
    }

    private void UpdateAgent(AgentController agent)
    {
        try
        {
            // Get the behavior manager and force an update
            var behaviorManager = agent.GetBehaviorManager();
            var context = agent.GetContext();

            if (behaviorManager != null && context != null)
            {
                behaviorManager.UpdateBehavior(context);
                lastUpdateTimes[agent] = Time.time;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error updating agent {agent.name}: {e.Message}");
            // Don't remove the agent here, let the cleanup handle it
        }
    }

    #endregion

    #region Agent Management

    public void RegisterAgent(AgentController agent)
    {
        if (agent != null && !allAgents.Contains(agent))
        {
            allAgents.Add(agent);
            lastUpdateTimes[agent] = Time.time;

            Debug.Log($"Registered agent {agent.name}. Total agents: {allAgents.Count}");
        }
    }

    public void UnregisterAgent(AgentController agent)
    {
        if (agent != null)
        {
            allAgents.Remove(agent);
            lastUpdateTimes.Remove(agent);

            // Adjust current index if needed
            if (currentAgentIndex >= allAgents.Count && allAgents.Count > 0)
            {
                currentAgentIndex = 0;
            }

            Debug.Log($"Unregistered agent {agent.name}. Total agents: {allAgents.Count}");
        }
    }

    [ContextMenu("Refresh Agent List")]
    public void RefreshAgentList()
    {
        allAgents.Clear();
        lastUpdateTimes.Clear();

        AgentController[] foundAgents = FindObjectsOfType<AgentController>();
        foreach (var agent in foundAgents)
        {
            RegisterAgent(agent);
        }

        currentAgentIndex = 0;
        Debug.Log($"Refreshed agent list. Found {allAgents.Count} agents");
    }

    private void CleanupDeadAgents()
    {
        var deadAgents = allAgents.Where(a => a == null).ToList();

        foreach (var deadAgent in deadAgents)
        {
            allAgents.Remove(deadAgent);
            lastUpdateTimes.Remove(deadAgent);
        }

        if (deadAgents.Count > 0)
        {
            Debug.Log($"Cleaned up {deadAgents.Count} dead agents. Remaining: {allAgents.Count}");
        }
    }

    #endregion

    #region Performance Monitoring

    private void UpdatePerformanceMetrics()
    {
        // Calculate current FPS
        currentFPS = 1f / Time.unscaledDeltaTime;

        // Update FPS history for rolling average
        fpsHistory.Enqueue(currentFPS);
        if (fpsHistory.Count > 30) // Keep last 30 frames for average
        {
            fpsHistory.Dequeue();
        }

        // Calculate average FPS
        averageFPS = fpsHistory.Average();
    }

    private void UpdateStatistics()
    {
        // Reset per-second counters
        if (Time.unscaledTime - lastStatsTime >= 1f)
        {
            totalAgentsProcessedThisSecond = 0;
        }
    }

    private void LogPerformanceStats()
    {
        float requiredInterval = CalculateRequiredUpdateInterval();
        float agentsPerSecond = totalAgentsProcessedThisSecond;
        float theoreticalMax = allAgents.Count / requiredInterval;

        Debug.Log($"CENTRAL_AGENT_MANAGER: " +
                 $"Agents={allAgents.Count}, " +
                 $"FPS={averageFPS:F1}, " +
                 $"Speed={Time.timeScale:F0}x, " +
                 $"UpdateInterval={requiredInterval*1000:F1}ms, " +
                 $"Processed/sec={agentsPerSecond:F0}/{theoreticalMax:F0}, " +
                 $"Coverage={agentsPerSecond/theoreticalMax*100:F1}%");
    }

    #endregion

    #region Public Interface

    public int GetAgentCount() => allAgents.Count;
    public float GetAverageFPS() => averageFPS;
    public float GetRequiredUpdateInterval() => CalculateRequiredUpdateInterval();
    public int GetAgentsProcessedThisFrame() => agentsProcessedThisFrame;

    /// <summary>
    /// Force update all agents immediately (use sparingly)
    /// </summary>
    [ContextMenu("Force Update All Agents")]
    public void ForceUpdateAllAgents()
    {
        foreach (var agent in allAgents)
        {
            if (agent != null)
            {
                UpdateAgent(agent);
            }
        }
        Debug.Log($"Force updated {allAgents.Count} agents");
    }

    /// <summary>
    /// Disable individual agent updates (they'll be managed centrally)
    /// </summary>
    public void DisableIndividualAgentUpdates()
    {
        foreach (var agent in allAgents)
        {
            if (agent != null)
            {
                agent.SetCentrallyManaged(true);
            }
        }
    }

    /// <summary>
    /// Re-enable individual agent updates
    /// </summary>
    public void EnableIndividualAgentUpdates()
    {
        foreach (var agent in allAgents)
        {
            if (agent != null)
            {
                agent.SetCentrallyManaged(false);
            }
        }
    }

    #endregion
}