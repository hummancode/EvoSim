#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// PERFORMANCE-OPTIMIZED Debug Console - reduces FPS impact
/// </summary>
public class PerformanceDebugConsole : EditorWindow
{
    [System.Serializable]
    public class LogEntry
    {
        public string message;
        public string category;
        public LogType logType;
        public float time;

        public LogEntry(string msg, string cat, LogType type)
        {
            message = msg;
            category = cat;
            logType = type;
            time = Time.realtimeSinceStartup;
        }
    }

    // PERFORMANCE SETTINGS
    private const int MAX_LOGS = 500; // Reduced from 1000
    private const float UPDATE_INTERVAL = 0.1f; // Update UI every 100ms instead of every frame
    private const int LOGS_PER_FRAME = 10; // Process max 10 logs per frame

    // Data
    private static List<LogEntry> logs = new List<LogEntry>();
    private static HashSet<string> categories = new HashSet<string>();
    private static Queue<LogEntry> pendingLogs = new Queue<LogEntry>(); // Buffer for processing

    // UI State
    private Vector2 scrollPos;
    private string searchText = "";
    private Dictionary<string, bool> categoryToggles = new Dictionary<string, bool>();
    private bool showErrors = true;
    private bool showWarnings = true;
    private bool showInfo = true;

    // Performance tracking
    private float lastUpdateTime = 0f;
    private List<LogEntry> cachedFilteredLogs = new List<LogEntry>();
    private bool needsRefresh = true;
    private bool isPaused = false; // NEW: Pause logging during high-speed

    [MenuItem("Window/Debug Tools/Performance Console")]
    public static void ShowWindow()
    {
        var window = GetWindow<PerformanceDebugConsole>();
        window.titleContent = new GUIContent("Performance Console");
        window.Show();
    }

    void OnEnable()
    {
        Application.logMessageReceived += OnLogMessageReceived;

        // Initialize
        foreach (string category in categories)
        {
            if (!categoryToggles.ContainsKey(category))
                categoryToggles[category] = true;
        }
    }

    void OnDisable()
    {
        Application.logMessageReceived -= OnLogMessageReceived;
    }

    // FIXED: Process pending logs in OnGUI instead of OnEditorUpdate
    void OnGUI()
    {
        // Process pending logs first
        ProcessPendingLogs();

        DrawPerformanceControls();
        DrawTopBar();
        DrawCategoryFilters();
        DrawLogList();
    }

    // NEW: Process pending logs method
    void ProcessPendingLogs()
    {
        // Process all pending logs (or limit for performance)
        int processed = 0;
        while (pendingLogs.Count > 0 && processed < LOGS_PER_FRAME)
        {
            var log = pendingLogs.Dequeue();
            logs.Add(log);
            categories.Add(log.category);

            if (!categoryToggles.ContainsKey(log.category))
                categoryToggles[log.category] = true;

            processed++;
            needsRefresh = true;
        }

        // Limit log count
        while (logs.Count > MAX_LOGS)
        {
            logs.RemoveAt(0);
            needsRefresh = true;
        }

        // If we processed logs, repaint
        if (processed > 0)
        {
            Repaint();
        }
    }

    // NEW: Performance controls
    void DrawPerformanceControls()
    {
        EditorGUILayout.BeginHorizontal("box");

        GUILayout.Label("Performance:", GUILayout.Width(80));

        // Pause button
        Color oldColor = GUI.backgroundColor;
        GUI.backgroundColor = isPaused ? Color.red : Color.green;
        if (GUILayout.Button(isPaused ? "PAUSED" : "ACTIVE", GUILayout.Width(80)))
        {
            isPaused = !isPaused;
        }
        GUI.backgroundColor = oldColor;

        GUILayout.Space(10);

        // Performance stats
        GUILayout.Label($"Logs: {logs.Count}/{MAX_LOGS}", GUILayout.Width(100));
        GUILayout.Label($"Pending: {pendingLogs.Count}", GUILayout.Width(80));
        GUILayout.Label($"FPS: {(1f / Time.unscaledDeltaTime):F0}", GUILayout.Width(60));

        GUILayout.FlexibleSpace();

        // Quick performance buttons
        if (GUILayout.Button("Fast Mode", GUILayout.Width(80)))
        {
            EnableFastMode();
        }

        if (GUILayout.Button("Normal Mode", GUILayout.Width(80)))
        {
            EnableNormalMode();
        }

        EditorGUILayout.EndHorizontal();
    }

