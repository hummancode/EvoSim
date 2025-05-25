using UnityEngine;

public class AgentIdentity : MonoBehaviour
{
    private static int nextId = 1;

    [SerializeField] private int agentId;
    [SerializeField] private int generation = 1;
    [SerializeField] private string agentName;

    void Awake()
    {
        // Assign unique ID if not already set
        if (agentId == 0)
        {
            agentId = nextId++;
        }

        // Generate name if not set
        if (string.IsNullOrEmpty(agentName))
        {
            agentName = $"Agent-{agentId}";
        }

        // Update GameObject name
        gameObject.name = $"{agentName} (Gen {generation})";
    }

    public int AgentId => agentId;
    public int Generation { get => generation; set => generation = value; }
    public string AgentName => agentName;

    public void SetGeneration(int newGeneration)
    {
        generation = newGeneration;
        gameObject.name = $"{agentName} (Gen {generation})";
    }
}