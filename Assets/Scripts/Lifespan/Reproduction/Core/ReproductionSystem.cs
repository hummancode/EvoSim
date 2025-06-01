using System;
using UnityEngine;

/// <summary>
/// Component that manages an agent's reproductive capabilities
/// </summary>
public class ReproductionSystem : MonoBehaviour, IReproductionSystem
{
    [SerializeField] private ReproductionConfig config;

    // Dependencies (to be injected)
    private IMateFinder mateFinder;
    private IEnergyProvider energyProvider;
    private IAgent selfAgent;

    // NEW - Partner monitoring
    private float lastPartnerCheck = 0f;
    private float partnerCheckInterval = 1f; // Check every second

    // State
    private MatingState matingState = new MatingState();

    // Events
    public event Action<IAgent> OnMatingStarted;
    public event Action OnMatingCompleted;

    // Interface implementation
    public bool CanMate => matingState.CanMateAgain(config?.matingCooldown ?? 10f) &&
                          energyProvider != null &&
                          energyProvider.HasEnoughEnergyForMating;

    public bool IsMating => matingState.IsMating;

    public float MatingProximity => config?.matingProximity ?? 0.4f;

    public float LastMatingTime => matingState.LastMatingTime;

    void Awake()
    {
        // AUTO-LOAD CONFIG - Try multiple methods
        LoadConfiguration();
    }

    /// <summary>
    /// Automatically loads reproduction configuration
    /// </summary>
    private void LoadConfiguration()
    {
        if (config != null)
        {
            Debug.Log("ReproductionConfig already assigned in inspector");
            return; // Already assigned in inspector
        }

        // Method 1: Load from Resources folder
        // Place your config in Assets/Resources/Configs/
        config = Resources.Load<ReproductionConfig>("Configs/ReproductionConfig");

        if (config != null)
        {
            Debug.Log("Loaded ReproductionConfig from Resources");
            return;
        }

        // Method 2: Find the first ReproductionConfig in the project
        config = FindConfigInProject();

        if (config != null)
        {
            Debug.Log("Found ReproductionConfig in project assets");
            return;
        }

        // Method 3: Load from a specific path using Resources
        config = Resources.Load<ReproductionConfig>("ReproductionConfig");

        if (config != null)
        {
            Debug.Log("Loaded ReproductionConfig from Resources root");
            return;
        }

        // Method 4: Create default config if none found
        Debug.LogWarning("No ReproductionConfig found, creating default configuration");
        config = CreateDefaultConfig();
    }

    /// <summary>
    /// Finds ReproductionConfig asset anywhere in the project
    /// </summary>
    private ReproductionConfig FindConfigInProject()
    {
#if UNITY_EDITOR
        // This only works in the editor
        string[] guids = UnityEditor.AssetDatabase.FindAssets("t:ReproductionConfig");

        if (guids.Length > 0)
        {
            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
            return UnityEditor.AssetDatabase.LoadAssetAtPath<ReproductionConfig>(path);
        }
#endif
        return null;
    }

    /// <summary>
    /// Creates a default configuration at runtime
    /// </summary>
    private ReproductionConfig CreateDefaultConfig()
    {
        var defaultConfig = ScriptableObject.CreateInstance<ReproductionConfig>();

        // Set default values
        defaultConfig.matingProximity = 1.0f;
        defaultConfig.matingDuration = 10f;
        defaultConfig.matingCooldown = 30f;
        defaultConfig.energyCost = 20f;
        defaultConfig.offspringPositionVariance = new Vector2(0.5f, 0.5f);

        return defaultConfig;
    }

    void Update()
    {
        if (IsMating && Time.time - lastPartnerCheck > partnerCheckInterval)
        {
            ValidateCurrentMating();
            lastPartnerCheck = Time.time;
        }
    }