    void DrawTopBar()
    {
        EditorGUILayout.BeginHorizontal();

        // Clear button
        if (GUILayout.Button("Clear", GUILayout.Width(60)))
        {
            logs.Clear();
            categories.Clear();
            categoryToggles.Clear();
            pendingLogs.Clear();
            cachedFilteredLogs.Clear();
            needsRefresh = true;
        }

        GUILayout.Space(10);

        // Log type filters
        bool oldShowErrors = showErrors;
        bool oldShowWarnings = showWarnings;
        bool oldShowInfo = showInfo;

        showErrors = GUILayout.Toggle(showErrors, $"Errors ({GetLogCount(LogType.Error)})", GUILayout.Width(80));
        showWarnings = GUILayout.Toggle(showWarnings, $"Warnings ({GetLogCount(LogType.Warning)})", GUILayout.Width(100));
        showInfo = GUILayout.Toggle(showInfo, $"Info ({GetLogCount(LogType.Log)})", GUILayout.Width(80));

        if (oldShowErrors != showErrors || oldShowWarnings != showWarnings || oldShowInfo != showInfo)
        {
            needsRefresh = true;
        }

        GUILayout.FlexibleSpace();

        // Search
        GUILayout.Label("Search:", GUILayout.Width(50));
        string oldSearchText = searchText;
        searchText = GUILayout.TextField(searchText, GUILayout.Width(200));

        if (oldSearchText != searchText)
        {
            needsRefresh = true;
        }

        EditorGUILayout.EndHorizontal();
    }

