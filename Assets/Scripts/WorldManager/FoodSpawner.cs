// ============================================================================
// FOOD SPAWNER - Just add folder reference
// ============================================================================

using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [Header("Prefab Settings")]
    [SerializeField] private GameObject foodPrefab;

    [Header("Organization")] // NEW - Just add this section
    [SerializeField] private Transform foodFolder; // Drag folder here in inspector!

    [Header("Spawn Settings")]
    [SerializeField] private int initialFoodCount = 150;
    [SerializeField] private float foodPerSecond = 0.5f;
    [SerializeField] private Vector2 worldBounds = new Vector2(17f, 9.5f);

    // Your existing fields...
    private float spawnAccumulator = 0f;
    private float lastUpdateTime = 0f;

    void Start()
    {
        // NEW - Create folder if not assigned
        if (foodFolder == null)
        {
            foodFolder = CreateFoodFolder();
        }

        SpawnInitialFood();
        lastUpdateTime = Time.time;
    }

    // NEW - Simple folder creation
    private Transform CreateFoodFolder()
    {
        GameObject folder = new GameObject("?? Food");
        return folder.transform;
    }

    // Your existing Update method stays the same...
    void Update()
    {
        float currentTime = Time.time;
        float deltaTime = currentTime - lastUpdateTime;
        lastUpdateTime = currentTime;

        spawnAccumulator += foodPerSecond * deltaTime;

        while (spawnAccumulator >= 1f)
        {
            SpawnFood();
            spawnAccumulator -= 1f;
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
        //FoodSpawnerVisualIntegration.SpawnFoodWithVisuals(foodPrefab, position, Quaternion.identity);
        // SIMPLE CHANGE - Just add the parent parameter!
        Instantiate(foodPrefab, position, Quaternion.identity, foodFolder);

        // Your existing code stays the same
        int currentFoodCount = FindObjectsOfType<Food>().Length;
        if (StatisticsManager.Instance != null)
        {
            StatisticsManager.Instance.ReportFoodCount(currentFoodCount);
        }
    }

    // Rest of your methods stay exactly the same...
    private Vector3 GetRandomPosition()
    {
        return new Vector3(
            Random.Range(-worldBounds.x, worldBounds.x),
            Random.Range(-worldBounds.y, worldBounds.y),
            0
        );
    }

    // Your existing methods...
    public float GetCurrentSpawnRate() => foodPerSecond;
    public float GetTheoreticalFoodCount() => 230f * foodPerSecond;
    public void SetSpawnRate(float newFoodPerSecond) => foodPerSecond = Mathf.Max(0f, newFoodPerSecond);
}
