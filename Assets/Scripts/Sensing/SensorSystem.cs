using System.Collections.Generic;
using System;
using UnityEngine;

public class SensorSystem : MonoBehaviour, ISensorSystem
{
    [Header("Sensor Settings")]
    [SerializeField] private float detectionRange = 4f;
    [SerializeField] private LayerMask agentLayer;
    [SerializeField] private LayerMask foodLayer;

    [Header("Performance Settings")]
    [SerializeField] private float cacheRefreshInterval = 0.05f; // Cache results for 50ms
    [SerializeField] private int maxDetectionResults = 20; // Limit results per query
    [SerializeField] private bool useNonAllocMethods = true; // Use garbage-free methods

    [Header("Debugging")]
    [SerializeField] private bool showDetectionGizmos = true;
    [SerializeField] private Color detectionRangeColor = new Color(0.2f, 0.8f, 0.2f, 0.3f);

    // Performance optimizations
    private Transform agentTransform;
    private Dictionary<CacheKey, CachedResult> cache = new Dictionary<CacheKey, CachedResult>();
    private Collider2D[] colliderBuffer; // Reusable buffer for non-alloc methods

    // Cache structures
    private struct CacheKey
    {
        public System.Type componentType;
        public float range;
        public int filterHash;

        public CacheKey(System.Type type, float range, int filterHash)
        {
            this.componentType = type;
            this.range = range;
            this.filterHash = filterHash;
        }

        public override bool Equals(object obj)
        {
            if (obj is CacheKey other)
            {
                return componentType == other.componentType &&
                       Mathf.Approximately(range, other.range) &&
                       filterHash == other.filterHash;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return componentType.GetHashCode() ^ range.GetHashCode() ^ filterHash;
        }
    }

    private struct CachedResult
    {
        public object result;
        public float cacheTime;
        public bool isValid;

        public CachedResult(object result, float cacheTime)
        {
            this.result = result;
            this.cacheTime = cacheTime;
            this.isValid = true;
        }

        public bool IsExpired(float currentTime, float refreshInterval)
        {
            return currentTime - cacheTime > refreshInterval;
        }
    }

    void Awake()
    {
        agentTransform = transform;

        // Initialize layer masks
        if (foodLayer == 0)
            foodLayer = LayerMask.GetMask("Food");
        if (agentLayer == 0)
            agentLayer = LayerMask.GetMask("Agent");

        // Initialize collider buffer for non-alloc methods
        colliderBuffer = new Collider2D[maxDetectionResults];
    }

    void Update()
    {
        // Clean expired cache entries periodically
        if (Time.frameCount % 60 == 0) // Every 60 frames (~1 second at 60fps)
        {
            CleanExpiredCache();
        }
    }

    /// <summary>
    /// OPTIMIZED: Get nearest entity with caching and performance optimizations
    /// </summary>
    public T GetNearestEntity<T>(float range = -1, Func<T, bool> filter = null) where T : Component
    {
        float detectionRadius = range > 0 ? range : detectionRange;

        // Create cache key
        int filterHash = filter?.GetHashCode() ?? 0;
        CacheKey key = new CacheKey(typeof(T), detectionRadius, filterHash);

        // Check cache first
        if (cache.TryGetValue(key, out CachedResult cached))
        {
            if (!cached.IsExpired(Time.time, cacheRefreshInterval))
            {
                return cached.result as T;
            }
        }

        // Perform detection
        T result = FindNearestEntityOptimized<T>(detectionRadius, filter);

        // Cache the result
        cache[key] = new CachedResult(result, Time.time);

        return result;
    }

    /// <summary>
    /// OPTIMIZED: Core detection logic with performance improvements
    /// </summary>
    private T FindNearestEntityOptimized<T>(float detectionRadius, Func<T, bool> filter) where T : Component
    {
        LayerMask layerToUse = DetermineLayerMaskForType<T>();

        int numFound;
        if (useNonAllocMethods)
        {
            // Use non-alloc method to avoid garbage collection
            numFound = Physics2D.OverlapCircleNonAlloc(
                agentTransform.position,
                detectionRadius,
                colliderBuffer,
                layerToUse
            );
        }
        else
        {
            // Fallback to regular method
            Collider2D[] foundColliders = Physics2D.OverlapCircleAll(
                agentTransform.position,
                detectionRadius,
                layerToUse
            );

            numFound = Mathf.Min(foundColliders.Length, maxDetectionResults);
            for (int i = 0; i < numFound; i++)
            {
                colliderBuffer[i] = foundColliders[i];
            }
        }

        T nearest = null;
        float closestDistanceSqr = float.MaxValue;

        // Process found colliders
        for (int i = 0; i < numFound; i++)
        {
            Collider2D collider = colliderBuffer[i];

            // Skip self
            if (collider.gameObject == gameObject)
                continue;

            // Get component (with caching)
            T component = GetComponentCached<T>(collider);
            if (component == null)
                continue;

            // Apply filter
            if (filter != null && !filter(component))
                continue;

            // Use squared distance for performance (avoid sqrt)
            float distanceSqr = (collider.transform.position - agentTransform.position).sqrMagnitude;
            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                nearest = component;
            }
        }

