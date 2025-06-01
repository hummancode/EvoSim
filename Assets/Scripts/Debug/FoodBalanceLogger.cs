using UnityEngine;

/// <summary>
/// Simple logger focused on food balance analysis
/// Logs every 10 game seconds regardless of time scale
/// </summary>
public class FoodBalanceLogger : MonoBehaviour
{
    [Header("Logging Settings")]
    [SerializeField] private float logIntervalGameSeconds = 10f;
    [SerializeField] private bool enableLogging = true;

    private float nextLogTime = 0f;

    void Start()
    {
        if (enableLogging)
        {
            // Log initial state immediately
            LogFoodBalance();
            nextLogTime = Time.time + logIntervalGameSeconds;
        }
    }

    void Update()
    {
        if (!enableLogging) return;

        // Check if it's time to log (using game time)
        if (Time.time >= nextLogTime)
        {
            LogFoodBalance();
            nextLogTime = Time.time + logIntervalGameSeconds;
        }
    }

    private void LogFoodBalance()
    {
        // Collect data
        var foodObjects = FindObjectsOfType<Food>();
        int totalFood = foodObjects.Length;

        var agentObjects = FindObjectsOfType<AgentController>();
        int totalAgents = agentObjects.Length;

        float currentTimeScale = Time.timeScale;
        float gameTime = Time.time;
        float realTime = Time.unscaledTime;

        // Calculate food spawner state if available
        var foodSpawner = FindObjectOfType<FoodSpawner>();
        float spawnInterval = 0f;
        int initialFoodCount = 0;

        if (foodSpawner != null)
        {
            // You might need to make these fields public or add getters to FoodSpawner
            // For now, we'll use reflection or estimate
            spawnInterval = 2.0f; // Default from your FoodSpawner
            initialFoodCount = 150; // Default from your FoodSpawner
        }

        // Calculate expected food based on theory
        float expectedFoodSpawnedSoFar = gameTime / spawnInterval;

        // Log in a simple, parseable format
        string logMessage = $"FOOD_BALANCE_LOG: " +
                          $"GameTime={gameTime:F1}s, " +
                          $"RealTime={realTime:F1}s, " +
                          $"Speed={currentTimeScale:F1}x, " +
                          $"FoodCount={totalFood}, " +
                          $"AgentCount={totalAgents}, " +
                          $"ExpectedSpawned={expectedFoodSpawnedSoFar:F0}, " +
                          $"SpawnInterval={spawnInterval:F1}s";

        Debug.Log(logMessage);

        // Additional detailed info for analysis
        if (foodObjects.Length > 0)
        {
            // Sample some food ages for decay analysis
            int sampleSize = Mathf.Min(5, foodObjects.Length);
            string ageInfo = "";

            for (int i = 0; i < sampleSize; i++)
            {
                var food = foodObjects[i];
                // You might need to add a method to Food class to get age
                // For now, we'll estimate or leave it out
                ageInfo += $"Food{i}_Age=Unknown ";
            }

            Debug.Log($"FOOD_SAMPLE_LOG: {ageInfo}Speed={currentTimeScale:F1}x");
        }

        // Performance check
        float currentFPS = 1f / Time.unscaledDeltaTime;
        Debug.Log($"PERFORMANCE_LOG: GameTime={gameTime:F1}s, Speed={currentTimeScale:F1}x, FPS={currentFPS:F1}");

        // Simple theoretical analysis
        AnalyzeFoodBalance(totalFood, gameTime, currentTimeScale, spawnInterval);
    }

    private void AnalyzeFoodBalance(int currentFood, float gameTime, float timeScale, float spawnInterval)
    {
        // Theoretical calculation
        float expectedSpawns = gameTime / spawnInterval;
        float foodLifespan = 230f; // From your Food class

        // At steady state, food count should stabilize
        // New food spawns every spawnInterval seconds
        // Food decays after foodLifespan seconds
        // So steady state = foodLifespan / spawnInterval
        float theoreticalSteadyState = foodLifespan / spawnInterval;

        float difference = currentFood - theoreticalSteadyState;
        float percentDifference = theoreticalSteadyState > 0 ? (difference / theoreticalSteadyState) * 100f : 0f;

        string analysisMessage = $"FOOD_ANALYSIS_LOG: " +
                               $"GameTime={gameTime:F1}s, " +
                               $"Speed={timeScale:F1}x, " +
                               $"Current={currentFood}, " +
                               $"Theoretical={theoreticalSteadyState:F0}, " +
                               $"Difference={difference:F0} ({percentDifference:F1}%), " +
                               $"ExpectedSpawns={expectedSpawns:F0}";

        Debug.Log(analysisMessage);

        // Flag significant deviations
        if (Mathf.Abs(percentDifference) > 20f && gameTime > 60f) // After 60 seconds, allow 20% tolerance
        {
            Debug.LogWarning($"FOOD_IMBALANCE_WARNING: {percentDifference:F1}% deviation from expected at {timeScale:F1}x speed");
        }
    }

    /// <summary>
    /// Force log now (for testing)
    /// </summary>
    [ContextMenu("Log Food Balance Now")]
    public void ForceLog()
    {
        LogFoodBalance();
    }

    /// <summary>
    /// Reset logging timer
    /// </summary>
    [ContextMenu("Reset Log Timer")]
    public void ResetLogTimer()
    {
        nextLogTime = Time.time + logIntervalGameSeconds;
        Debug.Log("Log timer reset");
    }
}