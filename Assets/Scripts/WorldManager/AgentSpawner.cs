using UnityEngine;

public class AgentSpawner : MonoBehaviour
{
    [Header("Prefab Settings")]
    [SerializeField] private GameObject agentPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private int initialAgentCount = 10;
    [SerializeField] private Vector2 worldBounds = new Vector2(17f, 9.5f);

    [Header("Offspring Settings")]
    [SerializeField] private bool enableMutation = true;
    [SerializeField] private float mutationRate = 0.1f;
    [SerializeField] private float mutationAmount = 0.2f;

    [Header("Statistics")]
    [SerializeField] private int totalAgentsBorn = 0;
    [SerializeField] private int totalAgentsDied = 0;
    [SerializeField] private int highestGeneration = 1;

    // Add properties to expose statistics
    public int TotalAgentsBorn => totalAgentsBorn;
    public int TotalAgentsDied => totalAgentsDied;
    public int HighestGeneration => highestGeneration;

    void Start()
    {
        SpawnInitialAgents();
    }

    private void SpawnInitialAgents()
    {
        for (int i = 0; i < initialAgentCount; i++)
        {
            SpawnAgent();
        }

        totalAgentsBorn = initialAgentCount;
        Debug.Log($"Spawned {initialAgentCount} agents");
    }

    /// <summary>
    /// Spawns a new agent at a random position
    /// </summary>
    public GameObject SpawnAgent()
    {
        Vector3 position = GetRandomPosition();
        return SpawnAgentAt(position);
    }

    /// <summary>
    /// Spawns a new agent at the specified position
    /// </summary>
    public GameObject SpawnAgentAt(Vector3 position)
    {
        GameObject agent = Instantiate(agentPrefab, position, Quaternion.identity);
        totalAgentsBorn++;

        return agent;
    }

    /// <summary>
    /// Spawns an offspring from two parent agents
    /// </summary>
    public GameObject SpawnOffspring(GameObject parent1, GameObject parent2, Vector3 position)
    {
        if (parent1 == null || parent2 == null)
        {
            Debug.LogWarning("Cannot spawn offspring: One or both parents are null");
            return null;
        }

        // Create the new agent
        GameObject offspring = SpawnAgentAt(position);

        // Get components
        AgentController parent1Agent = parent1.GetComponent<AgentController>();
        AgentController parent2Agent = parent2.GetComponent<AgentController>();
        AgentController offspringAgent = offspring.GetComponent<AgentController>();

        if (parent1Agent != null && parent2Agent != null && offspringAgent != null)
        {
            // Set generation
            int newGeneration = Mathf.Max(parent1Agent.Generation, parent2Agent.Generation) + 1;
            offspringAgent.Generation = newGeneration;

            // Update highest generation stat
            if (newGeneration > highestGeneration)
            {
                highestGeneration = newGeneration;
            }

            // If you have genome components, inherit traits
            InheritTraits(parent1, parent2, offspring);
        }

        Debug.Log($"Spawned offspring (Gen {offspringAgent.Generation}) from {parent1.name} and {parent2.name}");
        return offspring;
    }

    /// <summary>
    /// Handles agent death reporting
    /// </summary>
    public void HandleAgentDeath(GameObject agent, string cause)
    {
        totalAgentsDied++;

        // Get agent info
        AgentController agentController = agent.GetComponent<AgentController>();
        int generation = agentController != null ? agentController.Generation : 1;

        // Log death
        Debug.Log($"Agent {agent.name} (Gen {generation}) died from {cause}");

        // You might want to report this to a statistics manager
        // StatisticsManager.Instance.RecordDeath(cause);
    }

    /// <summary>
    /// Inherits traits from parents to offspring 
    /// (Implement this when you have a genome system)
    /// </summary>
    private void InheritTraits(GameObject parent1, GameObject parent2, GameObject offspring)
    {
        // This is a placeholder - replace with your actual trait inheritance system
        // Example:
        /*
        GenomeComponent parent1Genome = parent1.GetComponent<GenomeComponent>();
        GenomeComponent parent2Genome = parent2.GetComponent<GenomeComponent>();
        GenomeComponent offspringGenome = offspring.GetComponent<GenomeComponent>();
        
        if (parent1Genome != null && parent2Genome != null && offspringGenome != null)
        {
            // Inherit traits from parents with potential mutation
            offspringGenome.InheritFrom(parent1Genome, parent2Genome, enableMutation, mutationRate, mutationAmount);
        }
        */
    }

    /// <summary>
    /// Gets a random position within world bounds
    /// </summary>
    private Vector3 GetRandomPosition()
    {
        return new Vector3(
            Random.Range(-worldBounds.x, worldBounds.x),
            Random.Range(-worldBounds.y, worldBounds.y),
            0
        );
    }

    /// <summary>
    /// Gets world bounds
    /// </summary>
    public Vector2 GetWorldBounds()
    {
        return worldBounds;
    }

    /// <summary>
    /// Set world bounds
    /// </summary>
    public void SetWorldBounds(Vector2 bounds)
    {
        worldBounds = bounds;
    }
}