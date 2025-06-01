using UnityEngine;

[CreateAssetMenu(fileName = "ReproductionConfig", menuName = "Simulation/Reproduction Config")]
public class ReproductionConfig : ScriptableObject
{
    [Header("Detection Settings")]
    [Tooltip("Range for detecting potential mates")]
    public float mateDetectionRange = 5.0f;

    [Header("Mating Settings")]
    [Tooltip("How close agents need to be to actually start mating")]
    public float matingProximity = 1.0f;

    [Tooltip("How long mating takes")]
    public float matingDuration = 10f;

    [Tooltip("Cooldown before agent can mate again")]
    public float matingCooldown = 30f;

    [Tooltip("Energy cost for mating")]
    public float energyCost = 20f;

    [Header("Offspring Settings")]
    [Tooltip("Random offset range for offspring position")]
    public Vector2 offspringPositionVariance = new Vector2(0.5f, 0.5f);

    
    private void OnValidate()
    {
        // Ensure mate detection range is always >= mating proximity
        if (mateDetectionRange < matingProximity)
        {
            mateDetectionRange = matingProximity;
            Debug.LogWarning("Mate detection range cannot be smaller than mating proximity. Adjusted automatically.");
        }
    }
}