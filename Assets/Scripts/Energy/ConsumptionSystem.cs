using UnityEngine;

public class ConsumptionSystem : MonoBehaviour
{
    [SerializeField] private EnergySystem energySystem;
    [SerializeField] private float detectionRadius = 0.6f;

    // HIGH-SPEED FIX: Simple adaptive settings
    private float lastConsumptionCheck = 0f;

    void Awake()
    {
        if (energySystem == null)
            energySystem = GetComponent<EnergySystem>();
    }

    void Update()
    {
        // HIGH-SPEED FIX: Much more frequent checks at high speeds
        float checkInterval = GetCheckInterval();

        if (Time.time - lastConsumptionCheck > checkInterval)
        {
            CheckForFoodInRadius();
            lastConsumptionCheck = Time.time;
        }
    }

    /// <summary>
    /// HIGH-SPEED FIX: Adaptive check interval based on time scale
    /// </summary>
    private float GetCheckInterval()
    {
        float timeScale = Time.timeScale;

        if (timeScale > 75f)
            return 0.005f;  // Check every 0.005 game seconds at extreme speeds (200 Hz)
        else if (timeScale > 30f)
            return 0.01f;   // Check every 0.01 game seconds at high speeds (100 Hz)
        else if (timeScale > 10f)
            return 0.02f;   // Check every 0.02 game seconds at moderate speeds (50 Hz)
        else
            return 0.05f;   // Check every 0.05 game seconds at normal speeds (20 Hz)
    }

    /// <summary>
    /// HIGH-SPEED FIX: Adaptive detection radius based on time scale
    /// </summary>
    private float GetDetectionRadius()
    {
        float timeScale = Time.timeScale;

        if (timeScale > 75f)
            return detectionRadius * 4f;   // 4x radius at extreme speeds
        else if (timeScale > 30f)
            return detectionRadius * 2.5f; // 2.5x radius at high speeds
        else if (timeScale > 10f)
            return detectionRadius * 1.5f; // 1.5x radius at moderate speeds
        else
            return detectionRadius;         // Normal radius
    }

    /// <summary>
    /// HIGH-SPEED FIX: Reliable food detection that works at all speeds
    /// </summary>
    private void CheckForFoodInRadius()
    {
        float currentRadius = GetDetectionRadius();
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, currentRadius);

        foreach (var collider in colliders)
        {
            if (collider.TryGetComponent<IEdible>(out var food))
            {
                // Consume the food
                energySystem.AddEnergy(food.NutritionalValue);
                food.Consume();

                // HIGH-SPEED FIX: Log consumption at high speeds for debugging
                if (Time.timeScale > 30f && Debug.isDebugBuild)
                {
                    Debug.Log($"HIGH-SPEED FOOD: {gameObject.name} consumed food " +
                             $"(+{food.NutritionalValue} energy) at {Time.timeScale:F0}x speed, radius: {currentRadius:F1}");
                }

                break; // Only consume one food per check
            }
        }
    }

    // Keep the original trigger method as backup for normal speeds
    void OnTriggerEnter2D(Collider2D other)
    {
        // Only use trigger at normal speeds - at high speeds, the Update method handles it
        if (Time.timeScale < 10f && other.TryGetComponent<IEdible>(out var food))
        {
            energySystem.AddEnergy(food.NutritionalValue);
            food.Consume();

            if (Debug.isDebugBuild)
            {
                Debug.Log($"TRIGGER FOOD: {gameObject.name} consumed food via trigger (+{food.NutritionalValue} energy)");
            }
        }
    }

    /// <summary>
    /// HIGH-SPEED FIX: Visualization for debugging
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        float currentRadius = GetDetectionRadius();
        float timeScale = Time.timeScale;

        // Color based on speed
        if (timeScale > 75f)
            Gizmos.color = Color.red;
        else if (timeScale > 30f)
            Gizmos.color = Color.yellow;
        else
            Gizmos.color = Color.green;

        Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.3f);
        Gizmos.DrawSphere(transform.position, currentRadius);

        Gizmos.color = new Color(Gizmos.color.r, Gizmos.color.g, Gizmos.color.b, 0.8f);
        Gizmos.DrawWireSphere(transform.position, currentRadius);
    }
}