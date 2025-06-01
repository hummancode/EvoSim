using System.IO;
using UnityEngine;

public enum SaveLocation
{
    PersistentData,    // Cross-platform safe location
    Assets,           // Inside project (development only)
    Desktop,          // User's desktop
    Documents         // User's documents folder
}


public class StatisticsManager : MonoBehaviour, IStatisticsReporter
{
    private static StatisticsManager instance;
    public static StatisticsManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("StatisticsManager");
                instance = obj.AddComponent<StatisticsManager>();
                DontDestroyOnLoad(obj);
            }
            return instance;
        }
    }

    [Header("Settings")]
    [SerializeField] private float updateInterval = 1.0f; // Update every second
    [SerializeField] private bool enableLogging = true;

    [Header("Save/Load Settings")]
    [SerializeField] private bool autoSaveOnExit = false;
    [SerializeField] private bool autoSaveOnSimulationEnd = false;
    [SerializeField] private float autoSaveInterval = 300f; // Auto-save every 5 minutes (0 = disabled)
    [SerializeField] private SaveLocation saveLocation = SaveLocation.PersistentData;
    [SerializeField] private string saveFolder = "SimulationData";
    [SerializeField] private string baseFileName = "Simulation";

    private float lastAutoSaveTime;
    private string currentSimulationName;
    private bool simulationStarted = false;

    [Header("Data")]
    [SerializeField] private StatisticsData data = new StatisticsData();

    private float lastUpdateTime;

    // Properties
    public StatisticsData Data => data;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeStatistics();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitializeStatistics();
    }

    private void Update()
    {
        // Update time series data at regular intervals
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            RecordTimeSeriesData();
            lastUpdateTime = Time.time;
        }

        // Auto-save at intervals if enabled
        if (autoSaveInterval > 0 && Time.time - lastAutoSaveTime >= autoSaveInterval)
        {
            AutoSave();
            lastAutoSaveTime = Time.time;
        }
    }

    private void InitializeStatistics()
    {
        data.Reset();
        lastUpdateTime = Time.time;
        lastAutoSaveTime = Time.time;

        // Generate unique simulation name
        currentSimulationName = GenerateSimulationName();
        simulationStarted = true;

        if (enableLogging)
            Debug.Log($"Statistics Manager initialized for simulation: {currentSimulationName}");
    }

    private void RecordTimeSeriesData()
    {
        float currentTime = data.simulationDuration;

        // Record population over time
        data.populationOverTime.AddDataPoint(currentTime, data.currentPopulation);

        // Record food over time
        data.foodOverTime.AddDataPoint(currentTime, data.currentFood);
    }

    // IStatisticsReporter Implementation
    public void ReportAgentBorn()
    {
        data.currentPopulation++;
        data.totalBirths++;

        if (enableLogging)
            Debug.Log($"Agent born. Population: {data.currentPopulation}");
    }

    public void ReportAgentDied(float age, string cause, int generation = 1)
    {
        data.currentPopulation = Mathf.Max(0, data.currentPopulation - 1);
        data.totalDeaths++;

        // Record death details
        DeathData deathRecord = new DeathData(cause, age, data.simulationDuration, generation);
        data.deathRecords.Add(deathRecord);

        if (enableLogging)
            Debug.Log($"Agent died: {cause} at age {age:F1}. Population: {data.currentPopulation}");
    }

    public void ReportFoodCount(int count)
    {
        data.currentFood = count;
    }

    // Public utility methods
    public void ResetStatistics()
    {
        data.Reset();
        lastUpdateTime = Time.time;
        lastAutoSaveTime = Time.time;

        // Generate new simulation name for the reset simulation
        currentSimulationName = GenerateSimulationName();
        simulationStarted = true;

        if (enableLogging)
            Debug.Log($"Statistics reset - New simulation: {currentSimulationName}");
    }

    // End simulation (trigger auto-save if enabled)
    public void EndSimulation()
    {
        if (autoSaveOnSimulationEnd && simulationStarted)
        {
            SaveStatistics();
        }

        simulationStarted = false;

        if (enableLogging)
            Debug.Log($"Simulation ended: {currentSimulationName}");
    }

    // Save/Load functionality with enhanced features
    public void SaveStatistics(string customFileName = null)
    {
        try
        {
            // Create save directory
            string savePath = Path.Combine(Application.persistentDataPath, saveFolder);
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            // Use custom filename or generate automatic one
            string fileName = customFileName ?? $"{currentSimulationName}.json";
            string filePath = Path.Combine(savePath, fileName);

            // Add metadata to save data
            var saveData = new SimulationSaveData
            {
                simulationName = currentSimulationName,
                saveTimestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                statistics = data
            };

            string jsonData = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(filePath, jsonData);

            if (enableLogging)
                Debug.Log($"Statistics saved to: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save statistics: {e.Message}");
        }
    }

    public void LoadStatistics(string fileName)
    {
        try
        {
            string savePath = Path.Combine(Application.persistentDataPath, saveFolder);
            string filePath = Path.Combine(savePath, fileName);

            if (File.Exists(filePath))
            {
                string jsonData = File.ReadAllText(filePath);
                var saveData = JsonUtility.FromJson<SimulationSaveData>(jsonData);

                data = saveData.statistics;
                currentSimulationName = saveData.simulationName;

                if (enableLogging)
                    Debug.Log($"Statistics loaded: {saveData.simulationName} (saved on {saveData.saveTimestamp})");
            }
            else
            {
                Debug.LogWarning($"Statistics file not found: {filePath}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to load statistics: {e.Message}");
        }
    }

    // Get list of available save files
    public string[] GetAvailableSaves()
    {
        try
        {
            string savePath = Path.Combine(Application.persistentDataPath, saveFolder);
            if (!Directory.Exists(savePath))
                return new string[0];

            string[] files = Directory.GetFiles(savePath, "*.json");

            // Return just the filenames without path
            for (int i = 0; i < files.Length; i++)
            {
                files[i] = Path.GetFileName(files[i]);
            }

            return files;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to get save files: {e.Message}");
            return new string[0];
        }
    }

    // Get the appropriate save path based on settings
    private string GetSavePath()
    {
        // For now, let's use a simple approach to debug
        string basePath;

        try
        {
            switch (saveLocation)
            {
                case SaveLocation.Assets:
                    basePath = Path.Combine(Application.dataPath, "SavedSimulations");
                    break;

                case SaveLocation.Desktop:
                    basePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), "EvolutionSim");
                    break;

                case SaveLocation.Documents:
                    basePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), "EvolutionSim");
                    break;

                case SaveLocation.PersistentData:
                default:
                    basePath = Application.persistentDataPath;
                    break;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error determining save path, falling back to persistent data: {e.Message}");
            basePath = Application.persistentDataPath;
        }

        return Path.Combine(basePath, saveFolder);
    }

    // Auto-save functionality
    private void AutoSave()
    {
        if (simulationStarted && data.totalBirths > 0) // Only auto-save if simulation has activity
        {
            SaveStatistics();
        }
    }

    // Generate unique simulation name
    private string GenerateSimulationName()
    {
        string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        return $"{baseFileName}_{timestamp}";
    }

    // Manual save with custom name
    public void SaveWithCustomName(string customName)
    {
        string fileName = $"{customName}_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.json";
        SaveStatistics(fileName);
    }

    // Application lifecycle handlers
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus && autoSaveOnExit)
        {
            AutoSave();
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus && autoSaveOnExit)
        {
            AutoSave();
        }
    }

    private void OnDestroy()
    {
        if (autoSaveOnExit && simulationStarted)
        {
            AutoSave();
        }
    }

    // Debug method to get current stats summary
    public string GetStatsSummary()
    {
        return $"Simulation: {currentSimulationName}\n" +
               $"Population: {data.currentPopulation}, Food: {data.currentFood}, " +
               $"Births: {data.totalBirths}, Deaths: {data.totalDeaths}, " +
               $"Time: {data.simulationDuration:F1}s";
    }

    // Properties for external access
    public string CurrentSimulationName => currentSimulationName;
    public string SaveFolderPath => GetSavePath();
    public bool IsAutoSaveEnabled => autoSaveOnExit || autoSaveOnSimulationEnd || autoSaveInterval > 0;

    // Debug method to test save location
    [ContextMenu("Test Save Location")]
    private void TestSaveLocation()
    {
        Debug.Log($"=== SAVE LOCATION TEST ===");
        Debug.Log($"Save Location Setting: {saveLocation}");
        Debug.Log($"Calculated Path: {GetSavePath()}");
        Debug.Log($"Directory Exists: {Directory.Exists(GetSavePath())}");

        // Test save
        try
        {
            SaveStatistics("TEST_FILE.json");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Test save failed: {e.Message}");
        }
    }

    // TODO: Future refactoring candidates
    // - Move save/load logic to StatisticsSaveManager
    // - Move auto-save logic to AutoSaveController  
    // - Move file management to FileManager
    // - Keep only core data collection here
}

// Enhanced save data structure with metadata
[System.Serializable]
public class SimulationSaveData
{
    public string simulationName;
    public string saveTimestamp;
    public StatisticsData statistics;
}
