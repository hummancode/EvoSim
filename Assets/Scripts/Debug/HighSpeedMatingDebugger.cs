using UnityEngine;
using System.Linq;
using System.Collections.Generic;

/// <summary>
/// OPTIMIZED: Fast analysis every frame, slow logging periodically
/// Separates performance-critical analysis from expensive logging
/// </summary>
public class HighSpeedMatingDebugger : MonoBehaviour
{
    [Header("Analysis Settings")]
    [SerializeField] private bool enableAnalysis = true;
    [SerializeField] private float analysisInterval = 0.1f; // Fast analysis every 0.1s

    [Header("Logging Settings")]
    [SerializeField] private bool enableLogging = true;
    [SerializeField] private float loggingInterval = 2f; // Slow logging every 2s
    [SerializeField] private bool showDetailedLogs = true;
    [SerializeField] private bool analyzeFailureReasons = true;

    [Header("Performance")]
    [SerializeField] private int maxAgentsToAnalyze = 20; // Limit for performance
    [SerializeField] private bool cacheAgentList = true;

    // Analysis data (updated frequently)
    private MatingAnalysisData currentData = new MatingAnalysisData();
    private float lastAnalysisTime = 0f;
    private float lastLoggingTime = 0f;

    // Cached agents for performance
    private AgentController[] cachedAgents;
    private float lastAgentCacheUpdate = 0f;
    private float agentCacheRefreshInterval = 1f;

    // Statistics tracking
    [Header("Statistics")]
    [SerializeField] private int matingAttempts = 0;
    [SerializeField] private int successfulMatings = 0;
    [SerializeField] private int failedDueToDistance = 0;
    [SerializeField] private int failedDueToEnergy = 0;
    [SerializeField] private int failedDueToMaturity = 0;

    void Update()
    {
        if (!enableAnalysis) return;

        // FAST ANALYSIS: Run frequently for real-time monitoring
        if (Time.time - lastAnalysisTime >= analysisInterval)
        {
            FastMatingAnalysis();
            lastAnalysisTime = Time.time;
        }

        // SLOW LOGGING: Run infrequently for detailed reporting
        if (enableLogging && Time.time - lastLoggingTime >= loggingInterval)
        {
            SlowMatingLogging();
            lastLoggingTime = Time.time;
        }
    }

    /// <summary>
    /// FAST ANALYSIS: Lightweight data collection (runs every 0.1s)
    /// </summary>
    private void FastMatingAnalysis()
    {
        // Refresh agent cache if needed
        if (cacheAgentList && Time.time - lastAgentCacheUpdate > agentCacheRefreshInterval)
        {
            RefreshAgentCache();
        }

        AgentController[] agents = cacheAgentList ? cachedAgents : FindObjectsOfType<AgentController>();
        if (agents == null || agents.Length == 0)
        {
            currentData.Reset();
            return;
        }

        // Reset counters
        currentData.Reset();
        currentData.totalAgents = agents.Length;

        // OPTIMIZED: Limit analysis for performance
        int agentsToCheck = Mathf.Min(agents.Length, maxAgentsToAnalyze);

        // Fast loop - just count basic stats
        for (int i = 0; i < agentsToCheck; i++)
        {
            var agent = agents[i];
            if (agent == null) continue;

            var context = agent.GetContext();
            if (context == null) continue;

            // Quick checks without expensive calls
            bool isMature = context.Maturity?.CanReproduce == true;
            bool hasEnergy = context.Energy?.HasEnoughEnergyForMating == true;
            bool isHungry = context.Energy?.IsHungry == true;
            bool canMate = context.Reproduction?.CanMate == true;
            bool isMating = context.Reproduction?.IsMating == true;

            // Update counters
            if (isMature) currentData.matureAgents++;
            if (hasEnergy) currentData.energeticAgents++;
            if (isHungry) currentData.starvingAgents++;
            if (isMating) currentData.currentlyMating++;

            // Looking for mates = all conditions met but not currently mating
            if (hasEnergy && canMate && isMature && !isMating)
            {
                currentData.lookingForMates++;
            }
        }

        // Scale up counts if we sampled fewer agents
        if (agentsToCheck < agents.Length)
        {
            float scaleFactor = (float)agents.Length / agentsToCheck;
            currentData.ScaleUp(scaleFactor);
        }
    }

