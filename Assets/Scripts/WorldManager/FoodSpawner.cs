using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [Header("Prefab Settings")]
    [SerializeField] private GameObject foodPrefab;

    [Header("Spawn Settings - Rate Based")]
    [SerializeField] private int initialFoodCount = 150;
    [SerializeField] private float foodPerSecond = 0.5f; // 0.5 food per second = 1 food every 2 seconds
    [SerializeField] private Vector2 worldBounds = new Vector2(17f, 9.5f);

    // Rate-based accumulator
    private float spawnAccumulator = 0f;
    private float lastUpdateTime = 0f;

    void Start()
    {
        SpawnInitialFood();
        lastUpdateTime = Time.time;
    }

    void Update()
    {
        // Calculate time delta since last update
        float currentTime = Time.time;
        float deltaTime = currentTime - lastUpdateTime;
        lastUpdateTime = currentTime;

        // Accumulate spawn "debt" based on food per second rate
        spawnAccumulator += foodPerSecond * deltaTime;

        // Spawn food for each accumulated unit
        while (spawnAccumulator >= 1f)
        {
            SpawnFood();
            spawnAccumulator -= 1f;

            // Debug log every spawn
            //Debug.Log($"SPAWN: Time={Time.time:F2}, Rate={foodPerSecond}/s, Speed={Time.timeScale:F1}x, Accumulator={spawnAccumulator:F3}");
        }
    }

    private void SpawnInitialFood()
    {
        for (int i = 0; i < initialFoodCount; i++)
        {
            SpawnFood();
        }
        Debug.Log($"Spawned {initialFoodCount} initial food items");
    }

    private void SpawnFood()
    {
        Vector3 position = GetRandomPosition();
        Instantiate(foodPrefab, position, Quaternion.identity);

        // Optional: Report food count
        int currentFoodCount = FindObjectsOfType<Food>().Length;
        if (StatisticsManager.Instance != null)
        {
            StatisticsManager.Instance.ReportFoodCount(currentFoodCount);
        }
    }

    private Vector3 GetRandomPosition()
    {
        return new Vector3(
            Random.Range(-worldBounds.x, worldBounds.x),
            Random.Range(-worldBounds.y, worldBounds.y),
            0
        );
    }

    /// <summary>
    /// Get current effective spawn rate (for debugging)
    /// </summary>
    public float GetCurrentSpawnRate()
    {
        return foodPerSecond;
    }

    /// <summary>
    /// Calculate theoretical steady-state food count
    /// </summary>
    public float GetTheoreticalFoodCount()
    {
        // With current food lifespan of 230 seconds and rate of foodPerSecond:
        // Steady state = lifespan * spawn_rate = 230 * foodPerSecond
        return 230f * foodPerSecond;
    }

    /// <summary>
    /// Set new spawn rate at runtime
    /// </summary>
    public void SetSpawnRate(float newFoodPerSecond)
    {
        foodPerSecond = Mathf.Max(0f, newFoodPerSecond);
        Debug.Log($"Food spawn rate changed to {foodPerSecond} food/second");
    }
}