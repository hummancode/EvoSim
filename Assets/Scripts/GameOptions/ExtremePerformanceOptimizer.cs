using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Extreme performance optimizations for handling 1000+ agents at high speeds
/// WARNING: This will significantly reduce visual quality for performance
/// </summary>
public class ExtremePerformanceOptimizer : MonoBehaviour
{
    [Header("Aggressive Optimizations")]
    [SerializeField] private bool enableExtremeMode = false;
    [SerializeField] private float highSpeedThreshold = 50f;

    [Header("Rendering Optimizations")]
    [SerializeField] private bool disableAnimations = true;
    [SerializeField] private bool reduceSpriteRendering = true;
    [SerializeField] private bool useLODSystem = true;
    [SerializeField] private bool enableInstancing = true;

    [Header("Physics Optimizations")]
    [SerializeField] private bool optimizeCollisions = true;
    [SerializeField] private bool reducePhysicsSteps = true;
    [SerializeField] private bool disableUnusedComponents = true;

    [Header("Memory Optimizations")]
    [SerializeField] private bool enableObjectPooling = true;
    [SerializeField] private bool forceGarbageCollection = true;
    [SerializeField] private float gcInterval = 10f;

    [Header("Visual Reduction")]
    [SerializeField] private bool hideDistantAgents = true;
    [SerializeField] private float maxRenderDistance = 20f;
    [SerializeField] private bool useSimpleShapes = true;

    // Performance state
    private bool extremeModeActive = false;
    private float lastGCTime = 0f;
    private Camera mainCamera;

    // Original settings backup
    private int originalVSyncCount;
    private int originalQualityLevel;
    private ShadowQuality originalShadowQuality;
    private bool originalRealtimeReflections;

    void Start()
    {
        mainCamera = Camera.main;
        BackupOriginalSettings();
    }

    void Update()
    {
        bool shouldUseExtremeMode = enableExtremeMode && (Time.timeScale >= highSpeedThreshold);

        if (shouldUseExtremeMode != extremeModeActive)
        {
            if (shouldUseExtremeMode)
            {
                EnableExtremeMode();
            }
            else
            {
                DisableExtremeMode();
            }
            extremeModeActive = shouldUseExtremeMode;
        }

        if (extremeModeActive)
        {
            PerformRuntimeOptimizations();
        }

        // Periodic garbage collection
        if (forceGarbageCollection && Time.time - lastGCTime > gcInterval)
        {
            System.GC.Collect();
            lastGCTime = Time.time;
        }
    }

    #region Extreme Mode Toggle

    private void EnableExtremeMode()
    {
        Debug.Log("ENABLING EXTREME PERFORMANCE MODE - Visual quality will be severely reduced!");

        // Rendering optimizations
        OptimizeRendering();

        // Physics optimizations
        OptimizePhysics();

        // Component optimizations
        OptimizeComponents();

        // Visual optimizations
        OptimizeVisuals();

        // Unity settings
        OptimizeUnitySettings();
    }

    private void DisableExtremeMode()
    {
        Debug.Log("Disabling extreme performance mode - Restoring visual quality");
        RestoreOriginalSettings();
    }

    #endregion

    #region Rendering Optimizations

    private void OptimizeRendering()
    {
        if (reduceSpriteRendering)
        {
            // Reduce sprite render quality
            var renderers = FindObjectsOfType<SpriteRenderer>();
            foreach (var renderer in renderers)
            {
                // Disable sprite sorting for performance
                renderer.sortingLayerName = "Default";
                renderer.sortingOrder = 0;

                // Reduce sprite filtering
                if (renderer.sprite != null && renderer.sprite.texture != null)
                {
                    renderer.sprite.texture.filterMode = FilterMode.Point;
                }
            }
        }

        if (useLODSystem)
        {
            EnableAgentLODSystem();
        }

        // Disable shadows completely
        QualitySettings.shadows = ShadowQuality.Disable;

        // Disable real-time lighting
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
    }