    /// <summary>
    /// SLOW LOGGING: Detailed reporting and analysis (runs every 2s)
    /// </summary>
    private void SlowMatingLogging()
    {
        // Log current analysis data
        Debug.Log($"[MATING-DEBUG] Speed: {Time.timeScale:F0}x | " +
                 $"Total: {currentData.totalAgents} | Mature: {currentData.matureAgents} | " +
                 $"Energetic: {currentData.energeticAgents} | Looking: {currentData.lookingForMates} | " +
                 $"Mating: {currentData.currentlyMating} | Starving: {currentData.starvingAgents}");

        // DETAILED ANALYSIS: Only when there's a problem
        if (analyzeFailureReasons && currentData.lookingForMates > 2 && currentData.currentlyMating == 0)
        {
            Debug.LogWarning($"[MATING-PROBLEM] {currentData.lookingForMates} agents looking but NONE mating! Analyzing...");
            SlowFailureAnalysis();
        }

        if (showDetailedLogs)
        {
            LogDetailedMatingStats();
        }
    }

    /// <summary>
    /// SLOW FAILURE ANALYSIS: Expensive deep dive (only when problems detected)
    /// </summary>
    private void SlowFailureAnalysis()
    {
        var agents = FindObjectsOfType<AgentController>(); // Fresh data for analysis

        int pairsAnalyzed = 0;
        int noMatesFoundFailures = 0;
        int canMateWithFailures = 0;
        int distanceFailures = 0;

        // Get eligible agents (expensive operation, done rarely)
        var eligibleAgents = agents.Where(agent => {
            var context = agent.GetContext();
            return context?.Energy?.HasEnoughEnergyForMating == true &&
                   context?.Reproduction?.CanMate == true &&
                   context?.Maturity?.CanReproduce == true &&
                   !context.Reproduction.IsMating;
        }).Take(5).ToArray(); // Limit to 5 agents for performance

        Debug.Log($"[MATING-ANALYSIS] Found {eligibleAgents.Length} eligible agents for detailed analysis");

        // Analyze a few pairs
        foreach (var agent in eligibleAgents)
        {
            if (pairsAnalyzed >= 3) break; // Limit expensive analysis

            var context = agent.GetContext();

            // Find nearest potential mate (expensive)
            var nearestMate = context.MateFinder?.FindNearestPotentialMate();

            if (nearestMate == null)
            {
                noMatesFoundFailures++;
                if (showDetailedLogs)
                    Debug.Log($"[MATING-FAIL] {agent.name}: No mates found by MateFinder");
                continue;
            }

            // Check distance (expensive)
            float distance = context.MateFinder.GetDistanceTo(nearestMate);

            // Check if they can mate (expensive)
            bool canMateWith = context.Reproduction.CanMateWith(nearestMate);

            if (!canMateWith)
            {
                canMateWithFailures++;
                if (showDetailedLogs)
                {
                    Debug.Log($"[MATING-FAIL] {agent.name} -> {GetAgentName(nearestMate)}: " +
                             $"CanMateWith=false (distance: {distance:F2})");

                    // Only do deep analysis occasionally
                    if (pairsAnalyzed == 0) // Only for first pair
                    {
                        AnalyzeCanMateWithFailure(context, nearestMate, distance);
                    }
                }
            }

            pairsAnalyzed++;
        }

        // Summary of failure analysis
        Debug.LogWarning($"[MATING-FAILURE-SUMMARY] Analyzed {pairsAnalyzed} pairs: " +
                        $"NoMatesFound: {noMatesFoundFailures}, " +
                        $"CanMateWith=false: {canMateWithFailures}");
    }

    /// <summary>
    /// PERFORMANCE: Refresh cached agent list
    /// </summary>
    private void RefreshAgentCache()
    {
        try
        {
            cachedAgents = FindObjectsOfType<AgentController>();
            lastAgentCacheUpdate = Time.time;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error refreshing agent cache: {e.Message}");
            cachedAgents = new AgentController[0];
        }
    }

