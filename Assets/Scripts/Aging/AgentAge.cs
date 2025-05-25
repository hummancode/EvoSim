using System;
using UnityEngine;
/// <summary>
/// Simple age tracking component - just handles age progression
/// </summary>
public class AgentAge : MonoBehaviour
{
    [Header("Age Settings")]
    [SerializeField] private float currentAge = 0f;
    [SerializeField] private float maxAge = 100f;
    [SerializeField] private float ageRate = 1f; // Age units per second

    // Events
    public event Action<float> OnAgeChanged;
    public event Action OnDiedFromAge;

    // Properties
    public float CurrentAge => currentAge;
    public float MaxAge => maxAge;
    public float AgePercent => currentAge / maxAge;
    public bool IsDead => currentAge >= maxAge;

    void Update()
    {
        if (!IsDead)
        {
            AgeByAmount(ageRate * Time.deltaTime);
        }
    }

    public void AgeByAmount(float amount)
    {
        if (IsDead) return;

        currentAge += amount;

        if (currentAge >= maxAge)
        {
            currentAge = maxAge;
            OnDiedFromAge?.Invoke();
            return;
        }

        OnAgeChanged?.Invoke(currentAge);
    }

    public void SetAge(float age)
    {
        currentAge = Mathf.Clamp(age, 0f, maxAge);
        OnAgeChanged?.Invoke(currentAge);
    }

    public void InheritMaxAgeFromParents(AgentAge parent1, AgentAge parent2 = null)
    {
        if (parent2 != null)
        {
            // Average parents' max age with some variation
            float avgMaxAge = (parent1.maxAge + parent2.maxAge) / 2f;
            maxAge = avgMaxAge + UnityEngine.Random.Range(-5f, 5f);
        }
        else
        {
            maxAge = parent1.maxAge + UnityEngine.Random.Range(-3f, 3f);
        }

        maxAge = Mathf.Max(50f, maxAge); // Minimum lifespan
    }
}
