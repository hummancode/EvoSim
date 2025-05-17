using System.Collections.Generic;
using System;
using UnityEngine;

public class SensorSystem : MonoBehaviour
{
    [Header("Sensor Settings")]
    [SerializeField] private float detectionRange = 4f;
    [SerializeField] private LayerMask agentLayer; // For agent detection
    [SerializeField] private LayerMask foodLayer;  // For food detection

    [Header("Debugging")]
    [SerializeField] private bool showDetectionGizmos = true;
    [SerializeField] private Color detectionRangeColor = new Color(0.2f, 0.8f, 0.2f, 0.3f);

    // Reference to the parent agent's transform (cached for performance)
    private Transform agentTransform;

    void Awake()
    {
        agentTransform = transform;

        // Initialize default layers if not set
        if (foodLayer == 0)
        {
            foodLayer = LayerMask.GetMask("Food");
        }

        if (agentLayer == 0)
        {
            agentLayer = LayerMask.GetMask("Agent");
        }
    }

    // Implementation of ISensorCapability
    public T GetNearestEntity<T>(float range = -1, Func<T, bool> filter = null) where T : Component
    {
        // Use specified range or default
        float detectionRadius = range > 0 ? range : detectionRange;

        // Determine which layer mask to use based on type
        LayerMask layerToUse = DetermineLayerMaskForType<T>();

        // Find entities
        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            agentTransform.position,
            detectionRadius,
            layerToUse
        );

        T nearest = null;
        float closestDistanceSqr = float.MaxValue;

        foreach (Collider2D collider in colliders)
        {
            // Skip self
            if (collider.gameObject == gameObject)
                continue;

            // Get component of requested type
            T component = collider.GetComponent<T>();
            if (component == null)
                continue;

            // Apply filter if provided
            if (filter != null && !filter(component))
                continue;

            // Check distance
            float distanceSqr = (collider.transform.position - agentTransform.position).sqrMagnitude;
            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                nearest = component;
            }
        }

        return nearest;
    }

    public List<T> GetEntitiesInRange<T>(float range = -1, Func<T, bool> filter = null) where T : Component
    {
        // Use specified range or default
        float detectionRadius = range > 0 ? range : detectionRange;

        // Determine which layer mask to use based on type
        LayerMask layerToUse = DetermineLayerMaskForType<T>();

        // Find entities
        Collider2D[] colliders = Physics2D.OverlapCircleAll(
            agentTransform.position,
            detectionRadius,
            layerToUse
        );

        List<T> result = new List<T>();

        foreach (Collider2D collider in colliders)
        {
            // Skip self
            if (collider.gameObject == gameObject)
                continue;

            // Get component of requested type
            T component = collider.GetComponent<T>();
            if (component == null)
                continue;

            // Apply filter if provided
            if (filter != null && !filter(component))
                continue;

            result.Add(component);
        }

        return result;
    }

    public bool HasEntityInRange<T>(float range = -1, Func<T, bool> filter = null) where T : Component
    {
        return GetNearestEntity<T>(range, filter) != null;
    }

    public void SetDetectionRange(float range)
    {
        detectionRange = Mathf.Max(0.1f, range);
    }

    public float GetDetectionRange()
    {
        return detectionRange;
    }

    // Helper method to determine which layer mask to use based on the requested type
    private LayerMask DetermineLayerMaskForType<T>() where T : Component
    {
        // Check what type T is and return appropriate layer mask
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

        // Default: return all layers
        return -1;
    }

    // For backward compatibility
    public GameObject GetNearestFood()
    {
        Food food = GetNearestEntity<Food>();
        return food?.gameObject;
    }

    public IEdible GetNearestEdible()
    {
        return GetNearestEntity<MonoBehaviour>(filter: mb => mb is IEdible) as IEdible;
    }

    // Visualization for debugging
    void OnDrawGizmos()
    {
        if (!showDetectionGizmos) return;

        // Draw detection range circle
        Gizmos.color = detectionRangeColor;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw other visualizations...
    }
}