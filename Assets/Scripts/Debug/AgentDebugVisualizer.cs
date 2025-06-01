using UnityEngine;

/// <summary>
/// Debug visualization component for agents - shows detection ranges and other debug info
/// </summary>
public class AgentDebugVisualizer : MonoBehaviour
{
    [Header("Debug Visualization Controls")]
    [SerializeField] private bool showMateDetectionRange = true;
    [SerializeField] private bool showFoodDetectionRange = true;
    [SerializeField] private bool showMatingProximity = true;
    [SerializeField] private bool showOnlyWhenSelected = false; // If true, only shows when agent is selected

    [Header("Colors")]
    [SerializeField] private Color mateDetectionColor = Color.red;
    [SerializeField] private Color foodDetectionColor = Color.yellow;
    [SerializeField] private Color matingProximityColor = Color.magenta;

    [Header("Alpha Settings")]
    [SerializeField] private float wireAlpha = 0.8f;
    [SerializeField] private float solidAlpha = 0.1f;

    // Component references (cached for performance)
    private SensorSystem sensorSystem;
    private ReproductionSystem reproductionSystem;
    private AgentContext context;

    void Awake()
    {
        // Cache component references
        RefreshComponents();
    }

    /// <summary>
    /// Refresh component references (call if components change)
    /// </summary>
    public void RefreshComponents()
    {
        sensorSystem = GetComponent<SensorSystem>();
        reproductionSystem = GetComponent<ReproductionSystem>();

        // Try to get context from AgentController
        var agentController = GetComponent<AgentController>();
        if (agentController != null)
        {
            context = agentController.GetContext();
        }
    }

    /// <summary>
    /// Draw gizmos when object is selected (more detailed view)
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (showOnlyWhenSelected)
        {
            DrawAllGizmos();
        }
        else
        {
            // When selected, draw with full alpha for better visibility
            DrawGizmosWithAlpha(1.0f, 0.3f);
        }
    }

    /// <summary>
    /// Draw gizmos always (less detailed view)
    /// </summary>
    void OnDrawGizmos()
    {
        if (!showOnlyWhenSelected)
        {
            DrawAllGizmos();
        }
    }

    /// <summary>
    /// Draw all debug gizmos
    /// </summary>
    private void DrawAllGizmos()
    {
        DrawGizmosWithAlpha(wireAlpha, solidAlpha);
    }

    /// <summary>
    /// Draw gizmos with specified alpha values
    /// </summary>
    private void DrawGizmosWithAlpha(float wireAlpha, float solidAlpha)
    {
        Vector3 position = transform.position;

        // Draw food detection range (yellow)
        if (showFoodDetectionRange && sensorSystem != null)
        {
            float foodRange = sensorSystem.GetDetectionRange();
            DrawCircle(position, foodRange, foodDetectionColor, wireAlpha, solidAlpha);
        }

        // Draw mate detection range (red)
        if (showMateDetectionRange)
        {
            float mateRange = GetMateDetectionRange();
            if (mateRange > 0)
            {
                DrawCircle(position, mateRange, mateDetectionColor, wireAlpha, solidAlpha);
            }
        }

        // Draw mating proximity (magenta)
        if (showMatingProximity && reproductionSystem != null)
        {
            float matingProximity = reproductionSystem.MatingProximity;
            DrawCircle(position, matingProximity, matingProximityColor, wireAlpha, solidAlpha);
        }
    }

    /// <summary>
    /// Draw a circle with both wire and solid versions
    /// </summary>
    private void DrawCircle(Vector3 center, float radius, Color baseColor, float wireAlpha, float solidAlpha)
    {
        // Draw wire circle
        Color wireColor = baseColor;
        wireColor.a = wireAlpha;
        Gizmos.color = wireColor;
        Gizmos.DrawWireSphere(center, radius);

        // Draw solid circle (more transparent)
        Color solidColor = baseColor;
        solidColor.a = solidAlpha;
        Gizmos.color = solidColor;
        Gizmos.DrawSphere(center, radius);
    }

    /// <summary>
    /// Get mate detection range from various sources
    /// </summary>
    private float GetMateDetectionRange()
    {
        // Try to get from context first
        if (context?.MateFinder is SensorMateFinder sensorMateFinder)
        {
            return sensorMateFinder.GetMateDetectionRange();
        }

        // Fallback: try to get from reproduction config
        if (reproductionSystem != null)
        {
            var config = reproductionSystem.GetConfig();
            if (config != null)
            {
                return config.mateDetectionRange;
            }
        }

        // Final fallback: use sensor system range
        return sensorSystem?.GetDetectionRange() ?? 0f;
    }

    // ========================================================================
    // PUBLIC METHODS FOR RUNTIME CONTROL
    // ========================================================================

    public void ToggleMateDetectionRange()
    {
        showMateDetectionRange = !showMateDetectionRange;
    }

    public void ToggleFoodDetectionRange()
    {
        showFoodDetectionRange = !showFoodDetectionRange;
    }

    public void ToggleMatingProximity()
    {
        showMatingProximity = !showMatingProximity;
    }

    public void ToggleSelectedOnly()
    {
        showOnlyWhenSelected = !showOnlyWhenSelected;
    }

    public void SetMateDetectionColor(Color color)
    {
        mateDetectionColor = color;
    }

    public void SetFoodDetectionColor(Color color)
    {
        foodDetectionColor = color;
    }

    public void SetMatingProximityColor(Color color)
    {
        matingProximityColor = color;
    }
}