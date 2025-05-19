using UnityEngine;

public interface IBehaviorStrategy
{
    void Execute(AgentContext context);
    //bool ShouldTransition(AgentContext context, out IBehaviorStrategy nextStrategy);
}