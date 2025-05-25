using UnityEngine;
using System;

public class AgeSystem : MonoBehaviour, IAgeSystem
{
    [Header("Age Settings")]
    [SerializeField] private float age = 0f;
    [SerializeField] private float maxAge = 70f;
    [SerializeField] private float maturityAge = 20f;

    [Header("State")]
    [SerializeField] private bool isMature = false;

    // References - will be injected
    private DeathSystem deathSystem;
    private GeneticsSystem geneticsSystem;

    // Events
    public event Action OnMatured;
    public event Action OnDeath;

    // Properties
    public float Age => age;
    public float MaxAge => maxAge;
    public bool IsMature => isMature;

    // Initialization method for dependency injection
    public void Initialize(DeathSystem deathSystem, GeneticsSystem geneticsSystem)
    {
        this.deathSystem = deathSystem;
        this.geneticsSystem = geneticsSystem;

        // Get genetic values if available
        if (geneticsSystem != null)
        {
            maxAge = geneticsSystem.GetTraitValue(Genome.DEATH_AGE, maxAge);
            maturityAge = geneticsSystem.GetTraitValue(Genome.PUBERTY_AGE, maturityAge);

            Debug.Log($"Age system initialized: maxAge={maxAge}, maturityAge={maturityAge}");
        }
    }

    void Update()
    {
        // Increase age
        age += Time.deltaTime;

        // Check for maturity
        if (!isMature && age >= maturityAge)
        {
            isMature = true;
            Debug.Log($"Agent {gameObject.name} reached maturity at age {age}");
            OnMatured?.Invoke();
        }

        // Check for death by old age
        if (age >= maxAge)
        {
            Debug.Log($"Agent {gameObject.name} died of old age at {age} seconds");

            if (deathSystem != null)
            {
                deathSystem.Die("old age");
            }
            else
            {
                Debug.LogError($"DeathSystem is null for agent {gameObject.name}! Cannot die of old age.");
            }

            OnDeath?.Invoke();
        }
    }

    public void SetAgeValues(float maxAge, float maturityAge)
    {
        this.maxAge = maxAge;
        this.maturityAge = maturityAge;
        Debug.Log($"Age values updated: maxAge={maxAge}, maturityAge={maturityAge}");
    }
}