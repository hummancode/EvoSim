using UnityEngine;

public class EndMatingCommand : IReproductionCommand
{
    private readonly IAgent agent;

    public EndMatingCommand(IAgent agent)
    {
        this.agent = agent;
    }

    public void Execute()
    {
        if (agent?.ReproductionSystem != null)
        {
            Debug.Log($"Executing EndMatingCommand for {agent}");
            agent.ReproductionSystem.ResetMatingState();
        }
        else
        {
            Debug.LogError("Cannot execute EndMatingCommand: agent or reproduction system is null");
        }
    }
}