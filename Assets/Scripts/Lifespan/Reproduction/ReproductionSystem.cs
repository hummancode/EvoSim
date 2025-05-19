using System;
using System.Collections;
using UnityEngine;

public class ReproductionSystem : MonoBehaviour, IReproductionCapable
{
    [SerializeField] private ReproductionConfig config;

    // Dependencies (to be injected)
    private IMateFinder mateFinder;
    private IEnergyProvider energyProvider;
    private IAgent selfAgent;

    // State properties
    public bool IsMating { get; private set; }
    public float LastMatingTime { get; private set; } = -999f;
    public bool CanMate => !IsMating && Time.time - LastMatingTime >= config.matingCooldown;
    public float MatingProximity => config.matingProximity;

    // Current mating partner
    internal IAgent matingPartner;
    public GameObject matingPartnerGameObject
    {
        get
        {
            if (matingPartner is AgentAdapter adapter)
                return adapter.GameObject;
            return null;
        }
    }

    // Events
    public event Action<Vector3> OnOffspringRequested;
    public event Action<IAgent> OnMatingStarted;
    public event Action OnMatingCompleted;

    // Initialization method for dependency injection
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
    // Check if we can mate with a specific partner
    public bool CanMateWith(IAgent partner)
    {
        // Validate self can mate
        if (!CanMate || energyProvider == null || !energyProvider.HasEnoughEnergyForMating)
            return false;

        // Validate partner
        if (partner == null)
            return false;

        // Validate partner can mate
        if (!partner.ReproductionSystem.CanMate ||
            !partner.EnergySystem.HasEnoughEnergyForMating)
            return false;

        // Check distance
        float distance = mateFinder.GetDistanceTo(partner);
        return distance <= config.matingProximity;
    }

    // Initiate mating with a partner
    public void InitiateMating(IAgent partner)
    {
        Debug.Log("InitiateMating called with partner: " + (partner != null ? "valid" : "null"));

        // Validate we can mate
        if (!CanMate)
        {
            Debug.LogWarning("Cannot mate - CanMate is false");
            return;
        }

        if (energyProvider == null || !energyProvider.HasEnoughEnergyForMating)
        {
            Debug.LogWarning("Cannot mate - Not enough energy");
            return;
        }

        // Set mating state
        IsMating = true;
        matingPartner = partner;

        Debug.Log("Mating state set, IsMating: " + IsMating);

        // Notify that mating has started
        int subscribers = OnMatingStarted?.GetInvocationList().Length ?? 0;
        Debug.Log("OnMatingStarted has " + subscribers + " subscribers");

        OnMatingStarted?.Invoke(partner);
        Debug.Log("OnMatingStarted event invoked");

        // Start mating process
        StartCoroutine(MatingCoroutine());
        Debug.Log("MatingCoroutine started");
    }

    // Accept a mating invitation from another agent
    public void AcceptMating(IAgent partner)
    {
        Debug.Log("AcceptMating called with partner: " + (partner != null ? "valid" : "null"));

        // Validate we can mate
        if (!CanMate)
        {
            Debug.LogWarning("Cannot accept mating - CanMate is false");
            return;
        }

        if (energyProvider == null || !energyProvider.HasEnoughEnergyForMating)
        {
            Debug.LogWarning("Cannot accept mating - Not enough energy");
            return;
        }

        // Set mating state
        IsMating = true;
        matingPartner = partner;

        Debug.Log("Mating state accepted, IsMating: " + IsMating);

        // Notify that mating has started
        OnMatingStarted?.Invoke(partner);
        Debug.Log("OnMatingStarted event invoked for accepting agent");
    }

    // Coroutine for the mating process
    private IEnumerator MatingCoroutine()
    {
        Debug.Log("Mating coroutine started");

        // Wait for the mating duration
        yield return new WaitForSeconds(config.matingDuration);
        Debug.Log("Mating duration completed");

        // Ensure mating partner still exists and we have energy
        if (matingPartner != null &&
            energyProvider != null && energyProvider.HasEnoughEnergyForMating)
        {
            Debug.Log("Conditions valid for offspring creation");

            // Consume energy
            energyProvider.ConsumeEnergy(config.energyCost);

            // Calculate spawn position
            Vector3 spawnPosition = (selfAgent.Position + matingPartner.Position) / 2f;
            spawnPosition += new Vector3(
                UnityEngine.Random.Range(-0.5f, 0.5f),
                UnityEngine.Random.Range(-0.5f, 0.5f),
                0f
            );

            Debug.Log("About to request offspring at position: " + spawnPosition);

            // Request offspring
            OnOffspringRequested?.Invoke(spawnPosition);
            Debug.Log("Offspring requested event fired");

            // Make sure to reset the partner's mating state as well
            if (matingPartner is AgentAdapter adapter)
            {
                GameObject partnerObj = adapter.GameObject;
                if (partnerObj != null)
                {
                    ReproductionSystem partnerReproSystem = partnerObj.GetComponent<ReproductionSystem>();
                    if (partnerReproSystem != null)
                    {
                        Debug.Log("Resetting partner's mating state");
                        partnerReproSystem.ResetMatingState();
                    }
                    else
                    {
                        Debug.LogError("Partner's ReproductionSystem not found");
                    }
                }
                else
                {
                    Debug.LogError("Partner GameObject is null");
                }
            }
            else
            {
                Debug.LogError("Partner is not an AgentAdapter or is null: " +
                              (matingPartner != null ? matingPartner.GetType().Name : "null"));
            }
        }
        else
        {
            Debug.Log("Mating conditions no longer valid - No offspring created");
            if (matingPartner == null) Debug.Log("- Partner is null");
            if (energyProvider == null) Debug.Log("- Energy provider is null");
            if (energyProvider != null && !energyProvider.HasEnoughEnergyForMating)
                Debug.Log("- Not enough energy for mating");
        }

        // Reset own mating state
        Debug.Log("Resetting initiator's mating state");
        ResetPartnerMatingState();
        ResetMatingState();
       
    }
    private void ResetPartnerMatingState()
    {
        // Reset partner's mating state if they exist
        if (matingPartner != null)
        {
            if (matingPartner is AgentAdapter adapter && adapter.GameObject != null)
            {
                GameObject partnerObj = adapter.GameObject;
                ReproductionSystem partnerReproSystem = partnerObj.GetComponent<ReproductionSystem>();

                if (partnerReproSystem != null)
                {
                    Debug.Log("Resetting partner's mating state");
                    partnerReproSystem.ResetMatingState();
                }
            }
        }
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
    public void ResetMatingState()
    {
        Debug.Log("Resetting mating state for " + gameObject.name);
        IsMating = false;
        LastMatingTime = Time.time;
        matingPartner = null;

        // Notify that mating is completed
        OnMatingCompleted?.Invoke();
        Debug.Log("Mating completed event fired for " + gameObject.name);
    }
}