    private void EnableAgentLODSystem()
    {
        var agents = FindObjectsOfType<AgentController>();

        foreach (var agent in agents)
        {
            // Add LOD component if it doesn't exist
            var lodGroup = agent.GetComponent<LODGroup>();
            if (lodGroup == null)
            {
                lodGroup = agent.gameObject.AddComponent<LODGroup>();

                // Create simple LOD setup
                LOD[] lods = new LOD[2];

                // LOD 0: Full detail (close)
                var renderers = agent.GetComponentsInChildren<Renderer>();
                lods[0] = new LOD(0.5f, renderers);

                // LOD 1: No rendering (far)
                lods[1] = new LOD(0.01f, new Renderer[0]);

                lodGroup.SetLODs(lods);
                lodGroup.RecalculateBounds();
            }
        }
    }

    #endregion

    #region Physics Optimizations

    private void OptimizePhysics()
    {
        if (optimizeCollisions)
        {
            // Reduce collision detection accuracy for performance
            var agents = FindObjectsOfType<AgentController>();
            foreach (var agent in agents)
            {
                var rigidbody = agent.GetComponent<Rigidbody2D>();
                if (rigidbody != null)
                {
                    rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
                    rigidbody.sleepMode = RigidbodySleepMode2D.StartAwake;
                }

                // Disable unnecessary colliders
                var colliders = agent.GetComponents<Collider2D>();
                foreach (var collider in colliders)
                {
                    if (collider.isTrigger)
                    {
                        collider.enabled = false; // Disable trigger colliders in extreme mode
                    }
                }
            }
        }

        if (reducePhysicsSteps)
        {
            // Reduce physics step frequency
            Time.fixedDeltaTime = 0.04f; // 25 Hz instead of 50 Hz
        }
    }

    #endregion

    #region Component Optimizations

    private void OptimizeComponents()
    {
        if (disableUnusedComponents)
        {
            var agents = FindObjectsOfType<AgentController>();

            foreach (var agent in agents)
            {
                // Disable visual components that aren't critical
                DisableNonCriticalComponents(agent.gameObject);
            }
        }
    }

    private void DisableNonCriticalComponents(GameObject agent)
    {
        // Disable particle systems
        var particles = agent.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particles)
        {
            ps.Stop();
            
        }

        // Disable trail renderers
        var trails = agent.GetComponentsInChildren<TrailRenderer>();
        foreach (var trail in trails)
        {
            trail.enabled = false;
        }

        // Disable line renderers
        var lines = agent.GetComponentsInChildren<LineRenderer>();
        foreach (var line in lines)
        {
            line.enabled = false;
        }

