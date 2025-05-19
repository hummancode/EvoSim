

using UnityEngine;

[CreateAssetMenu(fileName = "ReproductionConfig", menuName = "Simulation/Reproduction Config")]
public class ReproductionConfig : ScriptableObject
{
    [Header("Reproduction Settings")]
    public float matingProximity = 1.0f;
    public float matingDuration = 10f;
    public float matingCooldown = 30f;
    public float energyCost = 20f;
}