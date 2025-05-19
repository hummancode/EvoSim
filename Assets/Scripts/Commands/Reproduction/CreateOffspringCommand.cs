
// Assets/Scripts/Commands/Reproduction/CreateOffspringCommand.cs
using UnityEngine;

public class CreateOffspringCommand : IReproductionCommand
{
    private readonly GameObject parent1;
    private readonly GameObject parent2;
    private readonly Vector3 position;
    private readonly AgentSpawner spawner;

    public CreateOffspringCommand(GameObject parent1, GameObject parent2, Vector3 position, AgentSpawner spawner)
    {
        this.parent1 = parent1;
        this.parent2 = parent2;
        this.position = position;
        this.spawner = spawner;
    }

    public void Execute()
    {
        if (spawner != null)
        {
            Debug.Log($"Executing CreateOffspringCommand at position {position}");
            spawner.SpawnOffspring(parent1, parent2, position);
        }
        else
        {
            Debug.LogError("Cannot execute CreateOffspringCommand: spawner is null");
        }
    }
}

// Assets/Scripts/Commands/Reproduction/InitiateMatingCommand.cs
