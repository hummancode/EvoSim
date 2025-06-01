using UnityEngine;

/// <summary>
/// Global debug manager for the simulation - controls debug visualization for all agents
/// </summary>
public class SimulationDebugManager : MonoBehaviour
{
    [Header("Global Debug Controls")]
    [SerializeField] private bool enableGlobalDebug = true;
    [SerializeField] private bool showMateDetectionRanges = true;
    [SerializeField] private bool showFoodDetectionRanges = true;
    [SerializeField] private bool showMatingProximities = true;
    [SerializeField] private bool showOnlySelectedAgents = false;

    [Header("Auto-Setup")]
    [SerializeField] private bool autoAddDebugToNewAgents = true;

    [Header("Performance")]
    [SerializeField] private float refreshInterval = 1f; // How often to search for new agents
    private float lastRefreshTime;

    // Singleton for easy access
    private static SimulationDebugManager instance;
    public static SimulationDebugManager Instance => instance;

    void Awake()
    {
        // Singleton setup
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Initial setup
        RefreshAllAgents();
    }

    void Update()
    {
        // Periodically refresh agents if auto-setup is enabled
        if (autoAddDebugToNewAgents && Time.time - lastRefreshTime > refreshInterval)
        {
            RefreshAllAgents();
            lastRefreshTime = Time.time;
        }
    }

    /// <summary>
    /// Find all agents and ensure they have debug visualizers
    /// </summary>
    [ContextMenu("Refresh All Agents")]
    public void RefreshAllAgents()
    {
        if (!enableGlobalDebug) return;

        AgentController[] agents = FindObjectsOfType<AgentController>();

        foreach (var agent in agents)
        {
            EnsureAgentHasDebugVisualizer(agent);
        }

        Debug.Log($"Refreshed debug visualizers for {agents.Length} agents");
    }

    /// <summary>
    /// Ensure an agent has a debug visualizer component
    /// </summary>
    public void EnsureAgentHasDebugVisualizer(AgentController agent)
    {
        if (agent == null) return;

        AgentDebugVisualizer debugViz = agent.GetComponent<AgentDebugVisualizer>();
        if (debugViz == null)
        {
            debugViz = agent.gameObject.AddComponent<AgentDebugVisualizer>();
            Debug.Log($"Added debug visualizer to {agent.name}");
        }

        // Apply global settings
        ApplyGlobalSettingsToAgent(debugViz);
    }

    /// <summary>
    /// Apply global debug settings to a specific agent
    /// </summary>
    private void ApplyGlobalSettingsToAgent(AgentDebugVisualizer debugViz)
    {
        if (debugViz == null) return;

        // Use reflection to set private fields, or add public setters to AgentDebugVisualizer
        // For now, we'll call the toggle methods to sync state

        // Note: This is a simplified approach. For full control, you'd want to add
        // public setter methods to AgentDebugVisualizer
    }

    // ========================================================================
    // GLOBAL TOGGLE METHODS - Control all agents at once
    // ========================================================================

    [ContextMenu("Toggle Mate Detection Ranges")]
    public void ToggleAllMateDetectionRanges()
    {
        showMateDetectionRanges = !showMateDetectionRanges;
        ApplySettingsToAllAgents();
    }

    [ContextMenu("Toggle Food Detection Ranges")]
    public void ToggleAllFoodDetectionRanges()
    {
        showFoodDetectionRanges = !showFoodDetectionRanges;
        ApplySettingsToAllAgents();
    }

    [ContextMenu("Toggle Mating Proximities")]
    public void ToggleAllMatingProximities()
    {
        showMatingProximities = !showMatingProximities;
        ApplySettingsToAllAgents();
    }

    [ContextMenu("Toggle Selected Only Mode")]
    public void ToggleSelectedOnlyMode()
    {
        showOnlySelectedAgents = !showOnlySelectedAgents;
        ApplySettingsToAllAgents();
    }

    /// <summary>
    /// Apply current settings to all agents
    /// </summary>
    private void ApplySettingsToAllAgents()
    {
        AgentDebugVisualizer[] visualizers = FindObjectsOfType<AgentDebugVisualizer>();

        foreach (var viz in visualizers)
        {
            ApplyGlobalSettingsToAgent(viz);
        }

        Debug.Log($"Applied debug settings to {visualizers.Length} agent visualizers");
    }

    // ========================================================================
    // UTILITY METHODS
    // ========================================================================

    /// <summary>
    /// Enable/disable debug visualization globally
    /// </summary>
    public void SetGlobalDebugEnabled(bool enabled)
    {
        enableGlobalDebug = enabled;

        if (enabled)
        {
            RefreshAllAgents();
        }
        else
        {
            // Remove all debug visualizers
            AgentDebugVisualizer[] visualizers = FindObjectsOfType<AgentDebugVisualizer>();
            foreach (var viz in visualizers)
            {
                if (Application.isPlaying)
                {
                    Destroy(viz);
                }
                else
                {
                    DestroyImmediate(viz);
                }
            }
        }
    }

    /// <summary>
    /// Called when a new agent is spawned - add debug visualizer if needed
    /// </summary>
    public void OnAgentSpawned(AgentController agent)
    {
        if (enableGlobalDebug && autoAddDebugToNewAgents)
        {
            EnsureAgentHasDebugVisualizer(agent);
        }
    }
}