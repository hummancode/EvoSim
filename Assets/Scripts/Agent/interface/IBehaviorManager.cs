/// <summary>
/// Interface for managing agent behaviors
/// </summary>
public interface IBehaviorManager
{
    void UpdateBehavior(AgentContext context);
    void SetInitialBehavior(AgentContext context);
    void ForceBehavior<T>(AgentContext context) where T : IBehaviorStrategy, new();
}