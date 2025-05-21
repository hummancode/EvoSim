

/// <summary>
/// Interface for building agent context objects
/// </summary>
public interface IAgentContextBuilder
{
    AgentContext BuildContext();
    void UpdateContext(AgentContext context);
}