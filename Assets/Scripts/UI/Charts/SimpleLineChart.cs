using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleLineChart : MonoBehaviour
{
    [Header("Chart Settings")]
    [SerializeField] private string chartTitle = "Chart";
    [SerializeField] private Color lineColor = Color.blue;
    [SerializeField] private float lineWidth = 2f;
    [SerializeField] private int maxDataPoints = 100;
    [SerializeField] private bool startFromTimeZero = true; // New option!

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI minValueText;
    [SerializeField] private TextMeshProUGUI maxValueText;
    [SerializeField] private TextMeshProUGUI currentValueText;
    [SerializeField] private RectTransform chartArea;

    [Header("Prefabs")]
    [SerializeField] private GameObject linePrefab;

    // Internal data
    private List<float> dataPoints = new List<float>();
    private List<float> timePoints = new List<float>();
    private List<GameObject> lineSegments = new List<GameObject>();

    // Chart bounds - fully dynamic
    private float minValue = float.MaxValue;
    private float maxValue = float.MinValue;
    private float minTime = float.MaxValue;
    private float maxTime = float.MinValue;

    void Start()
    {
        SetupChart();
    }

    private void SetupChart()
    {
        // Set title
        if (titleText != null)
            titleText.text = chartTitle;

        // Create line prefab if not assigned
        if (linePrefab == null)
            CreateLinePrefab();
    }

    public void AddDataPoint(float time, float value)
    {
        // Add new data point
        timePoints.Add(time);
        dataPoints.Add(value);

        // Remove old points if exceeding max
        while (dataPoints.Count > maxDataPoints)
        {
            dataPoints.RemoveAt(0);
            timePoints.RemoveAt(0);
        }

        // Update bounds
        UpdateBounds();

        // Redraw chart
        RedrawChart();

        // Update UI text
        UpdateUIText();
    }

    public void SetData(List<float> times, List<float> values)
    {
        // Clear existing data
        dataPoints.Clear();
        timePoints.Clear();

        // Add new data (limit to max points)
        int startIndex = Mathf.Max(0, values.Count - maxDataPoints);
        for (int i = startIndex; i < values.Count; i++)
        {
            if (i < times.Count)
            {
                timePoints.Add(times[i]);
                dataPoints.Add(values[i]);
            }
        }

        UpdateBounds();
        RedrawChart();
        UpdateUIText();
    }

    private void UpdateBounds()
    {
        if (dataPoints.Count == 0) return;

        // Reset bounds
        minValue = float.MaxValue;
        maxValue = float.MinValue;
        minTime = float.MaxValue;
        maxTime = float.MinValue;

        // Find actual min/max from all data
        for (int i = 0; i < dataPoints.Count; i++)
        {
            float value = dataPoints[i];
            float time = timePoints[i];

            if (value < minValue) minValue = value;
            if (value > maxValue) maxValue = value;
            if (time < minTime) minTime = time;
            if (time > maxTime) maxTime = time;
        }

        // Handle time bounds based on startFromTimeZero option
        if (startFromTimeZero)
        {
            minTime = 0f; // Always start from 0
            // maxTime stays as the latest time point
        }
        else
        {
            // Use dynamic time range (original behavior)
            if (maxTime - minTime < 0.01f)
            {
                maxTime = minTime + 1f;
            }
            else
            {
                // Add small time padding for dynamic mode
                float timeRange = maxTime - minTime;
                float timePadding = timeRange * 0.05f;
                minTime -= timePadding;
                maxTime += timePadding;
            }
        }

        // Handle value bounds - always dynamic
        float valueRange = maxValue - minValue;

        if (valueRange < 0.01f)
        {
            // If all values are the same, add some range
            minValue -= 1f;
            maxValue += 1f;
        }
        else
        {
            // Add 10% padding to value range
            float valuePadding = valueRange * 0.1f;
            minValue -= valuePadding;
            maxValue += valuePadding;
        }

        // Ensure minimum bounds for edge cases
        if (maxTime <= minTime)
        {
            maxTime = minTime + 1f;
        }
    }

    private void RedrawChart()
    {
        if (chartArea == null || dataPoints.Count < 2) return;

        // Clear existing lines
        ClearLines();

        // Get chart dimensions
        Rect chartRect = chartArea.rect;
        float chartWidth = chartRect.width;
        float chartHeight = chartRect.height;

        // Special handling for startFromTimeZero mode
        if (startFromTimeZero && dataPoints.Count > 0)
        {
            // Add line from time 0 to first data point if first point is not at time 0
            if (timePoints[0] > 0.01f)
            {
                Vector2 startPos = GetChartPosition(0f, dataPoints[0], chartWidth, chartHeight);
                Vector2 firstDataPos = GetChartPosition(timePoints[0], dataPoints[0], chartWidth, chartHeight);
                CreateLineSegment(startPos, firstDataPos);
            }
        }

        // Draw line segments between consecutive data points
        for (int i = 0; i < dataPoints.Count - 1; i++)
        {
            Vector2 startPos = GetChartPosition(timePoints[i], dataPoints[i], chartWidth, chartHeight);
            Vector2 endPos = GetChartPosition(timePoints[i + 1], dataPoints[i + 1], chartWidth, chartHeight);

            CreateLineSegment(startPos, endPos);
        }
    }

    private Vector2 GetChartPosition(float time, float value, float chartWidth, float chartHeight)
    {
        float normalizedX = (time - minTime) / (maxTime - minTime);
        float normalizedY = (value - minValue) / (maxValue - minValue);

        float posX = normalizedX * chartWidth - (chartWidth * 0.5f);
        float posY = normalizedY * chartHeight - (chartHeight * 0.5f);

        return new Vector2(posX, posY);
    }

    private void CreateLineSegment(Vector2 startPos, Vector2 endPos)
    {
        GameObject lineObj = Instantiate(linePrefab, chartArea);

        // Position line at midpoint
        Vector2 midPoint = (startPos + endPos) / 2f;
        lineObj.transform.localPosition = midPoint;

        // Calculate line properties
        Vector2 direction = endPos - startPos;
        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Set line size and rotation
        RectTransform lineRect = lineObj.GetComponent<RectTransform>();
        lineRect.sizeDelta = new Vector2(distance, lineWidth);
        lineRect.rotation = Quaternion.Euler(0, 0, angle);

        // Set line color
        Image lineImage = lineObj.GetComponent<Image>();
        if (lineImage != null)
            lineImage.color = lineColor;

        lineSegments.Add(lineObj);
    }

    private void ClearLines()
    {
        foreach (GameObject line in lineSegments)
        {
            if (line != null)
                DestroyImmediate(line);
        }
        lineSegments.Clear();
    }

    private void UpdateUIText()
    {
        if (minValueText != null)
            minValueText.text = $"Min: {minValue:F1}";

        if (maxValueText != null)
            maxValueText.text = $"Max: {maxValue:F1}";

        if (currentValueText != null && dataPoints.Count > 0)
            currentValueText.text = $"Current: {dataPoints[dataPoints.Count - 1]:F1}";

        // Update title with time range info if helpful
        if (titleText != null && startFromTimeZero)
        {
            string timeInfo = maxTime > 0 ? $" (0 - {maxTime:F1}s)" : "";
            titleText.text = chartTitle + timeInfo;
        }
        else if (titleText != null)
        {
            titleText.text = chartTitle;
        }
    }

    private void CreateLinePrefab()
    {
        GameObject prefab = new GameObject("LineSegment");
        prefab.AddComponent<RectTransform>();

        Image image = prefab.AddComponent<Image>();
        image.color = lineColor;

        linePrefab = prefab;
    }

    // Public interface
    public void SetTitle(string title)
    {
        chartTitle = title;
        UpdateUIText(); // Refresh title display
    }

    public void SetLineColor(Color color)
    {
        lineColor = color;
    }

    public void SetStartFromTimeZero(bool enabled)
    {
        startFromTimeZero = enabled;
        UpdateBounds();
        RedrawChart();
        UpdateUIText();
    }

    public void ClearChart()
    {
        dataPoints.Clear();
        timePoints.Clear();
        ClearLines();
        UpdateUIText();
    }

    public int DataPointCount => dataPoints.Count;
    public float LatestValue => dataPoints.Count > 0 ? dataPoints[dataPoints.Count - 1] : 0f;
    public float LatestTime => timePoints.Count > 0 ? timePoints[timePoints.Count - 1] : 0f;
}