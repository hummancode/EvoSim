// ============================================================================
// FILE: CleanPixelBackgroundGenerator.cs
// PURPOSE: Clean, non-distracting pixel art background for simulation focus
// ============================================================================

using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Generates clean, pixel art style backgrounds that don't distract from simulation
/// </summary>
public class BackgroundGenerator : MonoBehaviour
{
    [Header("Background Settings")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Vector2 worldBounds = new Vector2(30f, 20f);
    [SerializeField] private bool generateOnStart = true;

    [Header("Base Ground - Pixel Art Style")]
    [SerializeField] private Color groundColor1 = new Color(0.25f, 0.45f, 0.15f, 1f); // Muted dark green
    [SerializeField] private Color groundColor2 = new Color(0.30f, 0.50f, 0.20f, 1f); // Muted medium green
    [SerializeField] private Color groundColor3 = new Color(0.35f, 0.55f, 0.25f, 1f); // Muted light green
    [SerializeField] private int groundTextureSize = 256; // Smaller for pixel art
    [SerializeField] private float groundNoiseScale = 0.05f; // Less chaotic
    [SerializeField] private bool enableGroundVariation = true;

    [Header("Simple Grass Dots")]
    [SerializeField] private bool enableGrass = true;
    [SerializeField] private int grassDensity = 50; // Much lower density
    [SerializeField] private Vector2 grassScaleRange = new Vector2(0.2f, 0.4f); // Smaller
    [SerializeField] private Color grassColor1 = new Color(0.2f, 0.4f, 0.15f, 0.6f); // Subtle
    [SerializeField] private Color grassColor2 = new Color(0.25f, 0.45f, 0.2f, 0.6f); // Subtle
    [SerializeField] private int grassPixelSize = 8; // Small pixel clusters

    [Header("Minimal Decoration")]
    [SerializeField] private bool enableSmallBushes = false; // Disabled by default
    [SerializeField] private int bushDensity = 5; // Very sparse
    [SerializeField] private Vector2 bushScaleRange = new Vector2(0.5f, 1f);
    [SerializeField] private Color bushColor = new Color(0.2f, 0.35f, 0.15f, 0.7f); // Muted
    [SerializeField] private int bushPixelSize = 16;

    [Header("Optional Trees (Disabled by Default)")]
    [SerializeField] private bool enableTrees = false;
    [SerializeField] private int treeDensity = 2;
    [SerializeField] private Vector2 treeScaleRange = new Vector2(1f, 1.5f);
    [SerializeField] private Color treeColor = new Color(0.15f, 0.25f, 0.1f, 0.8f);

    [Header("Optional Flowers (Disabled by Default)")]
    [SerializeField] private bool enableFlowers = false;
    [SerializeField] private int flowerDensity = 8;
    [SerializeField] private Vector2 flowerScaleRange = new Vector2(0.15f, 0.25f);
    [SerializeField]
    private Color[] flowerColors = {
        new Color(0.8f, 0.3f, 0.3f, 0.7f),    // Muted red
        new Color(0.8f, 0.7f, 0.3f, 0.7f),    // Muted yellow
        new Color(0.6f, 0.3f, 0.8f, 0.7f),    // Muted purple
        new Color(0.8f, 0.8f, 0.8f, 0.7f)     // Muted white
    };

    [Header("Visual Style")]
    [SerializeField] private FilterMode pixelFilterMode = FilterMode.Point; // Crisp pixels
    [SerializeField] private bool enablePixelPerfect = true;
    [SerializeField] private float overallOpacity = 0.8f; // Subtle background

    [Header("Performance")]
    [SerializeField] private int maxBackgroundObjects = 200; // Much lower
    [SerializeField] private bool enableCulling = false; // Less needed with fewer objects

    // Generated objects
    private GameObject backgroundParent;
    private SpriteRenderer groundRenderer;
    private List<GameObject> backgroundObjects = new List<GameObject>();

    void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (generateOnStart)
        {
            GenerateBackground();
        }
    }

    /// <summary>
    /// Generate the complete clean background
    /// </summary>
    [ContextMenu("Generate Background")]
    public void GenerateBackground()
    {
        CreateBackgroundParent();
        GenerateCleanGroundTexture();
        GenerateMinimalVegetation();

        Debug.Log("Clean pixel background generated!");
    }

