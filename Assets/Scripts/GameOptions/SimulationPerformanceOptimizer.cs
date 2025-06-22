using UnityEngine;

/// <summary>
/// Performance optimizer that adjusts Unity settings for high-speed simulation
/// </summary>
public class SimulationPerformanceOptimizer : MonoBehaviour
{
    [Header("FPS Optimization")]
    [SerializeField] private bool enableFPSOptimization = true;
    [SerializeField] private int targetFPSForHighSpeed = 120; // Higher FPS for high speed
    [SerializeField] private float highSpeedThreshold = 10f;

    [Header("Physics Optimization")]
    [SerializeField] private bool optimizePhysics = true;
    [SerializeField] private float baseFixedDeltaTime = 0.02f; // 50 Hz physics
    [SerializeField] private float minFixedDeltaTime = 0.001f; // 1000 Hz max

    [Header("Rendering Optimization")]
    [SerializeField] private bool optimizeRendering = true;
    [SerializeField] private int baseVSyncCount = 1;

    [Header("Quality Adjustments")]
    [SerializeField] private bool enableQualityAdjustments = true;
    [SerializeField] private string highSpeedQualityLevel = "Fast";
    [SerializeField] private string normalSpeedQualityLevel = "Good";

    private float originalFixedDeltaTime;
    private int originalVSyncCount;
    private int originalQualityLevel;
    private int originalTargetFrameRate;

    void Start()
    {
        // Store original settings
        originalFixedDeltaTime = Time.fixedDeltaTime;
        originalVSyncCount = QualitySettings.vSyncCount;
        originalQualityLevel = QualitySettings.GetQualityLevel();
        originalTargetFrameRate = Application.targetFrameRate;

        ApplyOptimizations();
    }

    void Update()
    {
        // Continuously adjust settings based on current speed
        if (Time.frameCount % 30 == 0) // Check every 30 frames
        {
            ApplyOptimizations();
        }
    }

    private void ApplyOptimizations()
    {
        bool isHighSpeed = Time.timeScale >= highSpeedThreshold;

        if (enableFPSOptimization)
        {
            OptimizeFPS(isHighSpeed);
        }

        if (optimizePhysics)
        {
            OptimizePhysics();
        }

        if (optimizeRendering)
        {
            OptimizeRendering(isHighSpeed);
        }

        if (enableQualityAdjustments)
        {
            AdjustQualitySettings(isHighSpeed);
        }
    }

    private void OptimizeFPS(bool isHighSpeed)
    {
        if (isHighSpeed)
        {
            // Higher target FPS for high speed simulation
            Application.targetFrameRate = targetFPSForHighSpeed;
        }
        else
        {
            // Restore original or use -1 for unlimited
            Application.targetFrameRate = originalTargetFrameRate;
        }
    }

    private void OptimizePhysics()
    {
        // Adjust physics timestep based on game speed
        // At higher speeds, we need smaller physics steps for accuracy
        float adjustedDeltaTime = baseFixedDeltaTime / Time.timeScale;

        // But don't go below minimum (performance limit)
        adjustedDeltaTime = Mathf.Max(adjustedDeltaTime, minFixedDeltaTime);

        Time.fixedDeltaTime = adjustedDeltaTime;
    }

    private void OptimizeRendering(bool isHighSpeed)
    {
        if (isHighSpeed)
        {
            // Disable VSync for higher FPS
            QualitySettings.vSyncCount = 0;
        }
        else
        {
            // Restore VSync
            QualitySettings.vSyncCount = originalVSyncCount;
        }
    }

    private void AdjustQualitySettings(bool isHighSpeed)
    {
        string[] qualityNames = QualitySettings.names;

        if (isHighSpeed)
        {
            // Find and set high-speed quality level
            for (int i = 0; i < qualityNames.Length; i++)
            {
                if (qualityNames[i].Contains(highSpeedQualityLevel))
                {
                    QualitySettings.SetQualityLevel(i, true);
                    break;
                }
            }
        }
        else
        {
            // Find and set normal quality level
            for (int i = 0; i < qualityNames.Length; i++)
            {
                if (qualityNames[i].Contains(normalSpeedQualityLevel))
                {
                    QualitySettings.SetQualityLevel(i, true);
                    break;
                }
            }
        }
    }

    void OnDestroy()
    {
        // Restore original settings
        Time.fixedDeltaTime = originalFixedDeltaTime;
        QualitySettings.vSyncCount = originalVSyncCount;
        QualitySettings.SetQualityLevel(originalQualityLevel, true);
        Application.targetFrameRate = originalTargetFrameRate;
    }

    // Public methods for external control
    public void SetHighSpeedMode(bool enabled)
    {
        ApplyOptimizations();
    }

    [ContextMenu("Log Current Settings")]
    public void LogCurrentSettings()
    {
        Debug.Log($"=== PERFORMANCE SETTINGS ===\n" +
                 $"Time Scale: {Time.timeScale:F1}x\n" +
                 $"Target FPS: {Application.targetFrameRate}\n" +
                 $"Current FPS: {1f/Time.unscaledDeltaTime:F1}\n" +
                 $"Fixed Delta Time: {Time.fixedDeltaTime*1000:F2}ms\n" +
                 $"VSync Count: {QualitySettings.vSyncCount}\n" +
                 $"Quality Level: {QualitySettings.names[QualitySettings.GetQualityLevel()]}");
    }
}