    void DrawCategoryFilters()
    {
        if (categories.Count == 0) return;

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Categories:", GUILayout.Width(70));

        // All/None buttons
        if (GUILayout.Button("All", GUILayout.Width(30)))
        {
            foreach (string cat in categories)
                categoryToggles[cat] = true;
            needsRefresh = true;
        }

        if (GUILayout.Button("None", GUILayout.Width(40)))
        {
            foreach (string cat in categories)
                categoryToggles[cat] = false;
            needsRefresh = true;
        }

        GUILayout.Space(10);

        // Category toggles
        foreach (string category in categories.OrderBy(c => c))
        {
            if (!categoryToggles.ContainsKey(category))
                categoryToggles[category] = true;

            bool wasEnabled = categoryToggles[category];
            bool isEnabled = GUILayout.Toggle(wasEnabled, $"{category} ({GetCategoryCount(category)})", GUILayout.Width(100));

            if (wasEnabled != isEnabled)
            {
                categoryToggles[category] = isEnabled;
                needsRefresh = true;
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    void DrawLogList()
    {
        // PERFORMANCE OPTIMIZATION: Only recalculate filtered logs when needed
        if (needsRefresh)
        {
            cachedFilteredLogs = GetFilteredLogs();
            needsRefresh = false;
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        // PERFORMANCE OPTIMIZATION: Only draw visible logs
        float windowHeight = position.height - 120; // Approximate UI height
        float lineHeight = 20f;
        int visibleLines = Mathf.RoundToInt(windowHeight / lineHeight);
        int startIndex = Mathf.FloorToInt(scrollPos.y / lineHeight);
        int endIndex = Mathf.Min(startIndex + visibleLines + 5, cachedFilteredLogs.Count); // +5 for buffer

        // Add spacing for logs above visible area
        if (startIndex > 0)
        {
            GUILayout.Space(startIndex * lineHeight);
        }

        // Draw only visible logs
        for (int i = startIndex; i < endIndex; i++)
        {
            if (i >= cachedFilteredLogs.Count) break;

            var log = cachedFilteredLogs[i];
            DrawLogEntry(log);
        }

        // Add spacing for logs below visible area
        int remainingLogs = cachedFilteredLogs.Count - endIndex;
        if (remainingLogs > 0)
        {
            GUILayout.Space(remainingLogs * lineHeight);
        }

        EditorGUILayout.EndScrollView();
    }

    void DrawLogEntry(LogEntry log)
    {
        // RESTORED: Styled log entries with good performance
        GUIStyle logStyle = GetCachedLogStyle(log.logType);

        EditorGUILayout.BeginHorizontal(logStyle);

        // Create label style with proper text color
        GUIStyle labelStyle = GetCachedLabelStyle();

        // Time
        GUILayout.Label($"[{log.time:F1}s]", labelStyle, GUILayout.Width(60));

        // Category
        GUILayout.Label($"[{log.category}]", labelStyle, GUILayout.Width(100));

        // Message
        GUILayout.Label(log.message, labelStyle, GUILayout.ExpandWidth(true));

        EditorGUILayout.EndHorizontal();
    }

    // PERFORMANCE: Cache styles instead of creating them every frame
    private static Dictionary<LogType, GUIStyle> cachedLogStyles = new Dictionary<LogType, GUIStyle>();
    private static GUIStyle cachedLabelStyle;

    private GUIStyle GetCachedLogStyle(LogType logType)
    {
        if (!cachedLogStyles.ContainsKey(logType))
        {
            GUIStyle style = new GUIStyle("box");
            style.normal.background = MakeTexture(GetLogColor(logType));
            style.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            style.alignment = TextAnchor.MiddleLeft;
            style.padding = new RectOffset(5, 5, 2, 2);
            style.margin = new RectOffset(0, 0, 1, 1);

            cachedLogStyles[logType] = style;
        }

        return cachedLogStyles[logType];
    }

    private GUIStyle GetCachedLabelStyle()
    {
        if (cachedLabelStyle == null)
        {
            cachedLabelStyle = new GUIStyle(GUI.skin.label);
            cachedLabelStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
        }

        return cachedLabelStyle;
    }

    // RESTORED: Color coding for log types
    Color GetLogColor(LogType logType)
    {
        // Different colors for Light vs Dark Unity theme
        if (EditorGUIUtility.isProSkin) // Dark theme
        {
            switch (logType)
            {
                case LogType.Error: return new Color(0.8f, 0.2f, 0.2f);    // Dark red
                case LogType.Warning: return new Color(0.8f, 0.6f, 0.2f); // Dark orange
                default: return new Color(0.3f, 0.3f, 0.3f);              // Dark gray
            }
        }
        else // Light theme
        {
            switch (logType)
            {
                case LogType.Error: return new Color(1f, 0.8f, 0.8f);     // Light red
                case LogType.Warning: return new Color(1f, 0.9f, 0.7f);   // Light orange
                default: return new Color(0.95f, 0.95f, 0.95f);           // Light gray
            }
        }
    }

    // RESTORED: Texture creation helper
    private static Dictionary<Color, Texture2D> cachedTextures = new Dictionary<Color, Texture2D>();

    private Texture2D MakeTexture(Color color)
    {
        // Cache textures for performance
        if (!cachedTextures.ContainsKey(color))
        {
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            cachedTextures[color] = texture;
        }

        return cachedTextures[color];
    }

    List<LogEntry> GetFilteredLogs()
    {
        return logs.Where(log =>
        {
            // Filter by log type
            if (log.logType == LogType.Error && !showErrors) return false;
            if (log.logType == LogType.Warning && !showWarnings) return false;
            if (log.logType == LogType.Log && !showInfo) return false;

            // Filter by category
            if (categoryToggles.ContainsKey(log.category) && !categoryToggles[log.category])
                return false;

            // Filter by search text
            if (!string.IsNullOrEmpty(searchText) &&
                !log.message.ToLower().Contains(searchText.ToLower()))
                return false;

            return true;
        }).ToList();
    }

    int GetLogCount(LogType logType)
    {
        return logs.Count(l => l.logType == logType);
    }

    int GetCategoryCount(string category)
    {
        return logs.Count(l => l.category == category);
    }

    void OnLogMessageReceived(string logString, string stackTrace, LogType type)
    {
        // PERFORMANCE OPTIMIZATION: Skip if paused
        if (isPaused) return;

        // Extract category
        string category = "General";
        string cleanMessage = logString;

        if (logString.StartsWith("[") && logString.Contains("]"))
        {
            int endIndex = logString.IndexOf("]");
            category = logString.Substring(1, endIndex - 1);
            cleanMessage = logString.Substring(endIndex + 1).Trim();
        }

        // DEBUG: Print to Unity console to verify category extraction
        if (logs.Count < 10) // Only for first few logs to avoid spam
        {
            UnityEngine.Debug.Log($"Performance Console: Extracted category '{category}' from '{logString}'");
        }

        // PERFORMANCE OPTIMIZATION: Add to queue instead of processing immediately
        pendingLogs.Enqueue(new LogEntry(cleanMessage, category, type));

        // Drop old pending logs if queue gets too large
        while (pendingLogs.Count > MAX_LOGS)
        {
            pendingLogs.Dequeue();
        }
    }

    // Performance mode controls
    void EnableFastMode()
    {
        // Disable info logs, keep only warnings and errors
        showInfo = false;
        showWarnings = true;
        showErrors = true;
        needsRefresh = true;

        Debug.Log("[PERFORMANCE] Fast mode enabled - Info logs hidden");
    }

    void EnableNormalMode()
    {
        showInfo = true;
        showWarnings = true;
        showErrors = true;
        needsRefresh = true;

        Debug.Log("[PERFORMANCE] Normal mode enabled - All logs visible");
    }

    // Auto-pause during high speed simulation
    void Update()
    {
        // Auto-pause if game speed is very high to prevent FPS drop
        if (Application.isPlaying && Time.timeScale > 50f && !isPaused)
        {
            isPaused = true;
            Debug.Log("[PERFORMANCE] Debug console auto-paused due to high game speed");
        }
        else if (Application.isPlaying && Time.timeScale <= 10f && isPaused)
        {
            isPaused = false;
            Debug.Log("[PERFORMANCE] Debug console auto-resumed");
        }
    }
}
#endif