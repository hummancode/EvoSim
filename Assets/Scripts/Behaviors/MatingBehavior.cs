using UnityEngine;

public class MatingBehavior : IBehaviorStrategy
{
    private Transform partnerTransform;
    private IMovementStrategy stationaryStrategy;

    public MatingBehavior(Transform partner = null)
    {
        partnerTransform = partner;
        stationaryStrategy = new StationaryMovement();
    }

    public void Execute(AgentContext context)
    {
        // Stay still during mating
        context.Movement.SetMovementStrategy(stationaryStrategy);
    }
}