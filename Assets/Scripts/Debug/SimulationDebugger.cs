using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Debug component to track what's happening in the simulation at high speeds
/// </summary>
public class SimulationDebugger : MonoBehaviour
{
    [Header("Debug Settings")]
    [SerializeField] private bool enableDebugging = true;
    [SerializeField] private float debugUpdateInterval = 2f; // Check every 2 seconds
    [SerializeField] private bool logToConsole = true;
    [SerializeField] private bool showDebugGUI = true;

    [Header("Tracking")]
    [SerializeField] private bool trackPopulation = true;
    [SerializeField] private bool trackBehaviors = true;
    [SerializeField] private bool trackEnergy = true;
    [SerializeField] private bool trackMating = true;
    [SerializeField] private bool trackFood = true;

    // Debug data storage
    private List<DebugSnapshot> snapshots = new List<DebugSnapshot>();
    private float lastDebugTime;
    private DebugSnapshot currentSnapshot;

    private struct DebugSnapshot
    {
        public float gameTime;
        public float realTime;
        public float timeScale;

        // Population data
        public int totalAgents;
        public int adultAgents;
        public int childrenAgents;
        public int deadAgents; // Agents that died this frame

        // Behavior data
        public int wanderingAgents;
        public int foragingAgents;
        public int mateSeekingAgents;
        public int matingAgents;

        // Energy data
        public float avgEnergy;
        public int hungryAgents;
        public int starvingAgents;

        // Mating data
        public int matingPairs;
        public int newBirths;

        // Food data
        public int totalFood;
        public float foodSpawnRate; // Food spawned per game-second
        public float foodConsumptionRate; // Food consumed per game-second

        // Issues detected
        public List<string> issues;
    }

    void Start()
    {
        if (enableDebugging)
        {
            InvokeRepeating(nameof(TakeSnapshot), 1f, debugUpdateInterval);
        }
    }

    void TakeSnapshot()
    {
        if (!enableDebugging) return;

        currentSnapshot = new DebugSnapshot
        {
            gameTime = Time.time,
            realTime = Time.unscaledTime,
            timeScale = Time.timeScale,
            issues = new List<string>()
        };

        // Collect data
        CollectPopulationData();
        CollectBehaviorData();
        CollectEnergyData();
        CollectMatingData();
        CollectFoodData();

        // Analyze for issues
        AnalyzeForIssues();

        // Store snapshot
        snapshots.Add(currentSnapshot);

        // Keep only last 10 snapshots
        if (snapshots.Count > 10)
        {
            snapshots.RemoveAt(0);
        }

        // Log if enabled
        if (logToConsole)
        {
            LogSnapshot();
        }
    }

    private void CollectPopulationData()
    {
        if (!trackPopulation) return;

        var agents = FindObjectsOfType<AgentController>();
        currentSnapshot.totalAgents = agents.Length;

        int adults = 0, children = 0;

        foreach (var agent in agents)
        {
            var ageSystem = agent.GetAgeSystem();
            if (ageSystem != null && ageSystem.IsMature)
                adults++;
            else
                children++;
        }

        currentSnapshot.adultAgents = adults;
        currentSnapshot.childrenAgents = children;
    }

    private void CollectBehaviorData()
    {
        if (!trackBehaviors) return;

        var agents = FindObjectsOfType<AgentController>();
        int wandering = 0, foraging = 0, mateSeeking = 0, mating = 0;

        foreach (var agent in agents)
        {
            var context = agent.GetContext();
            if (context?.Reproduction?.IsMating == true)
            {
                mating++;
            }
            // You might need to add a way to get current behavior from AgentController
            // This is just a placeholder for now
        }

        currentSnapshot.wanderingAgents = wandering;
        currentSnapshot.foragingAgents = foraging;
        currentSnapshot.mateSeekingAgents = mateSeeking;
        currentSnapshot.matingAgents = mating;
    }

    private void CollectEnergyData()
    {
        if (!trackEnergy) return;

        var energySystems = FindObjectsOfType<EnergySystem>();
        float totalEnergy = 0f;
        int hungry = 0, starving = 0;

        foreach (var energy in energySystems)
        {
            totalEnergy += energy.EnergyPercent;

            if (energy.IsHungry)
                hungry++;

            if (energy.EnergyPercent < 0.1f) // Less than 10% energy
                starving++;
        }

        currentSnapshot.avgEnergy = energySystems.Length > 0 ? totalEnergy / energySystems.Length : 0f;
        currentSnapshot.hungryAgents = hungry;
        currentSnapshot.starvingAgents = starving;
    }

    private void CollectMatingData()
    {
        if (!trackMating) return;

        var reproSystems = FindObjectsOfType<ReproductionSystem>();
        int matingCount = 0;

        foreach (var repro in reproSystems)
        {
            if (repro.IsMating)
                matingCount++;
        }

        currentSnapshot.matingPairs = matingCount / 2; // Each pair involves 2 agents

        // Birth tracking would need to be implemented in AgentSpawner
        currentSnapshot.newBirths = 0; // Placeholder
    }

    private void CollectFoodData()
    {
        if (!trackFood) return;

        var foods = FindObjectsOfType<Food>();
        currentSnapshot.totalFood = foods.Length;

        // Calculate rates (would need more sophisticated tracking)
        currentSnapshot.foodSpawnRate = 0f; // Placeholder
        currentSnapshot.foodConsumptionRate = 0f; // Placeholder
    }

