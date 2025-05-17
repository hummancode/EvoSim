using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameSpeedController : MonoBehaviour
{
    [Header("Speed Settings")]
    [SerializeField] private float minSpeed = 0.1f;
    [SerializeField] private float maxSpeed = 50.0f;
    [SerializeField] private float defaultSpeed = 1.0f;
    [SerializeField] private bool useLogarithmicScale = true;

    [Header("UI References")]
    [SerializeField] private Slider speedSlider;
    [SerializeField] private TextMeshProUGUI speedText;

    void Start()
    {
        // Configure the slider
        if (speedSlider != null)
        {
            if (useLogarithmicScale)
            {
                // For logarithmic scale, slider goes from 0-1
                speedSlider.minValue = 0;
                speedSlider.maxValue = 1;

                // Convert default speed to slider value
                float sliderValue = LogToSlider(defaultSpeed);
                speedSlider.value = sliderValue;
            }
            else
            {
                // Linear scale
                speedSlider.minValue = minSpeed;
                speedSlider.maxValue = maxSpeed;
                speedSlider.value = defaultSpeed;
            }

            // Add listener for value changes
            speedSlider.onValueChanged.AddListener(OnSpeedSliderChanged);
        }

        // Set initial game speed
        SetGameSpeed(defaultSpeed);
    }

    // Called when slider value changes
    public void OnSpeedSliderChanged(float sliderValue)
    {
        float gameSpeed;

        if (useLogarithmicScale)
        {
            // Convert slider value to actual speed
            gameSpeed = SliderToLog(sliderValue);
        }
        else
        {
            // Direct linear mapping
            gameSpeed = sliderValue;
        }

        SetGameSpeed(gameSpeed);
    }

    // Convert slider value (0-1) to logarithmic speed
    private float SliderToLog(float sliderValue)
    {
        // Map 0-1 to min-max speed on logarithmic scale
        float minLog = Mathf.Log(minSpeed);
        float maxLog = Mathf.Log(maxSpeed);
        float logValue = minLog + (maxLog - minLog) * sliderValue;
        return Mathf.Exp(logValue);
    }

    // Convert logarithmic speed to slider value (0-1)
    private float LogToSlider(float speed)
    {
        float minLog = Mathf.Log(minSpeed);
        float maxLog = Mathf.Log(maxSpeed);
        float logValue = Mathf.Log(speed);
        return (logValue - minLog) / (maxLog - minLog);
    }

    // Set game speed to specific value
    public void SetGameSpeed(float speed)
    {
        // Apply speed to time scale
        Time.timeScale = speed;

        // Update UI text if available
        if (speedText != null)
        {
            speedText.text = $"Speed: {speed:F1}x";
        }
    }

    // Reset to default speed
    public void ResetSpeed()
    {
        if (speedSlider != null)
        {
            if (useLogarithmicScale)
            {
                speedSlider.value = LogToSlider(defaultSpeed);
            }
            else
            {
                speedSlider.value = defaultSpeed;
            }
        }
        else
        {
            SetGameSpeed(defaultSpeed);
        }
    }
}