    /// <summary>
    /// Create organized parent object
    /// </summary>
    private void CreateBackgroundParent()
    {
        if (backgroundParent != null)
            DestroyImmediate(backgroundParent);

        backgroundParent = new GameObject("Clean Pixel Background");
        backgroundParent.transform.position = Vector3.zero;

        // Set to background layer
        int backgroundLayer = LayerMask.NameToLayer("Background");
        if (backgroundLayer < 0) backgroundLayer = 0;
        backgroundParent.layer = backgroundLayer;
    }

    /// <summary>
    /// Generate clean, subtle ground texture
    /// </summary>
    private void GenerateCleanGroundTexture()
    {
        Texture2D groundTexture = CreateCleanGroundTexture();
        groundTexture.filterMode = pixelFilterMode; // Pixel art style

        Sprite groundSprite = Sprite.Create(groundTexture,
            new Rect(0, 0, groundTexture.width, groundTexture.height),
            new Vector2(0.5f, 0.5f),
            groundTextureSize / (worldBounds.x * 2));

        // Create ground GameObject
        GameObject groundObject = new GameObject("Ground");
        groundObject.transform.SetParent(backgroundParent.transform);
        groundObject.transform.position = Vector3.zero;

        groundRenderer = groundObject.AddComponent<SpriteRenderer>();
        groundRenderer.sprite = groundSprite;
        groundRenderer.sortingOrder = -100;

        // Apply overall opacity
        Color groundColor = groundRenderer.color;
        groundColor.a = overallOpacity;
        groundRenderer.color = groundColor;

        // Scale to cover world bounds
        float scaleX = (worldBounds.x * 2.2f) / (groundTexture.width / (float)groundTextureSize * worldBounds.x * 2);
        float scaleY = (worldBounds.y * 2.2f) / (groundTexture.height / (float)groundTextureSize * worldBounds.y * 2);
        groundObject.transform.localScale = new Vector3(scaleX, scaleY, 1);
    }

    /// <summary>
    /// Create clean, pixel art ground texture
    /// </summary>
    private Texture2D CreateCleanGroundTexture()
    {
        Texture2D texture = new Texture2D(groundTextureSize, groundTextureSize);
        Color[] pixels = new Color[groundTextureSize * groundTextureSize];

        for (int y = 0; y < groundTextureSize; y++)
        {
            for (int x = 0; x < groundTextureSize; x++)
            {
                Color pixelColor = groundColor1; // Default base color

                if (enableGroundVariation)
                {
                    // Very subtle variation for pixel art style
                    float noise1 = Mathf.PerlinNoise(x * groundNoiseScale, y * groundNoiseScale);

                    // Quantize the noise for pixel art effect
                    noise1 = Mathf.Round(noise1 * 4f) / 4f;

                    if (noise1 < 0.3f)
                        pixelColor = groundColor1;
                    else if (noise1 < 0.7f)
                        pixelColor = groundColor2;
                    else
                        pixelColor = groundColor3;
                }

                pixels[y * groundTextureSize + x] = pixelColor;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    /// <summary>
    /// Generate minimal, non-distracting vegetation
    /// </summary>
    private void GenerateMinimalVegetation()
    {
        float area = worldBounds.x * worldBounds.y * 4;
        float areaScale = area / 100f;

        // Only generate enabled elements
        if (enableGrass)
            GeneratePixelGrass(Mathf.RoundToInt(grassDensity * areaScale));

        if (enableSmallBushes)
            GeneratePixelBushes(Mathf.RoundToInt(bushDensity * areaScale));

        if (enableTrees)
            GeneratePixelTrees(Mathf.RoundToInt(treeDensity * areaScale));

        if (enableFlowers)
            GeneratePixelFlowers(Mathf.RoundToInt(flowerDensity * areaScale));
    }

    /// <summary>
    /// Generate simple grass dots
    /// </summary>
    private void GeneratePixelGrass(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 position = GetRandomPosition();
            Color grassColor = Random.value > 0.5f ? grassColor1 : grassColor2;
            float scale = Random.Range(grassScaleRange.x, grassScaleRange.y);

            GameObject grass = CreatePixelGrassObject(position, grassColor, scale);
            RegisterBackgroundObject(grass);
        }
    }

    /// <summary>
    /// Generate small bushes
    /// </summary>
    private void GeneratePixelBushes(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 position = GetRandomPosition();
            float scale = Random.Range(bushScaleRange.x, bushScaleRange.y);

            GameObject bush = CreatePixelBushObject(position, bushColor, scale);
            RegisterBackgroundObject(bush);
        }
    }

    /// <summary>
    /// Generate minimal trees
    /// </summary>
    private void GeneratePixelTrees(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 position = GetRandomPosition();
            float scale = Random.Range(treeScaleRange.x, treeScaleRange.y);

            GameObject tree = CreatePixelTreeObject(position, treeColor, scale);
            RegisterBackgroundObject(tree);
        }
    }

