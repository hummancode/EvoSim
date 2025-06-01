using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

/// <summary>
/// Advanced logging system for simulation analysis with separate log categories
/// </summary>
public class SimulationLogger : MonoBehaviour
{
    [Header("Logging Settings")]
    [SerializeField] private bool enableLogging = true;
    [SerializeField] private bool logToFile = true;
    [SerializeField] private bool logToConsole = true;
    [SerializeField] private float loggingInterval = 1f;

    [Header("Log Categories")]
    [SerializeField] private bool logPerformance = true;
    [SerializeField] private bool logPopulation = true;
    [SerializeField] private bool logEnergy = true;
    [SerializeField] private bool logMating = true;
    [SerializeField] private bool logFood = true;
    [SerializeField] private bool logCriticalIssues = true;

    [Header("Critical Thresholds")]
    [SerializeField] private int minPopulationWarning = 5;
    [SerializeField] private float lowEnergyThreshold = 0.2f;
    [SerializeField] private float highSpeedThreshold = 50f;

    // Logging infrastructure
    private static SimulationLogger instance;
    public static SimulationLogger Instance => instance;

    private Dictionary<LogCategory, StreamWriter> logWriters = new Dictionary<LogCategory, StreamWriter>();
    private List<LogEntry> sessionLog = new List<LogEntry>();
    private float lastLogTime;
    private string sessionID;

    public enum LogCategory
    {
        Performance,
        Population,
        Energy,
        Mating,
        Food,
        Critical,
        Analysis
    }

    public enum LogLevel
    {
        Info,
        Warning,
        Error,
        Critical
    }

    private struct LogEntry
    {
        public float gameTime;
        public float realTime;
        public float timeScale;
        public LogCategory category;
        public LogLevel level;
        public string message;
        public Dictionary<string, object> data;

        public LogEntry(LogCategory category, LogLevel level, string message, Dictionary<string, object> data = null)
        {
            this.gameTime = Time.time;
            this.realTime = Time.unscaledTime;
            this.timeScale = Time.timeScale;
            this.category = category;
            this.level = level;
            this.message = message;
            this.data = data ?? new Dictionary<string, object>();
        }
    }

    void Awake()
    {
        // Singleton setup
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeLogging();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (enableLogging)
        {
            InvokeRepeating(nameof(PerformPeriodicLogging), 1f, loggingInterval);
            LogInfo(LogCategory.Analysis, "=== SIMULATION SESSION STARTED ===");
        }
    }

    void InitializeLogging()
    {
        sessionID = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");

        if (logToFile)
        {
            string logDir = Path.Combine(Application.persistentDataPath, "SimulationLogs", sessionID);
            Directory.CreateDirectory(logDir);

            // Create separate log files for each category
            foreach (LogCategory category in Enum.GetValues(typeof(LogCategory)))
            {
                string fileName = $"{category.ToString().ToLower()}_log.txt";
                string filePath = Path.Combine(logDir, fileName);

                try
                {
                    var writer = new StreamWriter(filePath, false);
                    writer.WriteLine($"=== {category} LOG - Session {sessionID} ===");
                    writer.WriteLine($"Started at: {DateTime.Now}");
                    writer.WriteLine("Time(Game)\tTime(Real)\tSpeed\tLevel\tMessage\tData");
                    writer.Flush();

                    logWriters[category] = writer;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Failed to create log file for {category}: {e.Message}");
                }
            }

            Debug.Log($"Simulation logs will be saved to: {logDir}");
        }
    }

    void PerformPeriodicLogging()
    {
        if (!enableLogging) return;

        // Collect current simulation data
        var simData = CollectSimulationData();

        // Log performance data
        if (logPerformance)
        {
            LogPerformanceData(simData);
        }

        // Log population data
        if (logPopulation)
        {
            LogPopulationData(simData);
        }

        // Log energy data  
        if (logEnergy)
        {
            LogEnergyData(simData);
        }

        // Log mating data
        if (logMating)
        {
            LogMatingData(simData);
        }

        // Log food data
        if (logFood)
        {
            LogFoodData(simData);
        }

        // Analyze for critical issues
        if (logCriticalIssues)
        {
            AnalyzeCriticalIssues(simData);
        }
    }

    private SimulationData CollectSimulationData()
    {
        var data = new SimulationData();

        // Collect agents
        var agents = FindObjectsOfType<AgentController>();
        data.totalAgents = agents.Length;

        // Collect energy systems
        var energySystems = FindObjectsOfType<EnergySystem>();
        float totalEnergy = 0f;
        int hungryCount = 0;
        int starvingCount = 0;

        foreach (var energy in energySystems)
        {
            totalEnergy += energy.EnergyPercent;
            if (energy.IsHungry) hungryCount++;
            if (energy.EnergyPercent < lowEnergyThreshold) starvingCount++;
        }

        data.avgEnergy = energySystems.Length > 0 ? totalEnergy / energySystems.Length : 0f;
        data.hungryAgents = hungryCount;
        data.starvingAgents = starvingCount;

        // Collect mating data
        var reproSystems = FindObjectsOfType<ReproductionSystem>();
        int matingCount = 0;
        foreach (var repro in reproSystems)
        {
            if (repro.IsMating) matingCount++;
        }
        data.matingPairs = matingCount / 2;

        // Collect food data
        data.totalFood = FindObjectsOfType<Food>().Length;

        // Performance data
        data.fps = 1f / Time.unscaledDeltaTime;
        data.timeScale = Time.timeScale;

        return data;
    }