    /// <summary>
    /// EXPENSIVE: Deep analysis of CanMateWith failures (called rarely)
    /// </summary>
    private void AnalyzeCanMateWithFailure(AgentContext context, IAgent partner, float distance)
    {
        var reproduction = context.Reproduction;

        // Check self conditions
        if (!reproduction.CanMate)
        {
            Debug.Log($"  - Self CanMate=false (LastMating: {reproduction.LastMatingTime:F1}, " +
                     $"Current: {Time.time:F1}, IsMating: {reproduction.IsMating})");
        }

        // Check energy
        if (!context.Energy.HasEnoughEnergyForMating)
        {
            Debug.Log($"  - Self energy insufficient: {context.Energy.cEnergy:F1}");
        }

        // Check distance
        float matingProximity = reproduction.MatingProximity;
        Debug.Log($"  - Distance: {distance:F2}, Required: {matingProximity:F2}, " +
                 $"Within range: {distance <= matingProximity}");
    }

    private void LogDetailedMatingStats()
    {
        Debug.Log($"[MATING-STATS] Attempts: {matingAttempts} | " +
                 $"Successful: {successfulMatings} | " +
                 $"Failed - Distance: {failedDueToDistance} | " +
                 $"Energy: {failedDueToEnergy} | " +
                 $"Maturity: {failedDueToMaturity}");

        if (matingAttempts > 0)
        {
            float successRate = (float)successfulMatings / matingAttempts * 100f;
            Debug.Log($"[MATING-STATS] Success Rate: {successRate:F1}%");
        }
    }

    private string GetAgentName(IAgent agent)
    {
        try
        {
            if (agent is AgentAdapter adapter && adapter.IsValid())
                return adapter.GameObject.name;
            return "Unknown";
        }
        catch
        {
            return "Destroyed";
        }
    }

    // ========================================================================
    // PUBLIC API - Same as before
    // ========================================================================

    public static void LogMatingAttempt()
    {
        var debugger = FindObjectOfType<HighSpeedMatingDebugger>();
        if (debugger != null)
        {
            debugger.matingAttempts++;
        }
    }

    public static void LogSuccessfulMating()
    {
        var debugger = FindObjectOfType<HighSpeedMatingDebugger>();
        if (debugger != null)
        {
            debugger.successfulMatings++;
        }
    }

    public static void LogFailedMating(string reason, string details = null)
    {
        var debugger = FindObjectOfType<HighSpeedMatingDebugger>();
        if (debugger != null)
        {
            switch (reason.ToLower())
            {
                case "distance":
                    debugger.failedDueToDistance++;
                    break;
                case "energy":
                    debugger.failedDueToEnergy++;
                    break;
                case "maturity":
                    debugger.failedDueToMaturity++;
                    break;
            }
        }
    }

    [ContextMenu("Reset Statistics")]
    public void ResetStats()
    {
        matingAttempts = 0;
        successfulMatings = 0;
        failedDueToDistance = 0;
        failedDueToEnergy = 0;
        failedDueToMaturity = 0;
        Debug.Log("[MATING-DEBUG] Statistics reset");
    }

    [ContextMenu("Force Analysis")]
    public void ForceAnalysis()
    {
        FastMatingAnalysis();
        SlowMatingLogging();
    }
}

/// <summary>
/// LIGHTWEIGHT: Data structure for fast analysis
/// </summary>
[System.Serializable]
public struct MatingAnalysisData
{
    public int totalAgents;
    public int matureAgents;
    public int energeticAgents;
    public int lookingForMates;
    public int currentlyMating;
    public int starvingAgents;

    public void Reset()
    {
        totalAgents = 0;
        matureAgents = 0;
        energeticAgents = 0;
        lookingForMates = 0;
        currentlyMating = 0;
        starvingAgents = 0;
    }

    public void ScaleUp(float factor)
    {
        matureAgents = Mathf.RoundToInt(matureAgents * factor);
        energeticAgents = Mathf.RoundToInt(energeticAgents * factor);
        lookingForMates = Mathf.RoundToInt(lookingForMates * factor);
        currentlyMating = Mathf.RoundToInt(currentlyMating * factor);
        starvingAgents = Mathf.RoundToInt(starvingAgents * factor);
    }
}