using UnityEngine;

[System.Serializable]
public class GeneticTrait
{
    public string name;
    public float value;
    public float minValue;
    public float maxValue;
    public float mutationRate = 0.1f;
    public float mutationAmount = 0.2f;

    public GeneticTrait(string name, float value, float minValue, float maxValue,
                        float mutationRate = 0.1f, float mutationAmount = 0.2f)
    {
        this.name = name;
        this.value = Mathf.Clamp(value, minValue, maxValue);
        this.minValue = minValue;
        this.maxValue = maxValue;
        this.mutationRate = mutationRate;
        this.mutationAmount = mutationAmount;
    }

    public GeneticTrait Clone()
    {
        return new GeneticTrait(name, value, minValue, maxValue, mutationRate, mutationAmount);
    }

    public void Mutate()
    {
        if (Random.value < mutationRate)
        {
            float mutation = Random.Range(-mutationAmount, mutationAmount) * (maxValue - minValue);
            value = Mathf.Clamp(value + mutation, minValue, maxValue);
            Debug.Log($"Trait {name} mutated to {value}");
        }
    }
}