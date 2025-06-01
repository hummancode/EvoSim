using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TimeSeriesData
{
    public List<float> timestamps = new List<float>();
    public List<float> values = new List<float>();

    public void AddDataPoint(float time, float value)
    {
        timestamps.Add(time);
        values.Add(value);
    }

    public void Clear()
    {
        timestamps.Clear();
        values.Clear();
    }

    public int Count => timestamps.Count;

    public float GetLatestValue()
    {
        return values.Count > 0 ? values[values.Count - 1] : 0f;
    }
}

[System.Serializable]
public class DeathData
{
    public string cause;
    public float age;
    public float timestamp;
    public int generation;

    public DeathData(string cause, float age, float timestamp, int generation = 1)
    {
        this.cause = cause;
        this.age = age;
        this.timestamp = timestamp;
        this.generation = generation;
    }
}

[System.Serializable]
public class StatisticsData
{
    [Header("Time Series Data")]
    public TimeSeriesData populationOverTime = new TimeSeriesData();
    public TimeSeriesData foodOverTime = new TimeSeriesData();

    [Header("Death Statistics")]
    public List<DeathData> deathRecords = new List<DeathData>();

    [Header("Current Stats")]
    public int currentPopulation = 0;
    public int currentFood = 0;
    public int totalBirths = 0;
    public int totalDeaths = 0;

    [Header("Simulation Info")]
    public float simulationStartTime;
    public float simulationDuration => Time.time - simulationStartTime;

    public void Reset()
    {
        populationOverTime.Clear();
        foodOverTime.Clear();
        deathRecords.Clear();

        currentPopulation = 0;
        currentFood = 0;
        totalBirths = 0;
        totalDeaths = 0;

        simulationStartTime = Time.time;
    }

    // Helper methods for death statistics
    public Dictionary<string, int> GetDeathCountsByCause()
    {
        Dictionary<string, int> deathCounts = new Dictionary<string, int>();

        foreach (var death in deathRecords)
        {
            if (deathCounts.ContainsKey(death.cause))
                deathCounts[death.cause]++;
            else
                deathCounts[death.cause] = 1;
        }

        return deathCounts;
    }

    public float GetAverageDeathAge()
    {
        if (deathRecords.Count == 0) return 0f;

        float totalAge = 0f;
        foreach (var death in deathRecords)
        {
            totalAge += death.age;
        }

        return totalAge / deathRecords.Count;
    }
}