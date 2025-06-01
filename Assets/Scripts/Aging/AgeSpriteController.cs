
// FILE: AgeSpriteController.cs
// PURPOSE: Handles sprite scaling and coloring based on age
// ============================================================================

using UnityEngine;

/// <summary>
/// Handles sprite appearance changes based on age from AgeSystem
/// </summary>
public class AgeSpriteController : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private float maturityAge = 20f;
    [SerializeField] private float minScale = 0.5f;  // Baby size
    [SerializeField] private float maxScale = 1f;    // Adult size
    [SerializeField] private Color babyColor = new Color(0.2f, 0.4f, 0.8f, 1f); // Darker blue
    [SerializeField] private Color adultColor = Color.cyan;

    [Header("References")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private AgeSystem ageSystem;

    void Awake()
    {
        // Get components automatically
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (ageSystem == null)
            ageSystem = GetComponent<AgeSystem>();
    }

    void Update()
    {
        // Update appearance every frame based on current age
        UpdateSpriteAppearance();
    }

    private void UpdateSpriteAppearance()
    {
        if (spriteRenderer == null || ageSystem == null) return;

        float currentAge = ageSystem.Age;
        float maxAge = ageSystem.MaxAge;

        // Calculate scale (0 to maturityAge = minScale to maxScale)
        float maturityProgress = Mathf.Clamp01(currentAge / maturityAge);
        float targetScale = Mathf.Lerp(minScale, maxScale, maturityProgress);
        transform.localScale = Vector3.one * targetScale;

        // Calculate color progression
        Color targetColor = CalculateAgeColor(currentAge, maxAge);
        spriteRenderer.color = targetColor;
    }

    private Color CalculateAgeColor(float currentAge, float maxAge)
    {
        if (currentAge <= maturityAge)
        {
            // Baby to adult: dark blue to white
            float progress = currentAge / maturityAge;
            return Color.Lerp(babyColor, adultColor, progress);
        }
        else
        {
            // Adult to elderly: white to gray
            float elderlyProgress = (currentAge - maturityAge) / (maxAge - maturityAge);
            return Color.Lerp(adultColor, Color.gray, elderlyProgress * 0.3f);
        }
    }

    // Public methods for external use
    public bool IsBaby() => ageSystem != null && ageSystem.Age < maturityAge * 0.3f;
    public bool IsChild() => ageSystem != null && ageSystem.Age < maturityAge;
    public bool IsAdult() => ageSystem != null && ageSystem.Age >= maturityAge;
}