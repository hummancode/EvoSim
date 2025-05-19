using UnityEngine;

[CreateAssetMenu(fileName = "ReproductionConfig", menuName = "Simulation/Reproduction Config")]
public class ReproductionConfig : ScriptableObject
{
    [Header("Reproduction Settings")]
    public float matingProximity = 1.0f;
    public float matingDuration = 10f;
    public float matingCooldown = 10f;
    public float energyCost = 10f;

    [Header("Offspring Settings")]
    [Tooltip("Random offset range for offspring position")]
    public Vector2 offspringPositionVariance = new Vector2(0.5f, 0.5f);
}