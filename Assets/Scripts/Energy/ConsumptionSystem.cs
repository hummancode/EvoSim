using UnityEngine;

public class ConsumptionSystem : MonoBehaviour
{
    [SerializeField] private EnergySystem energySystem;
    [SerializeField] private float detectionRadius = 0.6f; // Manual detection radius

    void Awake()
    {
        if (energySystem == null)
            energySystem = GetComponent<EnergySystem>();
    }

    void Update()
    {
        // BACKUP: Manual collision detection for high speeds
        // This runs every frame and is more reliable than OnTriggerEnter at high timescales
        CheckForFoodInRadius();
    }

    /// <summary>
    /// Manual food detection that works reliably at high timescales
    /// </summary>
    private void CheckForFoodInRadius()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius);

        foreach (var collider in colliders)
        {
            if (collider.TryGetComponent<IEdible>(out var food))
            {
                // Consume the food
                energySystem.AddEnergy(food.NutritionalValue);
                food.Consume();

                Debug.Log($"Agent consumed food (manual detection) and gained {food.NutritionalValue} energy");
                break; // Only consume one food per frame
            }
        }
    }

    // Keep the original trigger method as backup
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<IEdible>(out var food))
        {
            energySystem.AddEnergy(food.NutritionalValue);
            food.Consume();
            Debug.Log("Agent consumed food (trigger) and gained " + food.NutritionalValue + " energy");
        }
    }
}