        // Disable audio sources
        var audioSources = agent.GetComponentsInChildren<AudioSource>();
        foreach (var audio in audioSources)
        {
            audio.enabled = false;
        }
    }

    #endregion

    #region Visual Optimizations

    private void OptimizeVisuals()
    {
        if (hideDistantAgents)
        {
            EnableDistanceCulling();
        }

        if (useSimpleShapes)
        {
            ReplaceSpritesWithSimpleShapes();
        }
    }

    private void EnableDistanceCulling()
    {
        if (mainCamera == null) return;

        var agents = FindObjectsOfType<AgentController>();
        Vector3 cameraPos = mainCamera.transform.position;

        foreach (var agent in agents)
        {
            float distance = Vector3.Distance(agent.transform.position, cameraPos);
            bool shouldRender = distance <= maxRenderDistance;

            var renderers = agent.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                renderer.enabled = shouldRender;
            }
        }
    }

    private void ReplaceSpritesWithSimpleShapes()
    {
        var agents = FindObjectsOfType<AgentController>();

        foreach (var agent in agents)
        {
            var spriteRenderer = agent.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                // Replace with simple colored quad
                spriteRenderer.sprite = CreateSimpleQuadSprite();
                spriteRenderer.material = CreateUnlitMaterial();
            }
        }
    }

    private Sprite CreateSimpleQuadSprite()
    {
        // Create a simple 1x1 white texture
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f);
    }

    private Material CreateUnlitMaterial()
    {
        // Use unlit shader for maximum performance
        Shader unlitShader = Shader.Find("Unlit/Color");
        Material material = new Material(unlitShader);
        return material;
    }

    #endregion

    #region Unity Settings Optimization

    private void OptimizeUnitySettings()
    {
        // Disable VSync for maximum framerate
        QualitySettings.vSyncCount = 0;

        // Set lowest quality level
        QualitySettings.SetQualityLevel(0, true);

        // Disable expensive rendering features
        QualitySettings.shadows = ShadowQuality.Disable;
        QualitySettings.realtimeReflectionProbes = false;
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
        QualitySettings.antiAliasing = 0;

        // Reduce texture quality
       

        // Optimize application settings
        Application.targetFrameRate = -1; // Unlimited framerate

        // Reduce screen resolution if possible (for extreme cases)
        if (Time.timeScale > 100f)
        {
            Screen.SetResolution(Screen.width / 2, Screen.height / 2, Screen.fullScreen);
        }
    }

    private void BackupOriginalSettings()
    {
        originalVSyncCount = QualitySettings.vSyncCount;
        originalQualityLevel = QualitySettings.GetQualityLevel();
        originalShadowQuality = QualitySettings.shadows;
        originalRealtimeReflections = QualitySettings.realtimeReflectionProbes;
    }

    private void RestoreOriginalSettings()
    {
        QualitySettings.vSyncCount = originalVSyncCount;
        QualitySettings.SetQualityLevel(originalQualityLevel, true);
        QualitySettings.shadows = originalShadowQuality;
        QualitySettings.realtimeReflectionProbes = originalRealtimeReflections;

        // Restore full resolution
        Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, Screen.fullScreen);
    }

    #endregion

    #region Runtime Optimizations

    private void PerformRuntimeOptimizations()
    {
        // Update distance culling every few frames
        if (hideDistantAgents && Time.frameCount % 10 == 0)
        {
            EnableDistanceCulling();
        }

        // Reduce update frequency for non-critical systems
        if (Time.frameCount % 30 == 0)
        {
            OptimizeNonCriticalSystems();
        }
    }

    private void OptimizeNonCriticalSystems()
    {
        // Disable debug visualizers
        var debugVisualizers = FindObjectsOfType<AgentDebugVisualizer>();
        foreach (var visualizer in debugVisualizers)
        {
            visualizer.enabled = false;
        }

        // Reduce UI update frequency
        var uiComponents = FindObjectsOfType<StatisticsUI>();
        foreach (var ui in uiComponents)
        {
            ui.enabled = false; // Disable UI updates completely in extreme mode
        }
    }

    #endregion

    #region Object Pooling

    private void EnableObjectPooling()
    {
        // This would require a more complex implementation
        // For now, just disable instantiation/destruction during extreme mode

        var spawners = FindObjectsOfType<AgentSpawner>();
        foreach (var spawner in spawners)
        {
            // You'd implement a pooling system here
            Debug.Log("Object pooling would be enabled here");
        }
    }

    #endregion

    #region Public Interface

    [ContextMenu("Force Enable Extreme Mode")]
    public void ForceEnableExtremeMode()
    {
        enableExtremeMode = true;
        EnableExtremeMode();
        extremeModeActive = true;
    }

    [ContextMenu("Force Disable Extreme Mode")]
    public void ForceDisableExtremeMode()
    {
        enableExtremeMode = false;
        DisableExtremeMode();
        extremeModeActive = false;
    }

    [ContextMenu("Log Performance Impact")]
    public void LogPerformanceImpact()
    {
        Debug.Log($"=== EXTREME PERFORMANCE MODE ===");
        Debug.Log($"Active: {extremeModeActive}");
        Debug.Log($"Time Scale: {Time.timeScale:F0}x");
        Debug.Log($"Current FPS: {1f/Time.unscaledDeltaTime:F1}");
        Debug.Log($"Physics Step: {Time.fixedDeltaTime*1000:F1}ms");
        Debug.Log($"VSync: {QualitySettings.vSyncCount}");
        Debug.Log($"Quality Level: {QualitySettings.names[QualitySettings.GetQualityLevel()]}");
        Debug.Log($"Shadows: {QualitySettings.shadows}");
    }

    public bool IsExtremeModeActive() => extremeModeActive;
    public float GetCurrentFPS() => 1f / Time.unscaledDeltaTime;

    #endregion
}