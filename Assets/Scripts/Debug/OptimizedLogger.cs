using UnityEngine;
using System.Collections.Generic;
using static OptimizedLogger;

/// <summary>
/// Performance-optimized debug logger that eliminates performance issues from Debug.Log spam
/// Drop-in replacement for Debug.Log with smart filtering and batching
/// </summary>
public static class OptimizedLogger
{
    [System.Flags]
    public enum LogLevel
    {
        None = 0,
        Error = 1,
        Warning = 2,
        Info = 4,
        Verbose = 8,
        All = Error | Warning | Info | Verbose
    }

    [System.Flags]
    public enum LogCategory
    {
        None = 0,
        Agent = 1,
        Food = 2,
        Reproduction = 4,
        Movement = 8,
        Energy = 16,
        Performance = 32,
        Spawning = 64,
        All = Agent | Food | Reproduction | Movement | Energy | Performance | Spawning
    }

    // Settings - configure these in your main script
    public static LogLevel currentLogLevel = LogLevel.Error | LogLevel.Warning; // Only errors and warnings by default
    public static LogCategory enabledCategories = LogCategory.All;
    public static bool enableBatching = true;
    public static float batchFlushInterval = 1f; // Flush batched logs every second
    public static int maxBatchSize = 50; // Max logs to batch before forcing flush

    // Performance settings
    private static bool isEnabled = true;
    private static float lastFlushTime = 0f;
    private static Queue<BatchedLog> batchedLogs = new Queue<BatchedLog>();
    private static Dictionary<string, int> messageCounters = new Dictionary<string, int>();
    private static Dictionary<string, float> lastLogTimes = new Dictionary<string, float>();

    private struct BatchedLog
    {
        public string message;
        public LogLevel level;
        public Object context;
        public float timestamp;
    }

    #region Public Logging Methods

    /// <summary>
    /// Log error message (always shown unless completely disabled)
    /// </summary>
    public static void LogError(string message, LogCategory category = LogCategory.None, Object context = null)
    {
        Log(message, LogLevel.Error, category, context);
    }

    /// <summary>
    /// Log warning message
    /// </summary>
    public static void LogWarning(string message, LogCategory category = LogCategory.None, Object context = null)
    {
        Log(message, LogLevel.Warning, category, context);
    }

    /// <summary>
    /// Log info message
    /// </summary>
    public static void LogInfo(string message, LogCategory category = LogCategory.None, Object context = null)
    {
        Log(message, LogLevel.Info, category, context);
    }

    /// <summary>
    /// Log verbose message (detailed debugging)
    /// </summary>
    public static void LogVerbose(string message, LogCategory category = LogCategory.None, Object context = null)
    {
        Log(message, LogLevel.Verbose, category, context);
    }

    /// <summary>
    /// Log agent-specific message
    /// </summary>
    public static void LogAgent(string message, LogLevel level = LogLevel.Info, Object context = null)
    {
        Log($"[AGENT] {message}", level, LogCategory.Agent, context);
    }

    /// <summary>
    /// Log performance metrics (special handling)
    /// </summary>
    public static void LogPerformance(string message, Object context = null)
    {
        Log($"[PERFORMANCE] {message}", LogLevel.Info, LogCategory.Performance, context);
    }

    /// <summary>
    /// Log with rate limiting to prevent spam
    /// </summary>
    public static void LogRateLimited(string message, float minInterval = 1f, LogLevel level = LogLevel.Info, LogCategory category = LogCategory.None, Object context = null)
    {
        string key = $"{message}_{level}_{category}";

        if (lastLogTimes.TryGetValue(key, out float lastTime))
        {
            if (Time.time - lastTime < minInterval)
            {
                return; // Skip this log due to rate limiting
            }
        }

        lastLogTimes[key] = Time.time;
        Log(message, level, category, context);
    }

    /// <summary>
    /// Log with counting to show frequency instead of spam
    /// </summary>
    public static void LogCounted(string baseMessage, LogLevel level = LogLevel.Info, LogCategory category = LogCategory.None, Object context = null)
    {
        if (!messageCounters.TryGetValue(baseMessage, out int count))
        {
            count = 0;
        }

        messageCounters[baseMessage] = count + 1;

        // Only log every 10th occurrence, but show the count
        if (count % 10 == 0)
        {
            Log($"{baseMessage} (x{count + 1})", level, category, context);
        }
    }

    #endregion

    #region Core Logging Logic

    private static void Log(string message, LogLevel level, LogCategory category, Object context)
    {
        // Early exit if logging is disabled
        if (!isEnabled) return;

        // Check if this log level is enabled
        if ((currentLogLevel & level) == 0) return;

        // Check if this category is enabled
        if (category != LogCategory.None && (enabledCategories & category) == 0) return;

        // Performance optimization: batch non-critical logs
        if (enableBatching && level != LogLevel.Error)
        {
            BatchLog(message, level, context);
        }
        else
        {
            // Critical logs (errors) go through immediately
            LogImmediate(message, level, context);
        }
    }

    private static void BatchLog(string message, LogLevel level, Object context)
    {
        batchedLogs.Enqueue(new BatchedLog
        {
            message = message,
            level = level,
            context = context,
            timestamp = Time.time
        });

        // Flush if batch is full or enough time has passed
        if (batchedLogs.Count >= maxBatchSize || Time.time - lastFlushTime >= batchFlushInterval)
        {
            FlushBatchedLogs();
        }
    }

