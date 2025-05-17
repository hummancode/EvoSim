using UnityEngine;

public class FoodSpawner : MonoBehaviour
{
    [Header("Prefab Settings")]
    [SerializeField] private GameObject foodPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private int initialFoodCount = 150;
    [SerializeField] private float spawnInterval = 2.0f;
    [SerializeField] private Vector2 worldBounds = new Vector2(17f, 9.5f);

    private float lastSpawnTime;

    void Start()
    {
        // Spawn initial food
        SpawnInitialFood();
    }

    void Update()
    {
        // Periodically spawn more food
        if (Time.time - lastSpawnTime > spawnInterval)
        {
            SpawnFood();
            lastSpawnTime = Time.time;
        }
    }

    private void SpawnInitialFood()
    {
        for (int i = 0; i < initialFoodCount; i++)
        {
            SpawnFood();
        }

        Debug.Log($"Spawned {initialFoodCount} food items");
    }

    private void SpawnFood()
    {
        // Get random position within world bounds
        Vector3 position = GetRandomPosition();

        // Spawn the food
        Instantiate(foodPrefab, position, Quaternion.identity);

        // Update last spawn time
        lastSpawnTime = Time.time;
    }

    private Vector3 GetRandomPosition()
    {
        return new Vector3(
            Random.Range(-worldBounds.x, worldBounds.x),
            Random.Range(-worldBounds.y, worldBounds.y),
            0
        );
    }
}