    private void LogPerformanceData(SimulationData data)
    {
        var perfData = new Dictionary<string, object>
        {
            ["fps"] = data.fps,
            ["timeScale"] = data.timeScale,
            ["frameTime"] = Time.unscaledDeltaTime * 1000f,
            ["physicsRate"] = 1f / Time.fixedDeltaTime
        };

        LogLevel level = LogLevel.Info;
        if (data.fps < 30f) level = LogLevel.Warning;
        if (data.fps < 15f) level = LogLevel.Error;

        Log(LogCategory.Performance, level,
            $"FPS: {data.fps:F1}, Speed: {data.timeScale:F1}x, Frame: {Time.unscaledDeltaTime * 1000f:F1}ms",
            perfData);

        // Critical performance warnings
        if (data.timeScale > highSpeedThreshold && data.fps < 30f)
        {
            LogCritical(LogCategory.Critical,
                $"PERFORMANCE DEGRADATION at {data.timeScale:F1}x speed - FPS dropped to {data.fps:F1}");
        }
    }

    private void LogPopulationData(SimulationData data)
    {
        var popData = new Dictionary<string, object>
        {
            ["totalAgents"] = data.totalAgents,
            ["timeScale"] = data.timeScale
        };

        LogLevel level = data.totalAgents < minPopulationWarning ? LogLevel.Warning : LogLevel.Info;

        Log(LogCategory.Population, level,
            $"Population: {data.totalAgents} agents",
            popData);

        // Population crash warning
        if (data.totalAgents < minPopulationWarning)
        {
            LogCritical(LogCategory.Critical,
                $"POPULATION CRISIS: Only {data.totalAgents} agents remaining at {data.timeScale:F1}x speed");
        }
    }

    private void LogEnergyData(SimulationData data)
    {
        var energyData = new Dictionary<string, object>
        {
            ["avgEnergy"] = data.avgEnergy,
            ["hungryAgents"] = data.hungryAgents,
            ["starvingAgents"] = data.starvingAgents,
            ["totalAgents"] = data.totalAgents,
            ["timeScale"] = data.timeScale
        };

        LogLevel level = LogLevel.Info;
        if (data.starvingAgents > data.totalAgents * 0.3f) level = LogLevel.Warning;
        if (data.starvingAgents > data.totalAgents * 0.7f) level = LogLevel.Error;

        Log(LogCategory.Energy, level,
            $"Energy: {data.avgEnergy:P1} avg, {data.hungryAgents} hungry, {data.starvingAgents} starving",
            energyData);

        // Energy crisis warning
        if (data.totalAgents > 0 && data.starvingAgents == data.totalAgents)
        {
            LogCritical(LogCategory.Critical,
                $"ENERGY CRISIS: ALL agents starving at {data.timeScale:F1}x speed");
        }
    }

    private void LogMatingData(SimulationData data)
    {
        var matingData = new Dictionary<string, object>
        {
            ["matingPairs"] = data.matingPairs,
            ["totalAgents"] = data.totalAgents,
            ["timeScale"] = data.timeScale
        };

        Log(LogCategory.Mating, LogLevel.Info,
            $"Mating: {data.matingPairs} active pairs",
            matingData);

        // Reproduction failure warning
        if (data.totalAgents > 10 && data.matingPairs == 0 && data.timeScale > 20f)
        {
            LogWarning(LogCategory.Critical,
                $"REPRODUCTION FAILURE: No mating occurring despite {data.totalAgents} agents at {data.timeScale:F1}x speed");
        }
    }

    private void LogFoodData(SimulationData data)
    {
        var foodData = new Dictionary<string, object>
        {
            ["totalFood"] = data.totalFood,
            ["foodPerAgent"] = data.totalAgents > 0 ? (float)data.totalFood / data.totalAgents : 0f,
            ["timeScale"] = data.timeScale
        };

        LogLevel level = LogLevel.Info;
        float foodPerAgent = data.totalAgents > 0 ? (float)data.totalFood / data.totalAgents : 0f;
        if (foodPerAgent < 0.5f) level = LogLevel.Warning;
        if (foodPerAgent < 0.2f) level = LogLevel.Error;

        Log(LogCategory.Food, level,
            $"Food: {data.totalFood} items ({foodPerAgent:F1} per agent)",
            foodData);
    }

