using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Monitors time scale changes and logs critical transitions
/// </summary>
public class SpeedChangeMonitor : MonoBehaviour
{
    [Header("Monitoring Settings")]
    [SerializeField] private float monitoringInterval = 0.1f;
    [SerializeField] private float significantSpeedChange = 5f; // Log if speed changes by more than this

    private float lastTimeScale = 1f;
    private float lastMonitorTime;
    private List<SpeedTransition> speedHistory = new List<SpeedTransition>();

    private struct SpeedTransition
    {
        public float fromSpeed;
        public float toSpeed;
        public float gameTime;
        public float realTime;
        public SimulationSnapshot beforeSnapshot;
        public SimulationSnapshot afterSnapshot;
    }

    private struct SimulationSnapshot
    {
        public int agentCount;
        public float avgEnergy;
        public int foodCount;
        public float fps;

        public SimulationSnapshot(bool takeNow = true)
        {
            if (takeNow)
            {
                agentCount = FindObjectsOfType<AgentController>().Length;

                var energySystems = FindObjectsOfType<EnergySystem>();
                float totalEnergy = 0f;
                foreach (var energy in energySystems)
                {
                    totalEnergy += energy.EnergyPercent;
                }
                avgEnergy = energySystems.Length > 0 ? totalEnergy / energySystems.Length : 0f;

                foodCount = FindObjectsOfType<Food>().Length;
                fps = 1f / Time.unscaledDeltaTime;
            }
            else
            {
                agentCount = 0;
                avgEnergy = 0f;
                foodCount = 0;
                fps = 0f;
            }
        }
    }

    void Start()
    {
        lastTimeScale = Time.timeScale;

        // Log initial state
        SimulationLogger.LogInfo(SimulationLogger.LogCategory.Analysis,
            $"Speed monitoring started at {Time.timeScale:F1}x");

        InvokeRepeating(nameof(MonitorSpeedChanges), monitoringInterval, monitoringInterval);
    }

    void MonitorSpeedChanges()
    {
        float currentTimeScale = Time.timeScale;

        // Check for significant speed changes
        if (Mathf.Abs(currentTimeScale - lastTimeScale) >= significantSpeedChange)
        {
            LogSpeedTransition(lastTimeScale, currentTimeScale);
        }

        // Check for critical speed thresholds
        CheckCriticalThresholds(currentTimeScale);

        lastTimeScale = currentTimeScale;
    }

    private void LogSpeedTransition(float fromSpeed, float toSpeed)
    {
        var beforeSnapshot = new SimulationSnapshot(true);

        var transition = new SpeedTransition
        {
            fromSpeed = fromSpeed,
            toSpeed = toSpeed,
            gameTime = Time.time,
            realTime = Time.unscaledTime,
            beforeSnapshot = beforeSnapshot
        };

        speedHistory.Add(transition);

        // Log the transition
        var transitionData = new Dictionary<string, object>
        {
            ["fromSpeed"] = fromSpeed,
            ["toSpeed"] = toSpeed,
            ["agentsBefore"] = beforeSnapshot.agentCount,
            ["energyBefore"] = beforeSnapshot.avgEnergy,
            ["foodBefore"] = beforeSnapshot.foodCount,
            ["fpsBefore"] = beforeSnapshot.fps
        };

        SimulationLogger.LogInfo(SimulationLogger.LogCategory.Analysis,
            $"SPEED TRANSITION: {fromSpeed:F1}x ? {toSpeed:F1}x",
            transitionData);

        // Schedule follow-up analysis
        Invoke(nameof(AnalyzeTransitionEffects), 2f); // Analyze effects after 2 seconds
    }

    private void AnalyzeTransitionEffects()
    {
        if (speedHistory.Count == 0) return;

        var lastTransition = speedHistory[speedHistory.Count - 1];
        var afterSnapshot = new SimulationSnapshot(true);

        // Calculate changes
        int agentChange = afterSnapshot.agentCount - lastTransition.beforeSnapshot.agentCount;
        float energyChange = afterSnapshot.avgEnergy - lastTransition.beforeSnapshot.avgEnergy;
        int foodChange = afterSnapshot.foodCount - lastTransition.beforeSnapshot.foodCount;
        float fpsChange = afterSnapshot.fps - lastTransition.beforeSnapshot.fps;

        var effectsData = new Dictionary<string, object>
        {
            ["speedChange"] = $"{lastTransition.fromSpeed:F1}x?{lastTransition.toSpeed:F1}x",
            ["agentChange"] = agentChange,
            ["energyChange"] = energyChange,
            ["foodChange"] = foodChange,
            ["fpsChange"] = fpsChange,
            ["agentsAfter"] = afterSnapshot.agentCount,
            ["energyAfter"] = afterSnapshot.avgEnergy,
            ["foodAfter"] = afterSnapshot.foodCount,
            ["fpsAfter"] = afterSnapshot.fps
        };

        // Determine severity
        bool hasNegativeEffects = agentChange < -2 || energyChange < -0.1f || fpsChange < -10f;

        if (hasNegativeEffects)
        {
            SimulationLogger.LogWarning(SimulationLogger.LogCategory.Critical,
                $"NEGATIVE EFFECTS after speed change {lastTransition.fromSpeed:F1}x?{lastTransition.toSpeed:F1}x: " +
                $"Agents: {agentChange:+0;-0}, Energy: {energyChange:+0.0;-0.0}, FPS: {fpsChange:+0;-0}",
                effectsData);
        }
        else
        {
            SimulationLogger.LogInfo(SimulationLogger.LogCategory.Analysis,
                $"Speed transition effects: Agents: {agentChange:+0;-0}, Energy: {energyChange:+0.0;-0.0}, FPS: {fpsChange:+0;-0}",
                effectsData);
        }

        // Update the transition record
        lastTransition.afterSnapshot = afterSnapshot;
        speedHistory[speedHistory.Count - 1] = lastTransition;
    }

