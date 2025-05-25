using UnityEngine;



public class GeneticsSystem : MonoBehaviour, IGeneticsSystem
{
    [SerializeField] private Genome genome;
    [SerializeField] private bool enableMutation = true;
    [SerializeField] private Color baseColor = Color.white; // Base color for visual rendering

    // Properties
    public Genome Genome => genome;
    public Color BaseColor => baseColor;

    void Awake()
    {
        // Initialize genome if not set
        if (genome == null)
        {
            genome = new Genome();
        }
    }

    public float GetTraitValue(string traitName, float defaultValue = 0f)
    {
        return genome.GetTraitValue(traitName, defaultValue);
    }

    public void SetTraitValue(string traitName, float value)
    {
        genome.SetTraitValue(traitName, value);
    }

    public Genome CombineWith(IGeneticsSystem other)
    {
        return genome.Combine(other.Genome, enableMutation);
    }

    // For offspring creation
    public void InheritFrom(GeneticsSystem parent1, GeneticsSystem parent2)
    {
        if (parent1 != null && parent2 != null)
        {
            genome = parent1.Genome.Combine(parent2.Genome, enableMutation);

            // Set base color as blend of parents (simple genetic visual trait)
            baseColor = Color.Lerp(parent1.BaseColor, parent2.BaseColor, 0.5f);

            // Add some mutation to color
            if (enableMutation && Random.value < 0.3f)
            {
                baseColor = Color.Lerp(baseColor, new Color(
                    Random.value, Random.value, Random.value), 0.2f);
            }
        }
        else if (parent1 != null)
        {
            genome = new Genome(parent1.Genome.GetTraits());
            baseColor = parent1.BaseColor;
        }
        else
        {
            genome = new Genome();
        }
    }
}