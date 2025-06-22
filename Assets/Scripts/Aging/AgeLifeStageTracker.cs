

using UnityEngine;
using System;

/// <summary>
/// Tracks life stages and fires events when they change
/// </summary>
public class AgeLifeStageTracker : MonoBehaviour
{
    public enum LifeStage { Baby, Child, Adult, Elderly }

    [Header("Life Stage Settings")]
    [SerializeField] private float childAge = 5f;
    [SerializeField] private float adultAge = 30f;
    [SerializeField] private float elderlyAge = 90f;

    [Header("References")]
    [SerializeField] private AgeSystem ageSystem;

    // Current state
    private LifeStage currentStage = LifeStage.Baby;

    // Events
    public event Action<LifeStage> OnLifeStageChanged;

    // Properties
    public LifeStage CurrentStage => currentStage;

    void Awake()
    {
        if (ageSystem == null)
            ageSystem = GetComponent<AgeSystem>();
    }

    void Update()
    {
        if (ageSystem == null) return;

        LifeStage newStage = CalculateLifeStage(ageSystem.Age);

        if (newStage != currentStage)
        {
            currentStage = newStage;
            OnLifeStageChanged?.Invoke(currentStage);
            //Debug.Log($"{gameObject.name} entered {currentStage} stage at age {ageSystem.Age:F1}");
        }
    }

    private LifeStage CalculateLifeStage(float age)
    {
        if (age < childAge) return LifeStage.Baby;
        if (age < adultAge) return LifeStage.Child;
        if (age < elderlyAge) return LifeStage.Adult;
        return LifeStage.Elderly;
    }

    // Public helper methods
    public bool IsBaby() => currentStage == LifeStage.Baby;
    public bool IsChild() => currentStage == LifeStage.Child;
    public bool IsAdult() => currentStage == LifeStage.Adult;
    public bool IsElderly() => currentStage == LifeStage.Elderly;
}