    private void ValidateCurrentMating()
    {
        if (!IsMating) return;

        // Check if partner is still valid
        matingState.ValidatePartner();

        // If mating ended due to invalid partner, notify
        if (!IsMating)
        {
            Debug.Log($"{gameObject.name}: Mating ended due to invalid partner");
            OnMatingCompleted?.Invoke();
        }
    }

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

        // Ensure config is loaded
        if (config == null)
        {
            LoadConfiguration();
        }

        Debug.Log("ReproductionSystem initialized successfully");
    }

    /// <summary>
    /// Checks if this agent can mate with the specified partner
    /// </summary>
    public bool CanMateWith(IAgent partner)
    {
        try
        {
            // Check if partner is valid/alive first
            if (partner == null) return false;

            if (partner is AgentAdapter adapter && !adapter.IsValid())
            {
                return false; // Partner is dead/destroyed
            }

            // Check self maturity
            AgeSystem ageSystem = GetComponent<AgeSystem>();
            if (ageSystem != null && !ageSystem.IsMature)
            {
                return false;
            }

            // Check partner maturity with safe access
            if (partner is AgentAdapter partnerAdapter && partnerAdapter.IsValid())
            {
                AgeSystem partnerAgeSystem = partnerAdapter.GetComponentSafely<AgeSystem>();
                if (partnerAgeSystem != null && !partnerAgeSystem.IsMature)
                {
                    return false;
                }
            }

            // Rest of checks with safe access
            if (!CanMate || energyProvider == null || !energyProvider.HasEnoughEnergyForMating)
                return false;

            // Safe partner system access
            var partnerReproduction = partner.ReproductionSystem;
            var partnerEnergy = partner.EnergySystem;

            if (partnerReproduction == null || partnerEnergy == null)
                return false;

            if (!partnerReproduction.CanMate || !partnerEnergy.HasEnoughEnergyForMating)
                return false;

            float distance = mateFinder.GetDistanceTo(partner);
            return distance <= config.matingProximity;
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Exception in CanMateWith: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// Initiates mating with a partner (called on the initiating agent)
    /// </summary>
    public void InitiateMating(IAgent partner)
    {
        try
        {
            if (!CanMate)
            {
                Debug.LogWarning($"{gameObject.name}: Cannot mate - CanMate is false");
                return;
            }

            // Validate partner before starting mating
            if (partner == null || (partner is AgentAdapter adapter && !adapter.IsValid()))
            {
                Debug.LogWarning($"{gameObject.name}: Cannot mate - partner is invalid or dead");
                return;
            }

            matingState.StartMating(partner);
            OnMatingStarted?.Invoke(partner);

            Debug.Log($"{gameObject.name}: Mating started with {GetPartnerName(partner)}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Exception in InitiateMating for {gameObject.name}: {e.Message}");
        }
    }

    /// <summary>
    /// Accepts mating with a partner (called on the accepting agent)
    /// </summary>
    public void AcceptMating(IAgent partner)
    {
        try
        {
            if (!CanMate)
            {
                Debug.LogWarning($"{gameObject.name}: Cannot accept mating - CanMate is false");
                return;
            }

            // Validate partner before accepting mating
            if (partner == null || (partner is AgentAdapter adapter && !adapter.IsValid()))
            {
                Debug.LogWarning($"{gameObject.name}: Cannot accept mating - partner is invalid or dead");
                return;
            }

            matingState.StartMating(partner);
            OnMatingStarted?.Invoke(partner);

            Debug.Log($"{gameObject.name}: Accepted mating with {GetPartnerName(partner)}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Exception in AcceptMating for {gameObject.name}: {e.Message}");
        }
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

    /// <summary>
    /// Safe helper method for getting partner name
    /// </summary>
    private string GetPartnerName(IAgent partner)
    {
        try
        {
            if (partner == null) return "null";
            if (partner is AgentAdapter adapter && adapter.IsValid())
                return adapter.GameObject.name;
            return "destroyed";
        }
        catch
        {
            return "unknown";
        }
    }
}