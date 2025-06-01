
using UnityEngine;
using System.Collections.Generic;
using System;

public class AgentContext
{
    private readonly GameObject agentGameObject;
    private readonly Dictionary<Type, object> serviceCache = new Dictionary<Type, object>();

    // Constructor for Service Locator approach
    public AgentContext(GameObject agent)
    {
        agentGameObject = agent;
    }

    // Generic service resolver with caching
    public T GetService<T>() where T : class
    {
        var type = typeof(T);

        // Check cache first for performance
        if (serviceCache.TryGetValue(type, out var cached))
            return cached as T;

        // Resolve the service
        T service = ResolveService<T>();

        // Cache the result
        if (service != null)
            serviceCache[type] = service;

        return service;
    }

    private T ResolveService<T>() where T : class
    {
        var type = typeof(T);

        // Handle Unity Components
        if (typeof(Component).IsAssignableFrom(type))
        {
            return agentGameObject.GetComponent<T>();
        }

        // Handle Interface mappings
        if (type == typeof(IAgent))
        {
            var controller = agentGameObject.GetComponent<AgentController>();
            return new AgentAdapter(controller) as T;
        }

        if (type == typeof(IMateFinder))
        {
            var sensor = agentGameObject.GetComponent<SensorSystem>();
            return new SensorMateFinder(sensor, agentGameObject) as T;
        }

        if (type == typeof(IMaturityProvider))
        {
            return agentGameObject.GetComponent<AgeSystem>() as T;
        }

        // If not found, log warning
        Debug.LogWarning($"Service {type.Name} not found on {agentGameObject.name}");
        return null;
    }

    // Clear cache if components change (optional)
    public void RefreshServices()
    {
        serviceCache.Clear();
    }

    // ========================================================================
    // PUBLIC PROPERTIES - Keep existing API for backward compatibility
    // ========================================================================

    public IAgent Agent => GetService<IAgent>();
    public MovementSystem Movement => GetService<MovementSystem>();
    public SensorSystem Sensor => GetService<SensorSystem>();
    public IMateFinder MateFinder => GetService<IMateFinder>();
    public EnergySystem Energy => GetService<EnergySystem>();
    public ReproductionSystem Reproduction => GetService<ReproductionSystem>();
    public IMaturityProvider Maturity => GetService<IMaturityProvider>();

    // ========================================================================
    // HELPER METHODS - Keep existing functionality for AgentContext
    // ========================================================================

    public bool HasFoodNearby()
    {
        return Sensor?.GetNearestEdible() != null;
    }

    public bool HasPotentialMatesNearby()
    {
        if (MateFinder == null)
        {
            return false;
        }
        return MateFinder.FindNearestPotentialMate() != null;
    }
}
