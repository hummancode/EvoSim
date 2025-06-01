// ============================================================================
// FILE: AgeBasedBehaviorHelper.cs
// PURPOSE: Helper for age-based behavior decisions
// ============================================================================

using UnityEngine;

/// <summary>
/// Helper class for age-based behavior logic
/// </summary>
public class AgeBasedBehaviorHelper : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AgeSystem ageSystem;
    [SerializeField] private AgeLifeStageTracker lifeStageTracker;

    void Awake()
    {
        if (ageSystem == null)
            ageSystem = GetComponent<AgeSystem>();

        if (lifeStageTracker == null)
            lifeStageTracker = GetComponent<AgeLifeStageTracker>();
    }

    // Behavior helper methods
    public bool CanReproduce()
    {
        return ageSystem != null &&
               ageSystem.IsMature &&
               lifeStageTracker != null &&
               (lifeStageTracker.IsAdult() || lifeStageTracker.IsElderly());
    }

    public bool ShouldPrioritizeFood()
    {
        // Young agents need more food
        return lifeStageTracker != null &&
               (lifeStageTracker.IsBaby() || lifeStageTracker.IsChild());
    }

    public float GetMovementSpeedModifier()
    {
        if (lifeStageTracker == null) return 1f;

        switch (lifeStageTracker.CurrentStage)
        {
            case AgeLifeStageTracker.LifeStage.Baby:
                return 0.6f; // Slow
            case AgeLifeStageTracker.LifeStage.Child:
                return 0.8f; // Slower
            case AgeLifeStageTracker.LifeStage.Adult:
                return 1f;   // Normal
            case AgeLifeStageTracker.LifeStage.Elderly:
                return 0.7f; // Slower again
            default:
                return 1f;
        }
    }

    public float GetCurrentAge() => ageSystem?.Age ?? 0f;
    public bool IsAdultAge() => lifeStageTracker?.IsAdult() ?? false;
    public bool IsChildAge() => lifeStageTracker?.IsChild() ?? lifeStageTracker?.IsBaby() ?? true;
}