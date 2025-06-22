using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

/// <summary>
/// Singleton coordinator for all mating processes in the simulation
/// FIXED: High-speed reliability improvements
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

    // HIGH-SPEED FIX: Add proximity-based instant mating for extreme speeds
    [Header("High-Speed Settings")]
    [SerializeField] private float emergencyMatingDistance = 2.5f;
    [SerializeField] private bool enableEmergencyMating = true;
    [SerializeField] private bool debugHighSpeed = false;

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
    /// FIXED: High-speed reliability improvements
    /// </summary>
    public bool RegisterMating(IAgent initiator, IAgent partner)
    {
        // HIGH-SPEED FIX: Emergency mating for very high speeds
        if (Time.timeScale > 75f && enableEmergencyMating)
        {
            return HandleEmergencyMating(initiator, partner);
        }

        // Ensure correct initiator order
        if (!EnsureCorrectInitiator(ref initiator, ref partner))
        {
            return false;
        }

        // Check if either agent is already mating
        if (IsAgentMating(initiator) || IsAgentMating(partner))
        {
            if (debugHighSpeed) Debug.Log("Can't register mating: One or both agents are already mating");
            return false;
        }

        // HIGH-SPEED FIX: Validate agents more thoroughly
        if (!ValidateAgentsForMating(initiator, partner))
        {
            if (debugHighSpeed) Debug.Log("Can't register mating: Agent validation failed");
            return false;
        }

        // Create new mating process
        MatingProcess process = new MatingProcess(initiator, partner);
        activeProcesses.Add(process);

        // Add agents to mating set
        matingAgents.Add(initiator);
        matingAgents.Add(partner);

        // Notify agents
        try
        {
            initiator.ReproductionSystem.InitiateMating(partner);
            partner.ReproductionSystem.AcceptMating(initiator);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error notifying agents of mating: {e.Message}");
            // Clean up on failure
            matingAgents.Remove(initiator);
            matingAgents.Remove(partner);
            activeProcesses.Remove(process);
            return false;
        }

        // Start mating coroutine
        StartCoroutine(HighSpeedMatingProcessCoroutine(process));

        if (debugHighSpeed)
            Debug.Log($"Registered mating between {GetAgentName(initiator)} and {GetAgentName(partner)} at {Time.timeScale:F0}x speed");

        return true;
    }

    /// <summary>
    /// HIGH-SPEED FIX: Emergency mating for extreme speeds (>75x)
    /// Skips duration, creates offspring immediately
    /// </summary>
    private bool HandleEmergencyMating(IAgent initiator, IAgent partner)
    {
        if (!ValidateAgentsForMating(initiator, partner))
            return false;

        if (IsAgentMating(initiator) || IsAgentMating(partner))
            return false;

        // Check if agents are reasonably close for emergency mating
        float distance = Vector3.Distance(initiator.Position, partner.Position);
        if (distance > emergencyMatingDistance)
        {
            if (debugHighSpeed)
                Debug.Log($"Emergency mating failed: distance {distance:F1} > {emergencyMatingDistance}");
            return false;
        }

        if (debugHighSpeed)
            Debug.Log($"EMERGENCY MATING at {Time.timeScale:F0}x speed: {GetAgentName(initiator)} + {GetAgentName(partner)}");

        // Immediate energy cost
        ReproductionConfig config = GetReproductionConfig(initiator);
        float energyCost = config != null ? config.energyCost : 20f;

        initiator.EnergySystem.ConsumeEnergy(energyCost);
        partner.EnergySystem.ConsumeEnergy(energyCost);

        // Create offspring immediately
        CreateOffspringImmediate(initiator, partner, config);

        // Brief mating state (just for 1 frame)
        StartCoroutine(BriefMatingState(initiator, partner));

        return true;
    }

    /// <summary>
    /// HIGH-SPEED FIX: Brief mating state for emergency mating
    /// </summary>
    private IEnumerator BriefMatingState(IAgent initiator, IAgent partner)
    {
        // Set mating state
        matingAgents.Add(initiator);
        matingAgents.Add(partner);

        try
        {
            initiator.ReproductionSystem.InitiateMating(partner);
            partner.ReproductionSystem.AcceptMating(initiator);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error in brief mating state: {e.Message}");
        }

        // Wait just 1 frame
        yield return null;

        // End mating immediately
        EndMatingForAgents(initiator, partner);
        matingAgents.Remove(initiator);
        matingAgents.Remove(partner);
    }

    /// <summary>
    /// HIGH-SPEED FIX: Improved agent validation
    /// </summary>
    private bool ValidateAgentsForMating(IAgent initiator, IAgent partner)
    {
        try
        {
            // Null checks
            if (initiator == null || partner == null)
                return false;

            // Adapter validity checks
            if (initiator is AgentAdapter ia && !ia.IsValid())
                return false;

            if (partner is AgentAdapter pa && !pa.IsValid())
                return false;

            // System availability checks
            var initReproduction = initiator.ReproductionSystem;
            var partnerReproduction = partner.ReproductionSystem;
            var initEnergy = initiator.EnergySystem;
            var partnerEnergy = partner.EnergySystem;

            if (initReproduction == null || partnerReproduction == null ||
                initEnergy == null || partnerEnergy == null)
                return false;

            // Capability checks
            if (!initReproduction.CanMate || !partnerReproduction.CanMate)
                return false;

            if (!initEnergy.HasEnoughEnergyForMating || !partnerEnergy.HasEnoughEnergyForMating)
                return false;

            return true;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Error validating agents for mating: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Check if an agent is currently mating
    /// </summary>
    public bool IsAgentMating(IAgent agent)
    {
        return matingAgents.Contains(agent);
    }

    /// <summary>
    /// HIGH-SPEED OPTIMIZED: Mating process coroutine with better validation
    /// </summary>
    private IEnumerator HighSpeedMatingProcessCoroutine(MatingProcess process)
    {
        // Get reproduction config from initiator
        ReproductionConfig config = GetReproductionConfig(process.Initiator);
        float matingDuration = config != null ? config.matingDuration : 10f;

        // HIGH-SPEED FIX: More frequent validation at high speeds
        float validationInterval = CalculateValidationInterval();
        float nextValidation = Time.time;

        float endTime = Time.time + matingDuration;

        // Main mating loop with adaptive validation
        while (Time.time < endTime)
        {
            // Validate agents periodically (more often at high speeds)
            if (Time.time >= nextValidation)
            {
                if (!process.CanProduceOffspring())
                {
                    if (debugHighSpeed)
                        Debug.Log($"Mating interrupted at {Time.timeScale:F0}x speed - conditions no longer met");
                    break;
                }
                nextValidation = Time.time + validationInterval;
            }

            yield return null; // This respects timeScale automatically
        }

        // Complete the mating process
        CompleteMatingProcess(process, config);
    }

    /// <summary>
    /// HIGH-SPEED FIX: Calculate validation interval based on time scale
    /// </summary>
    private float CalculateValidationInterval()
    {
        float timeScale = Time.timeScale;

        if (timeScale > 50f)
            return 0.05f; // Validate every 0.05 game seconds at very high speeds
        else if (timeScale > 20f)
            return 0.1f;  // Validate every 0.1 game seconds at high speeds
        else
            return 0.5f;  // Validate every 0.5 game seconds at normal speeds
    }

    /// <summary>
    /// HIGH-SPEED FIX: Complete mating process with better error handling
    /// </summary>
    private void CompleteMatingProcess(MatingProcess process, ReproductionConfig config)
    {
        bool canProduceOffspring = process.CanProduceOffspring();

        if (canProduceOffspring)
        {
            if (debugHighSpeed)
                Debug.Log($"Mating successful at {Time.timeScale:F0}x speed - producing offspring");

            // Apply energy cost
            float energyCost = config != null ? config.energyCost : 20f;
            process.Initiator.EnergySystem.ConsumeEnergy(energyCost);
            process.Partner.EnergySystem.ConsumeEnergy(energyCost);

            // Create offspring
            CreateOffspringImmediate(process.Initiator, process.Partner, config);
        }
        else
        {
            if (debugHighSpeed)
                Debug.Log($"Mating failed at {Time.timeScale:F0}x speed - conditions no longer met");
        }

        // End mating for both agents
        EndMatingForAgents(process.Initiator, process.Partner);

        // Remove from tracking
        matingAgents.Remove(process.Initiator);
        matingAgents.Remove(process.Partner);
        activeProcesses.Remove(process);

        process.Complete();
    }

    /// <summary>
    /// HIGH-SPEED FIX: Immediate offspring creation to avoid command system delays
    /// </summary>
    private void CreateOffspringImmediate(IAgent initiator, IAgent partner, ReproductionConfig config)
    {
        try
        {
            Vector2 variance = config != null ? config.offspringPositionVariance : new Vector2(0.5f, 0.5f);
            Vector3 offspringPosition = CalculateOffspringPosition(initiator, partner, variance);

            GameObject initiatorObj = (initiator as AgentAdapter)?.GameObject;
            GameObject partnerObj = (partner as AgentAdapter)?.GameObject;

            if (initiatorObj == null || partnerObj == null)
            {
                Debug.LogError("Cannot create offspring - parent GameObjects are null");
                return;
            }

            AgentSpawner spawner = FindObjectOfType<AgentSpawner>();
            if (spawner == null)
            {
                Debug.LogError("No AgentSpawner found");
                return;
            }

            // HIGH-SPEED FIX: Create multiple offspring at very high speeds for population balance
            int offspringCount = Time.timeScale > 75f ? Random.Range(2, 4) : Random.Range(1, 3);

            for (int i = 0; i < offspringCount; i++)
            {
                Vector3 position = offspringPosition + new Vector3(
                    Random.Range(-variance.x, variance.x),
                    Random.Range(-variance.y, variance.y),
                    0f
                );
                spawner.SpawnOffspring(initiatorObj, partnerObj, position);
            }

            if (debugHighSpeed)
                Debug.Log($"Created {offspringCount} offspring at {Time.timeScale:F0}x speed");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creating offspring: {e.Message}");
        }
    }

    /// <summary>
    /// HIGH-SPEED FIX: Safe agent mating state ending
    /// </summary>
    private void EndMatingForAgents(IAgent initiator, IAgent partner)
    {
        // End mating commands with error handling
        try
        {
            if (initiator?.ReproductionSystem != null)
            {
                ICommand endInitiatorCommand = new EndMatingCommand(initiator);
                CommandDispatcher.Instance.ExecuteCommand(endInitiatorCommand);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Error ending mating for initiator: {e.Message}");
        }

        try
        {
            if (partner?.ReproductionSystem != null)
            {
                ICommand endPartnerCommand = new EndMatingCommand(partner);
                CommandDispatcher.Instance.ExecuteCommand(endPartnerCommand);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Error ending mating for partner: {e.Message}");
        }
    }

    /// <summary>
    /// Calculate offspring position between two agents
    /// </summary>
    private Vector3 CalculateOffspringPosition(IAgent initiator, IAgent partner, Vector2 variance)
    {
        Vector3 midpoint = (initiator.Position + partner.Position) / 2f;
        return midpoint + new Vector3(
            Random.Range(-variance.x, variance.x),
            Random.Range(-variance.y, variance.y),
            0f
        );
    }

    /// <summary>
    /// Get reproduction config from an agent
    /// </summary>
    private ReproductionConfig GetReproductionConfig(IAgent agent)
    {
        try
        {
            if (agent is AgentAdapter adapter && adapter.IsValid())
            {
                ReproductionSystem reproSystem = adapter.GameObject.GetComponent<ReproductionSystem>();
                return reproSystem?.GetConfig();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Error getting reproduction config: {e.Message}");
        }
        return null;
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