        return nearest;
    }

    /// <summary>
    /// OPTIMIZED: Component caching to avoid repeated GetComponent calls
    /// </summary>
    private Dictionary<GameObject, Dictionary<System.Type, Component>> componentCache =
        new Dictionary<GameObject, Dictionary<System.Type, Component>>();

    private T GetComponentCached<T>(Collider2D collider) where T : Component
    {
        GameObject go = collider.gameObject;

        if (!componentCache.TryGetValue(go, out Dictionary<System.Type, Component> components))
        {
            components = new Dictionary<System.Type, Component>();
            componentCache[go] = components;
        }

        if (!components.TryGetValue(typeof(T), out Component component))
        {
            component = go.GetComponent<T>();
            components[typeof(T)] = component; // Cache even if null
        }

        return component as T;
    }

    /// <summary>
    /// OPTIMIZED: List version with performance improvements
    /// </summary>
    public List<T> GetEntitiesInRange<T>(float range = -1, Func<T, bool> filter = null) where T : Component
    {
        float detectionRadius = range > 0 ? range : detectionRange;
        LayerMask layerToUse = DetermineLayerMaskForType<T>();

        int numFound = Physics2D.OverlapCircleNonAlloc(
            agentTransform.position,
            detectionRadius,
            colliderBuffer,
            layerToUse
        );

        List<T> result = new List<T>(numFound); // Pre-allocate capacity

        for (int i = 0; i < numFound; i++)
        {
            Collider2D collider = colliderBuffer[i];

            if (collider.gameObject == gameObject)
                continue;

            T component = GetComponentCached<T>(collider);
            if (component == null)
                continue;

            if (filter != null && !filter(component))
                continue;

            result.Add(component);
        }

        return result;
    }

    public bool HasEntityInRange<T>(float range = -1, Func<T, bool> filter = null) where T : Component
    {
        // For boolean checks, we can exit early after finding first match
        return GetNearestEntity<T>(range, filter) != null;
    }

    /// <summary>
    /// Clean expired cache entries to prevent memory leaks
    /// </summary>
    private void CleanExpiredCache()
    {
        float currentTime = Time.time;
        List<CacheKey> expiredKeys = new List<CacheKey>();

        foreach (var kvp in cache)
        {
            if (kvp.Value.IsExpired(currentTime, cacheRefreshInterval * 2)) // Keep cache a bit longer
            {
                expiredKeys.Add(kvp.Key);
            }
        }

        foreach (var key in expiredKeys)
        {
            cache.Remove(key);
        }

        // Also clean component cache for destroyed objects
        List<GameObject> invalidObjects = new List<GameObject>();
        foreach (var kvp in componentCache)
        {
            if (kvp.Key == null) // GameObject was destroyed
            {
                invalidObjects.Add(kvp.Key);
            }
        }

        foreach (var obj in invalidObjects)
        {
            componentCache.Remove(obj);
        }
    }

    /// <summary>
    /// Force cache refresh for all or specific types
    /// </summary>
    public void RefreshCache(System.Type specificType = null)
    {
        if (specificType == null)
        {
            cache.Clear();
        }
        else
        {
            List<CacheKey> keysToRemove = new List<CacheKey>();
            foreach (var kvp in cache)
            {
                if (kvp.Key.componentType == specificType)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                cache.Remove(key);
            }
        }
    }

    // Existing methods with minor optimizations
    public void SetDetectionRange(float range)
    {
        float newRange = Mathf.Max(0.1f, range);
        if (!Mathf.Approximately(detectionRange, newRange))
        {
            detectionRange = newRange;
            cache.Clear(); // Clear cache when range changes
        }
    }

    public float GetDetectionRange()
    {
        return detectionRange;
    }

    private LayerMask DetermineLayerMaskForType<T>() where T : Component
    {
        if (typeof(T) == typeof(Food) || typeof(IEdible).IsAssignableFrom(typeof(T)))
        {
            return foodLayer;
        }
        else if (typeof(T) == typeof(AgentController) ||
                 typeof(T) == typeof(ReproductionSystem) ||
                 typeof(T) == typeof(EnergySystem))
        {
            return agentLayer;
        }

        return -1;
    }

    // Backward compatibility methods
    public GameObject GetNearestFood()
    {
        Food food = GetNearestEntity<Food>();
        return food?.gameObject;
    }

    public IEdible GetNearestEdible()
    {
        return GetNearestEntity<MonoBehaviour>(filter: mb => mb is IEdible) as IEdible;
    }

    // Performance monitoring
    public int GetCacheSize()
    {
        return cache.Count;
    }

    public int GetComponentCacheSize()
    {
        return componentCache.Count;
    }

    void OnDrawGizmos()
    {
        if (!showDetectionGizmos) return;

        Gizmos.color = detectionRangeColor;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }

    void OnDestroy()
    {
        // Clean up caches
        cache.Clear();
        componentCache.Clear();
    }
}