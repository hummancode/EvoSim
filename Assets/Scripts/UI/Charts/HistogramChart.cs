// ============================================================================
// FILE: HistogramChart.cs - For Death Cause and Age Distribution Analysis
// ============================================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class HistogramChart : MonoBehaviour
{
    [Header("Chart Settings")]
    [SerializeField] private string chartTitle = "Death Statistics";
    [SerializeField] private Color[] barColors = { Color.red, Color.blue, Color.yellow, Color.green, Color.magenta };
    [SerializeField] private float barSpacing = 2f;
    [SerializeField] private float maxBarHeight = 100f;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private RectTransform chartArea;
    [SerializeField] private RectTransform barsParent;
    [SerializeField] private RectTransform labelsParent;
    [SerializeField] private RectTransform legendParent;

    [Header("Prefabs")]
    [SerializeField] private GameObject barPrefab;
    [SerializeField] private GameObject labelPrefab;

    // Data and visuals
    private List<GameObject> barObjects = new List<GameObject>();
    private List<GameObject> labelObjects = new List<GameObject>();
    private Dictionary<string, int> currentData = new Dictionary<string, int>();

    void Start()
    {
        SetupChart();
    }

    private void SetupChart()
    {
        if (titleText != null)
            titleText.text = chartTitle;

        CreateDefaultPrefabs();
    }

    public void SetData(Dictionary<string, int> data, string title = null)
    {
        currentData = new Dictionary<string, int>(data);

        if (!string.IsNullOrEmpty(title))
        {
            chartTitle = title;
            if (titleText != null)
                titleText.text = chartTitle;
        }

        RedrawChart();
    }

    public void SetDeathCauseData(Dictionary<string, int> deathCounts, int totalDeaths)
    {
        currentData = deathCounts;
        chartTitle = $"Death Causes (Total: {totalDeaths})";

        if (titleText != null)
            titleText.text = chartTitle;

        RedrawChart();
    }

    public void SetAgeDistributionData(List<float> ages, float binSize = 10f)
    {
        // Create age bins
        Dictionary<string, int> ageBins = new Dictionary<string, int>();

        foreach (float age in ages)
        {
            int binIndex = Mathf.FloorToInt(age / binSize);
            string binLabel = $"{binIndex * binSize:F0}-{(binIndex + 1) * binSize:F0}";

            if (ageBins.ContainsKey(binLabel))
                ageBins[binLabel]++;
            else
                ageBins[binLabel] = 1;
        }

        SetData(ageBins, $"Death Age Distribution (Bin Size: {binSize})");
    }

    private void RedrawChart()
    {
        if (chartArea == null || currentData.Count == 0) return;

        ClearBars();

        var sortedData = currentData.OrderByDescending(kvp => kvp.Value).ToList();
        int maxValue = sortedData.Max(kvp => kvp.Value);

        float chartWidth = chartArea.rect.width;
        float barWidth = (chartWidth - (sortedData.Count - 1) * barSpacing) / sortedData.Count;

        for (int i = 0; i < sortedData.Count; i++)
        {
            var dataPoint = sortedData[i];
            CreateBar(dataPoint.Key, dataPoint.Value, maxValue, barWidth, i, sortedData.Count);
        }

        CreateLegend(sortedData);
    }

    private void CreateBar(string label, int value, int maxValue, float barWidth, int index, int totalBars)
    {
        // Create bar
        GameObject barObj = Instantiate(barPrefab, barsParent);
        barObjects.Add(barObj);

        // Calculate position
        float chartWidth = chartArea.rect.width;
        float totalBarArea = totalBars * barWidth + (totalBars - 1) * barSpacing;
        float startX = -totalBarArea * 0.5f;
        float barX = startX + index * (barWidth + barSpacing) + barWidth * 0.5f;

        // Calculate height
        float normalizedHeight = maxValue > 0 ? (float)value / maxValue : 0f;
        float barHeight = normalizedHeight * maxBarHeight;

        // Set bar properties
        RectTransform barRect = barObj.GetComponent<RectTransform>();
        barRect.sizeDelta = new Vector2(barWidth, barHeight);
        barRect.anchoredPosition = new Vector2(barX, barHeight * 0.5f);

        // Set bar color
        Image barImage = barObj.GetComponent<Image>();
        if (barImage != null)
        {
            Color barColor = barColors[index % barColors.Length];
            barImage.color = barColor;
        }

        // Create label
        CreateBarLabel(label, value, barX, barHeight);
    }

    private void CreateBarLabel(string text, int value, float x, float barHeight)
    {
        GameObject labelObj = Instantiate(labelPrefab, labelsParent);
        labelObjects.Add(labelObj);

        TextMeshProUGUI labelText = labelObj.GetComponent<TextMeshProUGUI>();
        if (labelText != null)
        {
            labelText.text = $"{text}\n{value}";
            labelText.fontSize = 10;
            labelText.alignment = TextAlignmentOptions.Center;
        }

        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchoredPosition = new Vector2(x, -20f);
        labelRect.sizeDelta = new Vector2(50, 30);
    }

    private void CreateLegend(List<KeyValuePair<string, int>> data)
    {
        if (legendParent == null) return;

        // Clear existing legend
        foreach (Transform child in legendParent)
        {
            DestroyImmediate(child.gameObject);
        }

        for (int i = 0; i < data.Count; i++)
        {
            GameObject legendItem = new GameObject($"Legend_{data[i].Key}");
            legendItem.transform.SetParent(legendParent);

            // Create legend with color and text
            RectTransform itemRect = legendItem.AddComponent<RectTransform>();
            itemRect.sizeDelta = new Vector2(150, 20);

            // Color indicator
            GameObject colorBox = new GameObject("ColorBox");
            colorBox.transform.SetParent(legendItem.transform);
            RectTransform boxRect = colorBox.AddComponent<RectTransform>();
            boxRect.sizeDelta = new Vector2(15, 15);
            boxRect.anchoredPosition = new Vector2(-60, 0);

            Image boxImage = colorBox.AddComponent<Image>();
            boxImage.color = barColors[i % barColors.Length];

            // Text label
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(legendItem.transform);
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = $"{data[i].Key}: {data[i].Value}";
            text.fontSize = 12;
            text.color = Color.white;

            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(120, 20);
            textRect.anchoredPosition = new Vector2(-10, 0);
        }
    }

    private void CreateDefaultPrefabs()
    {
        if (barPrefab == null)
        {
            barPrefab = new GameObject("BarPrefab");
            barPrefab.AddComponent<RectTransform>();
            barPrefab.AddComponent<Image>();
        }

        if (labelPrefab == null)
        {
            labelPrefab = new GameObject("LabelPrefab");
            labelPrefab.AddComponent<RectTransform>();
            labelPrefab.AddComponent<TextMeshProUGUI>();
        }
    }

    private void ClearBars()
    {
        foreach (GameObject bar in barObjects)
        {
            if (bar != null)
                DestroyImmediate(bar);
        }
        barObjects.Clear();

        foreach (GameObject label in labelObjects)
        {
            if (label != null)
                DestroyImmediate(label);
        }
        labelObjects.Clear();
    }

    public void ClearChart()
    {
        currentData.Clear();
        ClearBars();
    }
}

// ============================================================================
