using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple spatial partitioning system without Unity Jobs dependency
/// Provides O(1) spatial queries for massive performance improvement
/// </summary>
public class SpatialHashGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private float cellSize = 5f;
    [SerializeField] private Vector2 worldSize = new Vector2(50f, 50f);

    [Header("Performance")]
    [SerializeField] private int maxEntitiesPerCell = 50;
    [SerializeField] private bool enableDebugVisualization = false;

    // Grid data structures
    private Dictionary<int, HashSet<int>> grid = new Dictionary<int, HashSet<int>>();
    private Dictionary<int, Vector3> entityPositions = new Dictionary<int, Vector3>();
    private Dictionary<int, EntityType> entityTypes = new Dictionary<int, EntityType>();

    // Performance optimization
    private HashSet<int> tempCellResults = new HashSet<int>();
    private List<int> tempEntityList = new List<int>();

    // Grid dimensions
    private int gridWidth;
    private int gridHeight;
    private Vector2 gridOrigin;

    public enum EntityType
    {
        Agent,
        Food,
        Obstacle
    }

    private static SpatialHashGrid instance;
    public static SpatialHashGrid Instance => instance;

    #region Initialization

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGrid();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void InitializeGrid()
    {
        // Calculate grid dimensions
        gridWidth = Mathf.CeilToInt(worldSize.x / cellSize);
        gridHeight = Mathf.CeilToInt(worldSize.y / cellSize);
        gridOrigin = new Vector2(-worldSize.x * 0.5f, -worldSize.y * 0.5f);

        Debug.Log($"Spatial Grid initialized: {gridWidth}x{gridHeight} cells, cell size: {cellSize}");
    }

    #endregion

    #region Core Spatial Operations

    /// <summary>
    /// Register an entity in the spatial grid
    /// </summary>
    public void RegisterEntity(int entityID, Vector3 position, EntityType type)
    {
        // Remove from old position if it exists
        if (entityPositions.ContainsKey(entityID))
        {
            RemoveFromGrid(entityID, entityPositions[entityID]);
        }

        // Add to new position
        entityPositions[entityID] = position;
        entityTypes[entityID] = type;
        AddToGrid(entityID, position);
    }

    /// <summary>
    /// Update entity position
    /// </summary>
    public void UpdateEntityPosition(int entityID, Vector3 newPosition)
    {
        if (!entityPositions.ContainsKey(entityID))
        {
            Debug.LogWarning($"Trying to update unregistered entity {entityID}");
            return;
        }

        Vector3 oldPosition = entityPositions[entityID];
        int oldCellKey = GetCellKey(oldPosition);
        int newCellKey = GetCellKey(newPosition);

        // Only update grid if cell changed
        if (oldCellKey != newCellKey)
        {
            RemoveFromGrid(entityID, oldPosition);
            AddToGrid(entityID, newPosition);
        }

        entityPositions[entityID] = newPosition;
    }

    /// <summary>
    /// Unregister entity from spatial grid
    /// </summary>
    public void UnregisterEntity(int entityID)
    {
        if (entityPositions.TryGetValue(entityID, out Vector3 position))
        {
            RemoveFromGrid(entityID, position);
            entityPositions.Remove(entityID);
            entityTypes.Remove(entityID);
        }
    }

    #endregion

    #region Spatial Queries

    /// <summary>
    /// Find all entities within radius of position - ULTRA FAST
    /// </summary>
    public List<int> QueryEntitiesInRadius(Vector3 position, float radius, EntityType? typeFilter = null)
    {
        tempEntityList.Clear();
        tempCellResults.Clear();

        // Calculate which cells to check
        int cellRadius = Mathf.CeilToInt(radius / cellSize);
        Vector2Int centerCell = GetCellCoords(position);

        // Check all cells within radius
        for (int x = centerCell.x - cellRadius; x <= centerCell.x + cellRadius; x++)
        {
            for (int y = centerCell.y - cellRadius; y <= centerCell.y + cellRadius; y++)
            {
                int cellKey = GetCellKey(x, y);

                if (grid.TryGetValue(cellKey, out HashSet<int> cellEntities))
                {
                    tempCellResults.UnionWith(cellEntities);
                }
            }
        }

        // Filter by distance and type
        float radiusSquared = radius * radius;

        foreach (int entityID in tempCellResults)
        {
            if (entityPositions.TryGetValue(entityID, out Vector3 entityPos))
            {
                // Type filter
                if (typeFilter.HasValue && entityTypes[entityID] != typeFilter.Value)
                    continue;

                // Distance check
                Vector3 delta = entityPos - position;
                float distanceSquared = delta.x * delta.x + delta.y * delta.y;

                if (distanceSquared <= radiusSquared)
                {
                    tempEntityList.Add(entityID);
                }
            }
        }

        return new List<int>(tempEntityList);
    }

    /// <summary>
    /// Find nearest entity of specified type
    /// </summary>
    public int FindNearestEntity(Vector3 position, EntityType entityType, float maxRadius = 10f)
    {
        var nearbyEntities = QueryEntitiesInRadius(position, maxRadius, entityType);

        if (nearbyEntities.Count == 0)
            return -1;

        int closestEntity = -1;
        float closestDistanceSquared = float.MaxValue;

        foreach (int entityID in nearbyEntities)
        {
            if (entityPositions.TryGetValue(entityID, out Vector3 entityPos))
            {
                Vector3 delta = entityPos - position;
                float distanceSquared = delta.x * delta.x + delta.y * delta.y;

                if (distanceSquared < closestDistanceSquared)
                {
                    closestDistanceSquared = distanceSquared;
                    closestEntity = entityID;
                }
            }
        }

        return closestEntity;
    }

    /// <summary>
    /// Ultra-fast food finding
    /// </summary>
    public Vector3? FindNearestFood(Vector3 agentPosition, float searchRadius = 5f)
    {
        int foodEntityID = FindNearestEntity(agentPosition, EntityType.Food, searchRadius);

        if (foodEntityID != -1 && entityPositions.TryGetValue(foodEntityID, out Vector3 foodPosition))
        {
            return foodPosition;
        }

        return null;
    }

    /// <summary>
    /// Ultra-fast mate finding
    /// </summary>
    public List<int> FindPotentialMates(Vector3 agentPosition, float searchRadius = 3f)
    {
        return QueryEntitiesInRadius(agentPosition, searchRadius, EntityType.Agent);
    }

    #endregion

    #region Grid Management

    private void AddToGrid(int entityID, Vector3 position)
    {
        int cellKey = GetCellKey(position);

        if (!grid.TryGetValue(cellKey, out HashSet<int> cellEntities))
        {
            cellEntities = new HashSet<int>();
            grid[cellKey] = cellEntities;
        }

        cellEntities.Add(entityID);
    }

    private void RemoveFromGrid(int entityID, Vector3 position)
    {
        int cellKey = GetCellKey(position);

        if (grid.TryGetValue(cellKey, out HashSet<int> cellEntities))
        {
            cellEntities.Remove(entityID);

            // Clean up empty cells
            if (cellEntities.Count == 0)
            {
                grid.Remove(cellKey);
            }
        }
    }

    private int GetCellKey(Vector3 position)
    {
        Vector2Int coords = GetCellCoords(position);
        return GetCellKey(coords.x, coords.y);
    }

    private int GetCellKey(int x, int y)
    {
        return x * 73856093 ^ y * 19349663;
    }

    private Vector2Int GetCellCoords(Vector3 position)
    {
        int x = Mathf.FloorToInt((position.x - gridOrigin.x) / cellSize);
        int y = Mathf.FloorToInt((position.y - gridOrigin.y) / cellSize);

        // Clamp to grid bounds
        x = Mathf.Clamp(x, 0, gridWidth - 1);
        y = Mathf.Clamp(y, 0, gridHeight - 1);

        return new Vector2Int(x, y);
    }

    #endregion

    #region Auto-Registration

    [ContextMenu("Auto-Register All Entities")]
    public void AutoRegisterAllEntities()
    {
        // Clear existing data
        grid.Clear();
        entityPositions.Clear();
        entityTypes.Clear();

        // Register all agents
        AgentController[] agents = FindObjectsOfType<AgentController>();
        foreach (var agent in agents)
        {
            RegisterEntity(agent.GetInstanceID(), agent.transform.position, EntityType.Agent);
        }

        // Register all food
        Food[] foods = FindObjectsOfType<Food>();
        foreach (var food in foods)
        {
            RegisterEntity(food.GetInstanceID(), food.transform.position, EntityType.Food);
        }

        Debug.Log($"Auto-registered {agents.Length} agents and {foods.Length} food items");
    }

    public void UpdateAllEntityPositions()
    {
        // Update agent positions
        AgentController[] agents = FindObjectsOfType<AgentController>();
        foreach (var agent in agents)
        {
            int id = agent.GetInstanceID();
            if (entityPositions.ContainsKey(id))
            {
                UpdateEntityPosition(id, agent.transform.position);
            }
        }

        // Update food positions (if they move)
        Food[] foods = FindObjectsOfType<Food>();
        foreach (var food in foods)
        {
            int id = food.GetInstanceID();
            if (entityPositions.ContainsKey(id))
            {
                UpdateEntityPosition(id, food.transform.position);
            }
        }
    }

    #endregion

    #region Performance Monitoring

    void Update()
    {
        // Update spatial grid periodically
        if (Time.frameCount % 10 == 0)
        {
            UpdateAllEntityPositions();
        }
    }

    public string GetPerformanceStats()
    {
        int totalEntities = entityPositions.Count;
        int activeCells = grid.Count;

        return $"Simple Spatial Grid: " +
               $"Entities={totalEntities}, " +
               $"Active Cells={activeCells}, " +
               $"Grid Size={gridWidth}x{gridHeight}";
    }

    [ContextMenu("Log Performance Stats")]
    public void LogPerformanceStats()
    {
        Debug.Log(GetPerformanceStats());
    }

    #endregion

    #region Public Interface

    public int GetEntityCount() => entityPositions.Count;
    public int GetActiveCellCount() => grid.Count;
    public float GetCellSize() => cellSize;

    public void ClearAll()
    {
        grid.Clear();
        entityPositions.Clear();
        entityTypes.Clear();
    }

    #endregion
}