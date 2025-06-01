using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class StatisticsUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI currentStatsText;
    [SerializeField] private TextMeshProUGUI deathStatsText;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button resetButton;

    [Header("Chart Settings")]
    [SerializeField] private RectTransform populationChartParent;
    [SerializeField] private RectTransform foodChartParent;
    [SerializeField] private GameObject linePointPrefab; // Simple UI Image for chart points
    [SerializeField] private int maxChartPoints = 100;

    [Header("Update Settings")]
    [SerializeField] private float uiUpdateInterval = 0.5f;

    private float lastUIUpdate;
    private List<GameObject> populationChartPoints = new List<GameObject>();
    private List<GameObject> foodChartPoints = new List<GameObject>();

    private void Start()
    {
        SetupUI();
    }

    private void Update()
    {
        if (Time.time - lastUIUpdate >= uiUpdateInterval)
        {
            UpdateUI();
            lastUIUpdate = Time.time;
        }
    }

    private void SetupUI()
    {
        // Setup buttons
        if (saveButton != null)
            saveButton.onClick.AddListener(() => StatisticsManager.Instance.SaveStatistics());

        if (loadButton != null)
            loadButton.onClick.AddListener(() => LoadLatestSimulation());

        if (resetButton != null)
            resetButton.onClick.AddListener(() => StatisticsManager.Instance.ResetStatistics());

        // Create line point prefab if not assigned
        if (linePointPrefab == null)
        {
            CreateDefaultLinePointPrefab();
        }
    }

    private void UpdateUI()
    {
        if (StatisticsManager.Instance == null) return;

        StatisticsData data = StatisticsManager.Instance.Data;

        UpdateCurrentStats(data);
        UpdateDeathStats(data);
        UpdateCharts(data);
    }

    private void UpdateCurrentStats(StatisticsData data)
    {
        if (currentStatsText == null) return;

        currentStatsText.text = $"CURRENT STATS\n" +
                               $"Population: {data.currentPopulation}\n" +
                               $"Food: {data.currentFood}\n" +
                               $"Total Births: {data.totalBirths}\n" +
                               $"Total Deaths: {data.totalDeaths}\n" +
                               $"Simulation Time: {data.simulationDuration:F1}s";
    }

    private void UpdateDeathStats(StatisticsData data)
    {
        if (deathStatsText == null) return;

        var deathCounts = data.GetDeathCountsByCause();
        float avgAge = data.GetAverageDeathAge();

        string deathText = "DEATH STATISTICS\n";
        deathText += $"Average Death Age: {avgAge:F1}\n";
        deathText += "Deaths by Cause:\n";

        foreach (var kvp in deathCounts)
        {
            deathText += $"  {kvp.Key}: {kvp.Value}\n";
        }

        deathStatsText.text = deathText;
    }

    private void UpdateCharts(StatisticsData data)
    {
        UpdateChart(data.populationOverTime, populationChartParent, populationChartPoints, Color.blue);
        UpdateChart(data.foodOverTime, foodChartParent, foodChartPoints, Color.green);
    }

    private void UpdateChart(TimeSeriesData seriesData, RectTransform chartParent, List<GameObject> chartPoints, Color color)
    {
        if (chartParent == null || seriesData.Count == 0) return;

        // Clear old points if we have too many
        while (chartPoints.Count > maxChartPoints || chartPoints.Count > seriesData.Count)
        {
            if (chartPoints.Count > 0)
            {
                DestroyImmediate(chartPoints[0]);
                chartPoints.RemoveAt(0);
            }
        }

        // Get chart dimensions
        Rect chartRect = chartParent.rect;
        float chartWidth = chartRect.width;
        float chartHeight = chartRect.height;

        if (chartWidth <= 0 || chartHeight <= 0) return;

        // Calculate data ranges
        float minTime = seriesData.timestamps.Count > 0 ? seriesData.timestamps.Min() : 0;
        float maxTime = seriesData.timestamps.Count > 0 ? seriesData.timestamps.Max() : 1;
        float minValue = seriesData.values.Count > 0 ? seriesData.values.Min() : 0;
        float maxValue = seriesData.values.Count > 0 ? seriesData.values.Max() : 1;

        // Prevent division by zero
        if (maxTime - minTime < 0.01f) maxTime = minTime + 1f;
        if (maxValue - minValue < 0.01f) maxValue = minValue + 1f;

        // Update existing points or create new ones
        for (int i = 0; i < seriesData.Count; i++)
        {
            GameObject point;

            if (i < chartPoints.Count)
            {
                point = chartPoints[i];
            }
            else
            {
                point = Instantiate(linePointPrefab, chartParent);
                chartPoints.Add(point);
            }

            // Calculate position
            float normalizedX = (seriesData.timestamps[i] - minTime) / (maxTime - minTime);
            float normalizedY = (seriesData.values[i] - minValue) / (maxValue - minValue);

            float posX = normalizedX * chartWidth - (chartWidth * 0.5f);
            float posY = normalizedY * chartHeight - (chartHeight * 0.5f);

            // Set position
            RectTransform pointRect = point.GetComponent<RectTransform>();
            pointRect.anchoredPosition = new Vector2(posX, posY);

            // Set color
            Image pointImage = point.GetComponent<Image>();
            if (pointImage != null)
            {
                pointImage.color = color;
            }
        }
    }

    private void CreateDefaultLinePointPrefab()
    {
        GameObject prefab = new GameObject("LinePoint");
        prefab.AddComponent<RectTransform>();

        Image image = prefab.AddComponent<Image>();
        image.color = Color.white;

        RectTransform rect = prefab.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(2, 2);

        linePointPrefab = prefab;
    }

    // Public methods for manual updates
    public void ForceUpdateUI()
    {
        UpdateUI();
    }

    public void ClearCharts()
    {
        ClearChartPoints(populationChartPoints);
        ClearChartPoints(foodChartPoints);
    }

    private void ClearChartPoints(List<GameObject> points)
    {
        foreach (GameObject point in points)
        {
            if (point != null)
                DestroyImmediate(point);
        }
        points.Clear();
    }

    // Helper method to load the most recent simulation
    private void LoadLatestSimulation()
    {
        string[] availableSaves = StatisticsManager.Instance.GetAvailableSaves();

        if (availableSaves.Length > 0)
        {
            // Load the most recent file (assuming they're sorted by name/date)
            string latestFile = availableSaves[availableSaves.Length - 1];
            StatisticsManager.Instance.LoadStatistics(latestFile);
            Debug.Log($"Loaded latest simulation: {latestFile}");
        }
        else
        {
            Debug.Log("No saved simulations found to load");
        }
    }
}