using UnityEngine;

/// <summary>
/// Master performance controller that coordinates all optimization systems
/// Add this to your main GameObject to enable all performance features
/// </summary>
public class UltraPerformanceController : MonoBehaviour
{
    [Header("System Toggles")]
    [SerializeField] private bool enableMultiThreading = true;
    [SerializeField] private bool enableSpatialOptimization = true;
    [SerializeField] private bool enableExtremeMode = false;
    [SerializeField] private bool enableCentralManagement = true;

    [Header("Performance Thresholds")]
    [SerializeField] private float highSpeedThreshold = 10f;
    [SerializeField] private float extremeSpeedThreshold = 50f;
    [SerializeField] private float targetFPS = 60f;

    // System references
    private MultiThreadedAgentManager multiThreadedManager;
    private SpatialHashGrid spatialGrid;
    private ExtremePerformanceOptimizer extremeOptimizer;
    private CentralAgentManager centralManager;

    // Performance monitoring
    private float lastPerformanceCheck = 0f;
    private float performanceCheckInterval = 1f;

    void Awake()
    {
        Debug.Log("Initializing Ultra Performance System...");
        InitializePerformanceSystems();
    }

    void Start()
    {
        ConfigureInitialSettings();
        StartPerformanceMonitoring();
    }

    void Update()
    {
        MonitorAndAdjustPerformance();
    }

    #region System Initialization

