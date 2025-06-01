using UnityEngine;

public class DeathSystem : MonoBehaviour
{
    [SerializeField] private EnergySystem energySystem;

    // Events
    public System.Action<string> OnDeath;

    void Awake()
    {
        // Get reference if not set
        if (energySystem == null)
            energySystem = GetComponent<EnergySystem>();

        // Subscribe to energy system events
        energySystem.OnDeath += HandleEnergyDepletion;
    }

    void OnDestroy()
    {
        // Unsubscribe
        if (energySystem != null)
            energySystem.OnDeath -= HandleEnergyDepletion;
    }

    private void HandleEnergyDepletion()
    {
        Die("starvation");
    }

    public void Die(string cause)
    {
        Debug.Log($"Agent died from {cause}");
        float age = 0f;
        int generation = 1;

        AgeSystem ageSystem = GetComponent<AgeSystem>();
        if (ageSystem != null)
            age = ageSystem.Age;

        AgentController agentController = GetComponent<AgentController>();
        if (agentController != null)
            generation = agentController.Generation;

        StatisticsManager.Instance.ReportAgentDied(age, cause, generation);
        // Trigger event
        OnDeath?.Invoke(cause);

        // Destroy the agent
        Destroy(gameObject);
    }
}