    private static void FlushBatchedLogs()
    {
        if (batchedLogs.Count == 0) return;

        // Group similar messages to reduce spam
        Dictionary<string, int> groupedMessages = new Dictionary<string, int>();
        List<BatchedLog> uniqueLogs = new List<BatchedLog>();

        while (batchedLogs.Count > 0)
        {
            var log = batchedLogs.Dequeue();

            if (groupedMessages.TryGetValue(log.message, out int count))
            {
                groupedMessages[log.message] = count + 1;
            }
            else
            {
                groupedMessages[log.message] = 1;
                uniqueLogs.Add(log);
            }
        }

        // Output grouped messages
        foreach (var log in uniqueLogs)
        {
            int count = groupedMessages[log.message];
            string finalMessage = count > 1 ? $"{log.message} (x{count})" : log.message;

            LogImmediate(finalMessage, log.level, log.context);
        }

        lastFlushTime = Time.time;
    }

    private static void LogImmediate(string message, LogLevel level, Object context)
    {
        // Only call Unity's Debug.Log when we actually want to see the message
        switch (level)
        {
            case LogLevel.Error:
                if (context != null)
                    Debug.LogError(message, context);
                else
                    Debug.LogError(message);
                break;

            case LogLevel.Warning:
                if (context != null)
                    Debug.LogWarning(message, context);
                else
                    Debug.LogWarning(message);
                break;

            default:
                if (context != null)
                    Debug.Log(message, context);
                else
                    Debug.Log(message);
                break;
        }
    }

    #endregion

    #region Configuration Methods

    /// <summary>
    /// Configure logger for high-speed simulation (minimal logging)
    /// </summary>
    public static void ConfigureForHighSpeed()
    {
        currentLogLevel = LogLevel.Error; // Only errors
        enabledCategories = LogCategory.Performance; // Only performance logs
        enableBatching = true;
        batchFlushInterval = 5f; // Flush less frequently
        maxBatchSize = 100; // Larger batches

        Debug.Log("OptimizedLogger configured for high-speed simulation");
    }

    /// <summary>
    /// Configure logger for development (more detailed logging)
    /// </summary>
    public static void ConfigureForDevelopment()
    {
        currentLogLevel = LogLevel.All;
        enabledCategories = LogCategory.All;
        enableBatching = true;
        batchFlushInterval = 1f;
        maxBatchSize = 20;

        Debug.Log("OptimizedLogger configured for development");
    }

    /// <summary>
    /// Disable logging completely for maximum performance
    /// </summary>
    public static void DisableLogging()
    {
        isEnabled = false;
        batchedLogs.Clear();
        Debug.Log("OptimizedLogger disabled for maximum performance");
    }

    /// <summary>
    /// Enable logging
    /// </summary>
    public static void EnableLogging()
    {
        isEnabled = true;
        Debug.Log("OptimizedLogger enabled");
    }

    /// <summary>
    /// Force flush all batched logs immediately
    /// </summary>
    public static void FlushNow()
    {
        FlushBatchedLogs();
    }

    #endregion

    #region Statistics and Monitoring

    /// <summary>
    /// Get logging statistics
    /// </summary>
    public static string GetStats()
    {
        int totalCounted = 0;
        foreach (var count in messageCounters.Values)
        {
            totalCounted += count;
        }

        return $"OptimizedLogger Stats:\n" +
               $"• Enabled: {isEnabled}\n" +
               $"• Level: {currentLogLevel}\n" +
               $"• Categories: {enabledCategories}\n" +
               $"• Batched: {batchedLogs.Count}\n" +
               $"• Counted Messages: {totalCounted}\n" +
               $"• Rate Limited: {lastLogTimes.Count}";
    }

    /// <summary>
    /// Clear all counters and rate limits
    /// </summary>
    public static void ClearStats()
    {
        messageCounters.Clear();
        lastLogTimes.Clear();
        batchedLogs.Clear();
    }

    #endregion
}

// ============================================================================
// EASY MIGRATION: Replace your Debug.Log calls with these
// ============================================================================

/// <summary>
/// Drop-in replacement for Unity's Debug class with performance optimizations
/// </summary>
public static class DebugOptimized
{
    // Direct replacements for Debug.Log calls
    public static void Log(string message) => OptimizedLogger.LogInfo(message);
    public static void Log(string message, Object context) => OptimizedLogger.LogInfo(message, context: context);
    public static void LogWarning(string message) => OptimizedLogger.LogWarning(message);
    public static void LogWarning(string message, Object context) => OptimizedLogger.LogWarning(message, context: context);
    public static void LogError(string message) => OptimizedLogger.LogError(message);
    public static void LogError(string message, Object context) => OptimizedLogger.LogError(message, context: context);

    // Enhanced versions with categories
    public static void LogAgent(string message) => OptimizedLogger.LogAgent(message);
    public static void LogFood(string message) => OptimizedLogger.LogInfo($"[FOOD] {message}", LogCategory.Food);
    public static void LogReproduction(string message) => OptimizedLogger.LogInfo($"[REPRODUCTION] {message}", LogCategory.Reproduction);
}