    private void AnalyzeCriticalIssues(SimulationData data)
    {
        // Speed-related analysis
        if (data.timeScale > highSpeedThreshold)
        {
            LogInfo(LogCategory.Analysis,
                $"HIGH SPEED ANALYSIS: Running at {data.timeScale:F1}x - monitoring for instabilities");

            // Check for cascade failures at high speed
            bool hasIssues = false;

            if (data.totalAgents < minPopulationWarning)
            {
                LogCritical(LogCategory.Critical,
                    $"SPEED-RELATED POPULATION COLLAPSE: {data.totalAgents} agents at {data.timeScale:F1}x");
                hasIssues = true;
            }

            if (data.starvingAgents > data.totalAgents * 0.5f)
            {
                LogCritical(LogCategory.Critical,
                    $"SPEED-RELATED STARVATION: {data.starvingAgents}/{data.totalAgents} starving at {data.timeScale:F1}x");
                hasIssues = true;
            }

            if (data.fps < 15f)
            {
                LogCritical(LogCategory.Critical,
                    $"SPEED-RELATED PERFORMANCE COLLAPSE: {data.fps:F1} FPS at {data.timeScale:F1}x");
                hasIssues = true;
            }

            if (!hasIssues)
            {
                LogInfo(LogCategory.Analysis,
                    $"HIGH SPEED STABLE: {data.timeScale:F1}x running smoothly");
            }
        }
    }

    // Public logging methods
    public static void LogInfo(LogCategory category, string message, Dictionary<string, object> data = null)
    {
        Instance?.Log(category, LogLevel.Info, message, data);
    }

    public static void LogWarning(LogCategory category, string message, Dictionary<string, object> data = null)
    {
        Instance?.Log(category, LogLevel.Warning, message, data);
    }

    public static void LogError(LogCategory category, string message, Dictionary<string, object> data = null)
    {
        Instance?.Log(category, LogLevel.Error, message, data);
    }

    public static void LogCritical(LogCategory category, string message, Dictionary<string, object> data = null)
    {
        Instance?.Log(category, LogLevel.Critical, message, data);
    }

    private void Log(LogCategory category, LogLevel level, string message, Dictionary<string, object> data = null)
    {
        if (!enableLogging) return;

        var entry = new LogEntry(category, level, message, data);
        sessionLog.Add(entry);

        // Console logging with color coding
        if (logToConsole)
        {
            string consoleMessage = $"[{category}] {message}";

            switch (level)
            {
                case LogLevel.Info:
                    Debug.Log(consoleMessage);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(consoleMessage);
                    break;
                case LogLevel.Error:
                case LogLevel.Critical:
                    Debug.LogError(consoleMessage);
                    break;
            }
        }

        // File logging
        if (logToFile && logWriters.TryGetValue(category, out StreamWriter writer))
        {
            try
            {
                string dataString = data != null ? string.Join(";", data.Select(kvp => $"{kvp.Key}={kvp.Value}")) : "";
                writer.WriteLine($"{entry.gameTime:F2}\t{entry.realTime:F2}\t{entry.timeScale:F1}\t{level}\t{message}\t{dataString}");
                writer.Flush();
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to write to log file: {e.Message}");
            }
        }
    }

    /// <summary>
    /// Generate comprehensive analysis report
    /// </summary>
    [ContextMenu("Generate Analysis Report")]
    public void GenerateAnalysisReport()
    {
        var criticalEntries = sessionLog.Where(e => e.level == LogLevel.Critical || e.level == LogLevel.Error).ToList();

        string report = $"=== SIMULATION ANALYSIS REPORT ===\n";
        report += $"Session: {sessionID}\n";
        report += $"Duration: {Time.unscaledTime:F1} seconds\n";
        report += $"Max Speed Reached: {sessionLog.Max(e => e.timeScale):F1}x\n\n";

        if (criticalEntries.Any())
        {
            report += "CRITICAL ISSUES DETECTED:\n";
            foreach (var entry in criticalEntries.GroupBy(e => e.timeScale))
            {
                report += $"\nAt {entry.Key:F1}x speed:\n";
                foreach (var issue in entry)
                {
                    report += $"  • {issue.message}\n";
                }
            }
        }
        else
        {
            report += "? No critical issues detected during session\n";
        }

        Debug.Log(report);
    }

    void OnDestroy()
    {
        if (logToFile)
        {
            foreach (var writer in logWriters.Values)
            {
                writer?.Close();
            }
        }
    }

    void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            LogInfo(LogCategory.Analysis, "Application paused");
        }
        else
        {
            LogInfo(LogCategory.Analysis, "Application resumed");
        }
    }

    private struct SimulationData
    {
        public int totalAgents;
        public float avgEnergy;
        public int hungryAgents;
        public int starvingAgents;
        public int matingPairs;
        public int totalFood;
        public float fps;
        public float timeScale;
    }
}