    private void InitializePerformanceSystems()
    {
        // 1. Central Agent Management
        if (enableCentralManagement)
        {
            centralManager = GetOrAddComponent<CentralAgentManager>();
            Debug.Log("✓ Central Agent Manager initialized");
        }

        // 2. Multi-threading
        if (enableMultiThreading)
        {
            multiThreadedManager = GetOrAddComponent<MultiThreadedAgentManager>();
            Debug.Log("✓ Multi-threaded Agent Manager initialized");
        }

        // 3. Spatial optimization
        if (enableSpatialOptimization)
        {
            spatialGrid = GetOrAddComponent<SpatialHashGrid>();
            Debug.Log("✓ Spatial Hash Grid initialized");
        }

        // 4. Extreme optimizations
        if (enableExtremeMode)
        {
            extremeOptimizer = GetOrAddComponent<ExtremePerformanceOptimizer>();
            Debug.Log("✓ Extreme Performance Optimizer initialized");
        }

        // 5. Standard performance optimizer
        var standardOptimizer = GetOrAddComponent<SimulationPerformanceOptimizer>();
        Debug.Log("✓ Simulation Performance Optimizer initialized");
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

    #endregion

    #region Performance Configuration

    private void ConfigureInitialSettings()
    {
        // Configure systems based on current game state
        float currentSpeed = Time.timeScale;

        if (currentSpeed >= extremeSpeedThreshold && extremeOptimizer != null)
        {
            extremeOptimizer.ForceEnableExtremeMode();
        }
        else if (currentSpeed >= highSpeedThreshold)
        {
            EnableHighSpeedOptimizations();
        }

        // Register all existing agents with spatial grid
        if (spatialGrid != null)
        {
            spatialGrid.AutoRegisterAllEntities();
        }
    }

    private void EnableHighSpeedOptimizations()
    {
        Debug.Log("Enabling high-speed optimizations");

        // Disable individual agent updates in favor of central management
        if (centralManager != null)
        {
            centralManager.DisableIndividualAgentUpdates();
        }
    }

    #endregion

    #region Performance Monitoring

    private void StartPerformanceMonitoring()
    {
        lastPerformanceCheck = Time.time;
        InvokeRepeating(nameof(LogPerformanceStats), 5f, 5f);
    }

    private void MonitorAndAdjustPerformance()
    {
        if (Time.time - lastPerformanceCheck < performanceCheckInterval)
            return;

        float currentFPS = 1f / Time.unscaledDeltaTime;
        float currentSpeed = Time.timeScale;

        // Adaptive performance adjustment
        if (currentFPS < targetFPS * 0.7f) // FPS dropped below 70% of target
        {
            Debug.LogWarning($"Low FPS detected ({currentFPS:F1}), enabling additional optimizations");
            EnableAdditionalOptimizations();
        }
        else if (currentFPS > targetFPS * 1.3f && currentSpeed < highSpeedThreshold)
        {
            // FPS is high and speed is normal, can reduce optimizations
            DisableUnnecessaryOptimizations();
        }

        // Speed-based optimization switching
        if (currentSpeed >= extremeSpeedThreshold && extremeOptimizer != null && !extremeOptimizer.IsExtremeModeActive())
        {
            extremeOptimizer.ForceEnableExtremeMode();
        }
        else if (currentSpeed < extremeSpeedThreshold && extremeOptimizer != null && extremeOptimizer.IsExtremeModeActive())
        {
            extremeOptimizer.ForceDisableExtremeMode();
        }

        lastPerformanceCheck = Time.time;
    }

    private void EnableAdditionalOptimizations()
    {
        // Enable more aggressive optimizations when FPS is low
        if (multiThreadedManager != null)
        {
            // Could add more aggressive threading here
        }

        if (spatialGrid != null)
        {
            // Reduce spatial grid update frequency
        }

        // Reduce physics quality
        Time.fixedDeltaTime = Mathf.Min(Time.fixedDeltaTime * 1.5f, 0.05f);
    }

    private void DisableUnnecessaryOptimizations()
    {
        // Restore normal quality when performance allows
        Time.fixedDeltaTime = Mathf.Max(Time.fixedDeltaTime * 0.8f, 0.01f);
    }

    #endregion

    #region Performance Logging

    private void LogPerformanceStats()
    {
        float currentFPS = 1f / Time.unscaledDeltaTime;
        int agentCount = GetAgentCount();

        string performanceReport = $"=== ULTRA PERFORMANCE REPORT ===\n" +
                                 $"FPS: {currentFPS:F1} (Target: {targetFPS})\n" +
                                 $"Speed: {Time.timeScale:F0}x\n" +
                                 $"Agents: {agentCount}\n" +
                                 $"Systems Active:\n" +
                                 $"  • Central Management: {centralManager != null}\n" +
                                 $"  • Multi-Threading: {multiThreadedManager != null}\n" +
                                 $"  • Spatial Grid: {spatialGrid != null}\n" +
                                 $"  • Extreme Mode: {extremeOptimizer?.IsExtremeModeActive() ?? false}\n";

        // Add system-specific stats
        if (spatialGrid != null)
        {
            performanceReport += $"  • Spatial Stats: {spatialGrid.GetPerformanceStats()}\n";
        }

        if (centralManager != null)
        {
            performanceReport += $"  • Central Manager: {centralManager.GetAgentCount()} agents, {centralManager.GetAverageFPS():F1} FPS\n";
        }

        Debug.Log(performanceReport);
    }

    private int GetAgentCount()
    {
        if (centralManager != null)
            return centralManager.GetAgentCount();

        if (multiThreadedManager != null)
            return multiThreadedManager.GetAgentCount();

        return FindObjectsOfType<AgentController>().Length;
    }

    #endregion

    #region Public Interface

    [ContextMenu("Enable All Optimizations")]
    public void EnableAllOptimizations()
    {
        enableMultiThreading = true;
        enableSpatialOptimization = true;
        enableExtremeMode = true;
        enableCentralManagement = true;

        InitializePerformanceSystems();
        ConfigureInitialSettings();

        Debug.Log("All performance optimizations enabled!");
    }

    [ContextMenu("Disable All Optimizations")]
    public void DisableAllOptimizations()
    {
        enableMultiThreading = false;
        enableSpatialOptimization = false;
        enableExtremeMode = false;

        if (extremeOptimizer != null)
            extremeOptimizer.ForceDisableExtremeMode();

        if (centralManager != null)
            centralManager.EnableIndividualAgentUpdates();

        Debug.Log("All optimizations disabled!");
    }

    [ContextMenu("Benchmark Performance")]
    public void BenchmarkPerformance()
    {
        Debug.Log("Starting performance benchmark...");

        // Test different speeds and measure performance
        StartCoroutine(PerformanceBenchmarkCoroutine());
    }

    private System.Collections.IEnumerator PerformanceBenchmarkCoroutine()
    {
        float[] testSpeeds = { 1f, 10f, 25f, 50f, 100f, 200f };
        float originalSpeed = Time.timeScale;

        foreach (float speed in testSpeeds)
        {
            Time.timeScale = speed;
            yield return new WaitForSecondsRealtime(2f); // Wait 2 real seconds

            float avgFPS = 0f;
            int samples = 0;

            // Sample FPS for 1 second
            float sampleStart = Time.unscaledTime;
            while (Time.unscaledTime - sampleStart < 1f)
            {
                avgFPS += 1f / Time.unscaledDeltaTime;
                samples++;
                yield return null;
            }

            avgFPS /= samples;

            Debug.Log($"BENCHMARK: {speed:F0}x speed = {avgFPS:F1} FPS with {GetAgentCount()} agents");
        }

        Time.timeScale = originalSpeed;
        Debug.Log("Performance benchmark completed!");
    }

    [ContextMenu("Force Extreme Mode")]
    public void ForceExtremeMode()
    {
        if (extremeOptimizer != null)
        {
            extremeOptimizer.ForceEnableExtremeMode();
        }
        else
        {
            extremeOptimizer = GetOrAddComponent<ExtremePerformanceOptimizer>();
            extremeOptimizer.ForceEnableExtremeMode();
        }
    }

    [ContextMenu("Emergency Performance Recovery")]
    public void EmergencyPerformanceRecovery()
    {
        Debug.LogWarning("EMERGENCY PERFORMANCE RECOVERY ACTIVATED!");

        // Immediately enable all optimizations
        EnableAllOptimizations();

        // Force extreme mode regardless of speed
        ForceExtremeMode();

        // Reduce simulation complexity
        Time.timeScale = Mathf.Min(Time.timeScale, 10f);

        // Force garbage collection
        System.GC.Collect();

        Debug.LogWarning("Emergency optimizations applied. Visual quality severely reduced.");
    }

    #endregion

    #region Integration Helpers

    /// <summary>
    /// Call this when spawning new agents to register them with all systems
    /// </summary>
    public void RegisterNewAgent(AgentController agent)
    {
        if (centralManager != null)
        {
            centralManager.RegisterAgent(agent);
        }

        if (multiThreadedManager != null)
        {
            multiThreadedManager.RegisterAgent(agent);
        }

        if (spatialGrid != null)
        {
            spatialGrid.RegisterEntity(agent.GetInstanceID(), agent.transform.position, SpatialHashGrid.EntityType.Agent);
        }
    }

    /// <summary>
    /// Call this when agents die to unregister them from all systems
    /// </summary>
    public void UnregisterAgent(AgentController agent)
    {
        if (centralManager != null)
        {
            centralManager.UnregisterAgent(agent);
        }

        if (multiThreadedManager != null)
        {
            multiThreadedManager.UnregisterAgent(agent);
        }

        if (spatialGrid != null)
        {
            spatialGrid.UnregisterEntity(agent.GetInstanceID());
        }
    }

    /// <summary>
    /// Get optimized food position for an agent (uses spatial grid if available)
    /// </summary>
    public Vector3? FindNearestFoodOptimized(Vector3 agentPosition, float searchRadius = 5f)
    {
        if (spatialGrid != null)
        {
            return spatialGrid.FindNearestFood(agentPosition, searchRadius);
        }

        // Fallback to traditional method
        var foods = FindObjectsOfType<Food>();
        Food closest = null;
        float closestDistance = float.MaxValue;

        foreach (var food in foods)
        {
            float distance = Vector3.Distance(agentPosition, food.transform.position);
            if (distance < closestDistance && distance <= searchRadius)
            {
                closestDistance = distance;
                closest = food;
            }
        }

        return closest?.transform.position;
    }

    /// <summary>
    /// Get optimized mate finding (uses spatial grid if available)
    /// </summary>
    public AgentController FindNearestMateOptimized(Vector3 agentPosition, float searchRadius = 3f)
    {
        if (spatialGrid != null)
        {
            var mateIDs = spatialGrid.FindPotentialMates(agentPosition, searchRadius);

            foreach (int id in mateIDs)
            {
                var mate = FindAgentByInstanceID(id);
                if (mate != null && CanMateWith(mate))
                {
                    return mate;
                }
            }

            return null;
        }

        // Fallback to traditional method
        var agents = FindObjectsOfType<AgentController>();
        AgentController closest = null;
        float closestDistance = float.MaxValue;

        foreach (var agent in agents)
        {
            float distance = Vector3.Distance(agentPosition, agent.transform.position);
            if (distance < closestDistance && distance <= searchRadius && CanMateWith(agent))
            {
                closestDistance = distance;
                closest = agent;
            }
        }

        return closest;
    }

    private AgentController FindAgentByInstanceID(int instanceID)
    {
        var agents = FindObjectsOfType<AgentController>();
        foreach (var agent in agents)
        {
            if (agent.GetInstanceID() == instanceID)
                return agent;
        }
        return null;
    }

    private bool CanMateWith(AgentController agent)
    {
        var context = agent.GetContext();
        return context?.Reproduction?.CanMate == true &&
               context?.Energy?.HasEnoughEnergyForMating == true;
    }

    #endregion

    #region Performance Metrics

    public float GetCurrentFPS() => 1f / Time.unscaledDeltaTime;
    public float GetAverageFPS() => centralManager?.GetAverageFPS() ?? GetCurrentFPS();
    public bool IsHighSpeedMode() => Time.timeScale >= highSpeedThreshold;
    public bool IsExtremeSpeedMode() => Time.timeScale >= extremeSpeedThreshold;
    public bool IsExtremeModeActive() => extremeOptimizer?.IsExtremeModeActive() ?? false;

    public string GetDetailedPerformanceReport()
    {
        return $"Ultra Performance Controller Status:\n" +
               $"• FPS: {GetCurrentFPS():F1} (Avg: {GetAverageFPS():F1})\n" +
               $"• Speed: {Time.timeScale:F0}x\n" +
               $"• Agents: {GetAgentCount()}\n" +
               $"• High Speed Mode: {IsHighSpeedMode()}\n" +
               $"• Extreme Mode: {IsExtremeModeActive()}\n" +
               $"• Central Management: {centralManager != null}\n" +
               $"• Multi-Threading: {multiThreadedManager != null}\n" +
               $"• Spatial Grid: {spatialGrid != null} ({spatialGrid?.GetEntityCount() ?? 0} entities)\n" +
               $"• Physics Step: {Time.fixedDeltaTime * 1000:F1}ms\n" +
               $"• Memory: {System.GC.GetTotalMemory(false) / 1024 / 1024:F1} MB";
    }

    #endregion
}