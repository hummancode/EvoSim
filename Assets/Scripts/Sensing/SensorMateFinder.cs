using System;
using UnityEngine;

/// <summary>
/// Mate finder that uses sensor system with configurable detection range
/// </summary>
public class SensorMateFinder : IMateFinder
{
    private readonly SensorSystem sensorSystem;
    private readonly GameObject ownerObject;
    private  float mateDetectionRange;

    /// <summary>
    /// Constructor with configurable mate detection range
    /// </summary>
    /// <param name="sensorSystem">The sensor system to use</param>
    /// <param name="owner">The owner of this mate finder</param>
    /// <param name="mateDetectionRange">Detection range specifically for finding mates (optional)</param>
    public SensorMateFinder(SensorSystem sensorSystem, GameObject owner, float mateDetectionRange = -1f)
    {
        this.sensorSystem = sensorSystem ?? throw new ArgumentNullException(nameof(sensorSystem));
        this.ownerObject = owner ?? throw new ArgumentNullException(nameof(owner));

        // Use provided range, or fall back to sensor system's default range
        this.mateDetectionRange = mateDetectionRange > 0 ? mateDetectionRange : sensorSystem.GetDetectionRange();

        Debug.Log($"SensorMateFinder created for {owner.name} with mate detection range: {this.mateDetectionRange}");
    }

    /// <summary>
    /// Update the mate detection range (can be called after creation)
    /// </summary>
    /// <param name="newRange">The new detection range for mates</param>
    public void SetMateDetectionRange(float newRange)
    {
        if (newRange > 0)
        {
            mateDetectionRange = newRange;
            Debug.Log($"Updated mate detection range for {ownerObject.name} to: {mateDetectionRange}");
        }
    }

    /// <summary>
    /// Find nearest potential mate within the specified mate detection range
    /// </summary>
    public IAgent FindNearestPotentialMate(Func<IAgent, bool> additionalFilter = null)
    {
        if (sensorSystem == null)
        {
            Debug.LogError("SensorSystem is null in SensorMateFinder.FindNearestPotentialMate");
            return null;
        }

        try
        {
            // Use the mate-specific detection range
            AgentController potentialMate = sensorSystem.GetNearestEntity<AgentController>(
                range: mateDetectionRange, // Use mate-specific range
                filter: agent => {
                    // Skip self
                    if (agent == null || agent.gameObject == ownerObject)
                        return false;

                    // Create adapter to check validity
                    var agentAdapter = new AgentAdapter(agent);
                    if (!agentAdapter.IsValid())
                        return false;

                    // Check for reproduction system
                    ReproductionSystem repro = agentAdapter.GetComponentSafely<ReproductionSystem>();
                    if (repro == null || !repro.CanMate)
                        return false;

                    // Check for energy
                    EnergySystem energy = agentAdapter.GetComponentSafely<EnergySystem>();
                    if (energy == null || !energy.HasEnoughEnergyForMating)
                        return false;

                    // Check age/maturity
                    AgeSystem ageSystem = agentAdapter.GetComponentSafely<AgeSystem>();
                    if (ageSystem != null && !ageSystem.IsMature)
                        return false;

                    // Apply any additional filter if provided
                    return additionalFilter == null || additionalFilter(agentAdapter);
                }
            );

            return potentialMate != null ? new AgentAdapter(potentialMate) : null;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error in FindNearestPotentialMate: {e.Message}\n{e.StackTrace}");
            return null;
        }
    }

    /// <summary>
    /// Get distance to another agent
    /// </summary>
    public float GetDistanceTo(IAgent other)
    {
        if (other == null)
        {
            Debug.LogError("Other agent is null in SensorMateFinder.GetDistanceTo");
            return float.MaxValue;
        }

        if (other is AgentAdapter adapter && adapter.IsValid())
        {
            return Vector3.Distance(ownerObject.transform.position, adapter.Position);
        }

        Debug.LogError("Cannot get distance - other agent is not valid or not an AgentAdapter");
        return float.MaxValue;
    }

    /// <summary>
    /// Get the current mate detection range
    /// </summary>
    public float GetMateDetectionRange()
    {
        return mateDetectionRange;
    }

    /// <summary>
    /// Check if a potential mate is within detection range
    /// </summary>
    public bool IsWithinMateDetectionRange(IAgent other)
    {
        return GetDistanceTo(other) <= mateDetectionRange;
    }
}