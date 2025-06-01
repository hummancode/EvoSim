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
        try
        {
            // CRITICAL FIX: Check if agent still exists
            if (agent == null)
            {
                Debug.Log("EndMatingCommand: Agent is null (probably died during mating)");
                return;
            }

            // CRITICAL FIX: Check if agent adapter is still valid
            if (agent is AgentAdapter adapter && !adapter.IsValid())
            {
                Debug.Log("EndMatingCommand: Agent died during mating process");
                return;
            }

            // CRITICAL FIX: Check if reproduction system still exists
            var reproductionSystem = agent.ReproductionSystem;
            if (reproductionSystem == null)
            {
                Debug.Log("EndMatingCommand: Agent's reproduction system is null");
                return;
            }

            // Safe to execute
            Debug.Log($"Executing EndMatingCommand for {GetAgentName(agent)}");
            reproductionSystem.ResetMatingState();
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Error in EndMatingCommand: {e.Message}");
        }
    }

    private string GetAgentName(IAgent agent)
    {
        try
        {
            if (agent is AgentAdapter adapter && adapter.IsValid())
                return adapter.GameObject.name;
            return "unknown/dead agent";
        }
        catch
        {
            return "destroyed agent";
        }
    }
}