    /// <summary>
    /// Generate subtle flowers
    /// </summary>
    private void GeneratePixelFlowers(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 position = GetRandomPosition();
            Color flowerColor = flowerColors[Random.Range(0, flowerColors.Length)];
            float scale = Random.Range(flowerScaleRange.x, flowerScaleRange.y);

            GameObject flower = CreatePixelFlowerObject(position, flowerColor, scale);
            RegisterBackgroundObject(flower);
        }
    }

    // ========================================================================
    // PIXEL ART OBJECT CREATION
    // ========================================================================

    private GameObject CreatePixelGrassObject(Vector3 position, Color color, float scale)
    {
        GameObject grass = new GameObject("PixelGrass");
        grass.transform.SetParent(backgroundParent.transform);
        grass.transform.position = position;
        grass.transform.localScale = Vector3.one * scale;

        SpriteRenderer renderer = grass.AddComponent<SpriteRenderer>();
        renderer.sprite = CreatePixelGrassSprite();
        renderer.color = color;
        renderer.sortingOrder = -90 + Random.Range(0, 5);

        return grass;
    }

    private GameObject CreatePixelBushObject(Vector3 position, Color color, float scale)
    {
        GameObject bush = new GameObject("PixelBush");
        bush.transform.SetParent(backgroundParent.transform);
        bush.transform.position = position;
        bush.transform.localScale = Vector3.one * scale;

        SpriteRenderer renderer = bush.AddComponent<SpriteRenderer>();
        renderer.sprite = CreatePixelBushSprite();
        renderer.color = color;
        renderer.sortingOrder = -80 + Random.Range(0, 5);

        return bush;
    }

    private GameObject CreatePixelTreeObject(Vector3 position, Color color, float scale)
    {
        GameObject tree = new GameObject("PixelTree");
        tree.transform.SetParent(backgroundParent.transform);
        tree.transform.position = position;
        tree.transform.localScale = Vector3.one * scale;

        SpriteRenderer renderer = tree.AddComponent<SpriteRenderer>();
        renderer.sprite = CreatePixelTreeSprite();
        renderer.color = color;
        renderer.sortingOrder = -70 + Random.Range(0, 3);

        return tree;
    }

    private GameObject CreatePixelFlowerObject(Vector3 position, Color color, float scale)
    {
        GameObject flower = new GameObject("PixelFlower");
        flower.transform.SetParent(backgroundParent.transform);
        flower.transform.position = position;
        flower.transform.localScale = Vector3.one * scale;

        SpriteRenderer renderer = flower.AddComponent<SpriteRenderer>();
        renderer.sprite = CreatePixelFlowerSprite();
        renderer.color = color;
        renderer.sortingOrder = -85 + Random.Range(0, 10);

        return flower;
    }

    // ========================================================================
    // PIXEL ART SPRITE CREATION
    // ========================================================================

    private Sprite CreatePixelGrassSprite()
    {
        // Very simple grass: just a few pixels
        int size = grassPixelSize;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        // Clear background
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;

        // Simple grass pattern - just a few vertical pixels
        int centerX = size / 2;
        for (int y = 0; y < size * 0.8f; y++)
        {
            if (y < size && centerX < size)
                pixels[y * size + centerX] = Color.white;

            // Maybe add one more blade
            if (Random.value > 0.5f && y < size * 0.6f)
            {
                int offsetX = centerX + (Random.value > 0.5f ? 1 : -1);
                if (offsetX >= 0 && offsetX < size)
                    pixels[y * size + offsetX] = Color.white;
            }
        }

        texture.SetPixels(pixels);
        texture.filterMode = pixelFilterMode;
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0f));
    }

    private Sprite CreatePixelBushSprite()
    {
        // Simple bush: small cluster of pixels
        int size = bushPixelSize;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        // Clear background
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;

        // Simple bush shape - just a small cluster
        Vector2 center = new Vector2(size * 0.5f, size * 0.4f);
        float radius = size * 0.3f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = Vector2.Distance(pos, center);

                if (distance < radius && Random.value > 0.3f) // Sparse pixels
                {
                    pixels[y * size + x] = Color.white;
                }
            }
        }

        texture.SetPixels(pixels);
        texture.filterMode = pixelFilterMode;
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0f));
    }

    private Sprite CreatePixelTreeSprite()
    {
        // Simple tree: trunk + small canopy
        int size = 32;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        // Clear background
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;

        // Simple trunk - just 2 pixels wide
        int trunkX = size / 2;
        int trunkHeight = size / 2;
        for (int y = 0; y < trunkHeight; y++)
        {
            pixels[y * size + trunkX] = new Color(0.4f, 0.2f, 0.1f, 1f); // Brown
            if (trunkX + 1 < size)
                pixels[y * size + (trunkX + 1)] = new Color(0.4f, 0.2f, 0.1f, 1f);
        }

        // Simple canopy - small circle of pixels
        Vector2 canopyCenter = new Vector2(size * 0.5f, size * 0.75f);
        float canopyRadius = size * 0.25f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = Vector2.Distance(pos, canopyCenter);

                if (distance < canopyRadius && Random.value > 0.2f)
                {
                    pixels[y * size + x] = Color.white; // Will be colored by renderer
                }
            }
        }

        texture.SetPixels(pixels);
        texture.filterMode = pixelFilterMode;
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0f));
    }

    private Sprite CreatePixelFlowerSprite()
    {
        // Tiny flower: just a few pixels
        int size = 8;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];

        // Clear background
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.clear;

        // Simple flower - just center pixel and maybe 4 around it
        int center = size / 2;
        pixels[center * size + center] = Color.white;

        // Add petals randomly
        if (Random.value > 0.5f) pixels[(center-1) * size + center] = Color.white; // Up
        if (Random.value > 0.5f) pixels[(center+1) * size + center] = Color.white; // Down
        if (Random.value > 0.5f) pixels[center * size + (center-1)] = Color.white; // Left
        if (Random.value > 0.5f) pixels[center * size + (center+1)] = Color.white; // Right

        texture.SetPixels(pixels);
        texture.filterMode = pixelFilterMode;
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    // ========================================================================
    // HELPER METHODS
    // ========================================================================

    private Vector3 GetRandomPosition()
    {
        return new Vector3(
            Random.Range(-worldBounds.x, worldBounds.x),
            Random.Range(-worldBounds.y, worldBounds.y),
            0
        );
    }

    private void RegisterBackgroundObject(GameObject obj)
    {
        backgroundObjects.Add(obj);

        if (backgroundObjects.Count > maxBackgroundObjects)
        {
            GameObject oldest = backgroundObjects[0];
            backgroundObjects.RemoveAt(0);
            DestroyImmediate(oldest);
        }
    }

    // ========================================================================
    // PUBLIC INTERFACE
    // ========================================================================

    [ContextMenu("Clear Background")]
    public void ClearBackground()
    {
        if (backgroundParent != null)
            DestroyImmediate(backgroundParent);

        backgroundObjects.Clear();
    }

    [ContextMenu("Regenerate Background")]
    public void RegenerateBackground()
    {
        ClearBackground();
        GenerateBackground();
    }

    public void SetWorldBounds(Vector2 newBounds)
    {
        worldBounds = newBounds;
        if (Application.isPlaying)
            RegenerateBackground();
    }

    // Fixed public methods to properly update settings
    public void SetGrassEnabled(bool enabled)
    {
        enableGrass = enabled;
        if (Application.isPlaying) RegenerateBackground();
    }

    public void SetBushesEnabled(bool enabled)
    {
        enableSmallBushes = enabled;
        if (Application.isPlaying) RegenerateBackground();
    }

    public void SetTreesEnabled(bool enabled)
    {
        enableTrees = enabled;
        if (Application.isPlaying) RegenerateBackground();
    }

    public void SetFlowersEnabled(bool enabled)
    {
        enableFlowers = enabled;
        if (Application.isPlaying) RegenerateBackground();
    }
}