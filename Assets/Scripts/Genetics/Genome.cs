using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Genome
{
    [SerializeField] private List<GeneticTrait> traits = new List<GeneticTrait>();
    private Dictionary<string, GeneticTrait> traitDict = new Dictionary<string, GeneticTrait>();

    // Default traits
    public static readonly string DEATH_AGE = "DeathAge";
    public static readonly string PUBERTY_AGE = "PubertyAge";

    public Genome()
    {
        // Add default traits
        AddTrait(new GeneticTrait(DEATH_AGE, 70f, 30f, 120f));
        AddTrait(new GeneticTrait(PUBERTY_AGE, 20f, 10f, 40f));

        BuildDictionary();
    }

    // Initialize from collection
    public Genome(IEnumerable<GeneticTrait> initialTraits)
    {
        foreach (var trait in initialTraits)
        {
            AddTrait(trait.Clone());
        }

        BuildDictionary();
    }

    private void BuildDictionary()
    {
        traitDict.Clear();
        foreach (var trait in traits)
        {
            traitDict[trait.name] = trait;
        }
    }

    public void AddTrait(GeneticTrait trait)
    {
        traits.Add(trait);
        traitDict[trait.name] = trait;
    }

    public float GetTraitValue(string traitName, float defaultValue = 0f)
    {
        if (traitDict.TryGetValue(traitName, out GeneticTrait trait))
        {
            return trait.value;
        }
        return defaultValue;
    }

    public void SetTraitValue(string traitName, float value)
    {
        if (traitDict.TryGetValue(traitName, out GeneticTrait trait))
        {
            trait.value = Mathf.Clamp(value, trait.minValue, trait.maxValue);
        }
    }

    public bool HasTrait(string traitName)
    {
        return traitDict.ContainsKey(traitName);
    }

    public Genome Combine(Genome other, bool mutate = true)
    {
        Genome childGenome = new Genome();
        childGenome.traits.Clear();
        childGenome.traitDict.Clear();

        // Combine traits from both parents
        foreach (var trait in traits)
        {
            if (other.HasTrait(trait.name))
            {
                GeneticTrait parentTrait = Random.value < 0.5f ? trait : other.traitDict[trait.name];
                GeneticTrait childTrait = parentTrait.Clone();

                if (mutate)
                {
                    childTrait.Mutate();
                }

                childGenome.AddTrait(childTrait);
            }
            else
            {
                GeneticTrait childTrait = trait.Clone();
                if (mutate)
                {
                    childTrait.Mutate();
                }
                childGenome.AddTrait(childTrait);
            }
        }

        // Add traits unique to other parent
        foreach (var trait in other.traits)
        {
            if (!HasTrait(trait.name))
            {
                GeneticTrait childTrait = trait.Clone();
                if (mutate)
                {
                    childTrait.Mutate();
                }
                childGenome.AddTrait(childTrait);
            }
        }

        return childGenome;
    }

    public List<GeneticTrait> GetTraits()
    {
        return new List<GeneticTrait>(traits);
    }
}