    private void CheckCriticalThresholds(float currentSpeed)
    {
        // Define critical speed thresholds
        float[] criticalSpeeds = { 10f, 25f, 50f, 75f, 100f };

        foreach (float threshold in criticalSpeeds)
        {
            // Check if we just crossed this threshold (upward)
            if (lastTimeScale < threshold && currentSpeed >= threshold)
            {
                LogCriticalThresholdCrossed(threshold, true);
            }
            // Check if we just crossed this threshold (downward)
            else if (lastTimeScale > threshold && currentSpeed <= threshold)
            {
                LogCriticalThresholdCrossed(threshold, false);
            }
        }
    }

    private void LogCriticalThresholdCrossed(float threshold, bool isIncreasing)
    {
        var snapshot = new SimulationSnapshot(true);

        var thresholdData = new Dictionary<string, object>
        {
            ["threshold"] = threshold,
            ["direction"] = isIncreasing ? "increasing" : "decreasing",
            ["currentSpeed"] = Time.timeScale,
            ["agents"] = snapshot.agentCount,
            ["avgEnergy"] = snapshot.avgEnergy,
            ["food"] = snapshot.foodCount,
            ["fps"] = snapshot.fps
        };

        string direction = isIncreasing ? "ENTERED" : "EXITED";

        SimulationLogger.LogInfo(SimulationLogger.LogCategory.Critical,
            $"CRITICAL THRESHOLD {direction}: {threshold}x speed zone. " +
            $"State: {snapshot.agentCount} agents, {snapshot.avgEnergy:P1} energy, {snapshot.fps:F1} FPS",
            thresholdData);

        // Special warnings for high speeds
        if (isIncreasing && threshold >= 50f)
        {
            SimulationLogger.LogWarning(SimulationLogger.LogCategory.Critical,
                $"HIGH SPEED WARNING: Entering {threshold}x zone - monitoring for simulation instabilities");
        }
    }

    /// <summary>
    /// Generate speed analysis report
    /// </summary>
    [ContextMenu("Generate Speed Analysis Report")]
    public void GenerateSpeedAnalysisReport()
    {
        string report = "=== SPEED CHANGE ANALYSIS ===\n\n";

        if (speedHistory.Count == 0)
        {
            report += "No significant speed changes recorded.\n";
        }
        else
        {
            report += $"Recorded {speedHistory.Count} significant speed transitions:\n\n";

            foreach (var transition in speedHistory)
            {
                int agentChange = transition.afterSnapshot.agentCount - transition.beforeSnapshot.agentCount;
                float energyChange = transition.afterSnapshot.avgEnergy - transition.beforeSnapshot.avgEnergy;

                report += $"• {transition.fromSpeed:F1}x ? {transition.toSpeed:F1}x at t={transition.gameTime:F1}s\n";
                report += $"  Effects: Agents {agentChange:+0;-0}, Energy {energyChange:+0.0%;-0.0%}\n";

                if (agentChange < -2 || energyChange < -0.1f)
                {
                    report += $"  ?? Negative impact detected\n";
                }
                report += "\n";
            }
        }

        // Find problematic speeds
        var problemSpeeds = speedHistory.Where(t =>
            (t.afterSnapshot.agentCount - t.beforeSnapshot.agentCount) < -2 ||
            (t.afterSnapshot.avgEnergy - t.beforeSnapshot.avgEnergy) < -0.1f
        ).ToList();

        if (problemSpeeds.Any())
        {
            report += "PROBLEMATIC SPEED RANGES:\n";
            foreach (var prob in problemSpeeds)
            {
                report += $"• Around {prob.toSpeed:F1}x speed\n";
            }
        }
        else
        {
            report += "? No problematic speed ranges detected\n";
        }

        Debug.Log(report);

        // Also log to simulation logger
        SimulationLogger.LogInfo(SimulationLogger.LogCategory.Analysis,
            $"Speed analysis complete: {speedHistory.Count} transitions recorded");
    }
}