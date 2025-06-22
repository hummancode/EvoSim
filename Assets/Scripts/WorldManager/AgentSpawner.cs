using UnityEngine;

public class AgentSpawner : MonoBehaviour
{
    [Header("Prefab Settings")]
    [SerializeField] private GameObject agentPrefab;
    [SerializeField] private GameObject backupAgentPrefab; // Backup prefab

    [Header("Spawn Settings")]
    [SerializeField] private int initialAgentCount = 10;
    [SerializeField] private Vector2 worldBounds = new Vector2(27f, 19.5f);

    [Header("Age Configuration")]
    [SerializeField] private AgentLifespanConfig lifespanConfig;

    [Header("Statistics")]
    [SerializeField] private int totalAgentsBorn = 0;
    [SerializeField] private int totalAgentsDied = 0;
    [SerializeField] private int highestGeneration = 1;
    [Header("Organization")] // NEW - Just add this
    [SerializeField] private Transform agentsFolder; // Drag folder here!
    // Properties
    public int TotalAgentsBorn => totalAgentsBorn;
    public int TotalAgentsDied => totalAgentsDied;
    public int HighestGeneration => highestGeneration;

    void Start()
    {
        Debug.Log("[AGENT] Test agent message");
        Debug.Log("[FOOD] Test food message");
        Debug.LogWarning("[REPRODUCTION] Test warning");
        //Debug.LogError("[SPAWNING] Test error");
        if (agentsFolder == null)
        {
            agentsFolder = CreateAgentsFolder();
        }

        // CRITICAL: Validate prefab before starting
        if (!ValidateAgentPrefab())
        {
            Debug.LogError("AgentSpawner: No valid agent prefab available! Cannot spawn agents.");
            return;
        }

        if (lifespanConfig == null)
        {
            Debug.LogWarning("AgentLifespanConfig not assigned to AgentSpawner! Using default values.");
        }

        SpawnInitialAgents();
    }
    private Transform CreateAgentsFolder()
    {
        GameObject folder = new GameObject("🤖 Agents");
        return folder.transform;
    }
    /// <summary>
    /// CRITICAL: Validates that we have a working agent prefab
    /// </summary>
    private bool ValidateAgentPrefab()
    {
        // Check primary prefab
        if (agentPrefab != null)
        {
            Debug.Log($"Using primary agent prefab: {agentPrefab.name}");
            return true;
        }

        // Try backup prefab
        if (backupAgentPrefab != null)
        {
            Debug.LogWarning("Primary prefab is null, using backup agent prefab");
            agentPrefab = backupAgentPrefab;
            return true;
        }

        // Try to find a prefab in Resources
        GameObject resourcesPrefab = Resources.Load<GameObject>("AgentPrefab");
        if (resourcesPrefab != null)
        {
            Debug.LogWarning("Using agent prefab from Resources folder");
            agentPrefab = resourcesPrefab;
            return true;
        }

        // Last resort: try to find any AgentController in the scene as a template
        AgentController existingAgent = FindObjectOfType<AgentController>();
        if (existingAgent != null)
        {
            Debug.LogWarning("Creating prefab from existing agent in scene");
            agentPrefab = existingAgent.gameObject;
            return true;
        }

        return false;
    }

    private void SpawnInitialAgents()
    {
        for (int i = 0; i < initialAgentCount; i++)
        {
            GameObject agent = SpawnAgent();
            if (agent != null)
            {
                ConfigureInitialAgentAge(agent);
            }
            else
            {
                Debug.LogError($"Failed to spawn initial agent {i}");
            }
        }

        totalAgentsBorn = initialAgentCount;
        Debug.Log($"[AGENT] Spawned {initialAgentCount} initial agents");
    }

