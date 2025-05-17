using UnityEngine;

public class EnergySystem : MonoBehaviour, IEnergyProvider
{
    [Header("Energy Settings")]
    [SerializeField] private float currentEnergy = 100f;
    [SerializeField] private float maxEnergy = 180f;
    [SerializeField] private float energyConsumptionRate = 2f; // Energy consumed per second
    [SerializeField] private float hungerThreshold = 30f; // Energy level considered "hungry"
    [SerializeField] private float matingEnergyThreshold = 60f;

    // Properties
    public bool IsHungry => currentEnergy < hungerThreshold;
    public bool HasEnoughEnergyForMating => currentEnergy > matingEnergyThreshold;

    public bool IsDead => currentEnergy <= 0f;
    public float EnergyPercent => currentEnergy / maxEnergy;

    // Event for death
    public System.Action OnDeath;

    void Update()
    {
        // Consume energy over time
        ConsumeEnergy(energyConsumptionRate * Time.deltaTime);
    }

    public void ConsumeEnergy(float amount)
    {
        currentEnergy -= amount;

        // Check for death
        if (currentEnergy <= 0f)
        {
            currentEnergy = 0f;
            Die();
        }
    }

    public void AddEnergy(float amount)
    {
        // Add energy but don't exceed maximum
        currentEnergy = Mathf.Min(currentEnergy + amount, maxEnergy);
    }

    private void Die()
    {
        // Trigger death event
        OnDeath?.Invoke();

        // Let the agent controller handle actual destruction
    }

    // Methods to configure energy system based on traits
    public void SetMaxEnergy(float value)
    {
        maxEnergy = value;
        currentEnergy = Mathf.Min(currentEnergy, maxEnergy);
    }

    public void SetConsumptionRate(float value)
    {
        energyConsumptionRate = value;
    }
}