    private void AnalyzeForIssues()
    {
        // Detect common high-speed simulation issues

        // Issue 1: Population crash
        if (currentSnapshot.totalAgents < 5)
        {
            currentSnapshot.issues.Add("?? POPULATION CRASH: Very few agents remaining");
        }

        // Issue 2: No reproduction
        if (currentSnapshot.adultAgents > 10 && currentSnapshot.matingPairs == 0)
        {
            currentSnapshot.issues.Add("?? NO MATING: Adults present but no reproduction occurring");
        }

        // Issue 3: Mass starvation
        if (currentSnapshot.starvingAgents > currentSnapshot.totalAgents * 0.5f)
        {
            currentSnapshot.issues.Add("?? MASS STARVATION: Over 50% of agents are starving");
        }

        // Issue 4: Food shortage
        if (currentSnapshot.totalFood < currentSnapshot.totalAgents * 0.5f)
        {
            currentSnapshot.issues.Add("?? FOOD SHORTAGE: Not enough food for population");
        }

        // Issue 5: All agents hungry
        if (currentSnapshot.hungryAgents == currentSnapshot.totalAgents && currentSnapshot.totalAgents > 0)
        {
            currentSnapshot.issues.Add("?? UNIVERSAL HUNGER: All agents are hungry");
        }

        // Issue 6: No adults (only children)
        if (currentSnapshot.totalAgents > 0 && currentSnapshot.adultAgents == 0)
        {
            currentSnapshot.issues.Add("?? NO ADULTS: Only children present, can't reproduce");
        }

        // Issue 7: Time scale problems
        if (currentSnapshot.timeScale > 50f && currentSnapshot.issues.Count > 0)
        {
            currentSnapshot.issues.Add($"?? HIGH SPEED DETECTED: Issues may be due to {currentSnapshot.timeScale:F1}x speed");
        }
    }

    private void LogSnapshot()
    {
        string log = $"=== SIMULATION DEBUG (Game: {currentSnapshot.gameTime:F1}s, Speed: {currentSnapshot.timeScale:F1}x) ===\n" +
                    $"Population: {currentSnapshot.totalAgents} ({currentSnapshot.adultAgents} adults, {currentSnapshot.childrenAgents} children)\n" +
                    $"Energy: Avg {currentSnapshot.avgEnergy:P1}, {currentSnapshot.hungryAgents} hungry, {currentSnapshot.starvingAgents} starving\n" +
                    $"Mating: {currentSnapshot.matingPairs} pairs currently mating\n" +
                    $"Food: {currentSnapshot.totalFood} items available\n";

        if (currentSnapshot.issues.Count > 0)
        {
            log += "ISSUES DETECTED:\n";
            foreach (var issue in currentSnapshot.issues)
            {
                log += $"  {issue}\n";
            }
        }
        else
        {
            log += "? No issues detected\n";
        }

        Debug.Log(log);
    }

    void OnGUI()
    {
        if (!showDebugGUI || !enableDebugging || snapshots.Count == 0) return;

        var latest = snapshots.Last();

        int w = Screen.width, h = Screen.height;
        GUIStyle style = new GUIStyle();
        style.alignment = TextAnchor.UpperRight;
        style.fontSize = h * 2 / 80;
        style.normal.textColor = latest.issues.Count > 0 ? Color.red : Color.green;

        Rect rect = new Rect(w - 350, 10, 340, 300);

        string debugText = $"=== SIMULATION DEBUG ===\n" +
                          $"Time: {latest.gameTime:F1}s ({latest.timeScale:F1}x)\n" +
                          $"Population: {latest.totalAgents}\n" +
                          $"  Adults: {latest.adultAgents}\n" +
                          $"  Children: {latest.childrenAgents}\n" +
                          $"Energy: {latest.avgEnergy:P1} avg\n" +
                          $"  Hungry: {latest.hungryAgents}\n" +
                          $"  Starving: {latest.starvingAgents}\n" +
                          $"Mating: {latest.matingPairs} pairs\n" +
                          $"Food: {latest.totalFood}\n";

        if (latest.issues.Count > 0)
        {
            debugText += "\n?? ISSUES:\n";
            foreach (var issue in latest.issues.Take(3)) // Show max 3 issues
            {
                debugText += $"{issue}\n";
            }
        }

        GUI.Label(rect, debugText, style);
    }

    /// <summary>
    /// Get detailed report for manual analysis
    /// </summary>
    public string GetDetailedReport()
    {
        if (snapshots.Count == 0) return "No debug data available";

        var latest = snapshots.Last();
        var oldest = snapshots.First();

        string report = $"=== SIMULATION ANALYSIS REPORT ===\n\n";
        report += $"Time Range: {oldest.gameTime:F1}s to {latest.gameTime:F1}s (Speed: {latest.timeScale:F1}x)\n\n";

        report += $"Population Trend:\n";
        report += $"  Start: {oldest.totalAgents} ? End: {latest.totalAgents}\n";
        report += $"  Adults: {oldest.adultAgents} ? {latest.adultAgents}\n\n";

        report += $"Current State:\n";
        report += $"  Average Energy: {latest.avgEnergy:P1}\n";
        report += $"  Hungry Agents: {latest.hungryAgents}/{latest.totalAgents}\n";
        report += $"  Active Mating: {latest.matingPairs} pairs\n";
        report += $"  Food Available: {latest.totalFood}\n\n";

        if (latest.issues.Count > 0)
        {
            report += $"Critical Issues:\n";
            foreach (var issue in latest.issues)
            {
                report += $"  • {issue}\n";
            }
        }

        return report;
    }

    /// <summary>
    /// Reset debug data
    /// </summary>
    [ContextMenu("Reset Debug Data")]
    public void ResetDebugData()
    {
        snapshots.Clear();
        Debug.Log("Debug data reset");
    }

    /// <summary>
    /// Force take snapshot now
    /// </summary>
    [ContextMenu("Take Snapshot Now")]
    public void ForceSnapshot()
    {
        TakeSnapshot();
    }
}