using System;

/// <summary>
/// Interface for finding potential mates with configurable detection range
/// </summary>
public interface IMateFinder
{
    /// <summary>
    /// Find the nearest potential mate within detection range
    /// </summary>
    /// <param name="filter">Optional additional filter for mate selection</param>
    /// <returns>The nearest potential mate, or null if none found</returns>
    IAgent FindNearestPotentialMate(Func<IAgent, bool> filter = null);

    /// <summary>
    /// Get the distance to another agent
    /// </summary>
    /// <param name="other">The other agent</param>
    /// <returns>Distance to the other agent</returns>
    float GetDistanceTo(IAgent other);

    /// <summary>
    /// Get the current mate detection range
    /// </summary>
    /// <returns>The detection range for finding mates</returns>
    float GetMateDetectionRange();

    /// <summary>
    /// Check if another agent is within mate detection range
    /// </summary>
    /// <param name="other">The other agent to check</param>
    /// <returns>True if within range, false otherwise</returns>
    bool IsWithinMateDetectionRange(IAgent other);
}