    private void ConfigureInitialAgentAge(GameObject agent)
    {
        if (lifespanConfig == null || agent == null) return;

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
    /// FIXED: Spawns a new agent with comprehensive error handling
    /// </summary>
    public GameObject SpawnAgent()
    {
        Vector3 position = GetRandomPosition();
        return SpawnAgentAt(position);
    }

    /// <summary>
    /// FIXED: Spawns a new agent at the specified position with error handling
    /// </summary>
    public GameObject SpawnAgentAt(Vector3 position)
    {
        if (agentPrefab == null)
        {
            Debug.LogError("Cannot spawn agent: agentPrefab is null!");
            if (!ValidateAgentPrefab())
            {
                return null;
            }
        }

        try
        {
            // SIMPLE CHANGE - Just add the parent parameter!
            //GameObject agent = Instantiate(agentPrefab, position, Quaternion.identity, agentsFolder);
            GameObject agent = AgentControllerVisualIntegration.SpawnAgentWithVisuals(agentPrefab, position, Quaternion.identity);

            if (agent == null)
            {
                Debug.LogError("Instantiate returned null for agent prefab!");
                return null;
            }

            // Your existing code stays the same...
            totalAgentsBorn++;

            if (StatisticsManager.Instance != null)
            {
                StatisticsManager.Instance.ReportAgentBorn();
            }

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
        catch (System.Exception e)
        {
            Debug.LogError($"Exception while spawning agent: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// FIXED: Spawns offspring with comprehensive error handling
    /// </summary>
    public GameObject SpawnOffspring(GameObject parent1, GameObject parent2, Vector3 position)
    {
        //Debug.Log($"SpawnOffspring called with parents: {parent1?.name}, {parent2?.name}");

        // CRITICAL: Validate inputs
        if (parent1 == null)
        {
            Debug.LogError("Parent1 is null in SpawnOffspring - cannot proceed");
            return null;
        }

        // Validate that parent1 still exists
        try
        {
            string testName = parent1.name; // This will throw if destroyed
        }
        catch (MissingReferenceException)
        {
            Debug.LogError("Parent1 has been destroyed during mating process");
            return null;
        }

        // Create the new agent with error handling
        GameObject offspring = SpawnAgentAt(position);
        if (offspring == null)
        {
            Debug.LogError("Failed to spawn offspring - SpawnAgentAt returned null");
            return null;
        }

        // Get components safely - FIXED: Declare agentController variable properly
        AgentController parent1Agent = GetComponentSafely<AgentController>(parent1);
        AgentController parent2Agent = parent2 != null ? GetComponentSafely<AgentController>(parent2) : null;
        AgentController offspringAgent = GetComponentSafely<AgentController>(offspring);

        if (parent1Agent == null)
        {
            Debug.LogError("Parent1 does not have AgentController component");
            return offspring; // Return the offspring anyway, just won't have inheritance
        }

        if (offspringAgent == null)
        {
            Debug.LogError("Spawned offspring does not have AgentController component");
            return offspring;
        }

        try
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

            // Inherit genetic traits safely
            InheritGenetics(parent1Agent, parent2Agent, offspringAgent);

            // Setup age integration
            SimpleAgeIntegration.SetupOffspringAge(offspring, parent1, parent2);

            Debug.Log($" [AGENT] Successfully spawned offspring from {parent1.name} and {(parent2 != null ? parent2.name : "unknown")}");
            return offspring;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error during offspring setup: {e.Message}\n{e.StackTrace}");
            return offspring; // Return offspring even if inheritance failed
        }
    }

    /// <summary>
    /// Safe component getter that handles destroyed objects
    /// </summary>
    private T GetComponentSafely<T>(GameObject obj) where T : Component
    {
        if (obj == null) return null;

        try
        {
            return obj.GetComponent<T>();
        }
        catch (MissingReferenceException)
        {
            Debug.LogWarning($"Cannot get component {typeof(T).Name} - GameObject has been destroyed");
            return null;
        }
    }

    /// <summary>
    /// Handle genetic inheritance safely
    /// </summary>
    private void InheritGenetics(AgentController parent1Agent, AgentController parent2Agent, AgentController offspringAgent)
    {
        try
        {
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
                        deathAge = lifespanConfig.GetRandomMaxAge();
                        pubertyAge = lifespanConfig.GetRandomMaturityAge();
                    }

                    ageSystem.SetAgeValues(deathAge, pubertyAge);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error during genetic inheritance: {e.Message}");
        }
    }

    /// <summary>
    /// Handles agent death reporting
    /// </summary>
    public void HandleAgentDeath(GameObject agent, string cause)
    {
        totalAgentsDied++;

        // Get agent info safely
        AgentController agentController = GetComponentSafely<AgentController>(agent);
        int generation = agentController != null ? agentController.Generation : 1;

        Debug.Log($"Agent {agent?.name ?? "unknown"} (Gen {generation}) died from {cause}");
    }

    private Vector3 GetRandomPosition()
    {
        return new Vector3(
            Random.Range(-worldBounds.x, worldBounds.x),
            Random.Range(-worldBounds.y, worldBounds.y),
            0
        );
    }

    public Vector2 GetWorldBounds()
    {
        return worldBounds;
    }

    public void SetWorldBounds(Vector2 bounds)
    {
        worldBounds = bounds;
    }

    /// <summary>
    /// Emergency method to set a new agent prefab at runtime
    /// </summary>
    public void SetAgentPrefab(GameObject newPrefab)
    {
        if (newPrefab != null)
        {
            agentPrefab = newPrefab;
            Debug.Log($"Agent prefab changed to: {newPrefab.name}");
        }
    }

    /// <summary>
    /// Debug method to validate current state
    /// </summary>
    [ContextMenu("Validate Agent Spawner")]
    public void ValidateSpawner()
    {
        Debug.Log("=== AGENT SPAWNER VALIDATION ===");
        Debug.Log($"Agent Prefab: {(agentPrefab != null ? agentPrefab.name : "NULL")}");
        Debug.Log($"Backup Prefab: {(backupAgentPrefab != null ? backupAgentPrefab.name : "NULL")}");
        Debug.Log($"Lifespan Config: {(lifespanConfig != null ? "OK" : "NULL")}");
        Debug.Log($"Total Born: {totalAgentsBorn}");
        Debug.Log($"Total Died: {totalAgentsDied}");
        Debug.Log($"Highest Generation: {highestGeneration}");

        bool canSpawn = ValidateAgentPrefab();
        Debug.Log($"Can Spawn: {canSpawn}");
    }
}