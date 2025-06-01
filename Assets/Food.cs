using UnityEngine;

// Marker interface for anything that can be eaten
public interface IEdible
{
    // Only nutrient value property is required
    float NutritionalValue { get; }
    public void Consume();

}

// Food class implementation
public class Food : MonoBehaviour, IEdible
{
    [Header("Food Properties")]
    [SerializeField] private float nutritionalValue = 20.0f;
    [SerializeField] private float lifespanSeconds = 230.0f;

    private float spawnTime;

    // Property implementation from IEdible interface
    public float NutritionalValue => nutritionalValue;
    void Awake()
    {
        // Set the food object to the Food layer
        gameObject.layer = LayerMask.NameToLayer("Food");
    }
    void Start()
    {
        spawnTime = Time.time;

        // Assumes collider is already set up in the prefab
        // No need to generate one here
    }

    void Update()
    {
        // Food can expire after some time
        if (Time.time - spawnTime > lifespanSeconds)
        {
            Decay();
        }
    }

    public void Consume()
    {
        // Simple consumption - just destroy the food
        Destroy(gameObject);
    }

    private void Decay()
    {
        // Simple decay - just destroy the food
        Destroy(gameObject);
    }
}