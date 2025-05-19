using System;
using UnityEngine;

/// <summary>
/// Interface for objects capable of reproduction
/// </summary>
public interface IReproductionCapable
{
    // Properties
    bool CanMate { get; }
    bool IsMating { get; }
    float MatingProximity { get; }
    float LastMatingTime { get; }

    // Methods
    bool CanMateWith(IAgent partner);
    void InitiateMating(IAgent partner);
    void AcceptMating(IAgent partner);
    void ResetMatingState();
    
    IAgent GetCurrentPartner(); // Add this method
    // Events

    event Action<IAgent> OnMatingStarted;
    event Action OnMatingCompleted;
}