// ============================================================================
// FILE: DualAxisLineChart.cs - Combined Population/Food Chart with Two Y-Axes
// ============================================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class DualAxisLineChart : MonoBehaviour
{
    [Header("Chart Settings")]
    [SerializeField] private string chartTitle = "Population & Food Over Time";
    [SerializeField] private int maxDataPoints = 200;
    [SerializeField] private bool startFromTimeZero = true;

    [Header("Left Axis (Population)")]
    [SerializeField] private Color populationColor = Color.blue;
    [SerializeField] private string leftAxisLabel = "Population";
    [SerializeField] private float leftLineWidth = 2f;

    [Header("Right Axis (Food)")]
    [SerializeField] private Color foodColor = Color.green;
    [SerializeField] private string rightAxisLabel = "Food";
    [SerializeField] private float rightLineWidth = 2f;

    [Header("Grid & Ticks")]
    [SerializeField] private bool showGrid = true;
    [SerializeField] private Color gridColor = new Color(0.5f, 0.5f, 0.5f, 0.3f);
    [SerializeField] private int targetTickCount = 5;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI leftAxisLabelText;
    [SerializeField] private TextMeshProUGUI rightAxisLabelText;
    [SerializeField] private RectTransform chartArea;
    [SerializeField] private RectTransform leftTicksParent;
    [SerializeField] private RectTransform rightTicksParent;
    [SerializeField] private RectTransform bottomTicksParent;
    [SerializeField] private RectTransform legendArea;

    [Header("Prefabs")]
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private GameObject tickPrefab;
    [SerializeField] private GameObject gridLinePrefab;

    // Data storage
    private List<float> timePoints = new List<float>();
    private List<float> populationData = new List<float>();
    private List<float> foodData = new List<float>();

    // Visual elements
    private List<GameObject> populationLines = new List<GameObject>();
    private List<GameObject> foodLines = new List<GameObject>();
    private List<GameObject> gridLines = new List<GameObject>();
    private List<GameObject> tickElements = new List<GameObject>();

    // Chart bounds
    private float minTime, maxTime;
    private float minPopulation, maxPopulation;
    private float minFood, maxFood;

    void Start()
    {
        SetupChart();
    }

    private void SetupChart()
    {
        if (titleText != null)
            titleText.text = chartTitle;

        if (leftAxisLabelText != null)
            leftAxisLabelText.text = leftAxisLabel;

        if (rightAxisLabelText != null)
            rightAxisLabelText.text = rightAxisLabel;

        CreateDefaultPrefabs();
        CreateLegend();
    }

    public void AddDataPoint(float time, float population, float food)
    {
        timePoints.Add(time);
        populationData.Add(population);
        foodData.Add(food);

        // Remove old points if exceeding max
        while (timePoints.Count > maxDataPoints)
        {
            timePoints.RemoveAt(0);
            populationData.RemoveAt(0);
            foodData.RemoveAt(0);
        }

        UpdateBounds();
        RedrawChart();
    }

    public void SetData(List<float> times, List<float> populations, List<float> foods)
    {
        timePoints.Clear();
        populationData.Clear();
        foodData.Clear();

        int startIndex = Mathf.Max(0, times.Count - maxDataPoints);
        for (int i = startIndex; i < times.Count; i++)
        {
            if (i < populations.Count && i < foods.Count)
            {
                timePoints.Add(times[i]);
                populationData.Add(populations[i]);
                foodData.Add(foods[i]);
            }
        }

        UpdateBounds();
        RedrawChart();
    }

    private void UpdateBounds()
    {
        if (timePoints.Count == 0) return;

        // Time bounds
        minTime = startFromTimeZero ? 0f : timePoints.Min();
        maxTime = timePoints.Max();
        if (maxTime <= minTime) maxTime = minTime + 1f;

        // Population bounds (left axis)
        if (populationData.Count > 0)
        {
            minPopulation = 0f; // Always start population from 0
            maxPopulation = populationData.Max();
            if (maxPopulation <= minPopulation) maxPopulation = minPopulation + 10f;

            // Add 10% padding to top
            maxPopulation *= 1.1f;
        }

        // Food bounds (right axis)
        if (foodData.Count > 0)
        {
            minFood = 0f; // Always start food from 0
            maxFood = foodData.Max();
            if (maxFood <= minFood) maxFood = minFood + 10f;

            // Add 10% padding to top
            maxFood *= 1.1f;
        }
    }

    private void RedrawChart()
    {
        if (chartArea == null || timePoints.Count < 2) return;

        ClearVisualElements();

        Rect chartRect = chartArea.rect;
        float chartWidth = chartRect.width;
        float chartHeight = chartRect.height;

        // Draw grid first (behind lines)
        if (showGrid)
            DrawGrid(chartWidth, chartHeight);

        // Draw population line (left axis)
        DrawDataLine(timePoints, populationData, minTime, maxTime, minPopulation, maxPopulation,
                    populationColor, leftLineWidth, populationLines, chartWidth, chartHeight);

        // Draw food line (right axis)
        DrawDataLine(timePoints, foodData, minTime, maxTime, minFood, maxFood,
                    foodColor, rightLineWidth, foodLines, chartWidth, chartHeight);

        // Draw ticks and labels
        DrawTicks(chartWidth, chartHeight);
    }

    private void DrawDataLine(List<float> times, List<float> values, float minTime, float maxTime,
                             float minValue, float maxValue, Color lineColor, float lineWidth,
                             List<GameObject> lineContainer, float chartWidth, float chartHeight)
    {
        if (startFromTimeZero && times.Count > 0 && times[0] > 0.01f)
        {
            // Draw line from time 0 to first data point
            Vector2 startPos = GetChartPosition(0f, values[0], minTime, maxTime, minValue, maxValue, chartWidth, chartHeight);
            Vector2 firstDataPos = GetChartPosition(times[0], values[0], minTime, maxTime, minValue, maxValue, chartWidth, chartHeight);
            CreateLineSegment(startPos, firstDataPos, lineColor, lineWidth, lineContainer);
        }

        for (int i = 0; i < times.Count - 1; i++)
        {
            Vector2 startPos = GetChartPosition(times[i], values[i], minTime, maxTime, minValue, maxValue, chartWidth, chartHeight);
            Vector2 endPos = GetChartPosition(times[i + 1], values[i + 1], minTime, maxTime, minValue, maxValue, chartWidth, chartHeight);
            CreateLineSegment(startPos, endPos, lineColor, lineWidth, lineContainer);
        }
    }

    private Vector2 GetChartPosition(float time, float value, float minTime, float maxTime,
                                   float minValue, float maxValue, float chartWidth, float chartHeight)
    {
        float normalizedX = (time - minTime) / (maxTime - minTime);
        float normalizedY = (value - minValue) / (maxValue - minValue);

        float posX = normalizedX * chartWidth - (chartWidth * 0.5f);
        float posY = normalizedY * chartHeight - (chartHeight * 0.5f);

        return new Vector2(posX, posY);
    }

    private void CreateLineSegment(Vector2 startPos, Vector2 endPos, Color color, float width, List<GameObject> container)
    {
        GameObject lineObj = Instantiate(linePrefab, chartArea);

        Vector2 midPoint = (startPos + endPos) / 2f;
        lineObj.transform.localPosition = midPoint;

        Vector2 direction = endPos - startPos;
        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        RectTransform lineRect = lineObj.GetComponent<RectTransform>();
        lineRect.sizeDelta = new Vector2(distance, width);
        lineRect.rotation = Quaternion.Euler(0, 0, angle);

        Image lineImage = lineObj.GetComponent<Image>();
        if (lineImage != null)
            lineImage.color = color;

        container.Add(lineObj);
    }

    private void DrawGrid(float chartWidth, float chartHeight)
    {
        // Vertical grid lines (time)
        for (int i = 0; i <= targetTickCount; i++)
        {
            float normalizedX = (float)i / targetTickCount;
            float posX = normalizedX * chartWidth - (chartWidth * 0.5f);

            CreateGridLine(new Vector2(posX, -chartHeight * 0.5f), new Vector2(posX, chartHeight * 0.5f));
        }

        // Horizontal grid lines
        for (int i = 0; i <= targetTickCount; i++)
        {
            float normalizedY = (float)i / targetTickCount;
            float posY = normalizedY * chartHeight - (chartHeight * 0.5f);

            CreateGridLine(new Vector2(-chartWidth * 0.5f, posY), new Vector2(chartWidth * 0.5f, posY));
        }
    }

    private void CreateGridLine(Vector2 startPos, Vector2 endPos)
    {
        GameObject gridLine = Instantiate(gridLinePrefab, chartArea);

        Vector2 midPoint = (startPos + endPos) / 2f;
        gridLine.transform.localPosition = midPoint;

        Vector2 direction = endPos - startPos;
        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        RectTransform lineRect = gridLine.GetComponent<RectTransform>();
        lineRect.sizeDelta = new Vector2(distance, 1f);
        lineRect.rotation = Quaternion.Euler(0, 0, angle);

        Image lineImage = gridLine.GetComponent<Image>();
        if (lineImage != null)
            lineImage.color = gridColor;

        gridLines.Add(gridLine);
    }

    private void DrawTicks(float chartWidth, float chartHeight)
    {
        // Left axis ticks (Population)
        if (leftTicksParent != null)
        {
            for (int i = 0; i <= targetTickCount; i++)
            {
                float normalizedY = (float)i / targetTickCount;
                float value = Mathf.Lerp(minPopulation, maxPopulation, normalizedY);
                float posY = normalizedY * chartHeight - (chartHeight * 0.5f);

                CreateTick(leftTicksParent, new Vector2(0, posY), value.ToString("F0"), TextAnchor.MiddleRight);
            }
        }

        // Right axis ticks (Food)
        if (rightTicksParent != null)
        {
            for (int i = 0; i <= targetTickCount; i++)
            {
                float normalizedY = (float)i / targetTickCount;
                float value = Mathf.Lerp(minFood, maxFood, normalizedY);
                float posY = normalizedY * chartHeight - (chartHeight * 0.5f);

                CreateTick(rightTicksParent, new Vector2(0, posY), value.ToString("F0"), TextAnchor.MiddleLeft);
            }
        }

        // Bottom axis ticks (Time)
        if (bottomTicksParent != null)
        {
            for (int i = 0; i <= targetTickCount; i++)
            {
                float normalizedX = (float)i / targetTickCount;
                float time = Mathf.Lerp(minTime, maxTime, normalizedX);
                float posX = normalizedX * chartWidth - (chartWidth * 0.5f);

                CreateTick(bottomTicksParent, new Vector2(posX, 0), time.ToString("F0") + "s", TextAnchor.MiddleCenter);
            }
        }
    }

    private void CreateTick(Transform parent, Vector2 position, string text, TextAnchor alignment)
    {
        GameObject tickObj = Instantiate(tickPrefab, parent);
        tickObj.transform.localPosition = position;

        TextMeshProUGUI tickText = tickObj.GetComponent<TextMeshProUGUI>();
        if (tickText != null)
        {
            tickText.text = text;
            tickText.alignment = GetTextAlignment(alignment);
            tickText.fontSize = 10;
        }

        tickElements.Add(tickObj);
    }

    private TextAlignmentOptions GetTextAlignment(TextAnchor anchor)
    {
        switch (anchor)
        {
            case TextAnchor.MiddleLeft: return TextAlignmentOptions.MidlineLeft;
            case TextAnchor.MiddleRight: return TextAlignmentOptions.MidlineRight;
            case TextAnchor.MiddleCenter: return TextAlignmentOptions.Center;
            default: return TextAlignmentOptions.Center;
        }
    }

    private void CreateLegend()
    {
        if (legendArea == null) return;

        // Create population legend
        GameObject popLegend = CreateLegendItem(populationColor, leftAxisLabel);
        if (popLegend != null)
            popLegend.transform.SetParent(legendArea);

        // Create food legend
        GameObject foodLegend = CreateLegendItem(foodColor, rightAxisLabel);
        if (foodLegend != null)
            foodLegend.transform.SetParent(legendArea);
    }

    private GameObject CreateLegendItem(Color color, string label)
    {
        GameObject legendItem = new GameObject("LegendItem");
        legendItem.AddComponent<RectTransform>();

        // Color box
        GameObject colorBox = new GameObject("ColorBox");
        colorBox.transform.SetParent(legendItem.transform);
        RectTransform boxRect = colorBox.AddComponent<RectTransform>();
        boxRect.sizeDelta = new Vector2(15, 15);
        boxRect.anchoredPosition = new Vector2(-20, 0);

        Image boxImage = colorBox.AddComponent<Image>();
        boxImage.color = color;

        // Label text
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(legendItem.transform);
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.sizeDelta = new Vector2(100, 20);
        labelRect.anchoredPosition = new Vector2(30, 0);

        TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.text = label;
        labelText.fontSize = 12;
        labelText.color = Color.white;

        return legendItem;
    }

    private void CreateDefaultPrefabs()
    {
        if (linePrefab == null)
        {
            linePrefab = new GameObject("LinePrefab");
            linePrefab.AddComponent<RectTransform>();
            linePrefab.AddComponent<Image>();
        }

        if (tickPrefab == null)
        {
            tickPrefab = new GameObject("TickPrefab");
            tickPrefab.AddComponent<RectTransform>();
            tickPrefab.AddComponent<TextMeshProUGUI>();
        }

        if (gridLinePrefab == null)
        {
            gridLinePrefab = new GameObject("GridLinePrefab");
            gridLinePrefab.AddComponent<RectTransform>();
            gridLinePrefab.AddComponent<Image>();
        }
    }

    private void ClearVisualElements()
    {
        ClearList(populationLines);
        ClearList(foodLines);
        ClearList(gridLines);
        ClearList(tickElements);
    }

    private void ClearList(List<GameObject> list)
    {
        foreach (GameObject obj in list)
        {
            if (obj != null)
                DestroyImmediate(obj);
        }
        list.Clear();
    }

    public void ClearChart()
    {
        timePoints.Clear();
        populationData.Clear();
        foodData.Clear();
        ClearVisualElements();
    }
}