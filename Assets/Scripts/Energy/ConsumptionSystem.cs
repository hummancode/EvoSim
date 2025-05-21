using UnityEngine;

public class ConsumptionSystem : MonoBehaviour
{
    [SerializeField] private EnergySystem energySystem;

    void Awake()
    {
        Debug.Log("Consume init.");
        // Get reference if not set
        if (energySystem == null)
            energySystem = GetComponent<EnergySystem>();
    }

    // Add Initialize method
    public void Initialize(EnergySystem energy)
    {
        energySystem = energy;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("On trigger collide");
        // Check if it's food
        if (other.TryGetComponent<IEdible>(out var food))
        {
            // Add energy from food
            energySystem.AddEnergy(food.NutritionalValue);

            // Consume the food (destroys it)
            food.Consume();

            Debug.Log("Agent consumed food and gained " + food.NutritionalValue + " energy");
        }
    }
}