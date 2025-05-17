using System;
using System.Collections;
using UnityEngine;

public class ReproductionSystem : MonoBehaviour
{
    [Header("Reproduction Settings")]
    [SerializeField] private float matingProximity = 1.0f;  // How close to actually mate
    [SerializeField] private float matingDuration = 10f;
    [SerializeField] private float matingCooldown = 30f;
    [SerializeField] private float energyCost = 20f;

    // Events
    public event Action<Vector3> OnOffspringRequested;
    public event Action<GameObject> OnMatingStarted;
    public event Action OnMatingCompleted;

    // Properties
    public bool IsMating { get; private set; }
    public float LastMatingTime { get; private set; } = -999f;
    public bool CanMateAgain => !IsMating && Time.time - LastMatingTime >= matingCooldown;

    // References through interfaces
    private IEnergyProvider energyProvider;
    private SensorSystem sensorSystem;
    internal GameObject matingPartner;

    void Awake()
    {
        // Get required components
        energyProvider = GetComponent<IEnergyProvider>();
        sensorSystem = GetComponent<SensorSystem>();

        // Validate
        if (sensorSystem == null)
        {
            Debug.LogWarning("ReproductionSystem requires a SensorSystem component");
        }
    }

    // This method is used by both behaviors and direct checks 
    // to determine if mating is possible with a specific partner
    public bool CanMateWith(GameObject partner)
    {
        // Only proceed if we can mate
        if (!CanMateAgain || energyProvider == null || !energyProvider.HasEnoughEnergyForMating)
            return false;

        // Check if partner is valid
        if (partner == null)
            return false;

        // Check if partner has reproduction system
        ReproductionSystem otherReproSystem = partner.GetComponent<ReproductionSystem>();
        if (otherReproSystem == null || !otherReproSystem.CanMateAgain)
            return false;

        // Check if partner has energy
        IEnergyProvider otherEnergyProvider = partner.GetComponent<IEnergyProvider>();
        if (otherEnergyProvider == null || !otherEnergyProvider.HasEnoughEnergyForMating)
            return false;

        // Check distance
        float distance = Vector2.Distance(transform.position, partner.transform.position);
        return distance <= matingProximity;
    }

    // This method is called by behaviors when they want to try to mate with the nearest suitable agent
    public bool TryFindMate()
    {
        // Only proceed if we can mate
        if (!CanMateAgain || energyProvider == null || !energyProvider.HasEnoughEnergyForMating)
            return false;

        // Use sensor system to find a mate
        if (sensorSystem != null)
        {
            // Find a potential mate using the sensor system
            AgentController potentialMate = sensorSystem.GetNearestEntity<AgentController>(
                filter: agent => {
                    // Check for reproduction system
                    ReproductionSystem repro = agent.GetComponent<ReproductionSystem>();
                    if (repro == null || !repro.CanMateAgain)
                        return false;

                    // Check for energy
                    IEnergyProvider energy = agent.GetComponent<IEnergyProvider>();
                    if (energy == null || !energy.HasEnoughEnergyForMating)
                        return false;

                    return true;
                }
            );

            // If found a mate, check if close enough
            if (potentialMate != null)
            {
                if (CanMateWith(potentialMate.gameObject))
                {
                    // Start mating
                    InitiateMating(potentialMate.gameObject);

                    // Tell the other agent to accept mating
                    ReproductionSystem otherReproSystem = potentialMate.GetComponent<ReproductionSystem>();
                    otherReproSystem.AcceptMating(gameObject);

                    return true;
                }

                // Found a mate but not close enough
                return false;
            }
        }

        // No mate found
        return false;
    }

    // Returns nearest potential mate, even if not close enough to mate
    public GameObject GetNearestPotentialMate()
    {
        if (sensorSystem == null) return null;

        AgentController potentialMate = sensorSystem.GetNearestEntity<AgentController>(
            filter: agent => {
                // Check for reproduction system
                ReproductionSystem repro = agent.GetComponent<ReproductionSystem>();
                if (repro == null || !repro.CanMateAgain)
                    return false;

                // Check for energy
                IEnergyProvider energy = agent.GetComponent<IEnergyProvider>();
                if (energy == null || !energy.HasEnoughEnergyForMating)
                    return false;

                return true;
            }
        );

        return potentialMate?.gameObject;
    }

    public void InitiateMating(GameObject partner)
    {
        // Only proceed if we can mate
        if (!CanMateAgain || energyProvider == null || !energyProvider.HasEnoughEnergyForMating)
            return;

        IsMating = true;
        matingPartner = partner;

        // Notify that mating has started
        OnMatingStarted?.Invoke(partner);

        // Start mating process
        StartCoroutine(MatingCoroutine());
    }

    public void AcceptMating(GameObject partner)
    {
        // Only accept if we can mate
        if (!CanMateAgain || energyProvider == null || !energyProvider.HasEnoughEnergyForMating)
            return;

        IsMating = true;
        matingPartner = partner;

        // Notify that mating has started
        OnMatingStarted?.Invoke(partner);
    }

    private IEnumerator MatingCoroutine()
    {
        // Wait for the mating duration
        yield return new WaitForSeconds(matingDuration);

        // Ensure mating partner still exists and we have energy
        if (matingPartner != null &&
            energyProvider != null && energyProvider.HasEnoughEnergyForMating)
        {
            // Consume energy
            energyProvider.ConsumeEnergy(energyCost);

            // Request an offspring at the midpoint between partners
            Vector3 spawnPosition = (transform.position + matingPartner.transform.position) / 2f;
            spawnPosition += new Vector3(UnityEngine.Random.Range(-0.5f, 0.5f), UnityEngine.Random.Range(-0.5f, 0.5f), 0f);

            OnOffspringRequested?.Invoke(spawnPosition);
        }

        // Reset mating state
        IsMating = false;
        LastMatingTime = Time.time;
        matingPartner = null;

        // Notify that mating is completed
        OnMatingCompleted?.Invoke();
    }

    // Visualization for debugging
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, matingProximity);
    }
}