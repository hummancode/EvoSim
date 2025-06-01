using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

/// <summary>
/// Singleton coordinator for all mating processes in the simulation
/// </summary>
public class MatingCoordinator : MonoBehaviour
{
    private static MatingCoordinator instance;

    // Properties
    public static MatingCoordinator Instance
    {
        get
        {
            if (instance == null)
            {
                // Try to find existing instance
                instance = FindObjectOfType<MatingCoordinator>();

                // Create a new one if not found
                if (instance == null)
                {
                    GameObject obj = new GameObject("MatingCoordinator");
                    instance = obj.AddComponent<MatingCoordinator>();
                    DontDestroyOnLoad(obj);
                }
            }
            return instance;
        }
    }

    // Active mating processes
    private List<MatingProcess> activeProcesses = new List<MatingProcess>();

    // Set of agents currently involved in mating
    private HashSet<IAgent> matingAgents = new HashSet<IAgent>();

    private void Awake()
    {
        // Singleton pattern setup
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Register a new mating process between two agents
    /// </summary>
    public bool RegisterMating(IAgent initiator, IAgent partner)
    {
        // Ensure correct initiator order
        if (!EnsureCorrectInitiator(ref initiator, ref partner))
        {
            // Already registered or invalid order
            return false;
        }

        // Check if either agent is already mating
        if (IsAgentMating(initiator) || IsAgentMating(partner))
        {
            Debug.Log("Can't register mating: One or both agents are already mating");
            return false;
        }

        // Create new mating process
        MatingProcess process = new MatingProcess(initiator, partner);
        activeProcesses.Add(process);

        // Add agents to mating set
        matingAgents.Add(initiator);
        matingAgents.Add(partner);

        // Notify agents
        initiator.ReproductionSystem.InitiateMating(partner);
        partner.ReproductionSystem.AcceptMating(initiator);

        // Start mating coroutine
        StartCoroutine(MatingProcessCoroutine(process));

        Debug.Log($"Registered mating between {GetAgentName(initiator)} and {GetAgentName(partner)}");
        return true;
    }

    /// <summary>
    /// Check if an agent is currently mating
    /// </summary>
    public bool IsAgentMating(IAgent agent)
    {
        return matingAgents.Contains(agent);
    }

    /// <summary>
    /// Coroutine to handle mating duration and outcome
    /// </summary>
    // In MatingCoordinator.cs
    /// <summary>
    /// Coroutine to handle mating duration and outcome
    /// </summary>
    private IEnumerator MatingProcessCoroutine(MatingProcess process)
    {
        // Get reproduction config from initiator
        ReproductionConfig config = null;
        if (process.Initiator is AgentAdapter initiatorAdapter && initiatorAdapter.GameObject != null)
        {
            ReproductionSystem reproSystem = initiatorAdapter.GameObject.GetComponent<ReproductionSystem>();
            if (reproSystem != null)
            {
                config = reproSystem.GetConfig();
            }
        }

        // Default duration if config not found
        float matingDuration = config != null ? config.matingDuration : 10f;

        // FIXED: Use scaled time tracking instead of real time
        float startTime = Time.time;  // This respects timeScale
        float targetEndTime = startTime + matingDuration;

        // Frame-based waiting that respects timeScale
        while (Time.time < targetEndTime)
        {
            // Optional: Add validation check here
            if (!process.CanProduceOffspring())
            {
                Debug.Log("Mating interrupted - conditions no longer met");
                break;
            }

            yield return null; // Wait one frame
        }

        // Rest of your code stays the same...
        bool canProduceOffspring = process.CanProduceOffspring();

        if (canProduceOffspring)
        {
            Debug.Log("Mating successful - producing offspring");

            Vector2 variance = config != null ? config.offspringPositionVariance : new Vector2(0.5f, 0.5f);
            Vector3 offspringPosition = process.CalculateOffspringPosition(variance);

            float energyCost = config != null ? config.energyCost : 20f;
            process.Initiator.EnergySystem.ConsumeEnergy(energyCost);
            process.Partner.EnergySystem.ConsumeEnergy(energyCost);

            GameObject initiatorObj = (process.Initiator as AgentAdapter)?.GameObject;
            GameObject partnerObj = (process.Partner as AgentAdapter)?.GameObject;
            AgentSpawner spawner = FindObjectOfType<AgentSpawner>();

            ICommand createOffspringCommand = new CreateOffspringCommand(
                initiatorObj,
                partnerObj,
                offspringPosition,
                spawner,
                Random.Range(1, 4)
            );

            CommandDispatcher.Instance.ExecuteCommand(createOffspringCommand);
        }
        else
        {
            Debug.Log("Mating failed - conditions no longer met");
        }

        // End mating commands
        ICommand endInitiatorMatingCommand = new EndMatingCommand(process.Initiator);
        ICommand endPartnerMatingCommand = new EndMatingCommand(process.Partner);

        CommandDispatcher.Instance.ExecuteCommand(endInitiatorMatingCommand);
        CommandDispatcher.Instance.ExecuteCommand(endPartnerMatingCommand);

        // Remove from tracking
        matingAgents.Remove(process.Initiator);
        matingAgents.Remove(process.Partner);
        activeProcesses.Remove(process);

        process.Complete();
    }
    /// <summary>
    /// Ensures consistent initiator selection
    /// </summary>
    private bool EnsureCorrectInitiator(ref IAgent initiator, ref IAgent partner)
    {
        // Check if already registered with any order
        foreach (var process in activeProcesses)
        {
            if ((process.Initiator == initiator && process.Partner == partner) ||
                (process.Initiator == partner && process.Partner == initiator))
            {
                return false; // Already registered
            }
        }

        // Determine which should be initiator based on ID
        int initiatorID = GetAgentID(initiator);
        int partnerID = GetAgentID(partner);

        if (initiatorID > partnerID)
        {
            // Swap them to ensure consistent initiator
            var temp = initiator;
            initiator = partner;
            partner = temp;
        }

        return true;
    }

    /// <summary>
    /// Gets a unique ID for an agent
    /// </summary>
    private int GetAgentID(IAgent agent)
    {
        if (agent is AgentAdapter adapter && adapter.GameObject != null)
        {
            return adapter.GameObject.GetInstanceID();
        }
        return 0;
    }

    /// <summary>
    /// Gets a readable name for logging
    /// </summary>
    private string GetAgentName(IAgent agent)
    {
        if (agent is AgentAdapter adapter && adapter.GameObject != null)
        {
            return adapter.GameObject.name;
        }
        return "Unknown Agent";
    }
}