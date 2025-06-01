using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class StatisticsDashboard : MonoBehaviour
{
    [Header("Charts")]
    [SerializeField] private SimpleLineChart populationChart;
    [SerializeField] private SimpleLineChart foodChart;
    [SerializeField] private SimpleLineChart birthDeathChart;

    [Header("Current Stats Display")]
    [SerializeField] private TextMeshProUGUI populationText;
    [SerializeField] private TextMeshProUGUI foodText;
    [SerializeField] private TextMeshProUGUI birthsText;
    [SerializeField] private TextMeshProUGUI deathsText;
    [SerializeField] private TextMeshProUGUI simulationTimeText;

    [Header("Death Statistics")]
    [SerializeField] private TextMeshProUGUI deathCausesText;
    [SerializeField] private TextMeshProUGUI averageAgeText;
    [SerializeField] private Transform deathCausesParent;
    [SerializeField] private GameObject deathCauseBarPrefab;

    [Header("Generation Stats")]
    [SerializeField] private TextMeshProUGUI generationText;
    [SerializeField] private TextMeshProUGUI avgLifespanText;

    [Header("Controls")]
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button pauseButton;

    [Header("Settings")]
    [SerializeField] private float updateInterval = 0.5f;
    [SerializeField] private bool autoUpdate = true;

    private float lastUpdateTime;
    private List<GameObject> deathCauseBars = new List<GameObject>();

    void Start()
    {
        SetupDashboard();
    }

    void Update()
    {
        if (autoUpdate && Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateDashboard();
            lastUpdateTime = Time.time;
        }
    }

    private void SetupDashboard()
    {
        // Setup chart titles and colors
        if (populationChart != null)
        {
            populationChart.SetTitle("Population Over Time");
            populationChart.SetLineColor(Color.blue);
        }

        if (foodChart != null)
        {
            foodChart.SetTitle("Food Over Time");
            foodChart.SetLineColor(Color.green);
        }

        if (birthDeathChart != null)
        {
            birthDeathChart.SetTitle("Birth Rate vs Death Rate");
            birthDeathChart.SetLineColor(Color.red);
        }

        // Setup buttons
        SetupButtons();

        // Create death cause bar prefab if needed
        if (deathCauseBarPrefab == null)
            CreateDeathCauseBarPrefab();
    }

    private void SetupButtons()
    {
        if (saveButton != null)
            saveButton.onClick.AddListener(() => {
                // For now, just log - we'll implement proper saving later
                Debug.Log("Save button clicked - saving functionality to be implemented");
            });

        if (loadButton != null)
            loadButton.onClick.AddListener(() => {
                Debug.Log("Load button clicked - loading functionality to be implemented");
            });

        if (resetButton != null)
            resetButton.onClick.AddListener(() => {
                if (StatisticsManager.Instance != null)
                {
                    StatisticsManager.Instance.ResetStatistics();
                    ClearAllCharts();
                }
            });

        if (pauseButton != null)
            pauseButton.onClick.AddListener(() => {
                Time.timeScale = Time.timeScale > 0 ? 0 : 1;
                UpdatePauseButtonText();
            });
    }

    private void UpdateDashboard()
    {
        if (StatisticsManager.Instance == null) return;

        StatisticsData data = StatisticsManager.Instance.Data;

        UpdateCurrentStats(data);
        UpdateCharts(data);
        UpdateDeathStatistics(data);
        UpdateGenerationStats(data);
    }

    private void UpdateCurrentStats(StatisticsData data)
    {
        if (populationText != null)
            populationText.text = $"Population: {data.currentPopulation}";

        if (foodText != null)
            foodText.text = $"Food: {data.currentFood}";

        if (birthsText != null)
            birthsText.text = $"Total Births: {data.totalBirths}";

        if (deathsText != null)
            deathsText.text = $"Total Deaths: {data.totalDeaths}";

        if (simulationTimeText != null)
            simulationTimeText.text = $"Time: {data.simulationDuration:F1}s";
    }

    private void UpdateCharts(StatisticsData data)
    {
        // Update population chart
        if (populationChart != null && data.populationOverTime.Count > 0)
        {
            populationChart.SetData(data.populationOverTime.timestamps, data.populationOverTime.values);
        }

        // Update food chart
        if (foodChart != null && data.foodOverTime.Count > 0)
        {
            foodChart.SetData(data.foodOverTime.timestamps, data.foodOverTime.values);
        }

        // Update birth/death rate chart
        if (birthDeathChart != null)
        {
            UpdateBirthDeathChart(data);
        }
    }

    private void UpdateBirthDeathChart(StatisticsData data)
    {
        // Calculate birth rate (births per time unit)
        List<float> times = new List<float>();
        List<float> deathRate = new List<float>();

        if (data.deathRecords.Count > 0)
        {
            // Group deaths by time windows (e.g., every 10 seconds)
            float timeWindow = 10f;
            float maxTime = data.simulationDuration;

            for (float t = 0; t < maxTime; t += timeWindow)
            {
                int deathsInWindow = data.deathRecords.Count(d => d.timestamp >= t && d.timestamp < t + timeWindow);
                times.Add(t);
                deathRate.Add(deathsInWindow / timeWindow); // Deaths per second
            }

            birthDeathChart.SetData(times, deathRate);
        }
    }

    private void UpdateDeathStatistics(StatisticsData data)
    {
        var deathCounts = data.GetDeathCountsByCause();
        float avgAge = data.GetAverageDeathAge();

        if (averageAgeText != null)
            averageAgeText.text = $"Avg Death Age: {avgAge:F1}s";

        // Update death causes text
        if (deathCausesText != null)
        {
            string deathText = "Death Causes:\n";
            foreach (var kvp in deathCounts)
            {
                float percentage = (float)kvp.Value / data.totalDeaths * 100f;
                deathText += $"{kvp.Key}: {kvp.Value} ({percentage:F1}%)\n";
            }
            deathCausesText.text = deathText;
        }

        // Update death cause bars
        UpdateDeathCauseBars(deathCounts, data.totalDeaths);
    }

    private void UpdateDeathCauseBars(Dictionary<string, int> deathCounts, int totalDeaths)
    {
        if (deathCausesParent == null) return;

        // Clear existing bars
        ClearDeathCauseBars();

        if (totalDeaths == 0) return;

        // Create bars for each death cause
        float maxBarWidth = 200f;
        int maxDeaths = deathCounts.Values.Max();

        foreach (var kvp in deathCounts)
        {
            GameObject barObj = Instantiate(deathCauseBarPrefab, deathCausesParent);

            // Set bar width based on count
            float barWidth = (float)kvp.Value / maxDeaths * maxBarWidth;
            RectTransform barRect = barObj.GetComponent<RectTransform>();
            barRect.sizeDelta = new Vector2(barWidth, 20f);

            // Set label
            TextMeshProUGUI label = barObj.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                float percentage = (float)kvp.Value / totalDeaths * 100f;
                label.text = $"{kvp.Key}: {kvp.Value} ({percentage:F1}%)";
            }

            // Set color based on cause
            Image barImage = barObj.GetComponent<Image>();
            if (barImage != null)
            {
                barImage.color = GetColorForDeathCause(kvp.Key);
            }

            deathCauseBars.Add(barObj);
        }
    }

    private void UpdateGenerationStats(StatisticsData data)
    {
        // This would require additional data tracking
        // For now, just show placeholder
        if (generationText != null)
            generationText.text = "Highest Gen: N/A";

        if (avgLifespanText != null)
            avgLifespanText.text = $"Avg Lifespan: {data.GetAverageDeathAge():F1}s";
    }

    private Color GetColorForDeathCause(string cause)
    {
        switch (cause.ToLower())
        {
            case "starvation": return Color.red;
            case "old age": return Color.blue;
            case "disease": return Color.yellow;
            default: return Color.gray;
        }
    }

    private void ClearDeathCauseBars()
    {
        foreach (GameObject bar in deathCauseBars)
        {
            if (bar != null)
                DestroyImmediate(bar);
        }
        deathCauseBars.Clear();
    }

    private void ClearAllCharts()
    {
        populationChart?.ClearChart();
        foodChart?.ClearChart();
        birthDeathChart?.ClearChart();
        ClearDeathCauseBars();
    }

    private void CreateDeathCauseBarPrefab()
    {
        GameObject prefab = new GameObject("DeathCauseBar");
        prefab.AddComponent<RectTransform>();

        // Add background image
        Image bgImage = prefab.AddComponent<Image>();
        bgImage.color = Color.gray;

        // Add text label
        GameObject textObj = new GameObject("Label");
        textObj.transform.SetParent(prefab.transform);
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "Death Cause";
        text.fontSize = 12;
        text.color = Color.white;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        deathCauseBarPrefab = prefab;
    }

    private void UpdatePauseButtonText()
    {
        if (pauseButton != null)
        {
            TextMeshProUGUI buttonText = pauseButton.GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.text = Time.timeScale > 0 ? "Pause" : "Resume";
            }
        }
    }

    // Public methods for manual control
    public void ForceUpdate()
    {
        UpdateDashboard();
    }

    public void SetAutoUpdate(bool enabled)
    {
        autoUpdate = enabled;
    }
}