using System;
using UnityEngine;

/// <summary>
/// Component that manages an agent's reproductive capabilities
/// </summary>
public class ReproductionSystem : MonoBehaviour, IReproductionCapable
{
    [SerializeField] private ReproductionConfig config;

    // Dependencies (to be injected)
    private IMateFinder mateFinder;
    private IEnergyProvider energyProvider;
    private IAgent selfAgent;

    // State
    private MatingState matingState = new MatingState();

    // Events
   
    public event Action<IAgent> OnMatingStarted;
    public event Action OnMatingCompleted;

    // Interface implementation
    public bool CanMate => matingState.CanMateAgain(config?.matingCooldown ?? 30f) &&
                          energyProvider != null &&
                          energyProvider.HasEnoughEnergyForMating;

    public bool IsMating => matingState.IsMating;

    public float MatingProximity => config?.matingProximity ?? 1.0f;

    public float LastMatingTime => matingState.LastMatingTime;

    /// <summary>
    /// Initialize with required dependencies
    /// </summary>
    public void Initialize(IAgent self, IMateFinder finder, IEnergyProvider energy)
    {
        Debug.Log("ReproductionSystem.Initialize called");

        if (self == null)
        {
            Debug.LogError("selfAgent is null in ReproductionSystem.Initialize");
        }

        selfAgent = self;
        mateFinder = finder;
        energyProvider = energy;

        // Fallback if config is missing
        if (config == null)
        {
            Debug.LogWarning("ReproductionConfig not assigned, using default values");
            config = ScriptableObject.CreateInstance<ReproductionConfig>();
        }

        Debug.Log("ReproductionSystem initialized successfully");
    }

    /// <summary>
    /// Checks if this agent can mate with the specified partner
    /// </summary>
    public bool CanMateWith(IAgent partner)
    {
        // Validate self can mate
        if (!CanMate)
            return false;

        // Validate partner
        if (partner == null)
            return false;

        // Validate partner can mate
        if (!partner.ReproductionSystem.CanMate)
            return false;

        // Check distance
        float distance = mateFinder.GetDistanceTo(partner);
        return distance <= MatingProximity;
    }

    /// <summary>
    /// Initiates mating with a partner (called on the initiating agent)
    /// </summary>
    public void InitiateMating(IAgent partner)
    {
        Debug.Log($"{gameObject.name}: InitiateMating called with partner: {(partner != null ? "valid" : "null")}");

        // Validate we can mate
        if (!CanMate)
        {
            Debug.LogWarning($"{gameObject.name}: Cannot mate - CanMate is false");
            return;
        }

        // Set mating state
        matingState.StartMating(partner);

        Debug.Log($"{gameObject.name}: Mating state set, IsMating: {IsMating}");

        // Notify that mating has started
        OnMatingStarted?.Invoke(partner);
        Debug.Log($"{gameObject.name}: OnMatingStarted event invoked");
    }

    /// <summary>
    /// Accepts a mating request from another agent (called on the accepting agent)
    /// </summary>
    public void AcceptMating(IAgent partner)
    {
        Debug.Log($"{gameObject.name}: AcceptMating called with partner: {(partner != null ? "valid" : "null")}");

        // Validate we can mate
        if (!CanMate)
        {
            Debug.LogWarning($"{gameObject.name}: Cannot accept mating - CanMate is false");
            return;
        }

        // Set mating state
        matingState.StartMating(partner);

        Debug.Log($"{gameObject.name}: Mating state accepted, IsMating: {IsMating}");

        // Notify that mating has started
        OnMatingStarted?.Invoke(partner);
        Debug.Log($"{gameObject.name}: OnMatingStarted event invoked for accepting agent");
    }

    /// <summary>
    /// Resets mating state (called when mating completes or is interrupted)
    /// </summary>
    public void ResetMatingState()
    {
        Debug.Log($"Resetting mating state for {gameObject.name}");

        // Reset state
        matingState.EndMating();

        // Notify that mating is completed
        OnMatingCompleted?.Invoke();

        Debug.Log($"Mating completed event fired for {gameObject.name}");
    }

    /// <summary>
    /// Requests the creation of an offspring at the specified position
    /// </summary>
  
    /// <summary>
    /// Returns the config for external use
    /// </summary>
    public ReproductionConfig GetConfig()
    {
        return config;
    }

    // Visualization for debugging
    void OnDrawGizmosSelected()
    {
        if (config != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, config.matingProximity);
        }
    }
    public IAgent GetCurrentPartner()
    {
        return matingState.Partner;
    }
}