using UnityEngine;

public class AgentSpawner : MonoBehaviour
{
    [Header("Prefab Settings")]
    [SerializeField] private GameObject agentPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private int initialAgentCount = 10;
    [SerializeField] private Vector2 worldBounds = new Vector2(27f, 19.5f);

    // ADD THIS - Direct reference to config
    [Header("Age Configuration")]
    [SerializeField] private AgentLifespanConfig lifespanConfig;

    // Your existing fields...
    [Header("Statistics")]
    [SerializeField] private int totalAgentsBorn = 0;
    [SerializeField] private int totalAgentsDied = 0;
    [SerializeField] private int highestGeneration = 1;

    // Properties remain the same...
    public int TotalAgentsBorn => totalAgentsBorn;
    public int TotalAgentsDied => totalAgentsDied;
    public int HighestGeneration => highestGeneration;

    void Start()
    {
        // Validate config
        if (lifespanConfig == null)
        {
            Debug.LogError("AgentLifespanConfig not assigned to AgentSpawner! Using default values.");
        }

        SpawnInitialAgents();
    }

    private void SpawnInitialAgents()
    {
        for (int i = 0; i < initialAgentCount; i++)
        {
            GameObject agent = SpawnAgent();

            // UPDATED - Configure initial age
            ConfigureInitialAgentAge(agent);
        }

        totalAgentsBorn = initialAgentCount;
        Debug.Log($"Spawned {initialAgentCount} agents");
    }

    /// <summary>
    /// Configure age for newly spawned initial agents
    /// </summary>
    private void ConfigureInitialAgentAge(GameObject agent)
    {
        if (lifespanConfig == null) return;

        var ageSystem = agent.GetComponent<AgeSystem>();
        if (ageSystem != null)
        {
            float maxAge = lifespanConfig.GetRandomMaxAge();
            float maturityAge = lifespanConfig.GetRandomMaturityAge();

            ageSystem.SetAgeValues(maxAge, maturityAge);

            Debug.Log($"Initial agent {agent.name}: maxAge={maxAge:F1}, maturityAge={maturityAge:F1}");
        }
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
        StatisticsManager.Instance.ReportAgentBorn();
        if (SimulationDebugManager.Instance != null)
        {
            var agentController = agent.GetComponent<AgentController>();
            if (agentController != null)
            {
                SimulationDebugManager.Instance.OnAgentSpawned(agentController);
            }
        }
        return agent;
    }

    /// <summary>
    /// Spawns an offspring from two parent agents
    /// </summary>
    public GameObject SpawnOffspring(GameObject parent1, GameObject parent2, Vector3 position)
    {
        Debug.Log($"SpawnOffspring called with parents: {parent1?.name}, {parent2?.name}");

        if (parent1 == null)
        {
            Debug.LogError("Parent1 is null in SpawnOffspring");
            return null;
        }

        // Create the new agent
        GameObject offspring = SpawnAgentAt(position);

        // Get components
        AgentController parent1Agent = parent1.GetComponent<AgentController>();
        AgentController parent2Agent = parent2?.GetComponent<AgentController>();
        AgentController offspringAgent = offspring.GetComponent<AgentController>();

        if (parent1Agent != null && offspringAgent != null)
        {
            // Set generation
            int parentGen = parent1Agent.Generation;
            int parent2Gen = parent2Agent?.Generation ?? parentGen;
            int newGeneration = Mathf.Max(parentGen, parent2Gen) + 1;

            offspringAgent.Generation = newGeneration;
            Debug.Log($"Set offspring generation to {newGeneration}");

            // Update highest generation stat
            if (newGeneration > highestGeneration)
            {
                highestGeneration = newGeneration;
            }

            // Inherit genetic traits
            GeneticsSystem offspringGenetics = offspringAgent.GetGeneticsSystem();
            GeneticsSystem parent1Genetics = parent1Agent.GetGeneticsSystem();
            GeneticsSystem parent2Genetics = parent2Agent?.GetGeneticsSystem();

            if (offspringGenetics != null && parent1Genetics != null)
            {
                offspringGenetics.InheritFrom(parent1Genetics, parent2Genetics);

                // Update age system from genetics
                AgeSystem ageSystem = offspringAgent.GetAgeSystem();
                if (ageSystem != null)
                    
                {   
                    float deathAge = offspringGenetics.GetTraitValue(Genome.DEATH_AGE, 140f);
                    float pubertyAge = offspringGenetics.GetTraitValue(Genome.PUBERTY_AGE, 20f);
                    if (lifespanConfig != null)
                    {
                        deathAge =lifespanConfig.GetRandomMaxAge();
                        pubertyAge = lifespanConfig.GetRandomMaturityAge();
                    }
                    ageSystem.SetAgeValues(deathAge, pubertyAge);
                }
            }
        }
        SimpleAgeIntegration.SetupOffspringAge(offspring, parent1, parent2);
        Debug.Log($"Spawned offspring from {parent1.name} and {(parent2 != null ? parent2.name : "unknown")}");
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