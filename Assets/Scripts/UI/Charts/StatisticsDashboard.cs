// ============================================================================
// FILE: EnhancedStatisticsDashboard.cs - Updated Dashboard with New Chart Types
// ============================================================================

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class StatisticsDashboard : MonoBehaviour
{
    [Header("New Chart Components")]
    [SerializeField] private DualAxisLineChart populationFoodChart;
    [SerializeField] private HistogramChart deathCauseChart;
    [SerializeField] private HistogramChart ageDistributionChart;

    [Header("Current Stats Display")]
    [SerializeField] private TextMeshProUGUI populationText;
    [SerializeField] private TextMeshProUGUI foodText;
    [SerializeField] private TextMeshProUGUI birthsText;
    [SerializeField] private TextMeshProUGUI deathsText;
    [SerializeField] private TextMeshProUGUI simulationTimeText;

    [Header("Advanced Death Statistics")]
    [SerializeField] private TextMeshProUGUI averageAgeText;
    [SerializeField] private TextMeshProUGUI averageAgeByStarvationText;
    [SerializeField] private TextMeshProUGUI averageAgeByOldAgeText;
    [SerializeField] private TextMeshProUGUI mortalityRateText;

    [Header("Generation Stats")]
    [SerializeField] private TextMeshProUGUI generationText;
    [SerializeField] private TextMeshProUGUI avgLifespanText;

    [Header("Controls")]
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button exportChartsButton;

    [Header("Chart Update Settings")]
    [SerializeField] private float updateInterval = 0.5f;
    [SerializeField] private bool autoUpdate = true;
    [SerializeField] private float ageDistributionBinSize = 10f;

    [Header("Analysis Settings")]
    [SerializeField] private bool showRealTimeAnalysis = true;
    [SerializeField] private int minDeathsForAnalysis = 5;

    private float lastUpdateTime;
    private DeathAgeAnalyzer deathAnalyzer;

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
        SetupButtons();
        SetupCharts();
        Debug.Log("Enhanced Statistics Dashboard initialized");
    }

    private void SetupButtons()
    {
        if (saveButton != null)
            saveButton.onClick.AddListener(() => {
                StatisticsManager.Instance?.SaveStatistics();
                Debug.Log("Statistics saved");
            });

        if (loadButton != null)
            loadButton.onClick.AddListener(LoadLatestSimulation);

        if (resetButton != null)
            resetButton.onClick.AddListener(() => {
                StatisticsManager.Instance?.ResetStatistics();
                ClearAllCharts();
                Debug.Log("Statistics reset");
            });

        if (pauseButton != null)
            pauseButton.onClick.AddListener(() => {
                Time.timeScale = Time.timeScale > 0 ? 0 : 1;
                UpdatePauseButtonText();
            });

        if (exportChartsButton != null)
            exportChartsButton.onClick.AddListener(ExportChartData);
    }

    private void SetupCharts()
    {
        // Setup dual-axis population/food chart
        if (populationFoodChart != null)
        {
            Debug.Log("Population/Food dual-axis chart ready");
        }

        // Setup death cause histogram
        if (deathCauseChart != null)
        {
            Debug.Log("Death cause histogram ready");
        }

        // Setup age distribution histogram
        if (ageDistributionChart != null)
        {
            Debug.Log("Age distribution histogram ready");
        }
    }

    private void UpdateDashboard()
    {
        if (StatisticsManager.Instance == null) return;

        StatisticsData data = StatisticsManager.Instance.Data;

        UpdateCurrentStats(data);
        UpdateCharts(data);
        UpdateAdvancedDeathStatistics(data);
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
        // Update dual-axis population/food chart
        if (populationFoodChart != null && data.populationOverTime.Count > 0)
        {
            populationFoodChart.SetData(
                data.populationOverTime.timestamps,
                data.populationOverTime.values,
                data.foodOverTime.values
            );
        }

        // Update death cause histogram
        if (deathCauseChart != null && data.totalDeaths >= minDeathsForAnalysis)
        {
            var deathCounts = data.GetDeathCountsByCause();
            deathCauseChart.SetDeathCauseData(deathCounts, data.totalDeaths);
        }

        // Update age distribution histogram
        if (ageDistributionChart != null && data.deathRecords.Count >= minDeathsForAnalysis)
        {
            var deathAges = data.deathRecords.Select(d => d.age).ToList();
            ageDistributionChart.SetAgeDistributionData(deathAges, ageDistributionBinSize);
        }
    }

    private void UpdateAdvancedDeathStatistics(StatisticsData data)
    {
        if (data.deathRecords.Count == 0) return;

        // Create death analyzer for advanced statistics
        deathAnalyzer = new DeathAgeAnalyzer(data.deathRecords);

        // Overall average death age
        float avgAge = data.GetAverageDeathAge();
        if (averageAgeText != null)
            averageAgeText.text = $"Avg Death Age: {avgAge:F1}s";

        // Average death age by cause
        float avgStarvationAge = deathAnalyzer.GetAverageDeathAgeForCause("starvation");
        float avgOldAge = deathAnalyzer.GetAverageDeathAgeForCause("old age");

        if (averageAgeByStarvationText != null)
            averageAgeByStarvationText.text = $"Avg Starvation Age: {avgStarvationAge:F1}s";

        if (averageAgeByOldAgeText != null)
            averageAgeByOldAgeText.text = $"Avg Old Age Death: {avgOldAge:F1}s";

        // Calculate mortality rate (deaths per time unit)
        float mortalityRate = CalculateMortalityRate(data);
        if (mortalityRateText != null)
            mortalityRateText.text = $"Mortality Rate: {mortalityRate:F2}/min";
    }

    private float CalculateMortalityRate(StatisticsData data)
    {
        if (data.simulationDuration <= 0) return 0f;

        // Deaths per minute
        float timeInMinutes = data.simulationDuration / 60f;
        return timeInMinutes > 0 ? data.totalDeaths / timeInMinutes : 0f;
    }

    private void UpdateGenerationStats(StatisticsData data)
    {
        // Get highest generation from spawner
        AgentSpawner spawner = FindObjectOfType<AgentSpawner>();
        if (spawner != null && generationText != null)
        {
            generationText.text = $"Highest Gen: {spawner.HighestGeneration}";
        }

        if (avgLifespanText != null)
            avgLifespanText.text = $"Avg Lifespan: {data.GetAverageDeathAge():F1}s";
    }

    private void ClearAllCharts()
    {
        populationFoodChart?.ClearChart();
        deathCauseChart?.ClearChart();
        ageDistributionChart?.ClearChart();
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

    private void LoadLatestSimulation()
    {
        if (StatisticsManager.Instance == null) return;

        string[] availableSaves = StatisticsManager.Instance.GetAvailableSaves();

        if (availableSaves.Length > 0)
        {
            string latestFile = availableSaves[availableSaves.Length - 1];
            StatisticsManager.Instance.LoadStatistics(latestFile);
            Debug.Log($"Loaded latest simulation: {latestFile}");

            // Force update after loading
            UpdateDashboard();
        }
        else
        {
            Debug.Log("No saved simulations found to load");
        }
    }

    private void ExportChartData()
    {
        if (StatisticsManager.Instance == null) return;

        StatisticsData data = StatisticsManager.Instance.Data;

        // Create a comprehensive data export
        string exportData = CreateChartDataExport(data);

        // Save to a text file (you could extend this to CSV, JSON, etc.)
        string filename = $"ChartData_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
        string path = System.IO.Path.Combine(Application.persistentDataPath, filename);

        try
        {
            System.IO.File.WriteAllText(path, exportData);
            Debug.Log($"Chart data exported to: {path}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to export chart data: {e.Message}");
        }
    }

    private string CreateChartDataExport(StatisticsData data)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        sb.AppendLine("=== SIMULATION CHART DATA EXPORT ===");
        sb.AppendLine($"Export Time: {System.DateTime.Now}");
        sb.AppendLine($"Simulation Duration: {data.simulationDuration:F1}s");
        sb.AppendLine();

        // Population over time
        sb.AppendLine("POPULATION OVER TIME:");
        sb.AppendLine("Time(s),Population");
        for (int i = 0; i < data.populationOverTime.Count; i++)
        {
            sb.AppendLine($"{data.populationOverTime.timestamps[i]:F1},{data.populationOverTime.values[i]}");
        }
        sb.AppendLine();

        // Food over time
        sb.AppendLine("FOOD OVER TIME:");
        sb.AppendLine("Time(s),Food");
        for (int i = 0; i < data.foodOverTime.Count; i++)
        {
            sb.AppendLine($"{data.foodOverTime.timestamps[i]:F1},{data.foodOverTime.values[i]}");
        }
        sb.AppendLine();

        // Death causes
        sb.AppendLine("DEATH CAUSES:");
        sb.AppendLine("Cause,Count");
        var deathCounts = data.GetDeathCountsByCause();
        foreach (var kvp in deathCounts)
        {
            sb.AppendLine($"{kvp.Key},{kvp.Value}");
        }
        sb.AppendLine();

        // Death records
        sb.AppendLine("INDIVIDUAL DEATH RECORDS:");
        sb.AppendLine("Cause,Age,Time,Generation");
        foreach (var death in data.deathRecords)
        {
            sb.AppendLine($"{death.cause},{death.age:F1},{death.timestamp:F1},{death.generation}");
        }

        return sb.ToString();
    }

    // Public interface methods
    public void ForceUpdate()
    {
        UpdateDashboard();
    }

    public void SetAutoUpdate(bool enabled)
    {
        autoUpdate = enabled;
    }

    public void SetAgeDistributionBinSize(float binSize)
    {
        ageDistributionBinSize = Mathf.Max(1f, binSize);
        // Force update of age distribution chart
        if (ageDistributionChart != null && StatisticsManager.Instance != null)
        {
            var data = StatisticsManager.Instance.Data;
            if (data.deathRecords.Count >= minDeathsForAnalysis)
            {
                var deathAges = data.deathRecords.Select(d => d.age).ToList();
                ageDistributionChart.SetAgeDistributionData(deathAges, ageDistributionBinSize);
            }
        }
    }

    public void SetMinDeathsForAnalysis(int minDeaths)
    {
        minDeathsForAnalysis = Mathf.Max(1, minDeaths);
    }

    // Debug methods
    [ContextMenu("Force Chart Update")]
    public void ForceChartUpdate()
    {
        ForceUpdate();
    }

    [ContextMenu("Test Death Cause Data")]
    public void TestDeathCauseData()
    {
        if (deathCauseChart != null)
        {
            var testData = new Dictionary<string, int>
            {
                {"starvation", 15},
                {"old age", 8},
                {"disease", 3}
            };
            deathCauseChart.SetDeathCauseData(testData, 26);
        }
    }

    [ContextMenu("Test Age Distribution")]
    public void TestAgeDistribution()
    {
        if (ageDistributionChart != null)
        {
            var testAges = new List<float> { 12f, 25f, 34f, 45f, 23f, 67f, 89f, 34f, 45f, 56f, 78f, 23f, 34f };
            ageDistributionChart.SetAgeDistributionData(testAges, 20f);
        }
    }
}