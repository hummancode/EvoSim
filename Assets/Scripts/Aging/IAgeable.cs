using UnityEngine;
using System;

// ============================================================================
// CORE INTERFACES - Define contracts for system components
// ============================================================================

/// <summary>
/// Interface for objects that can age over time
/// </summary>
public interface IAgeable
{
    float CurrentAge { get; }
    float MaxAge { get; }
    LifeStage CurrentLifeStage { get; }
    void AgeBy(float amount);
    event Action<float> OnAgeChanged;
    event Action<LifeStage> OnLifeStageChanged;
}