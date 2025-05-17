using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DNA
{
    // Dictionary of genes - each gene represents a trait
    public Dictionary<string, float> genes = new Dictionary<string, float>();

    // Mutation settings
    public float mutationRate = 0.1f;
    public float mutationAmount = 0.2f;

    // Initialize with random values
    public void Initialize()
    {
        // Basic movement and survival traits
        genes["moveSpeed"] = Random.Range(1.0f, 5.0f);
        genes["maxEnergy"] = Random.Range(100.0f, 300.0f);
        genes["maturityAge"] = Random.Range(5.0f, 20.0f);
        genes["oldAge"] = Random.Range(40.0f, 100.0f);
        genes["metabolismRate"] = Random.Range(0.8f, 1.2f);

        // Advanced traits for future use
        genes["senseRange"] = Random.Range(1.0f, 10.0f);
        genes["foodPreference"] = Random.Range(0.0f, 1.0f);
        genes["aggressiveness"] = Random.Range(0.0f, 1.0f);
    }

    // Create child DNA by combining genes from two parents
    public static DNA Combine(DNA parent1, DNA parent2)
    {
        DNA childDNA = new DNA();

        // For each gene, randomly select from either parent with possibility of mutation
        foreach (var gene in parent1.genes)
        {
            string geneName = gene.Key;

            // Select gene from either parent
            float selectedGene = Random.value < 0.5f ?
                                parent1.genes[geneName] :
                                parent2.genes[geneName];

            // Apply mutation
            if (Random.value < childDNA.mutationRate)
            {
                // Determine mutation direction and amount
                float mutationFactor = 1.0f + (Random.value * 2 - 1) * childDNA.mutationAmount;
                selectedGene *= mutationFactor;
            }

            // Add the gene to the child
            childDNA.genes[geneName] = selectedGene;
        }

        return childDNA;
    }

    // Get gene value with fallback default
    public float GetGene(string geneName, float defaultValue = 1.0f)
    {
        if (genes.ContainsKey(geneName))
            return genes[geneName];
        else
            return defaultValue;
    }
}